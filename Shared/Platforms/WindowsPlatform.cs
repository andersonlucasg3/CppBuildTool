using Shared.Toolchains;

namespace Shared.Platforms;

public class WindowsPlatform : IHostPlatform
{
    public ETargetPlatform Platform => ETargetPlatform.Windows;
    public IToolchain Toolchain { get; }
    public IReadOnlyDictionary<ETargetPlatform, ITargetPlatform> SupportedTargetPlatforms { get; }

    public WindowsPlatform()
    {
        Toolchain = new VisualStudioToolchain();

        AndroidToolchain AndroidToolchain = new();

        SupportedTargetPlatforms = new Dictionary<ETargetPlatform, ITargetPlatform>
        {
            { ETargetPlatform.Windows, this },
            { ETargetPlatform.Android, new AndroidPlatform(AndroidToolchain) },
        };
    }
}
