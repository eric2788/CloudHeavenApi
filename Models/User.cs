using System;
using System.Linq;
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
            Badges = account.PersonBadgeses?.Where(p => p.Uuid == Uuid).Select(p => p.Badge).ToArray() ?? new Badge[0];
        }

        public User()
        {
        }

        public Guid Uuid { get; set; }
        public string UserName { get; set; }

        public string NickName { get; set; }

        public bool Admin { get; set; }
        public Badge[] Badges { get; set; }
    }
}