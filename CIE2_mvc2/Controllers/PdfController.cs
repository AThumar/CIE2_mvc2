using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CIE2_mvc2.Controllers
{
    public class PdfController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PdfController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // Display the upload page
        public IActionResult Index()
        {
            return View();
        }

        // Show Upload Modal
        public IActionResult Upload()
        {
            return PartialView("_UploadModal");
        }

        // Handle file upload
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string fileName)
        {
            if (file != null && file.Length > 0)
            {
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadPath); // Ensure directory exists

                var filePath = Path.Combine(uploadPath, fileName + Path.GetExtension(file.FileName));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return RedirectToAction("Index"); // Redirect to main page after upload
            }

            return BadRequest("Invalid file upload.");
        }
    }
}
