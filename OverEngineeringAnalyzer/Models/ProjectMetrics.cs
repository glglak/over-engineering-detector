public class ProjectMetrics
{
    public int TotalFiles { get; set; }
    public int TotalDirectories { get; set; }
    public int AverageNestingLevel { get; set; }
    public string DeepestDirectory { get; set; }
    public Dictionary<string, int> FileTypeDistribution { get; set; }
    public int Services { get; set; }
    public int Projects { get; set; }
    public int Patterns { get; set; }
    public int ArchitectureLayers { get; set; }
}

public class ProjectAnalysisResponse
{
    public string RoastingMessage { get; set; }
    public string RoastingTip { get; set; }
    public ProjectMetrics Metrics { get; set; }
}
public class ProjectStructure
{
    /// <summary>
    /// Represents the hierarchical structure of the project.
    /// </summary>
    public Dictionary<string, object> Structure { get; set; }
}
