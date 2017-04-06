using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using csgo_overwatch_vdm.vdm;
using DemoInfo;

namespace csgo_overwatch_vdm
{
    internal class Program
    {
        private static bool _liststeamids;
        private static string _steamid;
        private static int _round;

        private const int DEMO_SPEED_NORMAL = 1;
        private const int DEMO_SPEED_FASTFORWARD = 4;

        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintHelp();

                Console.ReadKey(); // TODO: remove
                return;
            }

            var fileArgument = -1;
            if (args.Length > 1)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];

                    if (arg.StartsWith("-") || arg.StartsWith("/"))
                    {
                        if (arg.Substring(1).ToLower().Equals("liststeamids"))
                        {
                            _liststeamids = true;
                        }
                        else if (arg.Substring(1).ToLower().Equals("steamid"))
                        {
                            _steamid = args[++i]; // get the next argv (make the loop jump to the next arg)
                        }
                    }
                    else if (fileArgument == -1) // not set yet
                    {
                        fileArgument = i;
                    }
                }
            }

            var fileName = args[fileArgument];

            if (_liststeamids)
            {
                PrintSteamIdList(fileName);

                Console.ReadKey(); // TODO: remove
                return;
            }

            if (string.IsNullOrEmpty(_steamid))
            {
                Console.WriteLine("[ERROR] steamid parameter is empty !\n");
                PrintHelp();
                Console.ReadKey(); // TODO: remove
                return;
            }

            if (!Regex.IsMatch(_steamid, @"^\d{17}$"))
            {
                Console.WriteLine("[ERROR] The SteamId provided is invalid (should be a number of 17 digits) !\n");
                PrintHelp();
                Console.ReadKey(); // TODO: remove
                return;
            }

            using (var fileStream = File.OpenRead(fileName))
            {
                Console.WriteLine("Parsing demo {0}", fileName);

                using (var parser = new DemoParser(fileStream))
                {
                    parser.ParseHeader();

                    parser.BotTakeOver += (sender, e) =>
                    {
                        if (_steamid.Equals(e.Taker.SteamID.ToString()))
                        {
                            var tick = ((DemoParser)sender).IngameTick;
                            var botName = "a bot"; // The lib does not send the Bot along with the event.
                            Console.WriteLine("[{0}] {1} took over {2}.", tick, e.Taker.Name, botName);

                            VdmGenerator.Add(new PlayCommandsAction
                            {
                                StartTick = tick,
                                Commands = "demo_timescale " + DEMO_SPEED_NORMAL
                            });
                        }
                    };

                    parser.PlayerKilled += (sender, e) =>
                    {
                        var tick = ((DemoParser) sender).IngameTick;
                        var killerName = e?.Killer?.Name ?? "[Someone]";
                        var victimName = e?.Victim?.Name;
                        if (victimName != null)
                        {
                            Console.WriteLine("[{0}] {1} killed {2}.", tick, killerName, victimName);
                        }
                        else
                        {
                            Console.WriteLine("[{0}] {1} killed himself.", tick, killerName);
                        }

                        var victimSteamId = e?.Victim?.SteamID.ToString();
                        if (victimSteamId == null) return; // Should not be possible (broken event if so)

                        var theParser = (DemoParser) sender;
                        var oponentTeam = GetOponentTeam(e.Killer);

                        if (!theParser.PlayingParticipants.Any(p => p.Team == oponentTeam && p.IsAlive) // If it was the last oponent alive
                            || victimSteamId.Equals(_steamid)) // or if it was our player
                        {
                            // Fast forward
                            VdmGenerator.Add(new PlayCommandsAction
                            {
                                StartTick = tick,
                                Commands = "demo_timescale " + DEMO_SPEED_FASTFORWARD
                            });
                        }
                    };

                    parser.RoundStart += (sender, e) =>
                    {
                        var tick = ((DemoParser) sender).IngameTick;
                        _round++;
                        Console.WriteLine("[{0}] The round #{1} has started.", tick, _round);

                        VdmGenerator.Add(new PlayCommandsAction
                        {
                            StartTick = tick,
                            Commands = "spec_player_by_accountid " + _steamid
                        });
                    };

                    parser.FreezetimeEnded += (sender, e) =>
                    {
                        var tick = ((DemoParser) sender).IngameTick;
                        Console.WriteLine("[{0}] Freeztime ended", tick);

                        VdmGenerator.Add(new PlayCommandsAction
                        {
                            StartTick = tick,
                            Commands = "demo_timescale " + DEMO_SPEED_NORMAL
                        });
                    };

                    parser.RoundEnd += (sender, e) =>
                    {
                        var tick = ((DemoParser) sender).IngameTick;
                        Console.WriteLine("[{0}] The round #{1} is over.", tick, _round);
                        VdmGenerator.Add(new PlayCommandsAction
                        {
                            StartTick = tick,
                            Commands = "demo_timescale " + DEMO_SPEED_FASTFORWARD
                        });
                    };

                    parser.ParseToEnd();
                }
            }

            var vdmFile = fileName.Replace(".dem", ".vdm");
            VdmGenerator.Generate(vdmFile);

            Console.ReadKey(); // TODO: remove
        }

        private static void PrintSteamIdList(string fileName)
        {
            using (var fileStream = File.OpenRead(fileName))
            {
                Console.WriteLine("==== List of the steamids of the players ====");
                using (var parser = new DemoParser(fileStream))
                {
                    parser.ParseHeader();
                    parser.ParseToEnd();
                    foreach (var player in parser.PlayingParticipants)
                    {
                        Console.WriteLine(" - {0}  {1}", player.SteamID, player.Name);
                    }
                }
            }
        }

        private static Team GetOponentTeam(Player p)
        {
            return p != null && p.Team == Team.CounterTerrorist
                ? Team.Terrorist : Team.CounterTerrorist;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("USAGE: csgo-overwatch-vdm filename.dem");
            Console.WriteLine("optional arguments:");
            Console.WriteLine(" -steamid <steamid>   SteamId of the player to generate the vdm for.");
            Console.WriteLine(" -liststeamids        List the SteamId of the players in the match.");
        }
    }
}