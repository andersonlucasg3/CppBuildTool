using Shared.IO;

namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

public class AdditionalIncludeDirectories(DirectoryReference InIncludeDirectory) : ATag($"$(SolutionDir){InIncludeDirectory.RelativePath};%({nameof(AdditionalIncludeDirectories)})");