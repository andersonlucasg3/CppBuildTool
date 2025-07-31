using Shared.Exceptions;
using Shared.Toolchains;

namespace Shared.Platforms;

public enum ETargetPlatform
{
    Any,
    iOS,
    tvOS,
    visionOS,
    macOS,
    Android,
    Windows
}

public interface ITargetPlatform
{
    public string Name => Platform.ToString();
    public ETargetPlatform Platform { get; }
    
    public IToolchain Toolchain { get; }
}

public class PlatformNotSupportedException : BaseException;