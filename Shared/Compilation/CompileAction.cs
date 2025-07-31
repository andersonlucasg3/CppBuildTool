namespace Shared.Compilation;

using IO;
using Projects;
using Sources;
using Toolchains;

public class CompileAction
{
    private readonly string _objectFileExtension;
    private readonly ISourceCollection _sourceCollection;

    private CompileDependency? _dependency = null;

    public readonly FileReference SourceFile;
    public readonly FileReference DependencyFile;
    public readonly FileReference ObjectFile;

    public CompileDependency Dependency 
    {
        get 
        {
            DependencyFile.UpdateExistance();
            if (_dependency is null && DependencyFile.bExists)
            {
                _dependency = new(DependencyFile, _objectFileExtension, _sourceCollection);
            }

            return _dependency!;
        }    
    }

    public readonly bool bShouldCompile;

    public string KeyName => SourceFile.RelativePath;
    
    public CompileAction(FileReference InSourceFile, DirectoryReference InObjectsDirectory, IToolchain InToolchain, ISourceCollection InSourceCollection)
    {
        _sourceCollection = InSourceCollection;

        SourceFile = InSourceFile;

        _objectFileExtension = InToolchain.GetObjectFileExtension(InSourceFile);
        ObjectFile = InObjectsDirectory.CombineFile($"{SourceFile.Name}{_objectFileExtension}");
        DependencyFile = $"{ObjectFile.FullPath}.d";

        bShouldCompile = ChecksumStorage.Shared.ShouldRecompile(this);
    }
}

public class LinkAction
{
    public readonly FileReference LinkedFile;
    
    public LinkAction(ModuleDefinition InModule, IToolchain InToolchain)
    {
        string Prefix = InToolchain.GetBinaryTypePrefix(InModule.BinaryType);
        string Extension = InToolchain.GetBinaryTypeExtension(InModule.BinaryType);
        
        DirectoryReference ConfigurationDirectory = ProjectDirectories.Shared.CreateBaseConfigurationDirectory(ECompileBaseDirectory.Binaries);
        LinkedFile = ConfigurationDirectory.CombineFile($"{Prefix}{InModule.OutputName}{Extension}");
    }
}