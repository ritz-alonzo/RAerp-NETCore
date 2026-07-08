using Microsoft.AspNetCore.Mvc;
using RAerp.Services.AddressServices;

namespace RAerp.Controllers.Admin
{
    public class AddressController : AdminController
    {
        private IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        public async Task<IActionResult> GetRegions()
        {
            var regions = await _addressService.GetRegionsSelectList();
            return Json(regions);
        }

        public async Task<IActionResult> GetCitiesByRegionCode(string regionCode)
        {
            if (!string.IsNullOrEmpty(regionCode))
            {
                var citiesByRegionCode = await _addressService.GetCitiesSelectList(regionCode);
                return Json(citiesByRegionCode);
            }

            return Json(null);
        }

        public async Task<IActionResult> GetBarangaysByCityCode(string cityCode)
        {
            if (!string.IsNullOrEmpty(cityCode))
            {
                var barangaysByCityCode = await _addressService.GetBarangaysSelectList(cityCode);
                return Json(barangaysByCityCode);
            }

            return Json(null);
        }
    }
}
