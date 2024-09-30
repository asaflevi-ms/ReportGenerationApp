using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ReportGenerationApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenService _tokenService;
        private const string Scope = "https://hls-repgen.azure-test.net/.default";
        private const string DraftReportPath = "radiology/generate-draft-report";

        public IndexModel(IHttpClientFactory httpClientFactory, TokenService tokenService)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
        }

        [BindProperty]
        public IFormFile JsonFile { get; set; }

        [TempData]
        public string InputJson { get; set; }
        [TempData]
        public string ResponseJson { get; set; }

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

            return Page();
        }

        public async Task<IActionResult> OnPostGenerateDraftReportAsync()
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

            // Format the JSON indenting
            var jsonDocument = JsonDocument.Parse(ResponseJson);
            ResponseJson = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions { WriteIndented = true });

            return Page();
        }

        public IActionResult OnPostClear()
        {
            InputJson = null;
            ResponseJson = null;
            return Page();
        }

        public void OnGet()
        {
        }
    }
}