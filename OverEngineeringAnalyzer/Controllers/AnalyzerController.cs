using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/analyzer")]
public class AnalyzerController : ControllerBase
{
    private readonly ILogger<AnalyzerController> _logger;
    private readonly OpenAIConfig _openAIConfig;
    private readonly HttpClient _httpClient;

    private const int MaxInputTokens = 4000; // Adjusted for input size limit.

    public AnalyzerController(ILogger<AnalyzerController> logger,IOptions<OpenAIConfig> openAIConfig, HttpClient httpClient)
    {
        _logger = logger;
        _openAIConfig = openAIConfig.Value;
        _httpClient = httpClient;

          _httpClient.BaseAddress = new Uri(_openAIConfig.Endpoint);
        _httpClient.DefaultRequestHeaders.Add("api-key", _openAIConfig.ApiKey);

    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeProject([FromBody] ProjectStructure request)
    {
          var url = $"/openai/deployments/{_openAIConfig.DeploymentName}/chat/completions?api-version={_openAIConfig.ApiVersion}";
    _logger.LogInformation("Requesting OpenAI API at: {BaseUrl}{Url}", _openAIConfig.Endpoint, url);

        if (request == null || request.Structure == null)
        {
            _logger.LogError("Invalid request received.");
            return BadRequest("Invalid request. Structure is required.");
        }

        _logger.LogInformation("Received project structure: {Structure}", request.Structure);

        // Serialize and validate structure size
        var inputStructureJson = SummarizeStructure(JsonSerializer.Serialize(request.Structure));
  _logger.LogInformation("inputStructureJson  {Length}", inputStructureJson.Length);

        if (!ValidateTokenCount(inputStructureJson, MaxInputTokens))
        {
            return BadRequest("Input too large to process. Please simplify your structure.");
        }

        // Prepare GPT payload with pattern-specific analysis
var gptPayload = new
{
    messages = new[]
    {
        new 
        { 
            role = "system", 
            content = @"
You are a witty, seasoned software architect with a knack for roasting project structures and delivering brutally honest, yet constructive feedback. Always respond in **valid JSON**. Do not include markdown, backticks, or line breaks. Ensure the JSON is directly parseable by JSON.parse(). 

Your analysis must include:
1. Architecture Patterns: Identify applied patterns (e.g., Clean, Onion, Microservices, etc.) and evaluate their effectiveness.
2. Anti-Patterns: Detect signs of bad practices like God classes, circular dependencies, or excessive boilerplate.
3. SOLID Principle Violations: Identify any violations of SOLID principles with examples.
4. Over-engineering Symptoms: Highlight unnecessary abstractions, redundant components, or excessive complexity.
5. Naming Conventions: Assess consistency and clarity of naming conventions.
6. Component Coupling: Check if components are overly coupled or not following dependency inversion.
7. Domain Model Issues: Spot any anomalies, missing domain logic, or misaligned boundaries.
8. Testing Structure: Review the presence and organization of test cases and folders.

Your response format:
{
    ""roastingMessage"": ""[A detailed, witty analysis of the project with examples, concise feedback, and naming actual architecture patterns.]"",
    ""roastingTip"": ""[A short, sarcastic roasting tip or advice for the team, emphasizing a funny or glaring issue in the project.]"",
    ""metrics"": {
        ""TotalFiles"": 0,
        ""TotalDirectories"": 0,
        ""AverageNestingLevels"": 0,
        ""DeepestDirectory"": ""string"",
        ""FileTypeDistribution"": {""fileType"": 0},
        ""Services"": 0,
        ""Projects"": 0,
        ""DesignPatterns"": [""pattern1"", ""pattern2""],
        ""ArchitectureLayers"": 0
    }
}

NEVER include any additional text or formatting outside this JSON. Failure to comply will result in invalid output."
        },
        new 
        { 
            role = "user", 
            content = @$"
Analyze this project structure. Provide critical feedback, highlight specific issues, and give sarcastic advice in the format above:

{inputStructureJson}"
        }
    },
    max_tokens = 4000,
    temperature = 0.9,
    top_p = 0.95,
    presence_penalty = 0.3,
    frequency_penalty = 0.2
};


        try
        {
            // Send request to Azure GPT and parse the response
            var gptResponse = await CallAzureGptApi(gptPayload);
            _logger.LogInformation("Gpt Response:"+gptResponse.ToString());
            // Extract metrics and patterns from GPT response
            var metrics = ParseMetricsFromGptResponse(gptResponse);

            var response = new ProjectAnalysisResponse
            {
                RoastingMessage = gptResponse["choices"]?[0]?["message"]?["content"]?.ToString(),
                Metrics = metrics
            };

 
        // Directly return the GPT response (assuming it's well-formed JSON)
        return Ok(gptResponse["choices"]?[0]?["message"]?["content"]);     
           }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during project analysis.");
            return StatusCode(500, "Internal server error.");
        }
    }

   private async Task<JsonNode> CallAzureGptApi(object payload)
{
    
    
            _httpClient.DefaultRequestHeaders.Add("api-key", _openAIConfig.ApiKey);

    var response = await _httpClient.PostAsync(
                      $"/openai/deployments/{_openAIConfig.DeploymentName}/chat/completions?api-version={_openAIConfig.ApiVersion}",

        new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
    );

    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();

    // Directly parse JSON response into JsonNode
    return JsonNode.Parse(content);
}

 private ProjectMetrics ParseMetricsFromGptResponse(JsonNode gptResponse)
{
    // Extract the roasting message (content) from GPT response
    var content = gptResponse["choices"]?[0]?["message"]?["content"]?.ToString();

    if (string.IsNullOrEmpty(content))
        throw new InvalidOperationException("Invalid GPT response format.");

    // Initialize metrics with default values
    var metrics = new ProjectMetrics
    {
        TotalFiles = 0,
        TotalDirectories = 0,
        AverageNestingLevel = 0,
        DeepestDirectory = "",
        FileTypeDistribution = new Dictionary<string, int>(),
        Services = 0,
        Projects = 0,
        Patterns = 0,
        ArchitectureLayers = 0
    };

    // Split content into lines
    var lines = content.Split("\n", StringSplitOptions.RemoveEmptyEntries);

    // Extract metrics by parsing specific lines
    metrics.TotalFiles = ExtractNumericMetric(lines, "Total Files");
    metrics.TotalDirectories = ExtractNumericMetric(lines, "Total Directories");
    metrics.AverageNestingLevel = ExtractNumericMetric(lines, "Average Nesting Levels");
    metrics.DeepestDirectory = ExtractStringMetric(lines, "Deepest Directory");
    metrics.FileTypeDistribution = ExtractFileTypeDistribution(lines);
    metrics.Services = ExtractNumericMetric(lines, "Services Detected");
    metrics.Projects = ExtractNumericMetric(lines, "Projects");
    metrics.Patterns = ExtractNumericMetric(lines, "Design Patterns");
    metrics.ArchitectureLayers = ExtractNumericMetric(lines, "Architecture Layers");

    return metrics;
}
private int ExtractNumericMetric(string[] lines, string metricName)
{
    var line = Array.Find(lines, l => l.StartsWith($"- **{metricName}**", StringComparison.OrdinalIgnoreCase));
    if (line != null && int.TryParse(line.Split(":")[1].Trim(), out int value))
    {
        return value;
    }
    return 0; // Default if not found
}

private string ExtractStringMetric(string[] lines, string metricName)
{
    var line = Array.Find(lines, l => l.StartsWith($"- **{metricName}**", StringComparison.OrdinalIgnoreCase));
    return line?.Split(":")[1].Trim() ?? ""; // Default if not found
}

private Dictionary<string, int> ExtractFileTypeDistribution(string[] lines)
{
    var distributionLine = Array.Find(lines, l => l.StartsWith("- **File Type Distribution**", StringComparison.OrdinalIgnoreCase));
    if (distributionLine == null) return new Dictionary<string, int>();

    // Extract CSV part from the line
    var csvStart = distributionLine.IndexOf("```csv") + 6; // Skip past "```csv"
    var csvEnd = distributionLine.IndexOf("```", csvStart);
    if (csvStart < 0 || csvEnd < csvStart) return new Dictionary<string, int>();

    var csvContent = distributionLine.Substring(csvStart, csvEnd - csvStart).Trim();
    var fileTypeDistribution = new Dictionary<string, int>();

    // Parse CSV
    foreach (var line in csvContent.Split("\n"))
    {
        var parts = line.Split(",");
        if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int count))
        {
            fileTypeDistribution[parts[0].Trim()] = count;
        }
    }

    return fileTypeDistribution;
}


    private bool ValidateTokenCount(string input, int maxAllowedTokens)
{
    // Approximation based on observations: consider non-ASCII, spaces, and special characters
    int estimatedTokens = input.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length + input.Length / 6;
    _logger.LogInformation("Estimated Tokens:"+estimatedTokens.ToString());
    return estimatedTokens <= maxAllowedTokens;
}


    private string SummarizeStructure(string structureJson)
    {
        // Summarize or truncate the JSON to fit within the token limit
        if (structureJson.Length > 12000) // ~3000 tokens approximation
        {
            structureJson = structureJson.Substring(0, 12000) + "... (truncated)";
        }
        return structureJson;
    }
}
