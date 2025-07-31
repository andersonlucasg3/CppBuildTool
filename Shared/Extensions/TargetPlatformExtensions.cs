using Shared.Platforms;

namespace Shared.Extensions;

public static class TargetPlatformExtensions
{
    public static string ToSolutionPlatform(this ETargetPlatform InTargetPlatform)
    {
        return InTargetPlatform switch
        {
            ETargetPlatform.Any => "Any CPU",
            ETargetPlatform.iOS => "iOS",
            ETargetPlatform.tvOS => "tvOS",
            ETargetPlatform.visionOS => "visionOS",
            ETargetPlatform.macOS => "macOS",
            ETargetPlatform.Android => "android-arm64-v8",
            ETargetPlatform.Windows => "x64",
            _ => throw new ArgumentOutOfRangeException(nameof(InTargetPlatform), InTargetPlatform, null),
        };
    }
}
