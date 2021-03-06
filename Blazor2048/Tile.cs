﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blazor2048
{
    public class Tile
    {
        public int AnimationFactor { get; set; } = 0;
        public bool NewTile { get; set; } = false;
        public bool Merged { get; set; } = false;

        [JsonConstructor]
        public Tile(int value)
        {
            Value = value;
        }

        public Tile(int value, int animationFactor) : this(value)
        {
            AnimationFactor = animationFactor;
        }

        public Tile(int value, bool merged) : this(value)
        {
            Merged = merged;
        }

        public int Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Tile tile)
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
