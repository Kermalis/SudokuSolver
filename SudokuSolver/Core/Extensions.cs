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

        public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> values)
        {
            foreach (T o in values)
                if (source.Contains(o))
                    return true;
            return false;
        }
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
        {
            foreach (T o in values)
                if (!source.Contains(o))
                    return false;
            return true;
        }
        public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> source)
        {
            if (source.Count() == 0) return new T[0];
            var inp = source.ToArray();
            IEnumerable<T> output = inp[0];
            for (int i = 1; i < inp.Length; i++)
                output = output.Intersect(inp[i]);
            return output;
        }
        public static IEnumerable<T> UniteAll<T>(this IEnumerable<IEnumerable<T>> source)
        {
            IEnumerable<T> output = new T[0];
            foreach (IEnumerable<T> i in source)
                output = output.Union(i);
            return output;
        }

        public static string Print<T>(this IEnumerable<T> arr) => "( " + string.Join(", ", arr) + " )";
    }
}
