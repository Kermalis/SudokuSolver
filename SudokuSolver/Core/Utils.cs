using System;
using System.Linq;

namespace SudokuSolver.Core
{
    public static class Utils
    {
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
                    array.SetValue(
                        InitializeJaggedArray(elementType, index + 1, lengths), i);
                }
            }

            return array;
        }

        public static bool AreAllSequencesEqual<T>(params T[][] sequences)
        {
            for (int i = 0; i < sequences.Length - 1; i++)
            {
                if (!sequences[i].SequenceEqual(sequences[i + 1]))
                    return false;
            }
            return true;
        }
    }
}
