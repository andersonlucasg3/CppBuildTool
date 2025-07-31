using Shared.IO;
using Shared.Misc;
using Shared.Projects;
using Shared.Processes;
using Shared.Platforms;
using Shared.Compilation;

namespace BuildTool.ProjectGeneration;

using Shared.Extensions;
using VisualStudio.Filters;
using VisualStudio.Projects;
using VisualStudio.Solutions;

public class VisualStudioProjectGenerator(ProjectDefinition InProjectDefinition, ITargetPlatform InTargetPlatform) : IProjectGenerator
{
    private readonly ECompileConfiguration[] _compileConfigurations = Enum.GetValues<ECompileConfiguration>();

    public void Generate()
    {
        DirectoryReference ProjectsDirectory = ProjectDirectories.Shared.CreateIntermediateProjectsDirectory();

        string ProgramsDirectory = Path.Combine(Environment.CurrentDirectory, "Programs");
        FileReference[] CSharpProjects = [.. Directory.EnumerateFiles(ProgramsDirectory, "*.csproj", SearchOption.AllDirectories)];

        Dictionary<string, ModuleDefinition> Modules = new();
        Modules.AddFrom(InProjectDefinition.GetModules(ETargetPlatform.Any), InProjectDefinition.GetModules(InTargetPlatform.Platform));

        Dictionary<ModuleDefinition, FileReference> ModuleVcxProjFileMap = 
            Modules.Values.ToDictionary(Module => Module, Module => ProjectsDirectory.CombineFile($"{Module.Name}.vcxproj"));

        FileReference SolutionFile = $"{InProjectDefinition.Name}.sln";
        Solution Solution = GenerateSolutionFile(SolutionFile, CSharpProjects, ModuleVcxProjFileMap);

        Parallelization.ForEach([.. Solution.Projects], Project =>
        {
            if (!Modules.TryGetValue(Project.ProjectName, out ModuleDefinition? Module)) return;

            ModuleDefinition[] ModuleDependencies = [
                .. Module.GetDependencies(ETargetPlatform.Any),
                .. Module.GetDependencies(InTargetPlatform.Platform)
            ];

            SolutionProject[] Dependencies = [.. ModuleDependencies.Select(DependencyModule => Solution.Projects.First(Project => Project.ProjectName == DependencyModule.Name))];
            
            GenerateVCXProj(InProjectDefinition.Name, Module, ModuleDependencies, Project, Dependencies, ModuleVcxProjFileMap[Module]);
        });
    }

    private Solution GenerateSolutionFile(FileReference InSolutionFile, FileReference[] InCSharpProjectFiles, Dictionary<ModuleDefinition, FileReference> InModuleProjectFileMap)
    {
        IndentedStringBuilder StringBuilder = new();

        Dictionary<SolutionProject, SolutionProject> NestedProjectsMap = [];

        SolutionProject ProgramsFolder = new("Programs", "Programs", [], [], ESolutionProjectKind.Folder);
        SolutionProject[] CSharpProjects = [.. InCSharpProjectFiles.Select(File => new SolutionProject(File.NameWithoutExtension, File.RelativePath, _compileConfigurations, [ETargetPlatform.Any], ESolutionProjectKind.CSharpProject))];
        Array.ForEach(CSharpProjects, Project => NestedProjectsMap.Add(Project, ProgramsFolder));

        SolutionProject EngineFolder = new("Engine Modules", "Engine Modules", [], [], ESolutionProjectKind.Folder);
        SolutionProject[] EngineProjects = [.. InModuleProjectFileMap.Select(KVPair => new SolutionProject(KVPair.Key.Name, KVPair.Value.RelativePath, _compileConfigurations, [InTargetPlatform.Platform]))];
        Array.ForEach(EngineProjects, Project => NestedProjectsMap.Add(Project, EngineFolder));

        SolutionProject[] Projects = [
            EngineFolder,
            .. EngineProjects,
            ProgramsFolder,
            .. CSharpProjects,
        ];

        Solution Solution = new(Projects, NestedProjectsMap);

        Solution.Build(StringBuilder);
        
        InSolutionFile.WriteAllText(StringBuilder.ToString());

        return Solution;
    }

    private static void GenerateVCXProj(string InProjectName, ModuleDefinition InModule, ModuleDefinition[] InModuleDependencies, SolutionProject InProject, SolutionProject[] Dependencies, FileReference InVcxProjFile)
    {
        IndentedStringBuilder StringBuilder = new();

        DirectoryReference[] DependenciesSourcesDirectories = [.. InModuleDependencies.Select(Dependency => Dependency.SourcesDirectory)];

        Project Project = new(new ProjectDependencies
            {
                ProjectName = InProjectName,
                Project = InProject,
                Dependencies = Dependencies,
                BinaryType = InModule.BinaryType,
                IntermediateDirectory = ProjectDirectories.Shared.CreateBaseDirectory(ECompileBaseDirectory.Intermediate),
                BinariesDirectory = ProjectDirectories.Shared.CreateBaseDirectory(ECompileBaseDirectory.Binaries),
                SourcesCollection = InModule.Sources!,
                ProjectSourcesDirectory = InModule.SourcesDirectory,
                DependenciesSourcesDirectories = DependenciesSourcesDirectories,
        }
        );

        Project.Build(StringBuilder);

        InVcxProjFile.WriteAllText(StringBuilder.ToString());

        // now the project filters file

        ProjectFilters ProjectFilters = new(InModule.RootDirectory, InModule.Sources!);

        StringBuilder.Clear();

        ProjectFilters.Build(StringBuilder);

        FileReference FiltersFile = InVcxProjFile.ChangeExtension(".vcxproj.filters");

        FiltersFile.WriteAllText(StringBuilder.ToString());
    }
}
