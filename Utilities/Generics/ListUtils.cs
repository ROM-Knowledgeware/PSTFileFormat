using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public class ListUtils
    {
        public static List<T> GetRemovedRange<T>(List<T> source, List<T> items)
        {
            List<T> result = new List<T>(source);
            foreach (T item in items)
            {
                result.Remove(item);
            }
            return result;
        }

        public static List<T> FromArrayList<T>(ArrayList arrayList)
        { 
            List<T> result = new List<T>();
            foreach(T entity in arrayList)
            {
                result.Add(entity);
            }
            return result;
        }

        public static List<T> GetSorted<T>(List<T> source)
        {
            List<T> result = new List<T>(source);
            result.Sort();
            return result;
        }

        public static List<T> GetDistinct<T>(List<T> source)
        {
            List<T> result = new List<T>();
            foreach (T entry in source)
            {
                if (!result.Contains(entry))
                {
                    result.Add(entry);
                }
            }
            return result;
        }

        public static List<T> ToList<T>(IEnumerable<T> collection)
        {
            List<T> result = new List<T>();
            foreach (T entry in collection)
            {
                result.Add(entry);
            }
            return result;
        }
    }
}
