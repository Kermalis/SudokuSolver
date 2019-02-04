using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kermalis.SudokuSolver.Core
{
    static class Utils
    {
        public static readonly ReadOnlyCollection<int> OneToNine = new ReadOnlyCollection<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        public static T CreateJaggedArray<T>(params int[] lengths)
        {
            return (T)InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths);
        }
        static object InitializeJaggedArray(Type type, int index, int[] lengths)
        {
            Array array = Array.CreateInstance(type, lengths[index]);
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
            if (source.Count() == 0)
            {
                return new T[0];
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
            IEnumerable<T> output = new T[0];
            foreach (IEnumerable<T> i in source)
            {
                output = output.Union(i);
            }
            return output;
        }

        public static string Print<T>(this IEnumerable<T> source)
        {
            return "( " + string.Join(", ", source) + " )";
        }
    }
}
