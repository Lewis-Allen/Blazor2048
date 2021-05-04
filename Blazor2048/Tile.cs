using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor2048
{
    public class Tile : ICloneable
    {
        public Tile(int value)
        {
            Value = value;
        }

        public int Value { get; set; }

        public object Clone()
        {
            return new Tile(Value);
        }
    }
}
