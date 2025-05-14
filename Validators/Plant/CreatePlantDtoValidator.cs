using FluentValidation;
using TE_Project.DTOs.Plant;

namespace TE_Project.Validators.Plant
{
    public class CreatePlantDtoValidator : AbstractValidator<CreatePlantDto>
    {
        public CreatePlantDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Plant name is required")
                .MaximumLength(100).WithMessage("Plant name cannot exceed 100 characters");
                
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}