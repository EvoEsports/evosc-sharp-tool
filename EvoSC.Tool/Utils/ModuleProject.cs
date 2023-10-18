using System.Text;
using EvoSC.Tool.Interfaces;
using EvoSC.Tool.Interfaces.Utils;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace EvoSC.Tool.Utils;

public class ModuleProject : IModuleProject
{
    public string Name { get; }
    public string Title { get; }
    public string Description { get; }
    public string Author { get; }

    public ModuleProject(string name, string title, string desc, string author)
    {
        Name = name;
        Title = title;
        Description = desc;
        Author = author;
    }
    
    public async Task GenerateAsync(IEvoScSolution solution, bool isInternal)
    {
        var projectGuid = Guid.NewGuid();
        var project = await GenerateProjectFileAsync(solution, isInternal);
        await AddProjectToSolutionAsync(solution, project, isInternal);
    }

    private async Task<ProjectRootElement> GenerateProjectFileAsync(IEvoScSolution solution, bool isInternal)
    {
        var projectDir = Path.Combine(
            Path.GetDirectoryName(solution.SolutionFilePath) ?? throw new InvalidOperationException("Invalid solution path."),
            isInternal ? ProjectDefaults.InternalProjectPath : ProjectDefaults.ExternalProjectPath,
            Name
        );
        var projectFile = Path.Combine(projectDir, $"{Name}.csproj");

        var project = ProjectRootElement.Create(NewProjectFileOptions.None);
        
        project.Sdk = ProjectDefaults.Sdk;

        // main project properties
        project.AddProperty(ProjectDefaults.PropertyTargetFramework, ProjectDefaults.TargetFramework);
        project.AddProperty(ProjectDefaults.PropertyImplicitUsings, ProjectDefaults.ImplicitUsings);
        project.AddProperty(ProjectDefaults.PropertyNullable, ProjectDefaults.Nullable);
        project.AddProperty(ProjectDefaults.PropertyRootNamespace, $"EvoSC.Modules.{(isInternal ? "Official" : Author)}.{Name}");
        project.AddProperty(ProjectDefaults.PropertyGenerateAssemblyInfo, ProjectDefaults.GenerateAssemblyInfo);
        project.AddProperty(ProjectDefaults.PropertyAssemblyName, Name);
        project.AddProperty(ProjectDefaults.PropertyTitle, Title);
        project.AddProperty(ProjectDefaults.PropertyDescription, Description);
        project.AddProperty(ProjectDefaults.PropertyVersion, ProjectDefaults.Version);
        project.AddProperty(ProjectDefaults.PropertyAuthors, Author);

        // resources such as templates and locales
        project.AddItem(ProjectDefaults.PropertyEmbeddedResource, ProjectDefaults.EmbeddedResourceTemplates);
        project.AddItem(ProjectDefaults.PropertyEmbeddedResource, ProjectDefaults.EmbeddedResourceLocalization);
        
        // EvoSC# project references
        var modulesProjectPath = solution.GetRelativePath(projectDir, ProjectDefaults.ModulesProjectPath);
        project.AddItem(ProjectDefaults.PropertyProjectReference, modulesProjectPath);

        if (isInternal)
        {
            var sourceGenProjectPath = solution.GetRelativePath(projectDir, ProjectDefaults.ModuleSourceGenProjectPath);
            var sourceGenItem = project.AddItem(ProjectDefaults.PropertyProjectReference, sourceGenProjectPath);
            
            sourceGenItem.AddMetadata(ProjectDefaults.PropertyOutputItemType, ProjectDefaults.OutputItemType)
                .ExpressedAsAttribute = true;
            sourceGenItem.AddMetadata(ProjectDefaults.PropertyReferenceOutputAssembly,
                    ProjectDefaults.ReferenceOutputAssembly)
                .ExpressedAsAttribute = true;
        }

        Directory.CreateDirectory(projectDir);
        project.Save(projectFile);
        return project;
    }

    private async Task AddProjectToSolutionAsync(IEvoScSolution solution, ProjectRootElement project, bool isInternal)
    {
        var projectRelativePath = Path.GetRelativePath(Path.GetDirectoryName(solution.SolutionFilePath), project.FullPath);
        var projectGuid = Guid.NewGuid();
        
        var solutionContents = await File.ReadAllTextAsync(solution.SolutionFilePath);

        var hasExternalModuleFolder = solution
            .SolutionFile
            .ProjectsInOrder
            .Any(p => p.ProjectName.Equals(ProjectDefaults.ExternalModulesFolderName, StringComparison.Ordinal));

        string? modified;
        
        // if external make sure the solution folder exists first
        if (!isInternal && !hasExternalModuleFolder)
        {
            var extModulesDir = Path.Combine(Path.GetFileName(solution.SolutionFilePath),
                ProjectDefaults.ExternalModulesFolderName);

            Directory.CreateDirectory(extModulesDir);
            
            modified = solutionContents.Insert(GetEndOfProjectSection(solutionContents),
                GenerateSolutionFolderSection(ProjectDefaults.ExternalModulesFolderName, ProjectDefaults.ExternalModulesFolderName));
            await File.WriteAllTextAsync(solution.SolutionFilePath, modified);
            await solution.RefreshAsync();
        }

        // Add the project to the solution
        modified = solutionContents.Insert(GetEndOfProjectSection(solutionContents),
            GenerateSolutionProjectSection(projectRelativePath, projectGuid));

        // Set the project configuration
        var projectConfSectionStart = modified.IndexOf("GlobalSection(ProjectConfigurationPlatforms) = postSolution", StringComparison.Ordinal);
        var projectConfSectionEnd = modified.IndexOf("EndGlobalSection", projectConfSectionStart, StringComparison.Ordinal);

        modified = modified.Insert(projectConfSectionEnd, GeneratePostSolutionConfig(projectGuid));

        // Set the solution folder for the project
        var nestedProjectsSectionStart = modified.IndexOf("GlobalSection(NestedProjects) = preSolution", StringComparison.Ordinal);
        var nestedProjectsSectionEnd = modified.IndexOf("EndGlobalSection", nestedProjectsSectionStart, StringComparison.Ordinal);
        modified = modified.Insert(nestedProjectsSectionEnd, GenerateNestedProjectsConfig(solution, projectGuid, isInternal));
        
        await File.WriteAllTextAsync(solution.SolutionFilePath, modified);
        await solution.RefreshAsync();
    }

    private int GetEndOfProjectSection(string contents) =>
        contents.LastIndexOf(ProjectDefaults.SolutionEndProjectSection, StringComparison.Ordinal) +
        ProjectDefaults.SolutionEndProjectSection.Length;

    private string GenerateSolutionProjectSection(string relativePath, Guid projectGuid)
    {
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.Append("Project(\"");
        sb.Append(ProjectDefaults.ProjectTypeGuidClassLibrary);
        sb.Append("\") = \"");
        sb.Append(Name);
        sb.Append("\", \"");
        sb.Append(relativePath);
        sb.Append("\", \"");
        sb.Append(projectGuid.ToString("B").ToUpper());
        sb.AppendLine("\"");
        sb.AppendLine(ProjectDefaults.SolutionEndProjectSection);
        
        return sb.ToString();
    }

    private string GenerateSolutionFolderSection(string name, string relativePath)
    {
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.Append("Project(\"");
        sb.Append(ProjectDefaults.ProjectTypeGuidSolutionFolder);
        sb.Append("\") = \"");
        sb.Append(name);
        sb.Append("\", \"");
        sb.Append(relativePath);
        sb.Append("\", \"");
        sb.Append(Guid.NewGuid().ToString("B").ToUpper());
        sb.AppendLine("\"");
        sb.AppendLine(ProjectDefaults.SolutionEndProjectSection);
        
        return sb.ToString();
    }
    
    private string GeneratePostSolutionConfig(Guid projectGuid)
    {
        var sb = new StringBuilder();

        sb.Append('\t');
        sb.Append(projectGuid.ToString("B").ToUpper());
        sb.AppendLine(".Debug|Any CPU.ActiveCfg = Debug|Any CPU");
        
        sb.Append("\t\t");
        sb.Append(projectGuid.ToString("B").ToUpper());
        sb.AppendLine(".Debug|Any CPU.Build.0 = Debug|Any CPU");
        
        sb.Append("\t\t");
        sb.Append(projectGuid.ToString("B").ToUpper());
        sb.AppendLine(".Release|Any CPU.ActiveCfg = Release|Any CPU");
        
        sb.Append("\t\t");
        sb.Append(projectGuid.ToString("B").ToUpper());
        sb.AppendLine(".Release|Any CPU.Build.0 = Release|Any CPU");

        sb.Append('\t');
        
        return sb.ToString();
    }

    private string GenerateNestedProjectsConfig(IEvoScSolution solution, Guid projectGuid, bool isInternal)
    {
        var folder = solution
            .SolutionFile
            .ProjectsInOrder
            .FirstOrDefault(p =>
                isInternal
                    ? p.ProjectName.Equals(ProjectDefaults.InternalModulesFolderName)
                    : p.ProjectName.Equals(ProjectDefaults.ExternalModulesFolderName)
            );

        if (folder == null)
        {
            throw new InvalidOperationException(
                $"Failed to find the {(isInternal ? "internal" : "external")} modules solution folder.");
        }

        var sb = new StringBuilder();

        sb.Append('\t');
        sb.Append(projectGuid.ToString("B").ToUpper());
        sb.Append(" = ");
        sb.Append(folder.ProjectGuid);
        sb.AppendLine("\t");
        
        return sb.ToString();
    }
}