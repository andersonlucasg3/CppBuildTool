using Shared.IO;

namespace BuildTool.ProjectGeneration.VisualStudio.Projects;

using ProjectXml;
using Shared.Projects;
using Solutions;

public class Globals(SolutionProject InProject, EModuleBinaryType InBinaryType, DirectoryReference InIntermediateDirectory, DirectoryReference InBinariesDirectory) 
    : PropertyGroup
{
    public readonly string ProjectNameProperty = InProject.ProjectName;
    public readonly string VCProjectVersionProperty = "17.0";
    public readonly string KeywordProperty = "Win32Proj";
    public readonly SolutionGuid ProjectGuidProperty = InProject.ProjectGuid;
    public readonly string RootNamespaceProperty = InProject.ProjectName;
    public readonly string WindowsTargetPlatformVersionProperty = "10.0";
    public readonly string OutputPathProperty = $"$(SolutionDir){InIntermediateDirectory.PlatformRelativePath}\\$(Platform)\\$(Configuration)";
    public readonly string IntDirProperty = $"$(SolutionDir){InBinariesDirectory.PlatformRelativePath}\\$(Platform)\\$(Configuration)\\$(ProjectName)";
    public readonly string OutDirProperty = $"$(OutputPath)";
    public readonly EModuleBinaryType ConfigurationTypeProperty = InBinaryType;

    protected override Parameter[] Parameters => [
        new Parameter("Label", "Globals")
    ];

    protected override Tag[] Contents => [
        new VCProjectVersion(VCProjectVersionProperty),
        new Keyword(KeywordProperty),
        new ProjectGuid(ProjectGuidProperty),
        new ProjectName(ProjectNameProperty),
        new RootNamespace(RootNamespaceProperty),
        new WindowsTargetPlatformVersion(WindowsTargetPlatformVersionProperty),
        new OutputPath(OutputPathProperty),
        new IntDir(IntDirProperty),
        new OutDir(OutDirProperty),
        new ConfigurationType(ConfigurationTypeProperty),
    ];

    public class VCProjectVersion(string InVersion) : Tag(InVersion);
    public class Keyword(string InKeyword) : Tag(InKeyword);
    public class ProjectGuid(SolutionGuid InGuid) : Tag(InGuid.ToString());
    public class ProjectName(string InName) : Tag(InName);
    public class RootNamespace(string InName) : Tag(InName);
    public class WindowsTargetPlatformVersion(string InVersion) : Tag(InVersion);
    public class OutputPath(string InPath) : Tag(InPath);
    public class IntDir(string InPath) : Tag(InPath);
    public class OutDir(string InPath) : Tag(InPath);
    public class ConfigurationType(EModuleBinaryType InBinaryType) : Tag(InBinaryType.ToString());
}
