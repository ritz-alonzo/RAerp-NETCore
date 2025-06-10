using RAerp.Data;
using Newtonsoft.Json;

namespace RAerp.Services.ApplicationServices
{
    public class ApplicationService : IApplicationService
    {
        public ApplicationDetails GetApplicationDetails()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var appDetailsFile = Directory.GetFiles(currentDirectory, "appdetails.json").FirstOrDefault();
            if (appDetailsFile != null)
            {
                var reader = new StreamReader(appDetailsFile);

                return JsonConvert.DeserializeObject<ApplicationDetails>(reader.ReadToEnd());
            }
            else
            {
                return new ApplicationDetails()
                {
                    ApplicationName = "NA",
                    Version = "0.000.000"
                };
            }
        }
    }
}
