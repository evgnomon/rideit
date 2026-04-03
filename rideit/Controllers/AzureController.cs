using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;

namespace rideit.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AzureController : ControllerBase
{
    private readonly ILogger<AzureController> _logger;

    public AzureController(ILogger<AzureController> logger)
    {
        _logger = logger;
    }

    // Demo: acquire an Azure token using DefaultAzureCredential
    // Works with Managed Identity, Azure CLI, env vars, etc.
    [HttpGet("token")]
    public async Task<IActionResult> GetToken([FromQuery] string scope = "https://management.azure.com/.default")
    {
        try
        {
            var credential = new DefaultAzureCredential();
            var tokenRequest = new TokenRequestContext(new[] { scope });
            var token = await credential.GetTokenAsync(tokenRequest);

            return Ok(new
            {
                expiresOn = token.ExpiresOn,
                scopeRequested = scope,
                tokenPreview = token.Token[..20] + "..."
            });
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogWarning(ex, "Azure authentication failed");
            return StatusCode(503, new
            {
                error = "Azure authentication not configured",
                hint = "Run 'az login' or set AZURE_CLIENT_ID/AZURE_TENANT_ID/AZURE_CLIENT_SECRET env vars"
            });
        }
    }

    // Info endpoint showing which credential sources are available
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            azureClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") is not null,
            azureTenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") is not null,
            azureClientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") is not null,
            hint = "DefaultAzureCredential tries: env vars -> managed identity -> Azure CLI -> Visual Studio -> etc."
        });
    }
}
