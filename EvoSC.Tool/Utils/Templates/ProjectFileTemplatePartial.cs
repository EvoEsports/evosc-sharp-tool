namespace EvoSC.Tool.Utils.Templates;

public partial class ProjectFileTemplate
{
    public string ModuleName { get; set; }
    public string ModuleAuthor { get; set; }
    public string ModuleTitle { get; set; }
    public string ModuleDescription { get; set; }
    public bool IsInternal { get; set; }
    public string ModulesProjectRelativePath { get; set; }
}
