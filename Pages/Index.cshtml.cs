using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace ReportGenerationApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenService _tokenService;

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
                return Page();
            }

            using var stream = JsonFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            InputJson = await reader.ReadToEndAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostGenerateDraftReportAsync()
        {
            if (string.IsNullOrEmpty(InputJson))
            {
                ModelState.AddModelError(string.Empty, "Please upload a valid JSON file first.");
                return Page();
            }

            using var content = new StringContent(InputJson, System.Text.Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();

            // Acquire the token using TokenService
            var token = await _tokenService.GetTokenAsync(new[] { "https://hls-repgen.azure-test.net/.default" });

            // Add the Bearer token to the request headers
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("https://reportgeneration.test.workspace.mshapis.com/radiology/generate-draft-report", content);

            ResponseJson = await response.Content.ReadAsStringAsync();

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