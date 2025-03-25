using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;
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
    [HttpPost]
    [Route("Home/ReceiveText")]
    public IActionResult ReceiveText([FromBody] TextRequest data)
    {
        if (data == null || string.IsNullOrEmpty(data.Text))
        {
            return BadRequest(new { message = "No text received" });
        }

        Console.WriteLine("📩 Received Text: " + data.Text);

        return Json(new { success = true, message = "Text received successfully", receivedText = data.Text });
    }

    // Define the request model
    public class TextRequest
    {
        public string Text { get; set; }
    }
    public IActionResult Plans()
    {
        return View();
    }



    [HttpPost]
    [Route("Home/CheckQuestion")]
    public async Task<IActionResult> CheckQuestion([FromBody] QuestionRequest request)
    {
        if (string.IsNullOrEmpty(request.Question) || string.IsNullOrEmpty(request.FileName))
        {
            return BadRequest(new { isValid = false, message = "Invalid request" });
        }

        string pdfPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", request.FileName);

        if (!System.IO.File.Exists(pdfPath))
        {
            return NotFound(new { isValid = false, message = "File not found" });
        }

        string pdfText = ExtractTextFromPdf(pdfPath);
        bool isValid = pdfText.Contains(request.Question, StringComparison.OrdinalIgnoreCase);

        if (isValid)
        {
            return Json(new { isValid = true, answer = "✅ Answer found in the document!" });
        }
        else
        {
            // Call Gemini AI if the answer is not found in the document
            string aiResponse = await GeminiAI.AskGeminiAsync(request.Question);
            return Json(new { isValid = false, answer = aiResponse });
        }
    }

    public class QuestionRequest
{
    public string Question { get; set; }
    public string FileName { get; set; }
}

    // Extract text from PDF
    private string ExtractTextFromPdf(string pdfPath)
    {
        StringBuilder text = new StringBuilder();
        using (PdfReader reader = new PdfReader(pdfPath))
        using (PdfDocument pdf = new PdfDocument(reader))
        {
            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                text.Append(PdfTextExtractor.GetTextFromPage(pdf.GetPage(i)));
            }
        }

        string extractedText = text.ToString().ToLower();

        // 🔍 Print extracted text in console
        Console.WriteLine("📄 Extracted PDF Text:");
        Console.WriteLine(extractedText);

        return extractedText;
    }
    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> AskAI([FromBody] UserQuery query)
    {
        if (string.IsNullOrEmpty(query.Question) || string.IsNullOrEmpty(query.FileName))
        {
            return BadRequest(new { message = "Question or file name is missing" });
        }

        // Check if the question exists in the selected document
        bool isInDocument = SearchInDocument(query.Question, query.FileName);

        if (isInDocument)
        {
            return Json(new { answer = "✅ Answer found in the document!" });
        }
        else
        {
            // If not in the document, ask Gemini AI
            string aiResponse = await GeminiAI.AskGeminiAsync(query.Question);
            return Json(new { answer = aiResponse });
        }
    }


    private bool SearchInDocument(string question, string fileName)
    {
        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName); // Use uploaded PDFs

        if (System.IO.File.Exists(filePath))
        {
            string pdfText = ExtractTextFromPdf(filePath); // Extract text instead of reading raw file
            return pdfText.Contains(question, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public class UserQuery
    {
        public string Question { get; set; } = string.Empty;  // Ensure non-nullable
        public string FileName { get; set; } = string.Empty;  // Add FileName
    }

}
