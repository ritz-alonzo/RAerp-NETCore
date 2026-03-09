using FluentValidation;
using RA.Core.Models.PluginModels.FormTypes;
using RA.FormTypes.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.FormTypes.Validators
{
    public class BaseFormModelValidator<TModel> : AbstractValidator<TModel>
        where TModel : BaseFormModel
    {
        public BaseFormModelValidator()
        {
            RuleFor(c => c.FormNbr)
                .NotEmpty()
                .WithMessage(FormTypeMessages.FormNbrNotExists);
        }
    }
}
