using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Place.Controllers;

[ApiController]
[Route("")]
public class PageController : ControllerBase
{
    private readonly ILogger<PageController> _logger;

    public PageController(ILogger<PageController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("{year}/{month}/{day}/{name}")]
    public IActionResult Get(uint year, uint month, uint day, string name)
    {
        // validate date
        if (!Utils.IsValidDate(year, month, day))
            return NotFound();

        // validate name
        if (!Utils.IsValidName(name))
            return NotFound();
        
        if (Program.TryGetPage(year, month, day, name, out string path))
            return PhysicalFile(path, "text/html");

        return NotFound();
    }
    
    [HttpGet]
    [Route("{id}")]
    public IActionResult Get(uint id)
    {
        if (!Program.TryGetMappedPage(id, out string page))
            return NotFound();
        
        return RedirectPermanentPreserveMethod(page);
    }
    
    [HttpGet]
    [Route("static/assets/{id}.{ext}")]
    public IActionResult Get(uint id, string ext)
    {
        if (Program.TryGetAsset(id, ext, out string path))
            return PhysicalFile(path, $"application/{ext}");

        return NotFound();
    }
}
