using FluentValidation;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.EntityTypes.Helpers;
using RA.WebFramework.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.EntityTypes.Validators
{
    /// <summary>
    /// Base Validator for all Entity Types
    /// </summary>
    public class BaseEntityModelValidator<TModel> : AbstractValidator<TModel>
        where TModel : BaseEntityModel
    {
        public BaseEntityModelValidator()
        {
            RuleFor(c => c.Code)
                .NotEmpty()
                .WithMessage(EntityTypeMessages.CodeRequired);

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage(EntityTypeMessages.NameRequired)
                .Matches(RegexConstants.EntityNameRegex).WithMessage("Invalid Name, remove numbers");
        }
    }
}
