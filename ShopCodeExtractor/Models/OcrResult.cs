using System.Collections.Generic;

namespace ShopCodeExtractor.Models
{
    // ── Main view model passed to Index.cshtml ──
    public class OcrResult
    {
        public string ExtractedText { get; set; }
        public string ShopCode { get; set; }
        public List<ShopDetailsResponse> ShopDetails { get; set; }  // list — API returns array
        public ShopDetailsApiResponse ApiRawResponse { get; set; }
    }

    // ── Wrapper matching the full API JSON response ──
    // {
    //   "code": 200,
    //   "status": true,
    //   "message": "Shop code details",
    //   "data": [ { ... } ]
    // }
    public class ShopDetailsApiResponse
    {
        public int code { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public List<ShopDetailsResponse> data { get; set; }
    }
}
