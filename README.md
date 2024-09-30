# ReportGenerationApp

## Overview
This application is made to demonstrate how to generate draft reports by calling an external report generation service. Below are the key configurations and steps to use the service.

## Configuration

### ReportGenerationBaseEndpoint
The base endpoint for the report generation service is configured as:
```csharp
private const string ReportGenerationBaseEndpoint = "https://reportgeneration.test.workspace.mshapis.com/";
```

### Generate Draft Report Path
The path to generate a draft report is:

```csharp
private const string DraftReportPath = "radiology/generate-draft-report";
```

### Scope Name and How to Acquire Token

```csharp
private const string Scope = "https://hls-repgen.azure-test.net/.default";
```

To acquire the token, you can use the TokenService class. Here is an example of how to acquire the token:

```csharp
var token = await _tokenService.GetTokenAsync(new[] { Scope });
```

```csharp
public class TokenService
{
    private readonly DefaultAzureCredential _credential;

    public TokenService()
    {
        _credential = new DefaultAzureCredential();
    }

    public async Task<string> GetTokenAsync(string[] scopes)
    {
        var tokenRequestContext = new TokenRequestContext(scopes);
        var token = await _credential.GetTokenAsync(tokenRequestContext);
        return token.Token;
    }
}
```

### How to Call Report Generation Service
To call the report generation service, you need to create an HttpClient with the base address and use it to send a POST request to the draft report path. Below is an example of how to do this:

```csharp
    try
    {
        using var content = new StringContent(InputJson, System.Text.Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient("ReportGenerationClient");

        // Acquire the token using TokenService
        var token = await _tokenService.GetTokenAsync(new[] { Scope });

        // Add the Bearer token to the request headers
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Use the relative path for the request
        var response = await client.PostAsync(DraftReportPath, content);

        ResponseJson = await response.Content.ReadAsStringAsync();
    }
    catch (Exception ex)
    {
        ResponseJson = $"Error: {ex.Message}";
    }
```



## Deployment and Configuration
Once you deployed your app (must by in Microsoft CORP Tenant ), you need to assign managed identity to your service.
Share your  Client (App) ID with ReportGeneration service team to add your service to the allow list. 