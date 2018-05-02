using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Core
{
    public static class Extensions
    {
        // Only gonna be used for the boards so hardcoding 9s is fine
        public static byte[][] ToJaggedArray(this byte[,] twoDimensionalArray)
        {
            var jaggedArray = new byte[9][];
            for (byte i = 0; i < 9; i++)
            {
                jaggedArray[i] = new byte[9];
                for (byte j = 0; j < 9; j++)
                {
                    jaggedArray[i][j] = twoDimensionalArray[j, i];
                }
            }
            return jaggedArray;
        }

        public static byte[] GetColumn(this byte[][] input, int x)
        {
            int amt = input[0].Length;
            var column = new byte[amt];
            Buffer.BlockCopy(input[x], 0, column, 0, amt);
            return column;
        }

        public static byte[] GetRow(this byte[][] input, int x)
        {
            int amt = input.Length;
            var row = new byte[amt];
            for(byte i = 0; i < amt; i++)
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
    }
}
