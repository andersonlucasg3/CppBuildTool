namespace Shared.Toolchains.Compilers;

using Platforms;
using Projects;
using Shared.Compilation;
using Shared.Extensions;


public static class CompilerDefinitionsProvider
{
    public static string[] GetAutomaticCompilerDefinitions(ITargetPlatform InTargetPlatform, ECompileConfiguration InConfiguration, ModuleDefinition InModule)
    {
        return [
            .. InTargetPlatform.Toolchain.GetAutomaticModuleCompilerDefinitions(InModule, InTargetPlatform.Platform),
            .. GeneratePlatformCompilerDefinitions(InTargetPlatform.Platform),
            .. InModule.GetCompilerDefinitions(ETargetPlatform.Any),
            .. InModule.GetCompilerDefinitions(InTargetPlatform.Platform),
            .. GetConfigurationDefinitions(InConfiguration),
        ];
    }

    private static string[] GetConfigurationDefinitions(ECompileConfiguration InConfiguration)
    {
        List<string> Defines = [];

        ECompileConfiguration[] Configurations = Enum.GetValues<ECompileConfiguration>();

        foreach (ECompileConfiguration Configuration in Configurations)
        {
            int Value = Configuration == InConfiguration ? 1 : 0;

            Defines.Add($"WITH_{Configuration.ToString().ToUpper()}={Value}");
        }

        return [.. Defines];
    }

    private static string[] GeneratePlatformCompilerDefinitions(ETargetPlatform InTargetPlatform)
    {
        List<string> PlatformDefinitions = [];

        ETargetPlatform[] AllPlatforms = Enum.GetValues<ETargetPlatform>();

        foreach (ETargetPlatform TargetPlatform in AllPlatforms)
        {
            if (TargetPlatform == ETargetPlatform.Any) continue;

            string PlatformDefine = $"PLATFORM_{TargetPlatform.ToString().ToUpper()}";

            int Value = InTargetPlatform == TargetPlatform ? 1 : 0;

            PlatformDefine += $"={Value}";

            PlatformDefinitions.Add(PlatformDefine);

            if (Value == 1)
            {
                PlatformDefinitions.Add($"PLATFORM_NAME={TargetPlatform.ToSourcePlatformName()}");
            }
        }

        ETargetPlatformGroup[] AllGroups = Enum.GetValues<ETargetPlatformGroup>();

        foreach (ETargetPlatformGroup TargetGroup in AllGroups)
        {
            if (TargetGroup == ETargetPlatformGroup.Any) continue;

            string GroupDefine = $"PLATFORM_GROUP_{TargetGroup.ToString().ToUpper()}";

            ETargetPlatformGroup CurrentGroup = ITargetPlatform.GetPlatformGroup(InTargetPlatform);

            int Value = CurrentGroup == TargetGroup ? 1 : 0;

            GroupDefine += $"={Value}";

            PlatformDefinitions.Add(GroupDefine);

            if (Value == 1)
            {
                // this is like:
                // #define PLATFORM_GROUP_NAME Microsoft
                PlatformDefinitions.Add($"PLATFORM_GROUP_NAME={TargetGroup}");
            }
        }

        ETargetPlatformType[] AllTypes = Enum.GetValues<ETargetPlatformType>();

        foreach (ETargetPlatformType TargetType in AllTypes)
        {
            if (TargetType == ETargetPlatformType.Any) continue;

            string TypeDefine = $"PLATFORM_TYPE_{TargetType.ToString().ToUpper()}";

            ETargetPlatformType CurrentType = ITargetPlatform.GetPlatformType(InTargetPlatform);

            int Value = CurrentType == TargetType ? 1 : 0;

            TypeDefine += $"={Value}";

            PlatformDefinitions.Add(TypeDefine);
        }

        return [.. PlatformDefinitions];
    }
}