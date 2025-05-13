using FluentValidation;
using TE_Project.DTOs.Auth;

namespace TE_Project.Validators.Auth
{
    public class RegisterAdminDtoValidator : AbstractValidator<RegisterAdminDto>
    {
        public RegisterAdminDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");

            RuleFor(x => x.TEID)
                .NotEmpty().WithMessage("TE ID is required")
                .Matches(@"^[A-Za-z0-9_\-]+$").WithMessage("TE ID contains invalid characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .When(x => !string.IsNullOrEmpty(x.Password))
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
                .Matches(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.PlantId)
                .InclusiveBetween(1, 5).WithMessage("Please select a valid plant (1-5)");
        }
    }
}