using System;
using System.Collections.Generic;
using System.Text;

namespace Wanderer
{
    public class Loot
    {
        public Position Position;
        public string Id;

        public Loot(Position position, string id)
        {
            Position = position;
            Id = id;
        }
    }
}
