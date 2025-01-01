using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ReportGenerationApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenService _tokenService;
        private const string Scope = "https://hls-repgen.azure-test.net/.default";
        private const string DraftReportPath = "radiology/generate-draft-report";
        private const string PriorReportSummarizationPath = "radiology/generate-prior-reports-summary";
        
        private readonly EnvironmentSettings _environmentSettings;
        public IndexModel(IHttpClientFactory httpClientFactory, TokenService tokenService, IOptions<EnvironmentSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _environmentSettings = options.Value;
        }

        [BindProperty]
        public IFormFile JsonFile { get; set; }

        [TempData]
        public string InputJson { get; set; }
        [TempData]
        public string ResponseJson { get; set; }
        [TempData]
        public string LastAction { get; set; }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (JsonFile == null || JsonFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload a valid JSON file.");
                ResponseJson = "Please upload a valid JSON file.";
                return Page();
            }

            using var stream = JsonFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            var jsonString = await reader.ReadToEndAsync();

            // Format the JSON
            var jsonDocument = JsonDocument.Parse(jsonString);
            InputJson = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions { WriteIndented = true });

            // Clear the output (right pane)
            ResponseJson = null;
            LastAction = null;

            return Page();
        }

        public async Task<IActionResult> OnPostGenerateDraftReportAsync()
        {
            LastAction = "[Draft Report]";
            return await GenerateReportAsync(DraftReportPath);
        }

        public async Task<IActionResult> OnPostPriorReportSummarizationAsync()
        {
            LastAction = "[Prior reports summary]";
            return await GenerateReportAsync(PriorReportSummarizationPath);
        }

        private async Task<IActionResult> GenerateReportAsync(string path)
        {
            if (string.IsNullOrEmpty(InputJson))
            {
                ModelState.AddModelError(string.Empty, "Please upload a valid JSON file first.");
                ResponseJson = "Please upload a valid JSON file first.";
                return Page();
            }

            try
            {
                using var content = new StringContent(InputJson, System.Text.Encoding.UTF8, "application/json");

                // environment settings by selectedEnvironment
                var selectedEnv = GetSelectedEnvironment();

                var client = _httpClientFactory.CreateClient(selectedEnv.Name);

                // Acquire the token using TokenService
                var token = await _tokenService.GetTokenAsync(new[] { selectedEnv.Scope });

                // Add the Bearer token to the request headers
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Add query parameter for API version
                var uriBuilder = new UriBuilder(client.BaseAddress + path);
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                query["api-version"] = "2024-08-21-preview";
                uriBuilder.Query = query.ToString();
                var requestUri = uriBuilder.ToString();
                
                // Use the requestUri for the request
                var response = await client.PostAsync(requestUri, content);
                // Use the relative path for the request

                ResponseJson = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                ResponseJson = $"Error: {ex.Message}";
            }

            // Format the JSON indenting
            var jsonDocument = JsonDocument.Parse(ResponseJson);
            ResponseJson = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions { WriteIndented = true });

            return Page();
        }

        private EnvironmentOptions GetSelectedEnvironment()
        {
            var selectedEnvironment = TempData["SelectedEnvironment"]?.ToString() ?? "CI";
            return selectedEnvironment switch
            {
                "Dev" => _environmentSettings.Dev,
                "Test" => _environmentSettings.Test,
                _ => _environmentSettings.CI
            };
        }

        public IActionResult OnPostClear()
        {
            InputJson = null;
            ResponseJson = null;
            return Page();
        }

        public void OnGet()
        {
            InputJson = null;
            ResponseJson = null;
            LastAction = null;
        }
    }
}