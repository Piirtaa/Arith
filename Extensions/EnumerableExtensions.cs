using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arith.Extensions
{
    /// <summary>
    /// Extensions that operate on enumerables 
    /// </summary>
    //[DebuggerStepThrough]
    public static class EnumerableExtensions
    {
        /// <summary>
        /// iterates thru each item and performs the action.  doesn't kack on nulls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void WithEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) { return; }
            if (action == null) { return; }

            foreach (T item in source)
            {
                action(item);
            }
        }
        /// <summary>
        /// iterates thru each item and performs the action until the break condition happens.  doesn't kack on nulls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <param name="breakCondition"></param>
        public static void WithEach<T>(this IEnumerable<T> source, Action<T> action, Func<T, bool> breakCondition)
        {
            if (source == null) { return; }
            if (action == null) { return; }

            foreach (T item in source)
            {
                if (breakCondition != null)
                {
                    if (breakCondition(item))
                    {
                        break;
                    }
                }
                action(item);
            }
        }
    }
}
