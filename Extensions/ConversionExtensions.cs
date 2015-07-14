using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arith.Extensions
{
    /// <summary>
    /// Extensions to convert strings to primitives
    /// </summary>
    public static class ConversionExtensions
    {
        public static T ConvertTo<T>(this object obj)
        {
            T returnValue = default(T);

            if (obj == null)
                return returnValue;

            if (typeof(T).IsAssignableFrom(obj.GetType()))
            {
                returnValue = (T)obj;
            }

            return returnValue;
        }
        public static List<TConverted> ConvertListTo<TConverted, TFrom>(this List<TFrom> list)
        {
            List<TConverted> returnValue = new List<TConverted>();

            list.WithEach(x =>
            {
                returnValue.Add(x.ConvertTo<TConverted>());
            });

            return returnValue;
        }

        public static List<object> ProjectList<T>(this IEnumerable<T> list, Func<T, object> projection)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if (projection == null)
                throw new ArgumentNullException("projection");

            List<object> returnValue = new List<object>();

            list.WithEach(x =>
            {
                returnValue.Add(projection(x));
            });

            return returnValue;
        }
    }
}
