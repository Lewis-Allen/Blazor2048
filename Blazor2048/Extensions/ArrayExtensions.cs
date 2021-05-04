using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor2048.Extensions
{
    public static class ArrayExtensions
    {
        public static bool HasConsecutiveDuplicates<T>(this T[] array)
        {
            for (var i = 0; i < array.Length - 1; i++)
            {
                if (array[i].Equals(array[i + 1]))
                {
                    return true;
                }
            }

            return false;
        }

        public static T[] Reversed<T>(this T[] array)
        {
            var reversed = new T[array.Length];
            for(var i = 0; i < array.Length; i++)
            {
                reversed[array.Length - 1 - i] = array[i];
            }
            return reversed;
        }

        /// <summary>
        /// Splits an array into several smaller arrays.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="array">The array to split.</param>
        /// <param name="size">The size of the smaller arrays.</param>
        /// <returns>An array containing smaller arrays.</returns>
        public static Tile[][] Split(this Tile[] array, int size)
        {
            List<Tile[]> tiles = new();

            for(var i = 0; i < 4; i++)
            {
                tiles.Add(array.Skip(i * 4).Take(4).ToArray());
            }

            return tiles.ToArray();
        }
    }
}
