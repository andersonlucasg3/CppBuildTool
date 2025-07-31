using Shared.IO;

namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

public class AdditionalIncludeDirectories(DirectoryReference InIncludeDirectory) : Tag($"$(SolutionDir){InIncludeDirectory.RelativePath};%({nameof(AdditionalIncludeDirectories)})");