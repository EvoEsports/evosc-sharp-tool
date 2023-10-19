using System.Text;
using EvoSC.Tool.Interfaces;
using EvoSC.Tool.Interfaces.Utils;
using EvoSC.Tool.Utils.Templates;
using Spectre.Console;

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
    
    public async Task GenerateAsync(IEvoScSolution solution, bool isInternal, StatusContext? status)
    {
        status?.Status("Generating project file");
        var project = await GenerateProjectFileAsync(solution, isInternal);

        status?.Status("Adding project to solution");
        await AddProjectToSolutionAsync(solution, project.ProjectFile, isInternal);

        status?.Status("Creating templates directory");
        Directory.CreateDirectory(Path.Combine(project.ProjectDir, "Templates"));

        status?.Status("Creating main module class file");
        await File.WriteAllTextAsync(Path.Combine(project.ProjectDir, $"{Name}.cs"), new MainModuleClassTemplate
        {
            Author = Author,
            ModuleName = Name,
            IsInternal = isInternal
        }.TransformText());

        status?.Status("Creating localization file");
        await File.WriteAllTextAsync(Path.Combine(project.ProjectDir, "Localization.resx"),
            new LocalizationFileTemplate().TransformText());

        status?.Status("Creating info.toml file");
        await File.WriteAllTextAsync(Path.Combine(project.ProjectDir, "info.toml"), new InfoFileTemplate
        {
            Name = Name,
            Title = Title,
            Description = Description,
            Author = Author
        }.TransformText());
    }

    private async Task<(string ProjectDir, string ProjectFile)> GenerateProjectFileAsync(IEvoScSolution solution, bool isInternal)
    {
        var projectDir = Path.Combine(
            Path.GetDirectoryName(solution.SolutionFilePath) ?? throw new InvalidOperationException("Invalid solution path."),
            isInternal ? ProjectDefaults.InternalProjectPath : ProjectDefaults.ExternalProjectPath,
            Name
        );
        var projectFile = Path.Combine(projectDir, $"{Name}.csproj");
        var modulesProjectPath = solution.GetRelativePath(projectDir, ProjectDefaults.ModulesProjectPath);

        var fileTemplate = new ProjectFileTemplate
        {
            ModuleName = Name,
            ModuleAuthor = Author,
            ModuleTitle = Title,
            ModuleDescription = Description,
            IsInternal = isInternal,
            ModulesProjectRelativePath = modulesProjectPath
        };

        Directory.CreateDirectory(projectDir);
        await File.WriteAllTextAsync(projectFile, fileTemplate.TransformText());

        return (projectDir, projectFile);
    }

    private async Task AddProjectToSolutionAsync(IEvoScSolution solution, string projectFile, bool isInternal)
    {
        var projectRelativePath = Path.GetRelativePath(Path.GetDirectoryName(solution.SolutionFilePath), projectFile);
        var projectGuid = Guid.NewGuid();
        
        var solutionContents = await File.ReadAllTextAsync(solution.SolutionFilePath);

        var hasExternalModuleFolder = solution
            .SolutionFile
            .ProjectsInOrder
            .Any(p => p.ProjectName.Equals(ProjectDefaults.ExternalModulesFolderName, StringComparison.Ordinal));

        string? modified = null;
        
        // if external make sure the solution folder exists first
        if (!isInternal && !hasExternalModuleFolder)
        {
            var extModulesDir = Path.Combine(Path.GetDirectoryName(solution.SolutionFilePath),
                ProjectDefaults.ExternalModulesFolderName);

            Directory.CreateDirectory(extModulesDir);
            
            modified = solutionContents.Insert(GetEndOfProjectSection(solutionContents),
                GenerateSolutionFolderSection(ProjectDefaults.ExternalModulesFolderName, ProjectDefaults.ExternalModulesFolderName));
            await File.WriteAllTextAsync(solution.SolutionFilePath, modified);
            await solution.RefreshAsync();
        }

        // Add the project to the solution
        modified = (modified ?? solutionContents).Insert(GetEndOfProjectSection(modified ?? solutionContents),
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
