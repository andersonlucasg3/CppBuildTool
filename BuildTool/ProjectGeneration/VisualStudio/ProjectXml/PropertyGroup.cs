namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

public abstract class PropertyGroup : TTagGroup<Tag>
{
    protected override string TagName => "PropertyGroup";
}
