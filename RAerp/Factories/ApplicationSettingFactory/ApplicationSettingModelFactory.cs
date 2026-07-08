using AutoMapper;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Data.Domain.Application;
using RA.WebFramework.Extensions;
using RAerp.Factories.CoreFactories;
using RAerp.Models.ApplicationSettingsModel;
using RAerp.Services.ApplicationSettingServices;

namespace RAerp.Factories.ApplicationSettingFactory
{
    public class ApplicationSettingModelFactory : IApplicationSettingModelFactory
    {
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IMapper _mapper;

        public ApplicationSettingModelFactory(
            IApplicationSettingService applicationSettingService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IMapper mapper)
        {
            _applicationSettingService = applicationSettingService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _mapper = mapper;
        }

        public async Task<ApplicationSettingModel> PrepareApplicationSettingModelAsync(ApplicationSettingModel model = null, Guid? id = null)
        {
            ApplicationSetting applicationSetting = null;

            // Load entity if id is provided
            if (id.IsNotNullOrEmpty())
            {
                applicationSetting = await _applicationSettingService.GetByIdAsync(id.Value);
            }

            // Create new model if not provided
            if (model == null)
            {
                model = new ApplicationSettingModel();
                
                // Map entity to model if entity exists
                if (applicationSetting != null)
                {
                    _mapper.Map(applicationSetting, model);
                }
            }

            // Prepare base model properties
            await _baseAdminModelFactory.PrepareBaseAdminModelAsync(model, applicationSetting);

            if (model.CreatedOn != DateTime.MinValue)
                model.CreatedOn = model.CreatedOn.ConvertUTCToLocalDateTime();
            if (model.ModifiedOn.HasValue)
                model.ModifiedOn = model.ModifiedOn.ConvertUTCToLocalDateTime();

            if (string.IsNullOrEmpty(model.DefaultTimeZone))
            {
                model.DefaultTimeZone = TimeZoneInfo.GetSystemTimeZones().Where(c => c.Id == "Singapore Standard Time").Select(c => c.Id).FirstOrDefault();
            }

            model.AvailableTimeZones = TimeZoneInfo.GetSystemTimeZones()
                                        .OrderBy(tz => tz.BaseUtcOffset)
                                        .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                                        {
                                            Value = c.Id,
                                            Text = $"(UTC{(c.BaseUtcOffset >= TimeSpan.Zero ? "+" : "")}{c.BaseUtcOffset:hh\\:mm}) {c.DisplayName}"
                                        }).ToList();

            return model;
        }
    }
}