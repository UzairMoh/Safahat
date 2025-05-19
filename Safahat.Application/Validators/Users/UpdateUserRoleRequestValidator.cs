using FluentValidation;
using Safahat.Application.DTOs.Requests.Users;
using Safahat.Models.Enums;

namespace Safahat.Application.Validators.Users;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role. Role must be a valid value.")
            .NotEqual(UserRole.Reader).WithMessage("Cannot set role to Reader. Please select Author or Admin.");
    }
}