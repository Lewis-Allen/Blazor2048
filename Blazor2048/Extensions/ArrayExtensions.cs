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

        public static T[][] Split<T>(this T[] array, int size)
        {
            T[][] tiles = new T[size][];

            for(var i = 0; i < size; i++)
            {
                tiles[i] = array.Skip(i * size).Take(size).ToArray();
            }

            return tiles;
        }

        public static T[][] Transpose<T>(this T[][] array)
        {
            var transposed = new T[array.Length][];

            for (var y = 0; y < transposed.Length; y++)
                transposed[y] = new T[array[y].Length];

            for(var y = 0; y < array.Length; y++)
            {
                for(var x = 0; x < array[y].Length; x++)
                {
                    transposed[x][y] = array[y][x];
                }
            }

            return transposed;
        }

        public static T[][] GetColumns<T>(this T[] array) => array.Split(4).ToArray();
        public static T[][] GetRows<T>(this T[] array) => array.Split(4).ToArray().Transpose();
    }
}
