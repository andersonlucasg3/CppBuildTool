namespace Shared.Sources;

using IO;

public interface ISourceCollection
{
    public string[] HeaderFilesExtensions { get; }
    public string[] SourceFilesExtensions { get; }
    public string[] AllFilesExtensions { get; }

    public FileReference[] HeaderFiles { get; }
    public FileReference[] SourceFiles { get; }
    public FileReference[] AllFiles { get; }

    public void GatherSourceFiles(DirectoryReference InSourceRootDirectory);
}