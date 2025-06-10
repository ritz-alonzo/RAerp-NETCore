namespace RAerp.Models.AddressesModel
{
    //{
    //    "id": 1376,
    //    "created_at": "2023-05-02T19:39:24.000000Z",
    //    "updated_at": "2023-05-02T19:39:24.000000Z",
    //    "name": "City of Parañaque",
    //    "code": "1381000000",
    //    "zip_code": "",
    //    "district": "Lone",
    //    "type": "City",
    //    "region_id": 14,
    //    "province_id": 65
    //}
    public class AddressCityModel
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public int ProvinceId { get; set; }
        public string Disctrict { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
