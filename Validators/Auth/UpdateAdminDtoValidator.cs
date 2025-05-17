using FluentValidation;
using TE_Project.DTOs.Auth;

namespace TE_Project.Validators.Auth
{
    public class UpdateAdminDtoValidator : AbstractValidator<UpdateAdminDto>
    {
        public UpdateAdminDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");

            RuleFor(x => x.TEID)
                .Matches(@"^TE\d+$").WithMessage("TE ID must be in the format TE followed by numbers (e.g. TE12345)")
                .When(x => !string.IsNullOrEmpty(x.TEID));
                
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.PlantId)
                .InclusiveBetween(1, 5).WithMessage("Please select a valid plant (1-5)");
        }
    }
}