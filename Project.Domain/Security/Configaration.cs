﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Security
{
    public class Configaration
    {
        public static SecretsConfiguration Secrets { get; set; } = new();
        public class SecretsConfiguration 
        {
            public string ApiKey { get; set; } = string.Empty;
            public string JwtPrivateKey {  get; set; } = string.Empty;
            public string PasswordSaltKey { get; set; } = string.Empty;
        }
    }
}
