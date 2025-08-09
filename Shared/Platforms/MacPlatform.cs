using Shared.Toolchains;

namespace Shared.Platforms;

public class MacPlatform : IHostPlatform
{
    public IReadOnlyDictionary<ETargetPlatform, ITargetPlatform> SupportedTargetPlatforms { get; }

    public ETargetPlatform Platform => ETargetPlatform.macOS;
    public IToolchain Toolchain { get; }

    public MacPlatform()
    {
        XcodeToolchain XcodeToolchain = new();
        AndroidToolchain AndroidToolchain = new();
        
        SupportedTargetPlatforms = new Dictionary<ETargetPlatform, ITargetPlatform>
        {
            { ETargetPlatform.macOS, this },
            { ETargetPlatform.iOS, new IOSPlatform(XcodeToolchain) },
            { ETargetPlatform.tvOS, new TVOSPlatform(XcodeToolchain) },
            { ETargetPlatform.visionOS, new VisionOSPlatform(XcodeToolchain) },
            { ETargetPlatform.Android, new AndroidPlatform(AndroidToolchain) }
        };
        
        Toolchain = XcodeToolchain;
    }
}

