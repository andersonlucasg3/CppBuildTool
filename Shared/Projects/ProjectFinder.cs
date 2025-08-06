using System.Reflection;
using System.Diagnostics;

namespace Shared.Projects;

using IO;
using Processes;
using Exceptions;

public static class ProjectFinder 
{
    class CSharpProjectCompilationException(string InMessage) : BaseException(InMessage);
    class FailedToCreateProjectInstanceException(string InMessage) : BaseException(InMessage);

    private static readonly Dictionary<string, ProjectDefinition> _loadedProjectsByName = [];
    private static readonly Dictionary<Type, ProjectDefinition> _loadedProjectsByType = [];

    public static void CompileProject(DirectoryReference InProjectRootDirectory, string InProjectName)
    {
        DirectoryReference IntermediateProjectsDirectory = InProjectRootDirectory.Combine(InProjectName, "Intermediate", "CSharpProjects");

        bool bSpawnNewProcess = !IntermediateProjectsDirectory.bExists || IntermediateProjectsDirectory.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly).Length == 0;

        FileReference InCSharpProjectFile = InProjectRootDirectory.CombineFile(InProjectName, $"{InProjectName}.csproj");

        ProcessResult ProcessResult = ProcessExecutorExtension.Run([
            "dotnet",
            "build", $"{InCSharpProjectFile.PlatformPath}",
            "-c", "Debug",
            "-o", $"{IntermediateProjectsDirectory.PlatformPath}",
        ], true);

        if (!ProcessResult.bSuccess) throw new CSharpProjectCompilationException(ProcessResult.StandardOutput);

        // For some reason, on windows when running compile for the first time
        // the load projects function won't find the output dlls
        // I fixed it by spawning a new process with the same arguments
        if (bSpawnNewProcess)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Process.GetCurrentProcess().MainModule?.FileName,
                Arguments = Environment.CommandLine.TrimStart(),
                UseShellExecute = false
            });

            Environment.Exit(0);
        }
    }

    public static void LoadProjects(DirectoryReference InProjectRootDirectory, string InProjectName)
    {
        DirectoryReference IntermediateProjectsDirectory = InProjectRootDirectory.Combine(InProjectName, "Intermediate", "CSharpProjects");

        FileReference[] DllFiles = IntermediateProjectsDirectory.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly);

        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            AssemblyName AssemblyName = new(args.Name);
            FileReference DllFile = IntermediateProjectsDirectory.CombineFile($"{AssemblyName.Name!}.dll");
            return DllFile.bExists ? Assembly.LoadFile(DllFile.PlatformPath) : null;
        };

        foreach (FileReference DllFile in DllFiles)
        {
            Assembly ProjectAssembly = Assembly.LoadFile(DllFile.PlatformPath);

            Type[] Types = ProjectAssembly.GetTypes();
            Type[] ProjectTypes = [.. Types.Where(Type => Type.IsClass && !Type.IsAbstract && Type.IsSubclassOf(typeof(ProjectDefinition)))];

            foreach (Type ProjectType in ProjectTypes)
            {
                if (Activator.CreateInstance(ProjectType) is not ProjectDefinition Project)
                {
                    throw new ProjectNotFoundException($"Could not create instance of project: {ProjectType.Name}");
                }

                if (!_loadedProjectsByType.TryAdd(ProjectType, Project) || !_loadedProjectsByName.TryAdd(Project.Name, Project))
                {
                    throw new FailedToCreateProjectInstanceException($"Project type already created: {ProjectType.Name}");
                }
            }
        }
    }

    public static TProject FindProject<TProject>()
        where TProject : ProjectDefinition
    {
        Type ProjectType = typeof(TProject);

        if (!_loadedProjectsByType.TryGetValue(ProjectType, out ProjectDefinition? Project))
        {
            throw new ProjectNotFoundException($"Missing project {ProjectType.Name}");
        }

        Project.Configure(Environment.CurrentDirectory);

        return (TProject)Project;
    }

    public static ProjectDefinition FindProject(string InProjectName)
    {
        if (!_loadedProjectsByName.TryGetValue(InProjectName, out ProjectDefinition? Project))
        {
            throw new ProjectNotFoundException($"Missing project {InProjectName}");
        }

        Project.Configure(Environment.CurrentDirectory);

        return Project;
    }
}

public class ProjectNotFoundException(string InMessage) : BaseException(InMessage);