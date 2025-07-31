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

        SupportedTargetPlatforms = new Dictionary<ETargetPlatform, ITargetPlatform>
        {
            { ETargetPlatform.Windows, this },
            // Android
            // Consoles
            // ...
        };
    }
}
