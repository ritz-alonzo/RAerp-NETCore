using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Data.Domain.Addresses;
using RAerp.Models.AddressesModel;

namespace RAerp.Services.AddressServices
{
    public interface IAddressService
    {
        #region CRUD
        Task<IEnumerable<Address>> GetList();
        Task<Address> GetById(Guid id);
        Task<Address> Insert(Address address);
        Task Update(Address address);
        #endregion

        Task<List<AddressBarangayModel>> GetAllBarangays();
        Task<List<AddressCityModel>> GetAllCities();
        Task<List<AddressRegionModel>> GetAllRegions();
        Task<List<AddressCityModel>> GetAllCitiesByRegionCode(string regionCode);
        Task<List<AddressBarangayModel>> GetAllBarangaysByCityCode(string cityCode);

        Task<List<SelectListItem>> GetRegionsSelectList();
        Task<List<SelectListItem>> GetCitiesSelectList(string regionCode = null);
        Task<List<SelectListItem>> GetBarangaysSelectList(string cityCode = null);
    }
}