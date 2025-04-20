using Microsoft.AspNetCore.Mvc;
using TalkLikeTv.EntityModels;


namespace TalkLikeTv.HttpClient.Controllers;

public class LanguagesController : Controller
{
    private readonly ILogger<LanguagesController> _logger;
    private readonly IHttpClientFactory _clientFactory;
    
    public LanguagesController(
        ILogger<LanguagesController> logger, 
        IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }
    
    public async Task<IActionResult> Languages()
    {
        
        ViewData["Title"] = "All Languages";
        var uri = "api/languages";

        var client = _clientFactory.CreateClient(
            name: "TalkLikeTv.WebApi");

        HttpRequestMessage request = new(
            method: HttpMethod.Get, requestUri: uri);

        var response = await client.SendAsync(request);

        var model = await response.Content
            .ReadFromJsonAsync<IEnumerable<Language>>();

        return View(model);
    }

}