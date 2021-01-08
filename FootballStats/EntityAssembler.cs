using FootballStats.Entities;
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
            Game game = new Game();
            dbContext.Games.Add(game);

            game.Date = DateTime.ParseExact((string)gameInfo["Laiks"], "yyyy/MM/dd", CultureInfo.InvariantCulture);
            game.ViewerCount = (int)gameInfo["Skatitaji"];
            game.Place = (string)gameInfo["Vieta"];

            List<JToken> teamTokens = gameInfo["Komanda"].Children().ToList();
            if (teamTokens.Count != 2) throw new InvalidDataException();
            teamTokens.ForEach(t => ProcessTeam(game, t));
        }

        private void ProcessTeam(Game game, JToken teamToken)
        {
            string teamName = (string)teamToken["Nosaukums"];
         
            Team team = dbContext.Teams.Find(teamName);
            if (team == null)
            {
                // Create new team entry
                team = new Team();
                team.Name = teamName;

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

                    dbContext.Players.Add(player);
                    team.Players.Add(player);
                }

                dbContext.Teams.Add(team);
            }
            else
            {
                // TODO: Check if team has already played on date.
            }

            TeamPlay teamPlay = new TeamPlay
            {
                Game = game,
                Team = team
            };
            dbContext.TeamPlays.Add(teamPlay);

            ICollection<Player> players = team.Players;
            // Initial players considered as swaps from null to player at 00:00
            List<JToken> basePlayerTokens = teamToken["Pamatsastavs"]["Speletajs"].Children().ToList();
            foreach (var token in basePlayerTokens)
            {
                Player player = players.Where(p => p.Number == (int)token["Nr"]).First();

                Swap swap = new Swap
                {
                    Time = new TimeSpan(0),
                    To = player
                };

                dbContext.Swaps.Add(swap);
                teamPlay.Swaps.Add(swap);
            }

            
        }

    }
}
