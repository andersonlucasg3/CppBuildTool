using Shared.Toolchains;

namespace Shared.Platforms;

public class TVOSPlatform : IOSPlatform
{
    public TVOSPlatform(XcodeToolchain InToolchain) : base(InToolchain)
    {
        
    }
}