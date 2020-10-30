using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kermalis.SudokuSolver.Core
{
    internal static class Utils
    {
        public static ReadOnlyCollection<int> OneToNine { get; } = new ReadOnlyCollection<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        public static T CreateJaggedArray<T>(params int[] lengths)
        {
            return (T)InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths);
        }

        private static object InitializeJaggedArray(Type type, int index, int[] lengths)
        {
            var array = Array.CreateInstance(type, lengths[index]);
            Type elementType = type.GetElementType();
            if (elementType != null)
            {
                for (int i = 0; i < lengths[index]; i++)
                {
                    array.SetValue(InitializeJaggedArray(elementType, index + 1, lengths), i);
                }
            }
            return array;
        }

        public static Cell[] GetColumnInBlock(this Cell[] block, int x)
        {
            var column = new Cell[3];
            for (int i = 0; i < 3; i++)
            {
                column[i] = block[(x * 3) + i];
            }
            return column;
        }
        public static Cell[] GetRowInBlock(this Cell[] block, int y)
        {
            var row = new Cell[3];
            for (int i = 0; i < 3; i++)
            {
                row[i] = block[(i * 3) + y];
            }
            return row;
        }

        public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> values)
        {
            foreach (T o in values)
            {
                if (source.Contains(o))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
        {
            foreach (T o in values)
            {
                if (!source.Contains(o))
                {
                    return false;
                }
            }
            return true;
        }
        public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> source)
        {
            if (!source.Any())
            {
                return Array.Empty<T>();
            }
            IEnumerable<T>[] inp = source.ToArray();
            IEnumerable<T> output = inp[0];
            for (int i = 1; i < inp.Length; i++)
            {
                output = output.Intersect(inp[i]);
            }
            return output;
        }
        public static IEnumerable<T> UniteAll<T>(this IEnumerable<IEnumerable<T>> source)
        {
            IEnumerable<T> output = Array.Empty<T>();
            foreach (IEnumerable<T> i in source)
            {
                output = output.Union(i);
            }
            return output;
        }

        public static string SingleOrMultiToString<T>(this IEnumerable<T> source)
        {
            int i = 0;
            foreach (T o in source)
            {
                if (++i > 1)
                {
                    return source.Print();
                }
            }
            if (i == 0)
            {
                throw new ArgumentException();
            }
            return source.ElementAt(0).ToString();
        }
        public static string Print<T>(this IEnumerable<T> source)
        {
            return "( " + string.Join(", ", source) + " )";
        }
    }
}
