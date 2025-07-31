using System.Runtime.InteropServices;

namespace Shared.Platforms;

public interface IHostPlatform : ITargetPlatform
{
    public IReadOnlyDictionary<ETargetPlatform, ITargetPlatform> SupportedTargetPlatforms { get; } 
    
    public static IHostPlatform GetHost()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacPlatform();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsPlatform();
        }

        throw new PlatformNotSupportedException();
    }
}