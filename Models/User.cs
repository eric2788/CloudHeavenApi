using System;
using CloudHeavenApi.Contexts;

namespace CloudHeavenApi.Models
{
    public class User
    {
        public User(WebAccount account)
        {
            Uuid = account.Uuid;
            UserName = account.UserName;
            NickName = account.NickName;
            Admin = account.Admin;
        }

        public User()
        {
        }

        public Guid Uuid { get; set; }
        public string UserName { get; set; }

        public string NickName { get; set; }

        public bool Admin { get; set; }
    }
}