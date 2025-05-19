using FluentValidation;
using Safahat.Application.DTOs.Requests.Tags;

namespace Safahat.Application.Validators.Tags;

public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters");

        RuleFor(x => x.Slug)
            .MaximumLength(50).WithMessage("Slug cannot exceed 50 characters")
            .Matches("^[a-z0-9-]*$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.Slug));
    }
}