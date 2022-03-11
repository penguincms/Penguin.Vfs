using System.Collections.Generic;

namespace VirtualFileSystem.Extensions
{
    internal static class IEnumerableExtensions
    {
        public static Stack<T> ToStack<T>(this IEnumerable<T> source)
        {
            Stack<T> stack = new();

            foreach (T item in source)
            {
                stack.Push(item);
            }

            return stack;
        }
    }
}