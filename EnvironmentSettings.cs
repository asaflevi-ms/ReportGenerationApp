public class EnvironmentOptions
{
    public required string Name { get; set; }
    public required string Endpoint { get; set; }
    public required string Scope { get; set; }
}

public class EnvironmentSettings
{
    public EnvironmentOptions Dev { get; set; }
    public  EnvironmentOptions Test { get; set; }
    public  EnvironmentOptions CI { get; set; }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureEnvironmentSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("EnvironmentSettings");
        if (section == null)
        {
            throw new InvalidOperationException("EnvironmentSettings section is missing in the configuration.");
        }
        //get  EnvironmentSettings
        var environmentSettings = section.Get<EnvironmentSettings>();
        if (environmentSettings == null)
        {
          throw new InvalidOperationException("Failed to bind EnvironmentSettings from the configuration.");
        }
        services.Configure<EnvironmentSettings>(section);
        
        return services;
    }
}