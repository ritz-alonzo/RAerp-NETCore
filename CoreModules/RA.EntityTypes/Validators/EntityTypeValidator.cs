using FluentValidation;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.EntityTypes.Helpers;

namespace RA.EntityTypes.Validators
{
    public class EntityTypeValidator : AbstractValidator<EntityTypeModel>
    {
        public EntityTypeValidator()
        {
            RuleFor(m => m.EntityName)
                .NotEmpty()
                .WithMessage(EntityTypeMessages.EntityNameRequired);
        }
    }
}
