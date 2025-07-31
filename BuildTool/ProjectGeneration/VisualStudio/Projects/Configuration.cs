using Shared.Projects;
using Shared.Platforms;
using Shared.Compilation;

namespace BuildTool.ProjectGeneration.VisualStudio.Projects;

using ProjectXml;

public class ConfigurationPropertyGroup(EModuleBinaryType InBinaryType, ECompileConfiguration InCompileConfiguration, ETargetPlatform InTargetPlatform) : PropertyGroup
{
    public readonly bool bUseDebugLibraries = InCompileConfiguration == ECompileConfiguration.Debug;
    public readonly string PlatformToolset = "v143";
    public readonly string CharacterSet = "Unicode";
    public readonly bool bWholeProgramOptimization = InCompileConfiguration == ECompileConfiguration.Release;

    protected override Parameter[] Parameters => [
        new Parameter("Label", "Configuration"),
        new Parameter("Condition", $"'$(Configuration)|$(Platform)'=='{InCompileConfiguration}|{InTargetPlatform}'"),
    ];

    protected override Tag[] Contents => [
        new ConfigurationType(InBinaryType),
        new UseDebugLibraries(bUseDebugLibraries),
        new PlatformToolset(PlatformToolset),
        new CharacterSet(CharacterSet),
        new WholeProgramOptimization(bWholeProgramOptimization),
    ];
}

public class ConfigurationType(EModuleBinaryType InBinaryType) : Tag(InBinaryType.ToString());
public class UseDebugLibraries(bool bInUseDebugLibraries) : Tag(bInUseDebugLibraries.ToString());
public class PlatformToolset(string InPlatformToolset) : Tag(InPlatformToolset);
public class CharacterSet(string InCharacterSet) : Tag(InCharacterSet);
public class WholeProgramOptimization(bool bInWholeProgramOptimization) : Tag(bInWholeProgramOptimization.ToString());
