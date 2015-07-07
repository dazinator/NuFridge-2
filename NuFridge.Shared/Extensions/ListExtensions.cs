using System;
using System.Collections.Generic;

namespace NuFridge.Shared.Extensions
{
    public static class ListExtensions
    {
        public static void RemoveWhere<TElement>(this IList<TElement> source, Func<TElement, bool> remove)
        {
            if (source == null)
                return;
            for (int index = 0; index < source.Count; ++index)
            {
                TElement element = source[index];
                if (remove(element))
                {
                    source.RemoveAt(index);
                    --index;
                }
            }
        }

        public static void AddRange<TElement>(this ICollection<TElement> source, IEnumerable<TElement> itemsToAdd)
        {
            if (itemsToAdd == null || source == null)
                return;
            foreach (TElement element in itemsToAdd)
                source.Add(element);
        }

        public static IEnumerable<TElement> Apply<TElement>(this IEnumerable<TElement> source, Action<TElement> apply)
        {
            foreach (TElement element in source)
            {
                apply(element);
                yield return element;
            }
        }
    }
}
