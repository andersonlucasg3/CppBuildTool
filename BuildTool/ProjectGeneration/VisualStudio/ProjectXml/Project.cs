
namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

using Solutions;

public class Project(SolutionGuid InGuid) : ATag(InGuid.ToString());