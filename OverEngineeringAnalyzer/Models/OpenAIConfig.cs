// Models/OpenAIConfig.cs
public class OpenAIConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;

       public string OpenAI_ApiKey { get; set; }
    public string OpenAI_DeploymentName { get; set; }
    public string OpenAI_ApiVersion { get; set; }
    public string OpenAI_Endpoint { get; set; }
}