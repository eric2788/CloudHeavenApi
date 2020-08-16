using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudHeavenApi.Contexts;

namespace CloudHeavenApi.Models
{
    public class User
    {
        public Guid Uuid { get; set; }
        public string UserName { get; set; }

        public string NickName { get; set; }

        public bool Admin { get; set; }
        public Badge[] Badges { get; set; }
    }
}
