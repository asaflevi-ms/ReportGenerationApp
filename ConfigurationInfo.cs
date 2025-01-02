
using Microsoft.Extensions.Options;

public interface IConfigurationInfo
{
    EnvironmentOptions GetSelectedEnvironment();
    void SetSelectedEnvironment(string name);
    EnvironmentSettings GetConfiguration();
}

public class ConfigurationInfo : IConfigurationInfo
{
    private readonly EnvironmentSettings _environmentSettings;
    private string _selectedEnvironment;

    public ConfigurationInfo(IOptions<EnvironmentSettings> options)
    {
        _environmentSettings = options.Value;
        _selectedEnvironment = "CI"; // Default to CI
    }

    public EnvironmentOptions GetSelectedEnvironment()
    {
        return _selectedEnvironment switch
        {
            "Dev" => _environmentSettings.Dev,
            "Test" => _environmentSettings.Test,
            _ => _environmentSettings.CI
        };
    }

    public void SetSelectedEnvironment(string name)
    {
        _selectedEnvironment = name;
    }

    public EnvironmentSettings GetConfiguration()
    {
        return _environmentSettings;
    }
}