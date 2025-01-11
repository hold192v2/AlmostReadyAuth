using Project.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Interfaces
{
    public interface IRabbitPublisher
    {
        void SendMessage(object obj);
        Task<bool> SendMessage(UserResponseDTO transaction);
    }
}
