
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<EnvironmentSettings>(builder.Configuration.GetSection("EnvironmentSettings"));
EnvironmentSettings environmentSettings = new EnvironmentSettings();
builder.Configuration.GetSection("EnvironmentSettings").Bind(environmentSettings);

// Register ConfigurationInfo as a singleton service
builder.Services.AddSingleton<IConfigurationInfo, ConfigurationInfo>();


builder.Services.AddHttpClient(environmentSettings!.Dev.Name, client =>
{
    client.BaseAddress = new Uri(environmentSettings!.Dev.Endpoint);
});

builder.Services.AddHttpClient(environmentSettings!.Test.Name, client =>
{
    client.BaseAddress = new Uri(environmentSettings!.Test.Endpoint);
});

builder.Services.AddHttpClient(environmentSettings!.CI.Name, client =>
{
    client.BaseAddress = new Uri(environmentSettings!.CI.Endpoint);
});



builder.Services.AddSingleton<TokenService>();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.UseSession();
app.Run();