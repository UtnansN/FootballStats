﻿using FootballStats.Entities;
using FootballStats.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats
{
    class EntityAssembler
    {

        private StatsContext dbContext;

        public EntityAssembler(StatsContext context)
        {
            dbContext = context;
        }

        public void AssembleAndAddEntry(JObject gameInfo)
        {
            Game game = new Game
            {
                Date = DateTime.ParseExact((string)gameInfo["Laiks"], "yyyy/MM/dd", CultureInfo.InvariantCulture),
                ViewerCount = (int)gameInfo["Skatitaji"],
                Place = (string)gameInfo["Vieta"]
            };

            // Link teams to game
            List<JToken> teamTokens = gameInfo["Komanda"].Children().ToList();
            if (teamTokens.Count != 2) throw new InvalidDataException();

            if (teamTokens.Any(t => PlayedOnGameDate(game, t)))
            {
               throw new GamePlayedException("Team has already played game on this date.");
            }
            teamTokens.ForEach(t => ProcessTeam(game, t));

            // Add lead judge to game
            JudgeGame leadJudgeGame = CreateJudgeEntry(gameInfo["VT"], JudgeType.Lead);
            leadJudgeGame.Game = game;
            dbContext.JudgeGames.Add(leadJudgeGame);

            // Add regular judges to game
            List<JToken> regularJudgeTokens = gameInfo["T"].Children().ToList();
            regularJudgeTokens.ForEach(t => {
                JudgeGame entry = CreateJudgeEntry(t, JudgeType.Regular);
                entry.Game = game;
                dbContext.JudgeGames.Add(entry);
            });

            dbContext.Games.Add(game);
        }

        private bool PlayedOnGameDate(Game game, JToken token)
        {
            Team team = dbContext.Teams.Local.Where(t => t.Name == (string)token["Nosaukums"]).FirstOrDefault();
            return team != null && team.Matchups.Any(tp => tp.Game.Date == game.Date);
        }

        private void ProcessTeam(Game game, JToken teamToken)
        {
            string teamName = (string)teamToken["Nosaukums"];

            Team team = dbContext.Teams.Local.Where(t => t.Name == (string)teamToken["Nosaukums"]).FirstOrDefault();
            if (team == null)
            {
                // Create new team entry
                team = new Team
                {
                    Name = teamName
                };

                // Add players to team
                List<JToken> playerTokens = teamToken["Speletaji"]["Speletajs"].Children().ToList();
                foreach (var token in playerTokens)
                {
                    PlayerRole role;
                    switch ((string)token["Loma"])
                    {
                        case "V":
                            role = PlayerRole.Goalkeeper;
                            break;
                        case "U":
                            role = PlayerRole.Attacker;
                            break;
                        case "A":
                            role = PlayerRole.Defender;
                            break;
                        default:
                            throw new InvalidDataException();
                    }

                    Player player = new Player
                    {
                        Role = role,
                        Number = (int)token["Nr"],
                        Name = (string)token["Vards"],
                        Surname = (string)token["Uzvards"]
                    };

                    team.Players.Add(player);
                }
                dbContext.Teams.Add(team);
            }
            else
            {
                // TODO: Add players if there is a difference between lists?
            }

            Matchup teamPlay = new Matchup
            {
                Game = game,
                Team = team
            };
            dbContext.TeamPlays.Add(teamPlay);

            // Initial players considered as swaps from themselves at 00:00
            ICollection<Player> players = team.Players;
            List<JToken> basePlayerTokens = teamToken["Pamatsastavs"]["Speletajs"].Children().ToList();
            foreach (var token in basePlayerTokens)
            {
                Player player = GetPlayerByNumber(players, (int)token["Nr"]);

                Swap swap = new Swap
                {
                    Time = new TimeSpan(0),
                    Player = player,
                    SwapTo = player
                };

                teamPlay.Swaps.Add(swap);
            }

            // If valid swap section exists, add swaps to swap table
            JToken swapToken = teamToken["Mainas"];
            if (ExistValidOptionalFields(swapToken, "Maina"))
            {
                foreach (var token in GetTokenEnumerable(swapToken["Maina"]))
                {
                    Swap swap = new Swap
                    {
                        Time = ExtractTime((string)token["Laiks"]),
                        Player = GetPlayerByNumber(players, (int)token["Nr1"]),
                        SwapTo = GetPlayerByNumber(players, (int)token["Nr2"])
                    };

                    teamPlay.Swaps.Add(swap);
                }
            }

            // If valid goal section exists, add goals to goal table
            JToken goalToken = teamToken["Varti"];
            if (ExistValidOptionalFields(goalToken, "VG"))
            {
                foreach (var token in GetTokenEnumerable(goalToken["VG"]))
                {
                    Goal goal = new Goal
                    {
                        Time = ExtractTime((string)token["Laiks"]),
                        Player = GetPlayerByNumber(players, (int)token["Nr"]),
                        Type = (string)token["Sitiens"] == "N" ? KickType.Regular : KickType.Penalty
                    };

                    if (ExistValidOptionalFields(token, "P"))
                    {
                        foreach (var assistToken in GetTokenEnumerable(token["P"]))
                        {
                            Assist assist = new Assist
                            {
                                Player = GetPlayerByNumber(players, (int)assistToken["Nr"])
                            };

                            goal.Assists.Add(assist);
                        }
                    }

                    teamPlay.Goals.Add(goal);
                }
            }

            // If valid penalty section exists, add penalties to penalty table
            JToken penaltyToken = teamToken["Sodi"];
            if (ExistValidOptionalFields(penaltyToken, "Sods"))
            {
                foreach (var token in GetTokenEnumerable(penaltyToken["Sods"]))
                {
                    Penalty penalty = new Penalty
                    {
                        Time = ExtractTime((string)token["Laiks"]),
                        Player = GetPlayerByNumber(players, (int)token["Nr"])
                    };

                    teamPlay.Penalties.Add(penalty);
                }
            }
        }

        private JudgeGame CreateJudgeEntry(JToken token, JudgeType type)
        {
            string name = (string)token["Vards"];
            string surname = (string)token["Uzvards"];

            Judge judge = dbContext.Judges.Where(j => j.Name == name && j.Surname == surname).FirstOrDefault();
            if (judge == null)
            {
                judge = new Judge()
                {
                    Name = name,
                    Surname = surname
                };
                dbContext.Judges.Add(judge);
            }

            return new JudgeGame()
            {
                Judge = judge,
                Type = type
            };
        }

        private Player GetPlayerByNumber(ICollection<Player> players, int num)
        {
            return players.Where(p => p.Number == num).First();
        }

        private bool ExistValidOptionalFields(JToken baseToken, string nextName)
        {
            return !IsNullOrEmpty(baseToken) && !IsNullOrEmpty(baseToken[nextName]);
        }

        private List<JToken> GetTokenEnumerable(JToken token)
        {
            if (token.Type == JTokenType.Array)
            {
                return token.Children().ToList();
            }
            else if (token.Type == JTokenType.Object)
            {
                return new List<JToken>() { token };
            }
            else
            {
                throw new InvalidDataException();
            }
        }

        private TimeSpan ExtractTime(string timeString)
        {
            int[] time = timeString.Split(':').Select(s => int.Parse(s)).ToArray();
            return new TimeSpan(time[0] / 60, time[0] % 60, time[1]);
        }

        private bool IsNullOrEmpty(JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null) ||
                   (token.Type == JTokenType.Undefined);
        }

    }
}
