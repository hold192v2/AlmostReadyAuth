using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.TelegramBot
{
    public class TelegramBotValidator : AbstractValidator<TelegramBotRequest>
    {
        public TelegramBotValidator() 
        {
            {
                RuleFor(x => x.Update);
                RuleFor(x => x.SecretToken);
            }
        }
    }
}
