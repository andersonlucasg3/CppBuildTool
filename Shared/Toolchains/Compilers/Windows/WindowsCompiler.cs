using Shared.Compilation;
using Shared.Projects;

namespace Shared.Toolchains.Compilers.Windows;

public class WindowsCompiler(string InClangPath, string InLinkPath) : CppCompiler
{
    private readonly string _clangPath = InClangPath;
    private readonly string _linkPath = InLinkPath;

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
            .. InCompileCommandInfo.CompilerDefinitions.Select(Define => $"/{Define}"),
            .. GetOptimizationArguments(InCompileCommandInfo.Configuration),
        ];
    }

    public override string[] GetLinkCommandLine(LinkCommandInfo InLinkCommandInfo)
    {
        return [
            _linkPath,
            .. InLinkCommandInfo.ObjectFiles.Select(ObjectFile => $"\"{ObjectFile.PlatformPath}\""),
            GetLinkArgumentForBinaryType(InLinkCommandInfo.Module.BinaryType),
            $"/OUT:\"{InLinkCommandInfo.LinkedFile.PlatformPath}\"",
            .. InLinkCommandInfo.LinkWithLibraries.Select(LinkLibrary => $"/defaultlib:{LinkLibrary}")
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

    private static string GetLinkArgumentForBinaryType(EModuleBinaryType InBinaryType)
    {
        return InBinaryType switch
        {
            EModuleBinaryType.Application => "/EXE",
            EModuleBinaryType.StaticLibrary => throw new NotSupportedException($"{InBinaryType}"),
            EModuleBinaryType.DynamicLibrary => "/DLL",
            EModuleBinaryType.ShaderLibrary => throw new ShaderLibraryNotSupportedOnPlatformException($"{InBinaryType}"),
            _ => throw new ArgumentOutOfRangeException(nameof(InBinaryType), InBinaryType, null),
        };
    }
}