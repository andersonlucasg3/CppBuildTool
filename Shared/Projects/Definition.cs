using Shared.IO;

namespace Shared.Projects;

public abstract class Definition
{
    public abstract string Name { get; }
    
    public abstract string SourcesRoot { get; }

    public DirectoryReference RootDirectory { get; protected set; } = "";
    public DirectoryReference SourcesDirectory { get; protected set; } = "";
}
