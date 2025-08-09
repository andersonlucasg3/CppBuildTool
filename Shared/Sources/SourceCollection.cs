namespace Shared.Sources;

using IO;
using Projects;
using Platforms;

public interface ISourceCollection
{
    public string[] HeaderFilesExtensions { get; }
    public string[] SourceFilesExtensions { get; }
    public string[] AllFilesExtensions { get; }

    public FileReference[] HeaderFiles { get; }
    public FileReference[] SourceFiles { get; }
    public FileReference[] AllFiles { get; }

    public void GatherSourceFiles(DirectoryReference InSourceRootDirectory, ETargetPlatform InTargetPlatform);

    public static ISourceCollection CreateSourceCollection(ETargetPlatform InTargetPlatform, EModuleBinaryType InBinaryType)
    {
        return ITargetPlatform.GetPlatformGroup(InTargetPlatform) switch
        {
            ETargetPlatformGroup.Apple => InBinaryType switch
            {
                EModuleBinaryType.ShaderLibrary => new MetalShaderSourceCollection(),
                _ => new AppleSourceCollection(),
            },
            ETargetPlatformGroup.Google => new CppSourceCollection(),
            ETargetPlatformGroup.Microsoft => new CppSourceCollection(),
            _ => throw new PlatformNotSupportedException(InTargetPlatform),
        };
    }
}