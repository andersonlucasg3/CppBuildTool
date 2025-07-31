namespace BuildTool.ProjectGeneration.VisualStudio.Projects;

using ProjectXml;

public class Import(string InProject) : Tag
{
    protected override Parameter[] Parameters { get; } = [
        new Parameter("Project", InProject),
    ];
}
