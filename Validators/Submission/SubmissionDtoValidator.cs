// Update to SubmissionDtoValidator.cs to enforce TE ID format
// File: Validators/Submission/SubmissionDtoValidator.cs

using FluentValidation;
using TE_Project.DTOs.Submission;
using TE_Project.Helpers;
using TE_Project.Repositories.Interfaces;
using TE_Project.Enums;

namespace TE_Project.Validators.Submission
{
    public class SubmissionDtoValidator : AbstractValidator<SubmissionDto>
    {
        public SubmissionDtoValidator(ISubmissionRepository submissionRepository)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Gender must be either Male or Female");

            RuleFor(x => x.TeId)
                .NotEmpty().WithMessage("TE ID is required")
                .MaximumLength(50).WithMessage("TE ID cannot exceed 50 characters")
                .Matches(@"^TE\d+$").WithMessage("TE ID must be in the format TE followed by numbers (e.g. TE12345)");

            RuleFor(x => x.Cin)
                .NotEmpty().WithMessage("CIN is required")
                .MaximumLength(50).WithMessage("CIN cannot exceed 50 characters")
                .Matches(RegexPatterns.CinPattern).WithMessage("CIN format is invalid")
                .MustAsync(async (cin, cancellation) => 
                    !await submissionRepository.ExistsAsync(s => s.Cin == cin)
                ).WithMessage("A submission with this CIN already exists");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required")
                .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past");

            RuleFor(x => x.PlantId)
                .InclusiveBetween(1, 5).WithMessage("Please select a valid plant (1-5)");

            RuleFor(x => x.GreyCard)
                .Matches(RegexPatterns.CGPattern).When(x => !string.IsNullOrEmpty(x.GreyCard))
                .WithMessage("Grey card number format is invalid");

            RuleFor(x => x.CinImage)
                .NotNull().WithMessage("CIN image is required")
                .Must(image => image == null || image.Length <= 1024 * 1024) // 1MB max
                .WithMessage("CIN image size must be less than 1MB");

            RuleFor(x => x.PicImage)
                .NotNull().WithMessage("Personal photo is required")
                .Must(image => image == null || image.Length <= 1024 * 1024) // 1MB max
                .WithMessage("Personal photo size must be less than 1MB");

            RuleFor(x => x.GreyCardImage)
                .NotNull().WithMessage("Grey card image is required")
                .When(x => !string.IsNullOrEmpty(x.GreyCard))
                .Must(image => image == null || image.Length <= 1024 * 1024) // 1MB max
                .WithMessage("Grey card image size must be less than 1MB");
        }
    }
}