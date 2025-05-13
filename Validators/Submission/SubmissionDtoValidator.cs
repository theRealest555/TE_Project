using FluentValidation;
using TE_Project.DTOs.Submission;
using TE_Project.Helpers;

namespace TE_Project.Validators.Submission
{
    public class SubmissionDtoValidator : AbstractValidator<SubmissionDto>
    {
        public SubmissionDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");

            RuleFor(x => x.TeId)
                .NotEmpty().WithMessage("TE ID is required")
                .MaximumLength(50).WithMessage("TE ID cannot exceed 50 characters");

            RuleFor(x => x.Cin)
                .NotEmpty().WithMessage("CIN is required")
                .MaximumLength(50).WithMessage("CIN cannot exceed 50 characters")
                .Matches(RegexPatterns.CinPattern).WithMessage("CIN format is invalid");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required")
                .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past");

            RuleFor(x => x.PlantId)
                .InclusiveBetween(1, 5).WithMessage("Please select a valid plant (1-5)");

            RuleFor(x => x.GreyCard)
                .Matches(RegexPatterns.CGPattern).When(x => !string.IsNullOrEmpty(x.GreyCard))
                .WithMessage("Grey card number format is invalid");

            RuleFor(x => x.CinImage)
                .NotNull().WithMessage("CIN image is required");

            RuleFor(x => x.PicImage)
                .NotNull().WithMessage("Personal photo is required");

            RuleFor(x => x.GreyCardImage)
                .NotNull().WithMessage("Grey card image is required")
                .When(x => !string.IsNullOrEmpty(x.GreyCard));
        }
    }
}