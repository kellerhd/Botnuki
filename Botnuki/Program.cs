using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Discord;
using Discord.Commands;

namespace Botnuki
{
    class Program
    {
        // class wide variables
        static string file = "Program.cs", eventMethod;
        private static readonly string[] ValidRoles = { "league", "smite", "paladins", "battlerite", "ark" }, AdminRoles = { "owners", "manager" };
        static bool activeGiveaway = true;
        static List<String> giveawayParticipants = new List<String>();
        static Random rng = new Random();
        static int r;

        static void Main(string[] args)
        {
            // method vars
            var bot = new DiscordClient();

            bot.MessageReceived += bot_MessageReceived;

            bot.ExecuteAndWait(async () =>
           {
               string path = AppDomain.CurrentDomain.BaseDirectory;
               path = path.Substring(0, path.Length - 10);
               await bot.Connect(File.ReadAllText($@"{path}inukitvsrvid.txt"), TokenType.Bot);
           }
            );
        }



        static void bot_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                // method variables
                var command = e.Message.RawText.ToLower();
                bool isAuthorisedUser = ValidationsAndUtilities.IsAuthorisedUser(e.Server, e.User);
                var param = command.Split(' ').ToList();
                User u = null;
                StringBuilder sr = new StringBuilder(string.Empty);
                string path = AppDomain.CurrentDomain.BaseDirectory;
                path = path.Substring(0, path.Length - 10);
                Server server = e.Server;
                string roleList = string.Empty, roleFail = string.Empty;
                int successfulAttempts = 0, unsuccessfulAttempts = 0;

                if (isAuthorisedUser == true)
                {
                    foreach (var p in param)
                    {
                        if (p.Substring(0, 1).Contains(@"@"))
                        {
                            u = ValidationsAndUtilities.GetUser(e.Server, p);
                            param.Remove(p);
                            break;
                        }

                        if (u == null) u = e.User;
                    }
                }
                else u = e.User;

                // command search
                
                switch (param[0])
                {
                    case "/roleadd":
                        foreach (var role in param)
                        {
                            var r = role.ToLower();

                            if (!ValidRoles.Contains(r)) continue;

                            var srvRole = ValidationsAndUtilities.GetRole(server, r);
                            if (srvRole == null)
                            {
                                roleFail += $"• {srvRole}\r\n";
                                unsuccessfulAttempts++;
                                continue;
                            }

                            u.AddRoles(srvRole);
                            successfulAttempts++;
                            roleList += $"• {srvRole}\r\n";
                            continue;
                        }

                        if (unsuccessfulAttempts > 0)
                            e.Channel.SendMessage($"{u.Mention} I could not grant some roles for one of the following reasons:\r\n• You have the role(s) already\r\n•The role(s) do not exist\r\n• I can't grant you the role(s)");
                        if (successfulAttempts > 0)
                            e.Channel.SendMessage($"There you go { u.Mention }! The following role(s) have been added: \r\n{roleList.Substring(0, 1).ToUpper() + roleList.Substring(1)}");
                        break;
                    case "/roleremove":
                        foreach (var role in param)
                        {
                            var r = role.ToLower();

                            if (!ValidRoles.Contains(r)) continue;

                            var srvRole = ValidationsAndUtilities.GetRole(e.Server, r);
                            if (srvRole == null)
                            {
                                roleFail += $"• {srvRole}\r\n";
                                unsuccessfulAttempts++;
                                continue;
                            }

                            u.RemoveRoles(srvRole);
                            successfulAttempts++;
                            roleList += $"• {srvRole}\r\n";
                            continue;
                        }
                        if (unsuccessfulAttempts > 0)
                            e.Channel.SendMessage($"{u.Mention } I could not complete for one of the following reasons:\r\n•The role(s) do not exist\r\n• I can't remove the role(s)");
                        if (successfulAttempts > 0)
                            e.Channel.SendMessage($"There you go { u.Mention }! The following role(s) have been removed: \r\n{roleList.Substring(0, 1).ToUpper() + roleList.Substring(1)}");
                        break;
                    case "/entergiveaway":
                        // break if the giveaway is not active
                        if (activeGiveaway == false)
                            e.Channel.SendMessage("There is no active giveaway at the moment!");
                        else
                        {
                            
                            sr.Append(File.ReadAllText($@"{path}giveawayParticipants.txt"));

                            // store the entering user's name on the giveaway list in a string
                            if (!giveawayParticipants.Contains(u.Name))
                            {
                                giveawayParticipants.Add(u.Name);
                                e.Channel.SendMessage($"Thank you {u.Mention}! Your entry has been recorded!");
                                sr.Append($"{u.Name},");
                                File.WriteAllText($@"{path}giveawayParticipants.txt", sr.ToString());
                            }
                            else
                            {
                                e.Channel.SendMessage($"You have already entered {u.Mention}!");
                            }
                        }
                        break;
                    case "/selectwinners":
                        
                        sr.Append(File.ReadAllText($@"{path}giveawayParticipants.txt"));

                        giveawayParticipants = sr.ToString().Split(',').ToList();

                        // first make sure there is an active giveaway
                        if (activeGiveaway == false)
                            e.Channel.SendMessage("There is no active giveaway at the moment!");
                        else if (giveawayParticipants.Count == 0)
                            e.Channel.SendMessage("There are no participants in this giveaway!");
                        else
                        {
                            // then, make sure the user invoking this message is a manager or an owner
                            foreach (string s in AdminRoles)
                            {
                                var srvRoles = ValidationsAndUtilities.GetRole(e.Server, s);
                                if (srvRoles == null) continue;

                                if (e.User.HasRole(srvRoles))
                                {
                                    int j = Convert.ToInt32(param[0]);

                                    j = (j == 0) ? 1 : j;

                                    string results = "The results are in! The winners are as follows!\r\n";

                                    for (int i = 0; i < j; i++)
                                    {
                                        r = rng.Next(giveawayParticipants.Count);

                                        results += $"{ ((i + 1).ToString())}. {giveawayParticipants[r]}\r\n";
                                        giveawayParticipants.RemoveAt(r);
                                    }

                                    e.Channel.SendMessage(results);

                                }
                            }
                        }
                        break;
                    case "/resetlist":
                        giveawayParticipants.Clear();
                        File.WriteAllText($@"{path}giveawayParticipants.txt", string.Empty);

                        e.Channel.SendMessage("Giveaway entrants list reset!");
                        break;
                    default:
                        break;
                }
                
            }
            catch (Exception ex)
            {
                eventMethod = "bot_MessageReceived";
                Console.Write(ErrorHandling.ThrowGenException(file, eventMethod, ex.Message));
                e.Channel.SendMessage(ErrorHandling.ThrowGenException(file, eventMethod, ex.Message));

                string path = AppDomain.CurrentDomain.BaseDirectory;
                path = path.Substring(0, path.Length - 10);
                var me = e.Server.GetUser(UInt64.Parse(File.ReadAllText($@"{path}myid.txt")));
                me.SendMessage(ErrorHandling.ThrowGenException(file, eventMethod, ex.Message));
            }
        }

    }
}
