using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Core
{
    public static class Extensions
    {
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
                output = output.Union(i).ToArray();
            return output;
        }

        public static string Print<T>(this IEnumerable<T> arr) => "( " + string.Join(", ", arr) + " )";
    }
}
