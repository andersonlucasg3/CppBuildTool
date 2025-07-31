
namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

using Solutions;

public class Project(SolutionGuid InGuid) : Tag(InGuid.ToString());