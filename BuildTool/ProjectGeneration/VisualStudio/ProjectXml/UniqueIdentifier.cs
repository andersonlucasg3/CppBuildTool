namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

using Solutions;

public class UniqueIdentifier() : Tag(SolutionGuid.NewGuid().ToString());
