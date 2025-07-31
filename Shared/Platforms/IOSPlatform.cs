using Shared.Toolchains;

namespace Shared.Platforms;

public class IOSPlatform(XcodeToolchain InToolchain) : ITargetPlatform
{
    public ETargetPlatform Platform => ETargetPlatform.iOS;
    public IToolchain Toolchain { get; } = InToolchain;
}