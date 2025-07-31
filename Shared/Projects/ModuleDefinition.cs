using System.Runtime.InteropServices;

namespace Shared.Projects;

using IO;
using Exceptions;
using Sources;
using Shared.Platforms;
using Projects.Platforms;


public abstract class ModuleDefinition : Definition
{
    private bool _bIsConfigured = false;
    private ProjectDefinition? _ownerProject = null;

    private readonly Dictionary<ETargetPlatform, HashSet<string>> _headerSearchPathPerPlatform = [];
    private readonly Dictionary<ETargetPlatform, HashSet<ModuleDefinition>> _dependenciesPerPlatform = [];

    private readonly List<string> _librarySearchPaths = [];
    private ISourceCollection? _sources = null;

    private readonly List<DirectoryReference> _copyResourcesDirectories = [];

    // can be overriden to change the output file name
    // for example in a library or application module with name X
    // the output linked file can have name Y
    public virtual string OutputName => Name;

    public abstract EModuleBinaryType BinaryType { get; }

    public ProjectDefinition OwnerProject => _ownerProject!;
    
    public IReadOnlyList<string> LibrarySearchPaths => _librarySearchPaths;
    public IReadOnlyList<DirectoryReference> CopyResourcesDirectories => _copyResourcesDirectories;

    public ISourceCollection Sources => _sources!;

    public PlatformSpecifics PlatformSpecifics { get; } = new();

    protected abstract void Configure(ProjectDefinition InOwnerProject);

    public IReadOnlySet<ModuleDefinition> GetDependencies(ETargetPlatform InTargetPlatform = ETargetPlatform.Any)
    {
        if (!_dependenciesPerPlatform.TryGetValue(InTargetPlatform, out HashSet<ModuleDefinition>? ModuleSet))
        {
            return new HashSet<ModuleDefinition>();
        }

        return ModuleSet;
    }

    public IReadOnlySet<string> GetHeaderSearchPaths(ETargetPlatform InTargetPlatform = ETargetPlatform.Any)
    {
        if (!_headerSearchPathPerPlatform.TryGetValue(InTargetPlatform, out HashSet<string>? SearchPathsSet))
        {
            return new HashSet<string>();
        }

        return SearchPathsSet;
    }

    protected void AddDependencyModuleNames(params string[] InModuleNames)
    {
        AddDependencyModuleNames(ETargetPlatform.Any, InModuleNames);
    }

    protected void AddDependencyModuleNames(ETargetPlatform InTargetPlatform, params string[] InModuleNames)
    {
        IReadOnlyDictionary<string, ModuleDefinition> ModulesMap = OwnerProject.GetModules(InTargetPlatform);

        if (!_dependenciesPerPlatform.TryGetValue(InTargetPlatform, out HashSet<ModuleDefinition>? ModuleSet))
        {
            ModuleSet = [];
            _dependenciesPerPlatform.Add(InTargetPlatform, ModuleSet);
        }

        foreach (string ModuleName in InModuleNames)
        {
            if (!ModulesMap.TryGetValue(ModuleName, out ModuleDefinition? DependencyModule))
            {
                throw new MissingDependencyModuleException(ModuleName, OwnerProject.Name);
            }

            ModuleSet.Add(DependencyModule);
        }
    }

    protected void AddHeaderSearchPaths(params string[] InHeaderSearchPaths)
    {
        AddHeaderSearchPaths(ETargetPlatform.Any, InHeaderSearchPaths);
    }

    protected void AddHeaderSearchPaths(ETargetPlatform InTargetPlatform, params string[] InHeaderSearchPaths)
    {
        if (!_headerSearchPathPerPlatform.TryGetValue(InTargetPlatform, out HashSet<string>? SearchPathsSet))
        {
            SearchPathsSet = [];
            _headerSearchPathPerPlatform.Add(InTargetPlatform, SearchPathsSet);
        }

        foreach (string HeaderSearchPath in InHeaderSearchPaths)
        {
            DirectoryReference SearchPath = OwnerProject.SourcesDirectory.Combine(HeaderSearchPath);
            SearchPathsSet.Add(SearchPath.PlatformRelativePath);
        }
    }

    protected void AddLibrarySearchPaths(params string[] InLibrarySearchPaths)
    {
        foreach (string LibrarySearchPath in InLibrarySearchPaths)
        {
            _librarySearchPaths.Add(LibrarySearchPath);
        }
    }

    protected void AddCopyResourcesFolders(params string[] InCopyResourcesFolders)
    {
        foreach (string CopyResourcesFolder in InCopyResourcesFolders)
        {
            DirectoryReference ResourcesDirectory = RootDirectory.Combine(CopyResourcesFolder);

            if (!ResourcesDirectory.bExists)
            {
                Console.WriteLine($"ERROR: not a directory {ResourcesDirectory.PlatformRelativePath}");

                continue;
            }

            _copyResourcesDirectories.Add(ResourcesDirectory);
        }
    }

    internal void Configure(ProjectDefinition InOwnerProject, DirectoryReference InRootDirectory)
    {
        if (_bIsConfigured) return;

        _bIsConfigured = true;

        _ownerProject = InOwnerProject;

        RootDirectory = InRootDirectory.Combine(Name);
        SourcesDirectory = RootDirectory.Combine(SourcesRoot);

        _sources = CreateSources();
        Sources.GatherSourceFiles(SourcesDirectory);

        Configure(InOwnerProject);
    }

    // TODO: review this in the future, there may be a better way to do this
    private ISourceCollection CreateSources()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return BinaryType switch
            {
                EModuleBinaryType.ShaderLibrary => new MetalShaderSourceCollection(),
                _ => new AppleSourceCollection(),
            };
        } 
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return BinaryType switch
            {
                // TODO: review this in the future if I want to support shader libraries on Windows
                EModuleBinaryType.ShaderLibrary => 
                    throw new ShaderLibraryNotSupportedOnPlatformException("EModuleBinaryType.ShaderLibrary not supported on Windows"),

                _ => new WindowsSourceCollection(),
            };
        }

        throw new PlatformNotSupportedException();
    }
}

public class MissingDependencyModuleException(string InModuleName, string InProjectName) 
    : BaseException($"Module '{InModuleName}' not found in project '{InProjectName}'.");
public class ShaderLibraryNotSupportedOnPlatformException(string InMessage) : BaseException(InMessage);
public class UnsupportedDependencyForCodeModuleException(string InModuleName) 
    : BaseException($"Module '{InModuleName}' not supported as code dependency.");