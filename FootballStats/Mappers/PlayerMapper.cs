using FootballStats.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Mappers
{
    public class PlayerMapper
    {
        public static Player MapToEntity(JToken token)
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

            return new Player
            {
                Role = role,
                Number = (int)token["Nr"],
                Name = (string)token["Vards"],
                Surname = (string)token["Uzvards"]
            };
        }
    }
}
