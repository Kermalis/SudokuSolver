using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Core
{
    public static class Extensions
    {
        public static int[] GetColumn(this int[][] input, int x)
        {
            int amt = input[0].Length;
            var column = new int[amt];
            Buffer.BlockCopy(input[x], 0, column, 0, amt*4);
            return column;
        }

        public static int[] GetRow(this int[][] input, int x)
        {
            int amt = input.Length;
            var row = new int[amt];
            for(int i = 0; i < amt; i++)
                row[i] = input[i][x];
            return row;
        }

        public static T[] GetColumn<T>(this T[] input, int x)
        {
            int amt = (int)Math.Sqrt(input.Length);
            var column = new T[amt];
            for (byte i = 0; i < amt; i++)
                column[i] = input[(x * amt) + i];
            return column;
        }

        public static T[] GetRow<T>(this T[] input, int x)
        {
            int amt = (int)Math.Sqrt(input.Length);
            var row = new T[amt];
            for (byte i = 0; i < amt; i++)
                row[i] = input[(i * amt) + x];
            return row;
        }

        public static IEnumerable<T> UniteAll<T>(this IEnumerable<IEnumerable<T>> input)
        {
            var output = new T[0];
            foreach (IEnumerable<T> i in input)
            {
                output = output.Union(i).ToArray();
            }
            return output;
        }

        public static string Print<T>(this IEnumerable<T> arr) => string.Format("({0})", string.Join(", ", arr));
    }
}
