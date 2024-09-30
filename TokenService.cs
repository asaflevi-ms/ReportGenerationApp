using Azure.Core;
using Azure.Identity;
using System.Threading.Tasks;

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