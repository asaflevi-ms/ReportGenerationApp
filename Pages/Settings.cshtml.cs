using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace ReportGenerationApp.Pages;

public class SettingsModel : PageModel
{
    private readonly ILogger<SettingsModel> _logger;
    [BindProperty]
    public string SelectedEnvironment { get; set; }

    public List<SelectListItem> Environments { get; set; }

    private readonly EnvironmentSettings _environmentSettings;
    public EnvironmentOptions SelectedEnvironmentOptions { get; set; }

    private readonly IConfigurationInfo _configurationInfo;

    public SettingsModel(ILogger<SettingsModel> logger, IOptions<EnvironmentSettings> options, IConfigurationInfo configurationInfo)
    {
        _logger = logger;
        _environmentSettings = options.Value;
        _configurationInfo = configurationInfo;
        Environments = new List<SelectListItem>
        {
            new SelectListItem { Value = _environmentSettings.Dev.Name, Text = _environmentSettings.Dev.Name },
            new SelectListItem { Value = _environmentSettings.Test.Name, Text = _environmentSettings.Test.Name },
            new SelectListItem { Value = _environmentSettings.CI.Name, Text = _environmentSettings.CI.Name }
        };

        SelectedEnvironment = _configurationInfo.GetSelectedEnvironment().Name;
    }

    public void OnGet()
    {
        SetSelectedEnvironmentOptions();
    }
    
    public IActionResult OnPost()
    {
        if (string.IsNullOrEmpty(SelectedEnvironment))
        {
            ModelState.AddModelError(string.Empty, "Please select an environment.");
            return Page();
        }

        // Save the selected environment to TempData
        TempData["SelectedEnvironment"] = SelectedEnvironment;

        _configurationInfo.SetSelectedEnvironment(SelectedEnvironment);
        SetSelectedEnvironmentOptions();

        return Page();
    }

    private void SetSelectedEnvironmentOptions()
    {
        SelectedEnvironmentOptions = SelectedEnvironment switch
        {
            var env when env == _environmentSettings.Dev.Name => _environmentSettings.Dev,
            var env when env == _environmentSettings.Test.Name => _environmentSettings.Test,
            var env when env == _environmentSettings.CI.Name => _environmentSettings.CI,
            _ => _environmentSettings.CI
        };
    }
}

