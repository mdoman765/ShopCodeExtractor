using Microsoft.AspNetCore.Mvc;
using ShopCodeExtractor.Models;
using System.IO;
using System.Text.RegularExpressions;
using Tesseract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace ShopCodeExtractor.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string uploadPath = Path.Combine(_env.WebRootPath, "uploads");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                string extractedText = RunOcr(filePath);
                string shopCode = ExtractShopCode(extractedText);

                var result = new OcrResult
                {
                    ExtractedText = extractedText,
                    ShopCode = shopCode
                };

                return View(result);
            }

            return View();
        }

        private string RunOcr(string imagePath)
        {
            string tessDataPath = Path.Combine(_env.ContentRootPath, "tessdata");

            using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(imagePath))
                {
                    using (var page = engine.Process(img))
                    {
                        return page.GetText();
                    }
                }
            }
        }

        private string ExtractShopCode(string text)
        {
            // Normalize text (remove weird OCR spacing)
            text = text.Replace("\n", " ").Replace("\r", " ");

            // Try to capture numbers after "Shop Code :"
            var match = Regex.Match(text, @"Shop\s*Code\s*[:\-]?\s*(\d+)", RegexOptions.IgnoreCase);

            if (match.Success)
                return match.Groups[1].Value;

            return "Not Found";
        }
    }
}