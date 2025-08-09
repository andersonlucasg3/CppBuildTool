namespace Shared.Toolchains;

using IO;
using Platforms;
using Projects;
using Toolchains.Compilers;

public class AndroidToolchain : ClangToolchain
{
    private readonly AndroidCompiler _compiler;

    public AndroidToolchain()
    {
        _compiler = new();   
    }

    public override string GetBinaryTypeExtension(EModuleBinaryType BinaryType)
    {
        return BinaryType switch
        {
            EModuleBinaryType.Application => throw new NotSupportedException("Android does not support EModuleBinaryType.Application"),
            EModuleBinaryType.StaticLibrary => ".a",
            EModuleBinaryType.DynamicLibrary => ".so",
            _ => throw new ArgumentOutOfRangeException(nameof(BinaryType), BinaryType, null)
        };
    }

    public override string GetBinaryTypePrefix(EModuleBinaryType BinaryType)
    {
        return BinaryType switch
        {
            EModuleBinaryType.Application => "",
            EModuleBinaryType.StaticLibrary => "lib",
            EModuleBinaryType.DynamicLibrary => "lib",
            EModuleBinaryType.ShaderLibrary => "",
            _ => throw new ArgumentOutOfRangeException(nameof(BinaryType), BinaryType, null)
        };
    }

    public override string[] GetCompileCommandline(CompileCommandInfo InCompileCommandInfo)
    {
        return _compiler.GetCompileCommandLine(InCompileCommandInfo);
    }

    public override string[] GetLinkCommandLine(LinkCommandInfo InLinkCommandInfo)
    {
        return _compiler.GetLinkCommandLine(InLinkCommandInfo);
    }

    public override string GetObjectFileExtension(FileReference InSourceFile)
    {
        return _compiler.GetObjectFileExtension();
    }

    public override string[] GetAutomaticModuleCompilerDefinitions(ModuleDefinition InModule, ETargetPlatform InTargetPlatform)
    {
        List<string> CompilerDefinitions = [];

        CompilerDefinitions.Add($"{InModule.Name.ToUpper()}_API=");

        ModuleDefinition[] Dependencies = [
            .. InModule.GetDependencies(ETargetPlatform.Any),
            .. InModule.GetDependencies(InTargetPlatform)
        ];

        foreach (ModuleDefinition Dependency in Dependencies)
        {
            CompilerDefinitions.Add($"{Dependency.Name.ToUpper()}_API=");
        }

        return [.. CompilerDefinitions];
    }
}