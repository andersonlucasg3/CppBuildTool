using Shared.IO;
using Shared.Misc;
using Shared.Projects.VisualStudio.ProjectXml;

namespace Shared.Projects.VisualStudio.CharpProjects;

public class CSharpProject : TTagGroup<IIndentedStringBuildable>
{
    protected override string TagName => "Project";

    protected override Parameter[] Parameters { get; } = [new Parameter("Sdk", "Microsoft.NET.Sdk")];

    protected override IIndentedStringBuildable[] Contents { get; }

    public CSharpProject(DirectoryReference InProjectRoot)
    {
        Contents = [
            new PropertyGroup(),
            new ItemGroup(InProjectRoot),
        ];
    }

    class ItemGroup(DirectoryReference InProjectRoot) : APropertyGroup
    {
        protected override string TagName => "ItemGroup";

        protected override ATag[] Contents => [ new Compile(InProjectRoot) ];
    }

    class PropertyGroup : APropertyGroup
    {
        protected override ATag[] Contents => [
            new CustomTag("OutputType", "library"),
            new CustomTag("TargetFramework", "net9.0"),
            new CustomTag("ImplicitUsings", "enable"),
            new CustomTag("Nullable", "enable"),
        ];
    }

    class Compile(DirectoryReference InProjectRoot) : ATag
    {
        protected override Parameter[] Parameters => [ new Parameter("Include", InProjectRoot.Combine("**/*.cs").PlatformPath) ];
    }

    class CustomTag(string InName, string InValue) : ATag(InValue)
    {
        protected override string TagName => InName;
    }
}