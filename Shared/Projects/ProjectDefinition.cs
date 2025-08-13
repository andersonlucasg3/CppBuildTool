namespace Shared.Projects;

using IO;
using Exceptions;
using Shared.Platforms;

public abstract class AProjectDefinition : ADefinition
{
    private bool _bIsConfigured = false;

    private readonly Dictionary<ETargetPlatform, HashSet<AProjectDefinition>> _dependencyProjectsPerPlatform = [];
    private readonly Dictionary<ETargetPlatform, Dictionary<string, AModuleDefinition>> _modulesPerPlatform = [];

    public IReadOnlyDictionary<string, AModuleDefinition> GetModules(ETargetPlatform InTargetPlatform)
    {
        if (!_modulesPerPlatform.TryGetValue(InTargetPlatform, out Dictionary<string, AModuleDefinition>? ModuleMap))
        {
            return new Dictionary<string, AModuleDefinition>();
        }

        return ModuleMap;
    }

    public IReadOnlySet<AProjectDefinition> GetDependencyProjects(ETargetPlatform InTargetPlatform)
    {
        if (!_dependencyProjectsPerPlatform.TryGetValue(InTargetPlatform, out HashSet<AProjectDefinition>? ProjectSet))
        {
            return new HashSet<AProjectDefinition>();
        }

        return ProjectSet;
    }

    protected abstract void Configure();

    protected void AddProjectDependency<TProject>(ETargetPlatform InTargetPlatform = ETargetPlatform.Any)
        where TProject : AProjectDefinition
    {
        TProject Project = ProjectFinder.FindProject<TProject>();

        if (!_dependencyProjectsPerPlatform.TryGetValue(InTargetPlatform, out HashSet<AProjectDefinition>? ProjectSet))
        {
            ProjectSet = [];
            _dependencyProjectsPerPlatform.Add(InTargetPlatform, ProjectSet);
        }

        ProjectSet.Add(Project);

        foreach (KeyValuePair<ETargetPlatform, Dictionary<string, AModuleDefinition>> Pair in Project._modulesPerPlatform)
        {
            _modulesPerPlatform.Add(Pair.Key, Pair.Value);
        }
    }

    protected void AddModule<TModule>(ETargetPlatform InTargetPlatform = ETargetPlatform.Any) 
        where TModule : AModuleDefinition, new()
    {
        TModule Module = new();
        
        if (!_modulesPerPlatform.TryGetValue(InTargetPlatform, out Dictionary<string, AModuleDefinition>? ModuleMap))
        {
            ModuleMap = [];
            _modulesPerPlatform.Add(InTargetPlatform, ModuleMap);
        }

        ModuleMap.Add(Module.Name, Module);
    }

    internal void Configure(DirectoryReference InRootDirectory)
    {
        if (_bIsConfigured) return;

        _bIsConfigured = true;

        RootDirectory = InRootDirectory.Combine(Name);
        SourcesDirectory = RootDirectory.Combine(SourcesRoot);

        Configure();

        foreach (Dictionary<string, AModuleDefinition> Dict in _modulesPerPlatform.Values)
        {
            foreach (AModuleDefinition Module in Dict.Values)
            {
                Module.Configure(this, SourcesDirectory);
            }
        }
    }
}

public class ModuleAlreadyDefinedException(string InModuleName) 
    : ABaseException($"Module '{InModuleName}' already exists in the project definition.");