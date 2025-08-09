using Shared.Toolchains;

namespace Shared.Platforms;

public class AndroidPlatform(AndroidToolchain InToolchain) : ITargetPlatform
{
    public ETargetPlatform Platform => ETargetPlatform.Android;

    public IToolchain Toolchain => InToolchain; 
}