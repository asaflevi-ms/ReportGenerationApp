using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;

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

        public string ResponseContent { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (JsonFile == null || JsonFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload a valid JSON file.");
                return Page();
            }

            using var stream = JsonFile.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var client = _httpClientFactory.CreateClient();

            // Acquire the token using TokenService
            var token = await _tokenService.GetTokenAsync(new[] { "https://hls-repgen.azure-test.net/.default" });

            // Add the Bearer token to the request headers
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("https://reportgeneration.dev.workspace.mshapis.com/radiology/generate-draft-report", content);

            ResponseContent = $"Status Code: {response.StatusCode}\n\n{await response.Content.ReadAsStringAsync()}";

            return Page();
        }

        public void OnGet()
        {
        }
    }
}