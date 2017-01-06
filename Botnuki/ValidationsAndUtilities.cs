using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using System.Text;
using System.Threading.Tasks;

namespace Botnuki
{
    public class ValidationsAndUtilities
    {
        private static readonly string[] AdminRoles = { "owners", "manager", "moderators" };

        public static bool IsAuthorisedUser(Server s, User u)
        {
            foreach(var ar in AdminRoles)
            {
                var srvRoles = GetRole(s, ar);
                if (srvRoles == null) continue;
                else
                {
                    if (u.HasRole(srvRoles)) return true;
                }
            }
            return false;
        }

        public static Role GetRole(Server s, string roleContains)
            =>
            (from serverRole in s.Roles
             where serverRole.Name.ToLower().Contains(roleContains)
             select s.GetRole(serverRole.Id)).FirstOrDefault();

        public static User GetUser(Server s, string userContains)
            =>
            (from user in s.Users
             where user.Name.ToLower().Contains(userContains)
             select s.GetUser(user.Id)).FirstOrDefault();
    }
}
