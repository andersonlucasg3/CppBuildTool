using Shared.Compilation;
using Shared.Projects;

namespace BuildTool.Compilation;

public enum ECompilationResult
{
    Waiting,
    NothingToCompile,
    CompilationSuccess,
    CompilationFailed,
    LinkUpToDate,
    LinkSuccess,
    LinkFailed,
}

public class CompileModuleInfo(ModuleDefinition InModule, LinkAction InLinkAction)
{
    public readonly ModuleDefinition Module = InModule;
    public readonly LinkAction Link = InLinkAction;
    
    public string ModuleName => Module.Name;
    
    public ECompilationResult Result = ECompilationResult.Waiting;
    public CompileAction[] CompileActions = [];
}