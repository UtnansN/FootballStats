using FootballStats.Entities;
using FootballStats.Exceptions;
using FootballStats.Mappers;
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

            
            Team team = dbContext.Teams.Local
                .Where(t => t.Name == (string)teamToken["Nosaukums"])
                .FirstOrDefault();

            Dictionary<int, Player> playersInCurrent = new Dictionary<int, Player>();
            List<JToken> playerTokens = teamToken["Speletaji"]["Speletajs"].Children().ToList();
            if (team == null)
            {
                // Create new team entry
                team = new Team
                {
                    Name = teamName
                };

                // Add players to team
                
                foreach (var token in playerTokens)
                {
                    Player curr = PlayerMapper.MapToEntity(token);

                    team.Players.Add(curr);
                    playersInCurrent.Add(curr.Number, curr);
                }
                dbContext.Teams.Add(team);
            }
            else
            {
                // Add players to team if there is a difference between lists
                List<Player> allPlayers = team.Players.ToList();
                foreach (var token in playerTokens)
                {
                    Player curr = PlayerMapper.MapToEntity(token);
                    Player existing = GetPlayerInList(allPlayers, curr);

                    if (existing != null)
                    {
                        curr = existing;
                    }
                    else
                    {
                        team.Players.Add(curr);
                    }
                    playersInCurrent.Add(curr.Number, curr);
                }
            }

            Matchup teamPlay = new Matchup
            {
                Game = game,
                Team = team
            };
            dbContext.TeamPlays.Add(teamPlay);

            // Initial players considered as swaps from themselves at 00:00
            List<JToken> basePlayerTokens = teamToken["Pamatsastavs"]["Speletajs"].Children().ToList();
            foreach (var token in basePlayerTokens)
            {
                //Player player = GetPlayerByNumber(players, (int)token["Nr"]);
                Player player = playersInCurrent[(int)token["Nr"]];

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
                    teamPlay.Swaps.Add(new Swap
                    {
                        Time = ExtractTime((string)token["Laiks"]),
                        Player = playersInCurrent[(int)token["Nr1"]],
                        SwapTo = playersInCurrent[(int)token["Nr2"]]
                    });
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
                        Player = playersInCurrent[(int)token["Nr"]],
                        Type = (string)token["Sitiens"] == "N" ? KickType.Regular : KickType.Penalty
                    };

                    if (ExistValidOptionalFields(token, "P"))
                    {
                        foreach (var assistToken in GetTokenEnumerable(token["P"]))
                        {
                            Assist assist = new Assist
                            {
                                Player = playersInCurrent[(int)assistToken["Nr"]]
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
                    teamPlay.Penalties.Add(new Penalty
                    {
                        Time = ExtractTime((string)token["Laiks"]),
                        Player = playersInCurrent[(int)token["Nr"]]
                    });
                }
            }
        }

        private Player GetPlayerInList(List<Player> playerList, Player player)
        {
            return playerList.Where(p => p.Name == player.Name && 
            p.Surname == player.Surname && 
            p.Number == player.Number &&
            p.Role == player.Role)
            .FirstOrDefault();
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
