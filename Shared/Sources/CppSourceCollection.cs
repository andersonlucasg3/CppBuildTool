namespace Shared.Sources;

using IO;
using Processes;
using Platforms;
using Shared.Extensions;

public class CppSourceCollection : ISourceCollection
{
    public static readonly string[] CSourceFilesExtensions = [".c", ".i"];
    public static readonly string[] CppSourceFileExtensions = [".cpp", ".cc", ".cxx", ".c++", ".ii"];
    public static readonly string[] CHeaderFilesExtensions = [".h"];
    public static readonly string[] CppHeaderFilesExtensions = [".hh", ".hpp", ".hxx"];

    public string[] HeaderFilesExtensions { get; private set; } = [];
    public string[] SourceFilesExtensions { get; private set; } = [];
    public string[] AllFilesExtensions { get; private set; } = [];


    public FileReference[] HeaderFiles { get; private set; } = [];
    public FileReference[] SourceFiles { get; private set; } = [];
    public FileReference[] AllFiles { get; private set; } = [];

    public void GatherSourceFiles(DirectoryReference InSourcesRootDirectory, ETargetPlatform InTargetPlatform)
    {
        HeaderFilesExtensions = GetHeaderFilesExtensions();
        SourceFilesExtensions = GetSourceFilesExtensions();
        AllFilesExtensions = [.. HeaderFilesExtensions, .. SourceFilesExtensions];

        List<FileReference> HeadersList = [];
        List<FileReference> SourcesList = [];

        List<ETargetPlatform> ExcludedPlatforms = [.. Enum.GetValues<ETargetPlatform>()];
        ExcludedPlatforms.Remove(ETargetPlatform.Any);
        ExcludedPlatforms.Remove(InTargetPlatform); // remove the current platform so we won't exclude it

        List<ETargetPlatformGroup> ExcludedPlatformGroups = [.. Enum.GetValues<ETargetPlatformGroup>()];
        ExcludedPlatformGroups.Remove(ETargetPlatformGroup.Any);
        ExcludedPlatformGroups.Remove(ITargetPlatform.GetPlatformGroup(InTargetPlatform));

        string[] ExcludedPlatformsPathComponent = [.. ExcludedPlatforms.Select(Each => $"/{Each.ToSourcePlatformName()}/")];
        string[] ExcludedPlatformGroupsPathComponent = [.. ExcludedPlatformGroups.Select(Each => $"/{Each}/")];

        Action[] Actions = [
            () => {
                Parallelization.ForEach(HeaderFilesExtensions, HeaderFileExtension =>
                {
                    FileReference[] Headers = [.. InSourcesRootDirectory.EnumerateFiles($"*{HeaderFileExtension}", SearchOption.AllDirectories)];

                    lock (this)
                    {
                        foreach (FileReference Header in Headers)
                        {
                            if (ExcludeSource(Header, ExcludedPlatformsPathComponent, ExcludedPlatformGroupsPathComponent)) continue;

                            HeadersList.Add(Header);
                        }
                    }
                });
            },
            () =>
            {
                Parallelization.ForEach(SourceFilesExtensions, SourceFileExtension =>
                {
                    FileReference[] Sources = [.. InSourcesRootDirectory.EnumerateFiles($"*{SourceFileExtension}", SearchOption.AllDirectories)];

                    lock (this)
                    {
                        foreach (FileReference Source in Sources)
                        {
                            if(ExcludeSource(Source, ExcludedPlatformsPathComponent, ExcludedPlatformGroupsPathComponent)) continue;

                            SourcesList.Add(Source);
                        }
                    }
                });
            }
        ];

        Parallelization.ForEach(Actions, Action => Action.Invoke());

        HeaderFiles = [.. HeadersList];
        SourceFiles = [.. SourcesList];

        AllFiles = [.. HeaderFiles, .. SourceFiles];
    }

    protected virtual string[] GetHeaderFilesExtensions()
    {
        return [
            .. CHeaderFilesExtensions,
            .. CppHeaderFilesExtensions,
        ];
    }

    protected virtual string[] GetSourceFilesExtensions()
    {
        return [
            .. CSourceFilesExtensions,
            .. CppSourceFileExtensions,
        ];
    }

    private static bool ExcludeSource(FileReference InSource, string[] ExcludedPlatforms, string[] ExcludedPlatformGroups)
    {
        return ExcludedPlatforms.Any(InSource.RelativePath.Contains) || ExcludedPlatformGroups.Any(InSource.RelativePath.Contains);
    }
}