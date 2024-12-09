using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class BotInputData : BaseEntity
    {
        public string UserIP { get; set; } = string.Empty;
        public string InputPhone { get; set; } = string.Empty;
        public BotInputData()
        {

        }
        public BotInputData(string userIP, string inputPhone) 
        {
            UserIP = userIP;
            InputPhone = inputPhone;
        }
    }
}
