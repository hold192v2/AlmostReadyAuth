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
            RuleFor(x => x.Phone).MinimumLength(12).MaximumLength(12).NotEmpty();
            RuleFor(x => x.Code).MinimumLength(6).MaximumLength(6);

        }
    }
}
