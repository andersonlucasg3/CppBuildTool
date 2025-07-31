namespace Shared.Projects.Platforms;

using Apple;

public class PlatformSpecifics
{
    public readonly MacPlatformSpecifics Mac = new();
    public readonly IOSPlatformSpecifics IOS = new();
}