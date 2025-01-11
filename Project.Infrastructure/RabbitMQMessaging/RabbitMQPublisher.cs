using Project.Application.DTOs;
using Project.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.RabbitMQMessaging
{
    public class RabbitMQPublisher : IRabbitPublisher
    {
        public void SendMessage(object obj)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendMessage(UserResponseDTO transaction)
        {
            throw new NotImplementedException();
        }
    }
}
