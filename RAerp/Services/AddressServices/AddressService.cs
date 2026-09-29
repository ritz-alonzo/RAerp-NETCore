using RAerp.Models.AddressesModel;
using RAerp.Helpers.PublicAPIEndpoints;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using RAerp.App_Data;
using RAerp.Domain.Addresses;

namespace RAerp.Services.AddressServices
{
    public class AddressService : IAddressService
    {
        HttpClient _httpClient = new HttpClient();

        private readonly RAerpContext _erpContext;

        public AddressService(RAerpContext erpContext)
        {
            _erpContext = erpContext;
        }

        #region CRUD

        public async Task<IEnumerable<Address>> GetList()
        {
            return await _erpContext.Address.AsNoTracking().ToListAsync();
        }

        public async Task<Address> GetById(Guid id)
        {
            return await _erpContext.Address.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Address> Insert(Address address)
        {
            address.CreatedOn = DateTime.UtcNow;
            _erpContext.Address.Add(address);
            await _erpContext.SaveChangesAsync();

            return address;
        }

        public async Task Update(Address address)
        {
            address.ModifiedOn = DateTime.UtcNow;
            _erpContext.Address.Update(address);
            await _erpContext.SaveChangesAsync();
        }

        #endregion

        #region All Records
        public async Task<List<AddressRegionModel>> GetAllRegions()
        {
            List<AddressRegionModel> regionListModel = new List<AddressRegionModel>();

            var request = new HttpRequestMessage(HttpMethod.Get, APIEndpointConstants.RegionEndpoint);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseObject = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseObject))
                {
                    regionListModel = JsonConvert.DeserializeObject<List<AddressRegionModel>>(responseObject);
                }
            }

            return regionListModel;
        }

        public async Task<List<AddressCityModel>> GetAllCities()
        {
            List<AddressCityModel> cityListModel = new List<AddressCityModel>();

            var request = new HttpRequestMessage(HttpMethod.Get, APIEndpointConstants.CityEndpoint);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseObject = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseObject))
                {
                    cityListModel = JsonConvert.DeserializeObject<List<AddressCityModel>>(responseObject);
                }
            }

            return cityListModel;
        }

        public async Task<List<AddressBarangayModel>> GetAllBarangays()
        {
            List<AddressBarangayModel> barangayListModel = new List<AddressBarangayModel>();

            var request = new HttpRequestMessage(HttpMethod.Get, APIEndpointConstants.BarangayEndpoint);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseObject = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseObject))
                {
                    barangayListModel = JsonConvert.DeserializeObject<List<AddressBarangayModel>>(responseObject);
                }
            }

            return barangayListModel;
        }

        #endregion

        #region Filtered Records

        public async Task<List<AddressCityModel>> GetAllCitiesByRegionCode(string regionCode)
        {
            List<AddressCityModel> cityListModel = new List<AddressCityModel>();

            if (string.IsNullOrEmpty(regionCode))
                throw new ArgumentNullException("Region code is empty.");

            var filteredRegionEndpointURI = APIEndpointConstants.RegionEndpoint + '/' + regionCode + "/cities";
            var request = new HttpRequestMessage(HttpMethod.Get, filteredRegionEndpointURI);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseObject = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseObject))
                {
                    cityListModel = JsonConvert.DeserializeObject<List<AddressCityModel>>(responseObject);
                }
            }

            return cityListModel;
        }

        public async Task<List<AddressBarangayModel>> GetAllBarangaysByCityCode(string cityCode)
        {
            List<AddressBarangayModel> barangayListModel = new List<AddressBarangayModel>();
            if (string.IsNullOrEmpty(cityCode))
                throw new ArgumentNullException("City Code is empty.");

            var filteredCityEndpointURI = APIEndpointConstants.CityEndpoint + '/' + cityCode + "/barangays";
            var request = new HttpRequestMessage(HttpMethod.Get, filteredCityEndpointURI);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseObject = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseObject))
                {
                    barangayListModel = JsonConvert.DeserializeObject<List<AddressBarangayModel>>(responseObject);
                }
            }

            return barangayListModel;
        }

        #endregion

        #region Select List

        public async Task<List<SelectListItem>> GetRegionsSelectList()
        {
            var regionList = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = string.Empty,
                    Text = "Select Region"
                }
            };

            var regions = await GetAllRegions();

            foreach (var region in regions)
            {
                regionList.Add(new SelectListItem()
                {
                    Value = region.Code,
                    Text = region.Name
                });
            }

            return regionList;
        }

        public async Task<List<SelectListItem>> GetCitiesSelectList(string regionCode = null)
        {
            var cityList = new List<SelectListItem>();

            if (string.IsNullOrEmpty(regionCode))
            {
                cityList.Add(new SelectListItem()
                {
                    Value = string.Empty,
                    Text = "Select City"
                });

                return cityList;
            }
            
            var citiesByRegionCode = await GetAllCitiesByRegionCode(regionCode);

            cityList = citiesByRegionCode.Select(c => new SelectListItem()
            {
                Value = c.Code,
                Text = c.Name
            }).ToList();

            return cityList;
        }

        public async Task<List<SelectListItem>> GetBarangaysSelectList(string cityCode = null)
        {
            var barangayList = new List<SelectListItem>();
            if (string.IsNullOrEmpty(cityCode))
            {
                barangayList.Add(new SelectListItem()
                {
                    Value = string.Empty,
                    Text = "Select Barangay"
                });
                
                return barangayList;
            }

            var barangaysByCityCode = await GetAllBarangaysByCityCode(cityCode);

            barangayList = barangaysByCityCode.Select(c => new SelectListItem()
            {
                Value = c.Code,
                Text = c.Name
            }).ToList();

            return barangayList;
        }

        #endregion
    }
}
