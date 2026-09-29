namespace RAerp.Models.ApiModel
{
    public class ApiRequestValidationModel
    {
        public bool IsValid { get; set; } = true;
        public string ValidationMessage { get; set; }
    }
}
