using Shared.Compilation;
using Shared.IO;
using Shared.Platforms;
using Shared.Processes;
using Shared.Projects;
using Shared.Sources;
using Shared.Toolchains;
using Shared.Toolchains.Compilers;

namespace BuildTool.Compilation;

public class CompileModuleTask(object InThreadSafeLock, CompileModuleInfo InInfo, ATargetPlatform InTargetPlatform, ECompileConfiguration InConfiguration)
{
    public void Compile(bool bPrintCompileCommands)
    {
        ISourceCollection SourceCollection = InInfo.SourceCollection;

        SourceCollection.GatherSourceFiles(InInfo.Module.SourcesDirectory);

        CompileAction[] SourceCompileActions = GenerateCompileActions(InTargetPlatform.Toolchain, SourceCollection, out InInfo.CompileActions);
        
        if (SourceCompileActions.Length == 0)
        {
            Console.WriteLine($"Nothing to compile, module {InInfo.ModuleName} is up to date.");

            InInfo.Result = ECompilationResult.NothingToCompile;
            
            return;
        }
            
        int CompileActionCount = SourceCompileActions.Length;
        
        Console.WriteLine($"Compiling module {InInfo.ModuleName} with {CompileActionCount} actions");
        
        bool bCompilationSuccessful = true;

        int Index = 0;
        Parallelization.ForEach(SourceCompileActions, InAction =>
        {
            DirectoryReference[] HeaderSearchPaths = [
                .. InInfo.Module.GetHeaderSearchPaths(ETargetPlatform.Any),
                .. InInfo.Module.GetHeaderSearchPaths(InTargetPlatform.Platform),
                .. InInfo.Module.GetDependencies(ETargetPlatform.Any).Select(DependencyModule => DependencyModule.SourcesDirectory),
                .. InInfo.Module.GetDependencies(InTargetPlatform.Platform).Select(DependencyModule => DependencyModule.SourcesDirectory)
            ];

            string[] CompilerDefinitions = CompilerDefinitionsProvider.GetAutomaticCompilerDefinitions(InTargetPlatform, InConfiguration, InInfo.Module);

            CompileCommandInfo CompileCommandInfo = new()
            {
                Module = InInfo.Module,
                SourcesDirectory = InInfo.Module.SourcesDirectory,
                TargetFile = InAction.SourceFile,
                DependencyFile = InAction.DependencyFile,
                ObjectFile = InAction.ObjectFile,
                HeaderSearchPaths = HeaderSearchPaths,
                Configuration = InConfiguration,
                TargetPlatform = InTargetPlatform.Platform,
                CompilerDefinitions = CompilerDefinitions
            };

            lock (InThreadSafeLock)
            {
                Console.WriteLine($"Compile [{InInfo.ModuleName}]: {InAction.SourceFile.Name}");
                if (bPrintCompileCommands)
                {
                    string[] CommandLine = InTargetPlatform.Toolchain.GetCompileCommandline(CompileCommandInfo);
                    Console.WriteLine($"    INFO: {string.Join(' ', CommandLine)}");
                }
            }
            
            ProcessResult CompileResult = InTargetPlatform.Toolchain.Compile(CompileCommandInfo);

            if (CompileResult.bSuccess)
            {
                ChecksumStorage.Shared.CompilationSuccess(InAction);
            }
            else
            {
                ChecksumStorage.Shared.CompilationFailed(InAction);
                
                bCompilationSuccessful = false;
            }

            lock (InThreadSafeLock)
            {
                if (CompileResult.bSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Compile [{++Index}/{CompileActionCount}] [{InInfo.ModuleName}]: {InAction.SourceFile.Name}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Compile [{++Index}/{CompileActionCount}] [{InInfo.ModuleName}]: {InAction.SourceFile.Name}{Environment.NewLine}{CompileResult.StandardError}");
                }

                Console.ResetColor();
            }
        });

        InInfo.Result = bCompilationSuccessful ? ECompilationResult.CompilationSuccess : ECompilationResult.CompilationFailed;
    }

    private CompileAction[] GenerateCompileActions(IToolchain InToolchain, ISourceCollection InSourceCollection, out CompileAction[] OutCompileActions)
    {
        ProjectDirectories Directories = ProjectDirectories.Shared;
        
        DirectoryReference ObjectsDirectory = Directories.CreateIntermediateObjectsDirectory(InInfo.ModuleName);

        List<CompileAction> FullCompileActionsList = [];
        List<CompileAction> FilteredCompileActionList = [];
        
        Parallelization.ForEach(InSourceCollection.SourceFiles, SourceFile =>
        {
            CompileAction SourceCompileAction = new(SourceFile, ObjectsDirectory, InToolchain, InSourceCollection);

            lock (InThreadSafeLock)
            {
                FullCompileActionsList.Add(SourceCompileAction);

                if (SourceCompileAction.bShouldCompile)
                {
                    FilteredCompileActionList.Add(SourceCompileAction);
                }
            }
        });
        OutCompileActions = [.. FullCompileActionsList];
        
        return [.. FilteredCompileActionList];
    }
}
