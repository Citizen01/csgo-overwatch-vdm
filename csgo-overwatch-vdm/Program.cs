using System;
using System.IO;
using csgo_overwatch_vdm.vdm;
using DemoInfo;

namespace csgo_overwatch_vdm
{
    class Program
    {
        static bool _listxuids;
        static string _xuid;
        static int _round;

        static void Main(string[] args)
        {
            Console.SetOut(Console.Out);
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
                        if (arg.Substring(1).ToLower().Equals("listxuids"))
                        {
                            _listxuids = true;
                        }
                        else if (arg.Substring(1).ToLower().Equals("xuid"))
                        {
                            _xuid = args[++i]; // get the next argv (make the loop jump to the next arg)
                        }
                    }
                    else if (fileArgument == -1) // not set yet
                    {
                        fileArgument = i;
                    }
                }
            }

            var fileName = args[fileArgument];

            if (_listxuids)
            {
                PrintXuidList(fileName);

                Console.ReadKey(); // TODO: remove
                return;
            }


            if (string.IsNullOrEmpty(_xuid))
            {
                Console.WriteLine("[ERROR] xuid parameter is empty !\n");
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
                        var tick = ((DemoParser) sender).CurrentTick;
                        var botName = "a bot"; // The lib does not send the Bot along with the event.
                        Console.WriteLine("{0} took over {1}.", e.Taker.Name, botName);

                        if (_xuid.Equals(e.Taker.SteamID.ToString()))
                        {
                            VdmGenerator.Add(new PlayCommandsAction
                            {
                                StartTick = tick,
                                Commands = "demo_timescale 1"
                            });
                        }
                    };

                    parser.PlayerKilled += (sender, e) =>
                    {
                        var tick = ((DemoParser) sender).CurrentTick;
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

                        if (e?.Victim != null && _xuid.Equals(e.Victim.SteamID.ToString()))
                        {
                            VdmGenerator.Add(new PlayCommandsAction
                            {
                                StartTick = tick,
                                Commands = "demo_timescale 4"
                            });
                        }
                    };

                    parser.RoundStart += (sender, e) =>
                    {
                        var tick = ((DemoParser) sender).CurrentTick;
                        _round++;
                        Console.WriteLine("[{0}] The round #{1} has started.", tick, _round);
                    };

                    parser.FreezetimeEnded += (sender, e) =>
                    {
                        var tick = ((DemoParser) sender).CurrentTick;
                        Console.WriteLine("[{0}] Freeztime ended", tick, _round);
                        VdmGenerator.Add(new PlayCommandsAction
                        {
                            StartTick = tick,
                            Commands = "demo_timescale 1"
                        });
                    };

                    parser.RoundEnd += (sender, e) =>
                    {
                        var tick = ((DemoParser) sender).CurrentTick;
                        Console.WriteLine("[{0}] The round #{1} is over.", tick, _round);
                        VdmGenerator.Add(new PlayCommandsAction
                        {
                            StartTick = tick,
                            Commands = "demo_timescale 4"
                        });
                    };

                    parser.ParseToEnd();
                }
            }

            Console.ReadKey(); // TODO: remove
        }

        private static void PrintXuidList(string fileName)
        {
            using (var fileStream = File.OpenRead(fileName))
            {
                Console.WriteLine("==== List of the xuids of the players ====");
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

        private static void PrintHelp()
        {
            Console.WriteLine("USAGE: csgo-overwatch-vdm filename.dem");
            Console.WriteLine("optional arguments:");
            Console.WriteLine(" -xuid <xuid>    xuid of the player to generate the vdm for.");
            Console.WriteLine(" -listxuids      list the xuid of the players in the match.");
        }
    }
}