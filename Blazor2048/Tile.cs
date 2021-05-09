﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blazor2048
{
    public class Tile : ICloneable
    {
        public int AnimationFactor { get; set; } = 0;
        public bool NewTile { get; set; } = false;

        public Tile(int value)
        {
            Value = value;
        }

        public int Value { get; set; }

        public object Clone()
        {
            return new Tile(Value);
        }

        public override bool Equals(object obj)
        {
            if(obj is Tile tile)
            {
                return tile.Value == Value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
