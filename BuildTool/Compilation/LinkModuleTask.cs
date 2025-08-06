using Shared.IO;
using Shared.Platforms;
using Shared.Processes;
using Shared.Projects;
using Shared.Toolchains;

namespace BuildTool.Compilation;

public class LinkModuleTask(object InThreadSafeLock, CompileModuleInfo InInfo, ITargetPlatform InTargetPlatform)
{
    private readonly ProjectDirectories _compileDirectories = ProjectDirectories.Shared;

    public void Link(Dictionary<ModuleDefinition, CompileModuleInfo> ModuleCompilationResultMap, bool bPrintLinkCommands)
    {
        if (InInfo.Result is ECompilationResult.NothingToCompile && InInfo.Link.LinkedFile.bExists)
        {
            Console.WriteLine($"Link [{InInfo.Module.Name}]: Up to date");

            InInfo.Result = ECompilationResult.LinkUpToDate;

            return;
        }

        bool bCanLink;
        do
        {
            ModuleDefinition[] Dependencies = [
                .. InInfo.Module.GetDependencies(InTargetPlatform.Platform),
                .. InInfo.Module.GetDependencies(ETargetPlatform.Any),
            ];

            bool bAnyDependencyFailed = Dependencies.Any(DependencyModule =>
            {
                ECompilationResult Result = ModuleCompilationResultMap[DependencyModule].Result;
                return Result == ECompilationResult.LinkFailed || Result == ECompilationResult.CompilationFailed;
            });

            bool bAllDependenciesSucceeded = Dependencies.All(DependencyModule =>
            {
                ECompilationResult Result = ModuleCompilationResultMap[DependencyModule].Result;
                return Result is ECompilationResult.LinkSuccess or ECompilationResult.LinkUpToDate;
            });

            if (bAnyDependencyFailed)
            {
                lock (InThreadSafeLock)
                {
                    InInfo.Result = ECompilationResult.LinkFailed;
                }

                return;
            }
            
            bCanLink = bAllDependenciesSucceeded;
            
            Thread.Sleep(1);
        } 
        while (!bCanLink);

        FileReference[] ObjectFiles = [.. InInfo.CompileActions.Select(Action => Action.ObjectFile)];

        DirectoryReference[] LibrarySearchPaths = [
            _compileDirectories.CreateBaseConfigurationDirectory(ECompileBaseDirectory.Binaries),
            .. InInfo.Module.LibrarySearchPaths
        ];

        string[] LinkWithLibraries = [.. InInfo.Module.GetLinkWithLibraries(InTargetPlatform.Platform)];
        
        LinkCommandInfo LinkCommandInfo = new()
        {
            Platform = InTargetPlatform.Platform,
            Module = InInfo.Module,
            LinkedFile = InInfo.Link.LinkedFile,
            LibrarySearchPaths = LibrarySearchPaths,
            ObjectFiles = ObjectFiles,
            TargetPlatform = InTargetPlatform.Platform,
            LinkWithLibraries = LinkWithLibraries
        };
        
        lock (InThreadSafeLock)
        {
            Console.WriteLine($"Link [{InInfo.Module.Name}]: {InInfo.Link.LinkedFile.Name}");
            if (bPrintLinkCommands)
            {
                string[] CommandLine = InTargetPlatform.Toolchain.GetLinkCommandLine(LinkCommandInfo);
                Console.WriteLine($"    INFO: {string.Join(' ', CommandLine)}");
            }
        }

        ProcessResult LinkResult = InTargetPlatform.Toolchain.Link(LinkCommandInfo);

        lock (InThreadSafeLock)
        {
            if (LinkResult.bSuccess)
            {
                InInfo.Result = ECompilationResult.LinkSuccess;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Link [{InInfo.ModuleName}]: {InInfo.Link.LinkedFile.Name}");
            }
            else
            {
                InInfo.Result = ECompilationResult.LinkFailed;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Link [{InInfo.ModuleName}]: {InInfo.Link.LinkedFile.Name}{Environment.NewLine}{LinkResult.StandardError}");
            }

            Console.ResetColor();
        }
    }
}
