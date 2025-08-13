namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

using Solutions;

public class UniqueIdentifier() : ATag(SolutionGuid.NewGuid().ToString());
