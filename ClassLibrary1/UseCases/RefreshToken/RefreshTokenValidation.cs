using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.RefreshToken
{
    public class RefreshTokenValidation : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenValidation() 
        {
            RuleFor(x => x.RefreshToken).NotEmpty().NotNull();
        }
    }
}
