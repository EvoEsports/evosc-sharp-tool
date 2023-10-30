namespace EvoSC.Tool.Utils;

public class ProjectDefaults
{
    public const string Sdk = "Microsoft.NET.Sdk";

    public const string PropertyTargetFramework = "TargetFramework";
    public const string PropertyImplicitUsings = "ImplicitUsings";
    public const string PropertyNullable = "Nullable";
    public const string PropertyRootNamespace = "RootNamespace";
    public const string PropertyGenerateAssemblyInfo = "GenerateAssemblyInfo";
    public const string PropertyAssemblyName = "AssemblyName";
    public const string PropertyTitle = "Title";
    public const string PropertyDescription = "Description";
    public const string PropertyVersion = "Version";
    public const string PropertyAuthors = "Authors";

    public const string PropertyProjectReference = "ProjectReference";
    public const string PropertyOutputItemType = "OutputItemType";
    public const string PropertyReferenceOutputAssembly = "ReferenceOutputAssembly";
    public const string PropertyEmbeddedResource = "EmbeddedResource";
    
    public const string TargetPostBuildEvent = "PostBuildEvent";
    public const string PropertyExec = "Exec";
    public const string ParameterCommand = "Command";
    
    public const string TargetFramework = "net7.0";
    public const string ImplicitUsings = "enable";
    public const string Nullable = "true";
    public const string GenerateAssemblyInfo = "false";
    public const string Version = "1.0.0";
    public const string OutputItemType = "Analyzer";
    public const string ReferenceOutputAssembly = "false";
    public const string EmbeddedResourceTemplates = @"Templates\**\*";
    public const string EmbeddedResourceLocalization = "Localization.resx";

    public const string InternalProjectPath = "src/Modules/";
    public const string ExternalProjectPath = $"{ExternalModulesFolderName}/";
    
    public const string ModuleSourceGenProjectPath = "src/EvoSC.Modules.SourceGeneration/EvoSC.Modules.SourceGeneration.csproj";
    public const string ModulesProjectPath = "src/EvoSC.Modules/EvoSC.Modules.csproj";

    public const string ProjectTypeGuidClassLibrary = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
    public const string ProjectTypeGuidSolutionFolder = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";
    
    public const string SolutionEndProjectSection = "EndProject";
    
    public const string InternalModulesFolderName = "Modules";
    public const string ExternalModulesFolderName = "ExternalModules";
}
