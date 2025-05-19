using FluentValidation;
using Safahat.Application.DTOs.Requests.Posts;

namespace Safahat.Application.Validators.Posts;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("Summary cannot exceed 500 characters");

        RuleFor(x => x.FeaturedImageUrl)
            .MaximumLength(255).WithMessage("Featured image URL cannot exceed 255 characters")
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Featured image URL must be a valid URL");

        RuleFor(x => x.CategoryIds)
            .NotNull().WithMessage("CategoryIds cannot be null");

        RuleFor(x => x.Tags)
            .NotNull().WithMessage("Tags cannot be null");

        RuleForEach(x => x.Tags)
            .MaximumLength(50).WithMessage("Tag cannot exceed 50 characters");
    }
}