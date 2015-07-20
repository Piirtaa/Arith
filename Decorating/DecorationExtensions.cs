using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Arith.Extensions;

namespace Arith.Decorating
{
    /// <summary>
    /// extensions that query the decorator chain, downwards
    /// </summary>
    public static class IDecorationExtensions
    {
        /// <summary>
        /// tells us if the object is a IDecoration
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsADecoration(this object obj)
        {
            if (obj == null)
                return false;

            if (obj is IDecoration)
                return true;

            return false;
        }

        /// <summary>
        /// if an object is a decoration, returns the first decoration
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetDecorated(this object obj)
        {
            if (obj == null)
                return null;

            if (!obj.IsADecoration())
                return null;

            return (obj as IDecoration).Decorated;
        }
       
        /// <summary>
        /// walking Decorated chain, gets the inner most decoration's core 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetInnerDecorated(this object obj)
        {
            object last = obj;
            object currentLayer = obj;

            //iterate down
            while (currentLayer != null)
            {
                currentLayer = currentLayer.GetDecorated();
                if (currentLayer != null)
                    last = currentLayer;
            }

            return last;
        }
        /// <summary>
        /// walks Decorated chain to the core, or until the stop condition is met
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stopWalkCondition"></param>
        /// <returns></returns>
        public static object WalkDecorationsToInner(this object obj, Func<object, bool> stopWalkCondition)
        {
            object currentLayer = obj;

            //iterate down
            while (currentLayer != null)
            {
                //check filter.  break/return
                if (stopWalkCondition(currentLayer))
                {
                    return currentLayer;
                }

                currentLayer = GetDecorated(currentLayer);
            }

            return null;
        }

        /// <summary>
        /// gets all the decorations, including the core, below this object including itself
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<object> GetSelfAndAllDecorationsBelow(this object obj)
        {
            List<object> returnValue = new List<object>();

            var match = obj.WalkDecorationsToInner((reg) =>
            {
                returnValue.Add(reg);
                return false;
            });

            return returnValue;
        }

        public static object CloneDecorationCake(this object obj, object inner)
        {
            if (obj == null)
                return null;

            var cake = obj.GetSelfAndAllDecorationsBelow();

            object rv = inner;
            for (int i = 0; i < cake.Count -1; i++)
            {
                IDecoration layer = cake[i] as IDecoration;
                rv = layer.ApplyThisDecorationTo(rv);
            }
            return rv;
        }
    }

    /// <summary>
    /// basically the mirror of IDecorationExtensions but going Upwards on the Decorator chain
    /// </summary>
    public static class IDecoratorAwareDecorationExtensions
    {
        public static bool IsADecoratorAwareDecoration(this object obj)
        {
            if (obj == null)
                return false;

            if (obj is IDecoratorAwareDecoration)
                return true;

            return false;
        }
        /// <summary>
        /// if this is stackaware it gets its decorator
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetDecorator(this object obj)
        {
            if (obj == null)
                return null;

            if (!obj.IsADecoratorAwareDecoration())
                return null;

            return (obj as IDecoratorAwareDecoration).Decorator;
        }
        /// <summary>
        /// Walks up the Decorator chain to the topmost
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetOuterDecorator(this object obj)
        {
            object last = obj;
            object currentLayer = obj;

            //iterate down
            while (currentLayer != null)
            {
                currentLayer = GetDecorator(currentLayer);
                if (currentLayer != null)
                    last = currentLayer;
            }

            return last;
        }
        /// <summary>
        /// walks towards the outwards (eg. towards the decorator)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stopWalkCondition"></param>
        /// <returns></returns>
        public static object WalkDecoratorsToOuter(this object obj, Func<object, bool> stopWalkCondition)
        {
            object currentLayer = obj;

            //iterate down
            while (currentLayer != null)
            {
                //check filter.  break/return
                if (stopWalkCondition(currentLayer))
                {
                    return currentLayer;
                }

                currentLayer = GetDecorator(currentLayer);
            }

            return null;
        }

        public static List<object> GetSelfAndAllDecoratorsAbove(this object obj)
        {
            List<object> returnValue = new List<object>();

            var match = obj.WalkDecoratorsToOuter((reg) =>
            {
                returnValue.Add(reg);
                return false;
            });

            return returnValue;
        }
    }

    /// <summary>
    /// extension methods that handle the "As" type conversions on a decoration cake
    /// </summary>
    public static class AsExtensions
    {
        /// <summary>
        /// looks for the "As face" by walking down the decorations 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="decorationType"></param>
        /// <param name="exactTypeMatch"></param>
        /// <returns></returns>
        public static object AsBelow(this object obj, Type decorationType, bool exactTypeMatch = true)
        {
            if (obj == null)
                return null;

            var match = obj.WalkDecorationsToInner((dec) =>
            {
                //if we're exact matching, the decoration has to be the same type
                if (exactTypeMatch && decorationType.Equals(dec.GetType()) == false)
                    return false;

                //if we're not exact matching, the decoration has to be Of the same type
                if (exactTypeMatch == false && (!(decorationType.IsAssignableFrom(dec.GetType()))))
                    return false;

                return true;

            });

            return match;
        }
        /// <summary>
        /// looks for the "As face" by walking down the decorations 
        /// </summary>
        public static T AsBelow<T>(this object obj, bool exactTypeMatch = true)
        {
            var rv = obj.AsBelow(typeof(T), exactTypeMatch);
            if (rv == null)
                return default(T);

            return (T)rv;
        }
        /// <summary>
        /// looks for the "As face" by walking up the decorators
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="decorationType"></param>
        /// <param name="exactTypeMatch"></param>
        /// <returns></returns>
        public static object AsAbove(this object obj, Type decorationType, bool exactTypeMatch = true)
        {
            if (obj == null)
                return null;

            var match = obj.WalkDecoratorsToOuter((dec) =>
            {
                //if we're exact matching, the decoration has to be the same type
                if (exactTypeMatch && decorationType.Equals(dec.GetType()) == false)
                    return false;

                //if we're not exact matching, the decoration has to be Of the same type
                if (exactTypeMatch == false && (!(decorationType.IsAssignableFrom(dec.GetType()))))
                    return false;

                return true;

            });

            return match;
        }
        /// <summary>
        /// looks for the "As face" by walking up the decorators
        /// </summary>
        public static T AsAbove<T>(this object obj, bool exactTypeMatch = true)
        {
            var rv = obj.AsAbove(typeof(T), exactTypeMatch);
            if (rv == null)
                return default(T);

            return (T)rv;
        }

        /// <summary>
        /// looks for the "As face" by walking ALL the decorations.  If DecoratorAware, walks down from Outer.  If not
        /// DecoratorAware, walks down from Self 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="decorationType"></param>
        /// <param name="exactTypeMatch"></param>
        /// <returns></returns>
        public static object As(this object obj, Type decorationType, bool exactTypeMatch)
        {
            if (obj == null)
                return null;

            //if decorator aware get the outer
            object topMost = obj;

            if (obj.IsADecoratorAwareDecoration())
            {
                topMost = obj.GetOuterDecorator();
            }

            return topMost.AsBelow(decorationType, exactTypeMatch);
        }
        /// <summary>
        /// looks for the "As face" by walking ALL the decorations.  If DecoratorAware, walks down from Outer.  If not
        /// DecoratorAware, walks down from Self 
        /// </summary>
        public static T As<T>(this object obj, bool exactTypeMatch = true)
        {
            var rv = obj.As(typeof(T), exactTypeMatch);
            if (rv == null)
                return default(T);

            return (T)rv;
        }

        /// <summary>
        /// If DecoratorAware, walks down from Outer.  If not DecoratorAware, walks down from Self 
        /// </summary>
        public static List<object> GetAllDecorations(this object obj)
        {
            List<object> rv = new List<object>();

            if (obj == null)
                return rv;

            //if decorator aware get the outer
            object topMost = obj;

            if (IDecoratorAwareDecorationExtensions.IsADecoratorAwareDecoration(obj))
            {
                topMost = IDecoratorAwareDecorationExtensions.GetOuterDecorator(obj);
            }

            rv = IDecorationExtensions.GetSelfAndAllDecorationsBelow(topMost);
            return rv;
        }

        /// <summary>
        /// find all decorations of a given type
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="typeToImplement"></param>
        /// <returns></returns>
        public static List<object> GetAllImplementingDecorations(this object obj, Type typeToImplement)
        {
            List<object> rv = new List<object>();

            var list = obj.GetAllDecorations();
            foreach (var each in list)
            {
                if (typeToImplement.IsInstanceOfType(each))
                    rv.Add(each);
            }

            return rv;
        }
        
        /// <summary>
        /// find all decorations of a given type
        /// </summary>
        public static List<T> GetAllImplementingDecorations<T>(this object obj)
        {
            Type type = typeof(T);
            var list = obj.GetAllImplementingDecorations(type);
            var castList = list.ConvertListTo<T, object>();
            return castList;
        }
    }

    public static class IsExtensions
    {
        /// <summary>
        /// if an object is a decoration, determines if a decoration exists in its cake
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Is(this object obj, Type decType, bool exactTypeMatch = true)
        {
            if (obj == null)
                return false;

            var dec = obj.As(decType, exactTypeMatch);
            return dec != null;
        }
        public static bool Is<T>(this object obj, bool exactTypeMatch = true)
        {
            return obj.Is(typeof(T), exactTypeMatch);
        }
        /// <summary>
        /// does a non-exact type match on all decorations
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="decTypes"></param>
        /// <returns></returns>
        public static bool Is(this object obj, params Type[] decTypes)
        {
            if (obj == null)
                return false;

            var decs = obj.GetAllDecorations();
            List<Type> allDecTypes = new List<Type>();
            foreach (var dec in decs)
            {
                allDecTypes.Add(dec.GetType());
            }

            bool rv = true;

            //iterate thru all the decorations to look for
            foreach (var decType in decTypes)
            {
                bool isFound = false;
                foreach (var actualDecType in allDecTypes)
                {
                    if (decType.IsAssignableFrom(actualDecType))
                    {
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {
                    rv = false;
                    break;
                }
            }

            return rv;
        }

        public static bool Is<T1, T2>(this object obj)
        {
            return obj.Is(typeof(T1), typeof(T2));
        }
        public static bool Is<T1, T2, T3>(this object obj)
        {
            return obj.Is(typeof(T1), typeof(T2), typeof(T3));
        }
        public static bool Is<T1, T2, T3, T4>(this object obj)
        {
            return obj.Is(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }
        public static bool Is<T1, T2, T3, T4, T5>(this object obj)
        {
            return obj.Is(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }
        public static bool Is<T1, T2, T3, T4, T5, T6>(this object obj)
        {
            return obj.Is(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }
        public static bool Is<T1, T2, T3, T4, T5, T6, T7>(this object obj)
        {
            return obj.Is(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        }
    }

    public static class DecorationExtensions
    {
        public static bool ToggleDecoration(this object obj, bool isEnabled, Type decType, bool exactTypeMatch = true)
        {
            if (obj == null)
                return false;

            var dec = obj.As(decType, exactTypeMatch);

            if (dec == null)
                return false;

            if (dec is ITogglingDecoration)
            {
                (dec as ITogglingDecoration).IsDecorationEnabled = isEnabled;
                return true;
            }
            return false;
        }
        public static bool ToggleDecoration<T>(this object obj, bool isEnabled, bool exactTypeMatch = true)
        {
            return obj.ToggleDecoration(isEnabled, typeof(T), exactTypeMatch);
        }
    }

    public static class IHasDecorationExtensions
    {
        /// <summary>
        /// if a cake constraint (aka IIsA) is declared anywhere in the stack
        /// we validate the current cake supports the constraint.  this is a topdown walk
        /// </summary>
        /// <param name="obj"></param>
        public static void ValidateIIsAConstraints(this object obj)
        {

            var cake = obj.GetAllDecorations();
            cake.WithEach(layer =>
            {
                if (layer is IIsA)
                {
                    //get all the interfaces it has, that derive from IHasDecoration
                    var layerType = layer.GetType();

                    var interfaces = layerType.GetInterfaces();
                    foreach (var interfaceType in interfaces)
                    {
                        if (!interfaceType.Name.Contains("IIsA`"))
                            continue;

                        var requiredDecorations = interfaceType.GetGenericArguments();

                        foreach (Type each in requiredDecorations)
                        {
                            if (obj.As(each, false) == null)
                                throw new InvalidOperationException(string.Format("required decoration {0} not found in cake", each.Name));
                        }
                    }
                }
            });
        }
    }

}
