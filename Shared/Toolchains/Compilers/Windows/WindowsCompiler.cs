namespace Shared.Toolchains.Compilers.Windows;

// TODO: implement this

public class WindowsCompiler : CppCompiler
{
    public override string[] GetCompileCommandLine(CompileCommandInfo InCompileCommandInfo)
    {
        throw new NotImplementedException();
    }

    public override string[] GetLinkCommandLine(LinkCommandInfo InLinkCommandInfo)
    {
        throw new NotImplementedException();
    }

    public override string GetObjectFileExtension()
    {
        throw new NotImplementedException();
    }
}