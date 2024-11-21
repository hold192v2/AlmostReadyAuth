using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.Create
{
    public class CreateUserValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserValidator() 
        {
            RuleFor(x => x.RoleIds).NotEmpty().NotNull();
            RuleFor(x => x.Phone).NotEmpty().MinimumLength(3).MaximumLength(100);  
            RuleFor(x => x.Password).NotEmpty().MinimumLength(3).MaximumLength(100);
        }
    }
}
