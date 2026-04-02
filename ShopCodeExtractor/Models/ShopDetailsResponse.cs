namespace ShopCodeExtractor.Models
{
    // Maps to each object inside the "data": [ ... ] array
    public class ShopDetailsResponse
    {
        public int id { get; set; }
        public string site_name { get; set; }   // shop name
        public string site_ownm { get; set; }   // owner name
        public string site_adrs { get; set; }   // address
        public string site_code { get; set; }   // shop code
        public string site_mob1 { get; set; }   // mobile number
        public int lfcl_id { get; set; }
        public int cont_id { get; set; }
    }
}
