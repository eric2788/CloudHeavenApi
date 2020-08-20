using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudHeavenApi.Models
{
    public class Identity
    {
        public Guid UUID { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
    }
}
