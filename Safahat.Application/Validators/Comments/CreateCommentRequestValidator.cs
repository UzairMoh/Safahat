using FluentValidation;
using Safahat.Application.DTOs.Requests.Comments;

namespace Safahat.Application.Validators.Comments;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId is required");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required")
            .MaximumLength(1000).WithMessage("Comment content cannot exceed 1000 characters");

        RuleFor(x => x.ParentCommentId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("ParentCommentId must be a valid Guid when provided");
    }
}