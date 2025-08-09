namespace Shared.Toolchains;

using IO;
using Projects;
using Platforms;
using Exceptions;
using Compilers.Android;

public class AndroidToolchain : ClangToolchain
{
    const string SupportedAndroidNdkVersion = "29.0.13846066";

    private readonly AndroidCompiler _compiler;

    public AndroidToolchain()
    {
        string AndroidPrebuiltPlatform;
        string ExpectedAndroidSdkPath;
        if (IHostPlatform.IsWindows())
        {
            AndroidPrebuiltPlatform = "windows-x86_64";
            ExpectedAndroidSdkPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE")!, "AppData", "Local", "Android", "Sdk");
        }
        else if (IHostPlatform.IsMacOS())
        {
            AndroidPrebuiltPlatform = "darwin-x86_64";
            ExpectedAndroidSdkPath = Path.Combine(Environment.GetEnvironmentVariable("HOME")!, "Library", "Android", "sdk");
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        string ExpectedAndroidNdkPath = Path.Combine(ExpectedAndroidSdkPath, "ndk", SupportedAndroidNdkVersion);


        DirectoryReference ExpectedAndroidSdkDirectory = ExpectedAndroidSdkPath;
        DirectoryReference AndroidHome = Environment.GetEnvironmentVariable("ANDROID_HOME") ?? ExpectedAndroidSdkPath;

        DirectoryReference AndroidNdk = ExpectedAndroidNdkPath;

        if (!AndroidHome.bExists)
        {
            AndroidHome = ExpectedAndroidSdkDirectory;
        }

        if (!AndroidHome.bExists || !AndroidNdk.bExists)
        {
            throw new AndroidSdkNotInstalledException(AndroidHome.bExists, AndroidNdk.bExists);
        }

        DirectoryReference PrebuiltPlatformRoot = AndroidNdk.Combine("toolchains", "llvm", "prebuilt", AndroidPrebuiltPlatform);

        _compiler = new(PrebuiltPlatformRoot);
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

public class AndroidSdkNotInstalledException(bool bHaveSdk, bool bHaveNdk) : BaseException($"HaveSdk: {bHaveSdk}, HaveNdk: {bHaveNdk}");