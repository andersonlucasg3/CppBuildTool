using Shared.IO;
using Shared.Compilation;
using Shared.Platforms;

namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

public class ItemDefinitionGroup(DirectoryReference[] InIncludeDirectories, ECompileConfiguration InCompileConfiguration, ETargetPlatform InTargetPlatform) : TTagGroup<ItemDefinitionGroup.ClCompile>
{
    protected override Parameter[] Parameters => [ 
		new Parameter("Condition", $"'$(Configuration)|$(Platform)'=='{InCompileConfiguration}|{InTargetPlatform}'"),
    ];

    protected override ClCompile[] Contents => [
		new ClCompile(InIncludeDirectories)
	];

    public class ClCompile(DirectoryReference[] InIncludeDirectories) : TTagGroup<Tag>
    {
        protected override Tag[] Contents => [
            .. InIncludeDirectories.Select(InIncludeDirectories => new AdditionalIncludeDirectories(InIncludeDirectories)),
            new LanguageStandard(), new LanguageStandard_C(),
        ];
    }
}
