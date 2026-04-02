using Microsoft.AspNetCore.Mvc;
using ShopCodeExtractor.Models;
using System.IO;
using System.Text.RegularExpressions;
using Tesseract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ShopCodeExtractor.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly HttpClient _httpClient;

        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
            _httpClient = new HttpClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid image file.");
                return View();
            }

            // ?? Save uploaded file ??
            string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string filePath = Path.Combine(uploadPath, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // ?? Run OCR ??
            string extractedText = RunOcr(filePath);

            // ?? Extract Shop Code ??
            string shopCode = ExtractShopCode(extractedText);

            // ?? Call API ??
            ShopDetailsApiResponse apiResponse = await GetShopDetailsFullResponse(shopCode);

            var result = new OcrResult
            {
                ExtractedText = extractedText,
                ShopCode = shopCode,
                ShopDetails = apiResponse?.data,   // List<ShopDetailsResponse>
                ApiRawResponse = apiResponse
            };

            return View(result);
        }

        // ?? OCR using Tesseract ??
        private string RunOcr(string imagePath)
        {
            string tessDataPath = Path.Combine(_env.ContentRootPath, "tessdata");

            using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);

            return page.GetText();
        }

        // ?? Extract Shop Code via Regex ??
        private string ExtractShopCode(string text)
        {
            text = text.Replace("\n", " ").Replace("\r", " ").Trim();
            var match = Regex.Match(text, @"Shop\s*Code\s*[:\-]?\s*(\d+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : "Not Found";
        }

        // ?? Call External API ??
        private async Task<ShopDetailsApiResponse> GetShopDetailsFullResponse(string shopCode)
        {
            if (string.IsNullOrEmpty(shopCode) || shopCode == "Not Found")
                return null;

            try
            {
                var url = "http://spror.prgfms.com/api/v1/retail/shopDetails";

                var requestBody = new
                {
                    shop_code = shopCode.Trim(),
                    cont_name = "Saudi Arabia",
                    latitude = "23.5134646",
                    longitude = "44.8224735",
                    countryName = "Saudi Arabia",
                    countryCode = "SA",
                    state = "Riyadh Province",
                    city = "",
                    postalCode = "19928",
                    addressLine = "19928, Saudi Arabia"
                };

                var json = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"[API] Calling for shopCode: {shopCode}");
                var response = await _httpClient.PostAsync(url, content);
                Console.WriteLine($"[API] Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API] Response: {responseString}");

                var apiResponse = JsonSerializer.Deserialize<ShopDetailsApiResponse>(
                    responseString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return apiResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Error: {ex.Message}");
                return null;
            }
        }
    }
}
