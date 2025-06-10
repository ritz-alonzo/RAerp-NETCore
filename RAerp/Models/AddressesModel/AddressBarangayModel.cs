namespace RAerp.Models.AddressesModel
{
    //{
    //    "id": 36213,
    //    "created_at": "2023-05-02T19:39:23.000000Z",
    //    "updated_at": "2023-05-02T19:39:23.000000Z",
    //    "name": "Bagumbuhay",
    //    "code": "1381300007",
    //    "status": "6681",
    //    "region_id": 14,
    //    "province_id": 65,
    //    "city_municipality_id": 1367
    //},
    public class AddressBarangayModel
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public int ProvinceId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
