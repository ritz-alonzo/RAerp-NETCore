using Microsoft.AspNetCore.Mvc.Rendering;

namespace RAerp.Models.AddressesModel
{
    public class AddressModel
    {
        public AddressModel()
        {
            Regions = new List<SelectListItem>();
            Cities = new List<SelectListItem>();
            Barangays = new List<SelectListItem>();
        }

        public Guid Id { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string RegionCode { get; set; }
        public string CityCode { get; set; }
        public string BarangayCode { get; set; }
        public string ZipCode { get; set; }
        public List<SelectListItem> Regions { get; set; }
        public List<SelectListItem> Cities { get; set; }
        public List<SelectListItem> Barangays { get; set; }

    }
}
