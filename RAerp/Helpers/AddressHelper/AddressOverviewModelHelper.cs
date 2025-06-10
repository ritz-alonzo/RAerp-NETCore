using RA.Core.Models.OverviewModels;
using RA.Data.Domain.Addresses;

namespace RAerp.Helpers.AddressHelper
{
    public static class AddressOverviewModelHelper
    {
        public static AddressOverviewModel PrepareOverviewModel(Address address)
        {
            var model = new AddressOverviewModel();

            if (address == null) 
                return model;

            model.Id = address.Id;
            model.AddressLine1 = address.AddressLine1;
            model.AddressLine2 = address.AddressLine2;
            model.AddressLine3 = address.AddressLine3;
            model.ZipCode = address.ZipCode;
            model.RegionCode = address.RegionCode;
            model.CityCode = address.CityCode;
            model.BarangayCode = address.BarangayCode;

            return model;
        }

        public static Address PrepareAddressEntity(AddressOverviewModel model)
        {
            var address = new Address();

            if (model == null) 
                return address;

            address.Id = model.Id;
            address.AddressLine1 = model.AddressLine1;
            address.AddressLine2 = model.AddressLine2;
            address.AddressLine3 = model.AddressLine3;
            address.ZipCode = model.ZipCode; 
            address.RegionCode = model.RegionCode; 
            address.CityCode = model.CityCode;
            address.BarangayCode = model.BarangayCode;

            return address;
        }

        public static Address PrepareAddressRemapping(Address originalAddress, Address updatedAddress)
        {
            if (originalAddress == null || updatedAddress == null)
                throw new ArgumentNullException(nameof(Address));

            originalAddress.RegionCode = updatedAddress.RegionCode;
            originalAddress.CityCode = updatedAddress.CityCode;
            originalAddress.BarangayCode = updatedAddress.BarangayCode;
            originalAddress.AddressLine1 = updatedAddress.AddressLine1;
            originalAddress.AddressLine2 = updatedAddress.AddressLine2;
            originalAddress.AddressLine3 = updatedAddress.AddressLine3;
            originalAddress.ZipCode = updatedAddress.ZipCode;

            return originalAddress;
        }
    }
}
