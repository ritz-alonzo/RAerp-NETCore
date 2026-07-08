using FluentValidation;
using RAerp.Models.ApplicationSettingsModel;

namespace RAerp.Validators
{
    public class ApplicationSettingValidator : AbstractValidator<ApplicationSettingModel>
    {
        public ApplicationSettingValidator()
        {
            RuleFor(x => x.ApplicationName)
                .NotEmpty()
                .WithMessage("Application Name is required")
                .MaximumLength(200)
                .WithMessage("Application Name cannot exceed 200 characters");

            RuleFor(x => x.ClientId)
                .MaximumLength(500)
                .WithMessage("Client ID cannot exceed 500 characters");

            RuleFor(x => x.ClientSecret)
                .MaximumLength(500)
                .WithMessage("Client Secret cannot exceed 500 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}