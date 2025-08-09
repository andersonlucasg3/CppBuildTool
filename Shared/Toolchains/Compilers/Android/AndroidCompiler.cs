namespace Shared.Toolchains.Compilers;

public class AndroidCompiler : CppCompiler
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