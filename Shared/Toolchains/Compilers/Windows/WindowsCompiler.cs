using Shared.Compilation;

namespace Shared.Toolchains.Compilers.Windows;

public class WindowsCompiler(string InClangPath) : CppCompiler
{
    private readonly string _clangPath = InClangPath;

    public override string[] GetCompileCommandLine(CompileCommandInfo InCompileCommandInfo)
    {
        return [
            _clangPath,
            "/showIncludes",
            "/c",
            InCompileCommandInfo.TargetFile.PlatformPath,
            $"/I{InCompileCommandInfo.SourcesDirectory.PlatformPath}",
            .. InCompileCommandInfo.HeaderSearchPaths.Select(IncludeDirectory => $"/I{IncludeDirectory.PlatformPath}"),
            $"/Fo{InCompileCommandInfo.ObjectFile.PlatformPath}",
            "/std:c++20",
            "/W4",
            "/EHsc",
            "/GR",
            .. GetOptimizationArguments(InCompileCommandInfo.Configuration),
        ];
    }

    public override string[] GetLinkCommandLine(LinkCommandInfo InLinkCommandInfo)
    {
        return [
            _clangPath,
            "/LD",
            .. InLinkCommandInfo.ObjectFiles.Select(ObjectFile => ObjectFile.PlatformPath),
            "/link",
            $"/OUT:{InLinkCommandInfo.LinkedFile.Name}"
        ];
    }

    public override string GetObjectFileExtension()
    {
        return ".obj";
    }

    private static string[] GetOptimizationArguments(ECompileConfiguration InConfiguration)
    {
        return InConfiguration switch
        {
            ECompileConfiguration.Debug => ["/DDEBUG", "/Zi"],
            ECompileConfiguration.Release => ["/flto", "/O3", "/DNDEBUG"],
            _ => throw new ArgumentOutOfRangeException(nameof(InConfiguration), InConfiguration, null)
        };
    }
}