using Shared.Toolchains;

namespace Shared.Platforms;

public class VisionOSPlatform(XcodeToolchain InToolchain) : ITargetPlatform
{
    public ETargetPlatform Platform => ETargetPlatform.visionOS;
    public IToolchain Toolchain => InToolchain;
}