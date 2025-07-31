namespace Shared.Sources;

using IO;
using Processes;

public abstract class CppSourceCollection : ISourceCollection
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

    public void GatherSourceFiles(DirectoryReference InSourcesRootDirectory)
    {
        HeaderFilesExtensions = GetHeaderFilesExtensions();
        SourceFilesExtensions = GetSourceFilesExtensions();
        AllFilesExtensions = [.. HeaderFilesExtensions, .. SourceFilesExtensions];

        List<FileReference> HeadersList = [];
        List<FileReference> SourcesList = [];

        Action[] Actions = [
            () => {
                Parallelization.ForEach(HeaderFilesExtensions, HeaderFileExtension =>
                {
                    FileReference[] Headers = [.. InSourcesRootDirectory.EnumerateFiles($"*{HeaderFileExtension}", SearchOption.AllDirectories)];

                    lock (this)
                    {
                        HeadersList.AddRange(Headers);
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
                        SourcesList.AddRange(Sources);
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
}