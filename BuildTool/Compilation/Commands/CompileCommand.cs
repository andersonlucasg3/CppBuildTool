using Shared.CommandLines;
using Shared.Commands;
using Shared.Compilation;
using Shared.Exceptions;
using Shared.Platforms;
using Shared.Projects;
using Shared.Extensions;
using Shared.Processes;
using Shared.IO;
using BuildTool.Resources;

namespace BuildTool.Compilation.Commands;

public class Compile : IExecutableCommand
{
    public string Name => "Compile";
    public string Example { get; } = string.Join(" ",
        "-Project=/path/to/project",
        "[-Modules=module1,module2,...]",
        $"-Platform=[{string.Join("|", Enum.GetNames<ETargetPlatform>())}]",
        $"-Configuration=[{string.Join('|', Enum.GetNames<ECompileConfiguration>())}]"
    );

    public readonly object _threadSafeLock = new();
    
    public bool Execute(IReadOnlyDictionary<string, ICommandLineArgument> Arguments)
    {
        string ProjectName = Arguments.GetArgumentValue<string>("Project", true) ?? "";
        string PlatformString = Arguments.GetArgumentValue<string>("Platform", true) ?? "";
        string ConfigurationString = Arguments.GetArgumentValue<string>("Configuration", true) ?? "";

        string[] Modules = Arguments.GetArrayArgument<string>("Modules");

        bool bRecompile = Arguments.ContainsKey("Recompile");
        bool bPrintCompileCommands = Arguments.ContainsKey("PrintCompileCommands");
        bool bPrintLinkCommands = Arguments.ContainsKey("PrintLinkCommands");
        
        ETargetPlatform CompilePlatform = PlatformString.ToEnum<ETargetPlatform>();
        ECompileConfiguration CompileConfiguration = ConfigurationString.ToEnum<ECompileConfiguration>();

        DirectoryReference RootDirectory = Environment.CurrentDirectory;
        ProjectFinder.CompileProject(RootDirectory, ProjectName);
        ProjectFinder.LoadProjects(RootDirectory, ProjectName);

        ProjectDefinition Project = ProjectFinder.FindProject(ProjectName);
        
        IHostPlatform HostPlatform = IHostPlatform.GetHost();
        if (!HostPlatform.SupportedTargetPlatforms.TryGetValue(CompilePlatform, out ITargetPlatform? TargetPlatform)) throw new TargetPlatformNotSupportedException(PlatformString);
        
        ProjectDirectories.Create(Project, TargetPlatform, CompileConfiguration);
        
        if (bRecompile)
        {
            Clean Clean = new();
            Clean.Execute(Arguments);
        }
        
        // must have all modules here, not only the selected ones due to dependency
        Dictionary<string, ModuleDefinition> AllModulesMap = [];
        AllModulesMap.AddFrom(Project.GetModules(ETargetPlatform.Any), Project.GetModules(TargetPlatform.Platform));

        ModuleDefinition[] AllModules = [.. AllModulesMap.Values];

        ModuleDefinition[] SelectedModules;
        if (Modules is null || Modules.Length == 0)
        {
            SelectedModules = AllModules;
            Console.WriteLine($"WARNING: No module specified, will compile all: {string.Join(", ", AllModulesMap.Keys)}");
        }
        else if (Modules.Length == 1)
        {
            string ModuleName = Modules[0];

            if (!AllModulesMap.TryGetValue(ModuleName, out ModuleDefinition? ModuleDefinition))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Module {ModuleName} not found in project {Project.Name}");
                Console.ResetColor();
                return false;
            }

            // TODO: fix this so specifying a module that has more than one layer of dependencies will not
            // cause a forever waiting linkage from dependencies
            SelectedModules = [
                ModuleDefinition,
                .. ModuleDefinition.GetDependencies(ETargetPlatform.Any),
                .. ModuleDefinition.GetDependencies(TargetPlatform.Platform)
            ];
            Console.WriteLine($"Compiling specified module: {ModuleName}");
        }
        else
        {
            SelectedModules = [.. AllModules.Where(Each => Modules.Contains(Each.Name))];
            Console.WriteLine($"Compiling specified modules: {string.Join(", ", Modules)}");
        }
        
        Console.WriteLine($"Compiling on {HostPlatform.Name} platform targeting {TargetPlatform.Name}");

        Dictionary<ModuleDefinition, CompileModuleInfo> ModuleCompilationResultMap = AllModules.ToDictionary(
            Module => Module,
            Module => new CompileModuleInfo(Module, new (Module, TargetPlatform.Toolchain))
        );
        
        CompileModuleInfo[] CompileModuleInfos = [.. SelectedModules.Select(Module => ModuleCompilationResultMap[Module])];

        ChecksumStorage.Shared.LoadChecksums();

        bool bSuccess = true;
        Parallelization.ForEach(CompileModuleInfos, ModuleInfo =>
        {
            CompileModuleTask CompileTask = new(_threadSafeLock, ModuleInfo, TargetPlatform, CompileConfiguration);
            CompileTask.Compile(bPrintCompileCommands);

            CopyResourcesTask CopyResourcesTask = new(ModuleInfo.Module);
            CopyResourcesTask.Copy();

            CompileModuleInfo Info = ModuleCompilationResultMap[ModuleInfo.Module];

            bSuccess &= Info.Result is ECompilationResult.CompilationSuccess or ECompilationResult.NothingToCompile;

            LinkModuleTask LinkTask = new(_threadSafeLock, ModuleInfo, TargetPlatform);
            LinkTask.Link(ModuleCompilationResultMap, bPrintLinkCommands);
        });

        ChecksumStorage.Shared.SaveChecksums();

        if (bSuccess)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Project {Project.Name} compiled successfully");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Project {Project.Name} generated compile errors");
        }
        
        Console.ResetColor();

        return bSuccess;
    }
}

public class TargetPlatformNotSupportedException(string InMessage) : BaseException(InMessage);