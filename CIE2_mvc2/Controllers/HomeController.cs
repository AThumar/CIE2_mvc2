using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

public class HomeController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HomeController(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    // Handle File Upload
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Message"] = "No file selected!";
            return RedirectToAction("Upload");
        }

        // Set upload folder path
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

        // Create the folder if it doesn't exist
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Save the file in wwwroot/uploads
        string filePath = Path.Combine(uploadsFolder, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        TempData["Message"] = "File uploaded successfully!";
        return RedirectToAction("Upload");
    }

    // Display Uploaded Files
    public IActionResult Upload()
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
        var files = Directory.Exists(uploadsFolder) ? Directory.GetFiles(uploadsFolder) : Array.Empty<string>();

        // Pass file names to the view
        List<string> fileNames = new List<string>();
        foreach (var file in files)
        {
            fileNames.Add(Path.GetFileName(file));
        }

        ViewBag.Files = fileNames;
        return View();
    }
}
