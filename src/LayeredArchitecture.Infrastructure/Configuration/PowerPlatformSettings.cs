namespace HART.Infrastructure.Configuration;

public class PowerPlatformSettings
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "1";

    public string GetFullUrl()
    {
        return $"{ApiUrl}?api-version={ApiVersion}";
    }
}
