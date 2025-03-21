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

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("❌ No file selected.");
                return BadRequest("No file selected!");
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
                Console.WriteLine($"📁 Created folder: {uploadsFolder}");
            }

            string filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            Console.WriteLine($"✅ File uploaded: {filePath}");
            return Ok("File uploaded successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Upload failed: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
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
    public IActionResult PdfViewer(string fileName)
    {
        ViewBag.FileName = fileName;
        return View();
    }

}
