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
    Windows,
}

public enum ETargetPlatformGroup
{
    Any,
    Apple,
    Google,
    Microsoft,
    Sony,
    Nintendo,
}

public enum ETargetPlatformType
{
    Any,
    Mobile,
    Desktop,
    GameConsole,
}

public interface ITargetPlatform
{
    public string Name => Platform.ToString();
    public ETargetPlatform Platform { get; }

    public IToolchain Toolchain { get; }

    public static ETargetPlatformGroup GetPlatformGroup(ETargetPlatform InTargetPlatform)
    {
        return InTargetPlatform switch
        {
            ETargetPlatform.Any => ETargetPlatformGroup.Any,
            ETargetPlatform.iOS => ETargetPlatformGroup.Apple,
            ETargetPlatform.tvOS => ETargetPlatformGroup.Apple,
            ETargetPlatform.visionOS => ETargetPlatformGroup.Apple,
            ETargetPlatform.macOS => ETargetPlatformGroup.Apple,
            ETargetPlatform.Android => ETargetPlatformGroup.Google,
            ETargetPlatform.Windows => ETargetPlatformGroup.Microsoft,
            _ => throw new PlatformNotSupportedException(InTargetPlatform),
        };
    }

    public static ETargetPlatformType GetPlatformType(ETargetPlatform InTargetPlatform)
    {
        return InTargetPlatform switch
        {
            ETargetPlatform.Any => ETargetPlatformType.Any,
            ETargetPlatform.iOS => ETargetPlatformType.Mobile,
            ETargetPlatform.tvOS => ETargetPlatformType.Mobile,
            ETargetPlatform.visionOS => ETargetPlatformType.Mobile,
            ETargetPlatform.macOS => ETargetPlatformType.Desktop,
            ETargetPlatform.Android => ETargetPlatformType.Mobile,
            ETargetPlatform.Windows => ETargetPlatformType.Desktop,
            _ => throw new PlatformNotSupportedException(InTargetPlatform),
        };
    }
}

public class PlatformNotSupportedException : BaseException
{
    public PlatformNotSupportedException() : base() {}
    public PlatformNotSupportedException(ETargetPlatform InTargetPlatform) : base($"{InTargetPlatform}") {}
}