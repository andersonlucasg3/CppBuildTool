using System.Runtime.InteropServices;

namespace Shared.Platforms;

public interface IHostPlatform : ITargetPlatform
{
    public IReadOnlyDictionary<ETargetPlatform, ITargetPlatform> SupportedTargetPlatforms { get; }

    public static IHostPlatform GetHost()
    {
        if (IsMacOS())
        {
            return new MacPlatform();
        }

        if (IsWindows())
        {
            return new WindowsPlatform();
        }

        throw new PlatformNotSupportedException();
    }

    public static bool IsWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    public static bool IsMacOS()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }
}