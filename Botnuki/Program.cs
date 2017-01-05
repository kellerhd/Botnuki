using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using System.Text;
using System.IO;

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
                //variable declaration
                var command = e.Message.RawText.ToLower();
                var clist = getArray(command);
                int successfulAttempts = 0, unsuccessfulAttempts = 0;
                Server server = e.Server;

                // command search
                if (command.StartsWith("/roleadd"))
                {
                    // remove first 
                    clist.Remove("/roleadd");

                    if (clist.Count > 0)
                    {
                        string roleList = string.Empty, roleFail = string.Empty;
                        foreach (string s in clist)
                        {
                            if (!ValidRoles.Contains(s)) continue;

                            var srvRole = GetRole(server, s);
                            if (srvRole == null)
                            {
                                roleFail += $"• {srvRole}\r\n";
                                unsuccessfulAttempts++;
                                continue;
                            }

                                e.Message.User.AddRoles(srvRole);
                                successfulAttempts++;
                                roleList += $"• {srvRole}\r\n";
                                continue;
                        }

                        if (unsuccessfulAttempts > 0)
                            e.Channel.SendMessage($"{e.User.Mention} I could not grant some roles for one of the following reasons:\r\n• You have the role(s) already\r\n•The role(s) do not exist\r\n• I can't grant you the role(s)");
                        if (successfulAttempts > 0)
                            e.Channel.SendMessage($"There you go { e.User.Mention }! The following role(s) have been added: \r\n{roleList.Substring(0,1).ToUpper() + roleList.Substring(1)}");
                    }
                    else
                        e.Channel.SendMessage($@"The /roleadd command will allow you to request certain roles that are available to you. The current roles available are as follows: Smite, League, Paladins, Battlerite");
                }
                else if (command.StartsWith("/roleremove"))
                {
                    // remove first 
                    clist.Remove("/roleremove");

                    if (clist.Count > 0)
                    {
                        string roleList = string.Empty, roleFail = string.Empty;
                        foreach (string s in clist)
                        {
                            if (!ValidRoles.Contains(s)) continue;

                            var srvRole = GetRole(server, s);
                            if (srvRole == null)
                            {
                                roleFail += $"• {srvRole}\r\n";
                                unsuccessfulAttempts++;
                                continue;
                            }

                                e.Message.User.RemoveRoles(srvRole);
                                successfulAttempts++;
                                roleList += $"• {srvRole}\r\n";
                        }

                        if (unsuccessfulAttempts > 0)
                            e.Channel.SendMessage($"{e.User.Mention } I could not complete for one of the following reasons:\r\n•The role(s) do not exist\r\n• I can't remove the role(s)");
                        if (successfulAttempts > 0)
                            e.Channel.SendMessage($"There you go { e.User.Mention }! The following role(s) have been removed: \r\n{roleList.Substring(0, 1).ToUpper() + roleList.Substring(1)}");
                    }
                    else
                        e.Channel.SendMessage("The /roleremove command will allow you to request certain roles that are available to you to be removed. The current roles available are as follows: Smite, League, Paladins, Battlerite");
                }
                else if (command.StartsWith("/entergiveaway"))
                {
                        // break if the giveaway is not active
                        if (activeGiveaway == false)
                        e.Channel.SendMessage("There is no active giveaway at the moment!");
                    else
                    {
                        StringBuilder sr = new StringBuilder(string.Empty);
                        string path = AppDomain.CurrentDomain.BaseDirectory;
                        path = path.Substring(0, path.Length - 10);
                        sr.Append(File.ReadAllText($@"{path}giveawayParticipants.txt"));
                        
                        var user = e.User;
                        // store the entering user's name on the giveaway list in a string
                        if (!giveawayParticipants.Contains(user.Name))
                        {
                            giveawayParticipants.Add(user.Name);
                            e.Channel.SendMessage($"Thank you {user.Mention}! Your entry has been recorded!");
                            sr.Append($"{user.Name},");
                            File.WriteAllText($@"{path}giveawayParticipants.txt", sr.ToString());
                        }
                        else
                        {
                            e.Channel.SendMessage($"You have already entered {user.Mention}!");
                        }
                    }
                }
                else if (command.StartsWith("/drawwinners"))
                {
                    StringBuilder sr = new StringBuilder(string.Empty);
                    string path = AppDomain.CurrentDomain.BaseDirectory;
                    path = path.Substring(0, path.Length - 10);
                    sr.Append(File.ReadAllText($@"{path}giveawayParticipants.txt"));

                    giveawayParticipants = sr.ToString().Split(',').ToList();

                    // first make sure there is an active giveaway
                    if (activeGiveaway == false)
                        e.Channel.SendMessage("There is no active giveaway at the moment!");
                    else if (giveawayParticipants.Count == 0)
                        e.Channel.SendMessage("There are no participants in this giveaway!");
                    else
                    {
                        
                        // next, remove the first part of the command string
                        clist.Remove("/drawwinners");

                        if (clist.Count > 0)
                        {
                            // then, make sure the user invoking this message is a manager or an owner
                            foreach (string s in AdminRoles)
                            {
                                var srvRoles = GetRole(server, s);
                                if (srvRoles == null) continue;

                                if (e.User.HasRole(srvRoles))
                                {
                                    int j = Convert.ToInt32(clist[0]);

                                    j = (j == 0) ? 1 : j;

                                    string results = "The results are in! The winners are as follows!\r\n";

                                    for (int i = 0; i < j; i++)
                                    {
                                        r = rng.Next(giveawayParticipants.Count);

                                        results += $"{ ((i + 1).ToString())}. {giveawayParticipants[r]}\r\n";
                                        giveawayParticipants.RemoveAt(r);
                                    }

                                    giveawayParticipants.Clear();
                                    File.WriteAllText($@"{path}giveawayParticipants.txt", string.Empty);
                                    e.Channel.SendMessage(results);

                                }
                            }
                        }
                        else
                            e.Channel.SendMessage("Enter the number of winners for this giveaway!");
                    }
                }
            }
            catch(Exception ex)
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
