using Project.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.HadlerResponce
{
    public class Response
    {
        public string Message { get; set; }
        public int Status { get; set; }
        public EndResponse? Data { get; set; }
        public Response(string message, int status) 
        {
            Message = message; 
            Status = status;
        }
        public Response(string message, int status, EndResponse? data)
        {
            Message = message;
            Status = status;
            Data = data;
        }
    }
}
