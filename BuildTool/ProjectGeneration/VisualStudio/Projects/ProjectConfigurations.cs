using Shared.Compilation;
using Shared.Platforms;

namespace BuildTool.ProjectGeneration.VisualStudio.Projects;

using ProjectXml;

public class ProjectConfigurations(ECompileConfiguration[] InCompileConfigurations, ETargetPlatform[] InTargetPlatforms) : TItemGroup<ProjectConfiguration>
{
    protected override string TagName => "ItemGroup";

    protected override Parameter[] Parameters => [
        new Parameter("Label", "ProjectConfigurations")    
    ];

    protected override ProjectConfiguration[] Contents => [
        .. InCompileConfigurations.SelectMany(Config => InTargetPlatforms.Select(Plat => new ProjectConfiguration(Config, Plat)))
    ];
}

public class ProjectConfiguration(ECompileConfiguration InCompileConfiguration, ETargetPlatform InTargetPlatform) : TTagGroup<Tag>
{
    protected override Parameter[] Parameters => [
        new Parameter("Include", $"{InCompileConfiguration}|{InTargetPlatform}")
    ];

    protected override Tag[] Contents => [
        new Configuration(InCompileConfiguration),
        new Platform(InTargetPlatform),
    ];
}

public class Configuration(ECompileConfiguration InConfiguration) : Tag(InConfiguration.ToString());
public class Platform(ETargetPlatform InPlatform) : Tag(InPlatform.ToString());