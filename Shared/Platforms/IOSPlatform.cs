using Shared.Toolchains;

namespace Shared.Platforms;

public class IOSPlatform(XcodeToolchain InToolchain) : ITargetPlatform
{
    public virtual ETargetPlatform Platform => ETargetPlatform.iOS;

    public IToolchain Toolchain { get; } = InToolchain;
}