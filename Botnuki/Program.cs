using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Botnuki
{
    class Program
    {
        // class wide variables
        static string file = "Program.cs";
        static string eventMethod;
        private static readonly string[] ValidRoles = { "league", "smite", "paladins", "battlerite", "ark" };

        static void Main(string[] args)
        {
            var bot = new DiscordClient();

            bot.MessageReceived += bot_MessageReceived;

            bot.ExecuteAndWait(async () =>
           {
               string path = AppDomain.CurrentDomain.BaseDirectory;
               path = path.Substring(0, path.Length - 10);
               await bot.Connect(System.IO.File.ReadAllText($@"{path}inukitvsrvid.txt"), TokenType.Bot);
           }
            );
        }



        static void bot_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                //variable declaration
                var command = e.Message.RawText.ToLower();
                var clist = getArray(command);
                int successfulAttempts = 0;
                Server server = e.Server;

                // command search
                if (command.StartsWith("/roleadd"))
                {
                    // remove first 
                    clist.Remove("/roleadd");

                    if (clist.Count > 0)
                    {
                        foreach (string s in clist)
                        {
                            if (!ValidRoles.Contains(s)) continue;

                            var srvRole = GetRole(server, s);
                            if (srvRole == null) continue;

                            e.Message.User.AddRoles(srvRole);
                            successfulAttempts++;
                        }

                        if (successfulAttempts == 0)
                            e.Channel.SendMessage(e.User.Mention + " it looks like the role(s) you requested either don't exist or aren't a role I can manage :(");
                        else
                            e.Channel.SendMessage("There you go " + e.User.Mention + "! Your " + successfulAttempts + " role(s) have been added!");
                    }
                    else
                        e.Channel.SendMessage("The /roleadd command will allow you to request certain roles that are available to you. The current roles available are as follows: Smite, League, Paladins, Battlerite");
                }
                else if (command.StartsWith("/roleremove"))
                {
                    // remove first 
                    clist.Remove("/roleremove");

                    if (clist.Count > 0)
                    {
                        foreach (string s in clist)
                        {
                            if (!ValidRoles.Contains(s)) continue;

                            var srvRole = GetRole(server, s);
                            if (srvRole == null) continue;

                            e.Message.User.RemoveRoles(srvRole);
                            successfulAttempts++;
                        }

                        if (successfulAttempts == 0)
                            e.Channel.SendMessage(e.User.Mention + " it looks like the role(s) you requested to be removed either don't exist or aren't a role I can manage :(");
                        else
                            e.Channel.SendMessage("There you go " + e.User.Mention + "! Your " + successfulAttempts + " role(s) have been removed!");
                    }
                    else
                        e.Channel.SendMessage("The /roleremove command will allow you to request certain roles that are available to you to be removed. The current roles available are as follows: Smite, League, Paladins, Battlerite");
                }
            }
            catch(Exception ex)
            {
                eventMethod = "bot_MessageReceived";
                ErrorHandling.ThrowGenException(file, eventMethod, ex.Message);
            }
        }

        static List<String> getArray(string command)
        {
            var array = command.Split(' ');
            return array.ToList(); 
        }

        private static Discord.Role GetRole(Server s, string roleContains)
            =>
            (from serverRole in s.Roles
             where serverRole.Name.ToLower().Contains(roleContains)
             select s.GetRole(serverRole.Id)).FirstOrDefault();
    }
}
