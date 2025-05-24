namespace redmineGUI.Models;

using System.ComponentModel.DataAnnotations;

public class ApiKeyModel
{
    [Required(ErrorMessage = "Export API Key is required")]
    public string ApiKeyExport { get; set; }

    [Required(ErrorMessage = "Export Base URL is required")]
    [Url(ErrorMessage = "Must be a valid URL")]
    public string BaseUrlExport { get; set; }

    [Required(ErrorMessage = "Import API Key is required")]
    public string ApiKeyImport { get; set; }

    [Required(ErrorMessage = "Import Base URL is required")]
    [Url(ErrorMessage = "Must be a valid URL")]
    public string BaseUrlImport { get; set; }
}
