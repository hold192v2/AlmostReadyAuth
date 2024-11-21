using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.Authentication
{
    public class AuthenticationValidator : AbstractValidator<AuthenticationRequest>
    {
        public AuthenticationValidator() 
        { 
            RuleFor(x => x.Phone).MinimumLength(3).MaximumLength(100).NotEmpty();
            RuleFor(x => x.Password).MinimumLength(3).MaximumLength(100).NotEmpty();

        }
    }
}
