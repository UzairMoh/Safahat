using FluentValidation;
using Safahat.Application.DTOs.Requests.Comments;

namespace Safahat.Application.Validators.Comments;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("PostId must be a positive number");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required")
            .MaximumLength(1000).WithMessage("Comment content cannot exceed 1000 characters");
    }
}