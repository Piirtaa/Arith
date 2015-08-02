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
        /// gets the decoration immediately above the inner decorated instance
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetInnerDecoration(this object obj)
        {
            object rv = obj;
            object currentLayer = obj;

            //iterate down
            while (currentLayer != null)
            {
                var next = currentLayer.GetDecorated();
                if (!next.IsADecoration())
                {
                    rv = currentLayer;
                    //break out of the loop
                    currentLayer = null;
                }
                else
                {
                    currentLayer = next;
                }
            }

            return rv;
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
        public static object WalkDecorationsToInner(this object obj,
            Func<object, bool, bool> stopWalkCondition)
        {
            object currentLayer = obj;

            //iterate down
            while (currentLayer != null)
            {
                var nextLayer = GetDecorated(currentLayer);
                bool isInner = nextLayer == null;

                //check filter.  break/return
                if (stopWalkCondition(currentLayer, isInner))
                {
                    return currentLayer;
                }

                currentLayer = nextLayer;
            }

            return null;
        }

        /// <summary>
        /// gets all the decorations, including the core, below this object including itself.
        /// From outer to inner.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<object> GetSelfAndAllDecorationsBelow(this object obj)
        {
            List<object> returnValue = new List<object>();

            var match = obj.WalkDecorationsToInner((reg, isInner) =>
            {
                returnValue.Add(reg);
                return false;
            });

            return returnValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="inner"></param>
        /// <returns></returns>
        public static object CloneDecorationCake(this object obj, object inner)
        {
            if (obj == null)
                return null;

            var cake = obj.GetSelfAndAllDecorationsBelow();

            //is there something to decorate?
            if (cake.Count == 1)
            {
                //nope, return inner
                return inner;
            }

            object rv = inner;
            //remove the old inner
            cake.RemoveAt(cake.Count - 1);
            for (int i = cake.Count - 1; i >= 0; i--)
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
        public static object WalkDecoratorsToOuter(this object obj,
            Func<object, bool, bool> stopWalkCondition)
        {
            object currentLayer = obj;

            //iterate down
            while (currentLayer != null)
            {
                var nextLayer = GetDecorator(currentLayer);
                bool isOuter = nextLayer == null;

                //check filter.  break/return
                if (stopWalkCondition(currentLayer, isOuter))
                {
                    return currentLayer;
                }

                currentLayer = nextLayer;
            }

            return null;
        }

        /// <summary>
        /// from inner to outer
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<object> GetSelfAndAllDecoratorsAbove(this object obj)
        {
            List<object> returnValue = new List<object>();

            var match = obj.WalkDecoratorsToOuter((reg, isOuter) =>
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
        public static bool HaveSamecakeName(this object obj, object obj2)
        {
            if (obj == null)
                return false;

            if (obj2 == null)
                return false;

            if (!obj.IsADecoration())
                return false;

            if (!obj2.IsADecoration())
                return false;

            var name1 = (obj as IDecoration).CakeName;
            var name2 = (obj2 as IDecoration).CakeName;
            return string.Equals(name1, name2);
        }

        public static bool ValidatecakeName(string cakeName,
            object obj2)
        {
            if (obj2 == null)
                return false;

            if (!obj2.IsADecoration())
                return false;

            var decName = (obj2 as IDecoration).CakeName;
            return string.Equals(decName, cakeName);
        }
        #region AsInnermost
        /// <summary>
        /// gets the inner most matching face, including Inner if it does
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="decorationType"></param>
        /// <param name="cakeName"></param>
        /// <param name="exactTypeMatch"></param>
        /// <returns></returns>
        public static object AsInnermost(this object obj,
           Type decorationType,
           string cakeName,
           bool exactTypeMatch = true
           )
        {
            if (obj == null)
                return null;

            //get the object above Inner
            object inner = obj.GetInnerDecoration();

            //check Inner for compliance
            var rv = inner.AsBelow(decorationType, cakeName, false);

            //can't find it? look above now
            if(rv == null)
                rv = inner.AsAbove(decorationType, cakeName, false);
            return rv;
        }
        public static object AsInnermost(this object obj,
Type decorationType,
bool exactTypeMatch = true)
        {
            string cakeName = null;
            if (obj.IsADecoration())
                cakeName = (obj as IDecoration).CakeName;

            return AsInnermost(obj, decorationType, cakeName, exactTypeMatch);
        }
        public static T AsInnermost<T>(this object obj,
    bool exactTypeMatch = true)
        {
            var rv = obj.AsInnermost(typeof(T), exactTypeMatch);
            if (rv == null)
                return default(T);

            return (T)rv;
        }
        #endregion

        #region AsBelow

        /// <summary>
        /// looks for the "As face" by walking down the decorations 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="decorationType"></param>
        /// <param name="exactTypeMatch"></param>
        /// <returns></returns>
        public static object AsBelow(this object obj,
            Type decorationType,
            string cakeName,
            bool exactTypeMatch = true
            )
        {
            if (obj == null)
                return null;

            
            var match = obj.WalkDecorationsToInner((dec, isInner) =>
            {
                var decObj = dec;
                var layerType = dec.GetType();

                if (!isInner)
                    if (!ValidatecakeName(cakeName, decObj))
                        return false;

                //if we're exact matching, the decoration has to be the same type
                if (exactTypeMatch && decorationType.Equals(layerType) == false)
                    return false;

                //if we're not exact matching, the decoration has to be Of the same type
                if (exactTypeMatch == false && (!(decorationType.IsAssignableFrom(layerType))))
                    return false;


                return true;

            });

            return match;
        }
        public static object AsBelow(this object obj,
Type decorationType,
bool exactTypeMatch = true)
        {
            string cakeName = null;
            if (obj.IsADecoration())
                cakeName = (obj as IDecoration).CakeName;

            return AsBelow(obj, decorationType, cakeName, exactTypeMatch);
        }
        /// <summary>
        /// looks for the "As face" by walking down the decorations 
        /// </summary>
        public static T AsBelow<T>(this object obj,
            bool exactTypeMatch = true)
        {
            var rv = obj.AsBelow(typeof(T), exactTypeMatch);
            if (rv == null)
                return default(T);

            return (T)rv;
        }
        #endregion

        #region AsAbove
        /// <summary>
        /// looks for the "As face" by walking up the decorators
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="decorationType"></param>
        /// <param name="exactTypeMatch"></param>
        /// <returns></returns>
        public static object AsAbove(this object obj,
            Type decorationType,
            string cakeName,
            bool exactTypeMatch = true)
        {
            if (obj == null)
                return null;

            var match = obj.WalkDecoratorsToOuter((dec, isOuter) =>
            {
                var decObj = dec;

                if (!ValidatecakeName(cakeName, decObj))
                    return false;

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
        public static object AsAbove(this object obj,
Type decorationType,
bool exactTypeMatch = true)
        {
            string cakeName = null;
            if (obj.IsADecoration())
                cakeName = (obj as IDecoration).CakeName;

            return AsAbove(obj, decorationType, cakeName, exactTypeMatch);
        }
        /// <summary>
        /// looks for the "As face" by walking up the decorators
        /// </summary>
        public static T AsAbove<T>(this object obj,
            bool exactTypeMatch = true)
        {
            var rv = obj.AsAbove(typeof(T), exactTypeMatch);
            if (rv == null)
                return default(T);

            return (T)rv;
        }

        #endregion

        #region As
        /// <summary>
        /// looks for the "As face" by walking ALL the decorations.  If DecoratorAware, walks down from Outer.  If not
        /// DecoratorAware, walks down from Self 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="decorationType"></param>
        /// <param name="exactTypeMatch"></param>
        /// <returns></returns>
        public static object As(this object obj,
            Type decorationType,
            string cakeName,
            bool exactTypeMatch)
        {
            if (obj == null)
                return null;

            //if decorator aware get the outer
            object topMost = obj;

            if (obj.IsADecoratorAwareDecoration())
            {
                topMost = obj.GetOuterDecorator();
            }

            return topMost.AsBelow(decorationType, cakeName, exactTypeMatch);
        }
        public static object As(this object obj,
Type decorationType,
bool exactTypeMatch)
        {
            string cakeName = null;
            if (obj.IsADecoration())
                cakeName = (obj as IDecoration).CakeName;

            return As(obj, decorationType, cakeName, exactTypeMatch);
        }
        /// <summary>
        /// looks for the "As face" by walking ALL the decorations.  If DecoratorAware, walks down from Outer.  If not
        /// DecoratorAware, walks down from Self 
        /// </summary>
        public static T As<T>(this object obj,
            bool exactTypeMatch = true)
        {
            var rv = obj.As(typeof(T), exactTypeMatch);
            if (rv == null)
                return default(T);

            return (T)rv;
        }

        #endregion

        #region Decoration Lists
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

            if (obj.IsADecoratorAwareDecoration())
            {
                topMost = obj.GetOuterDecorator();
            }

            rv = topMost.GetSelfAndAllDecorationsBelow();
            return rv;
        }

        /// <summary>
        /// find all decorations of a given type
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="typeToImplement"></param>
        /// <returns></returns>
        public static List<object> GetAllImplementingDecorations(this object obj,
            Type typeToImplement,
            string cakeName = null)
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
        public static List<T> GetAllImplementingDecorations<T>(this object obj,
            string cakeName = null)
        {
            Type type = typeof(T);
            var list = obj.GetAllImplementingDecorations(type);
            var castList = list.ConvertListTo<T, object>();
            return castList;
        }
        #endregion
    }

    public static class HasExtensions
    {
        /// <summary>
        /// if an object has a decoration, determines if a decoration exists in its cake
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Has(this object obj, Type decType, string cakeName = null, bool exactTypeMatch = true)
        {
            if (obj == null)
                return false;

            var dec = obj.As(decType,cakeName, exactTypeMatch);
            return dec != null;
        }
        public static bool Has<T>(this object obj, string cakeName = null, bool exactTypeMatch = true)
        {
            return obj.Has(typeof(T),cakeName, exactTypeMatch);
        }
        ///// <summary>
        ///// does a non-exact type match on all decorations
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <param name="decTypes"></param>
        ///// <returns></returns>
        //public static bool Has(this object obj, params Type[] decTypes)
        //{
        //    if (obj == null)
        //        return false;

        //    var decs = obj.GetAllDecorations();
        //    List<Type> allDecTypes = new List<Type>();
        //    foreach (var dec in decs)
        //    {
        //        allDecTypes.Add(dec.GetType());
        //    }

        //    bool rv = true;

        //    //iterate thru all the decorations to look for
        //    foreach (var decType in decTypes)
        //    {
        //        bool isFound = false;
        //        foreach (var actualDecType in allDecTypes)
        //        {
        //            if (decType.IsAssignableFrom(actualDecType))
        //            {
        //                isFound = true;
        //                break;
        //            }
        //        }

        //        if (!isFound)
        //        {
        //            rv = false;
        //            break;
        //        }
        //    }

        //    return rv;
        //}

        //public static bool Has<T1, T2>(this object obj)
        //{
        //    return obj.Has(typeof(T1), typeof(T2));
        //}
        //public static bool Has<T1, T2, T3>(this object obj)
        //{
        //    return obj.Has(typeof(T1), typeof(T2), typeof(T3));
        //}
        //public static bool Has<T1, T2, T3, T4>(this object obj)
        //{
        //    return obj.Has(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        //}
        //public static bool Has<T1, T2, T3, T4, T5>(this object obj)
        //{
        //    return obj.Has(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        //}
        //public static bool Has<T1, T2, T3, T4, T5, T6>(this object obj)
        //{
        //    return obj.Has(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        //}
        //public static bool Has<T1, T2, T3, T4, T5, T6, T7>(this object obj)
        //{
        //    return obj.Has(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        //}
    }

    public static class DecorationExtensions
    {
        public static bool ToggleDecoration(this object obj,
            bool isEnabled,
            Type decType,
            bool exactTypeMatch = true,
            string cakeName = null)
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
        public static bool ToggleDecoration<T>(this object obj,
            bool isEnabled,
            bool exactTypeMatch = true,
            string cakeName = null)
        {
            return obj.ToggleDecoration(isEnabled, typeof(T), exactTypeMatch);
        }
    }

    public static class IHasADecorationExtensions
    {
        /// <summary>
        /// decorates as a T if the decoration is not present, using the supplied factory.
        /// does an exact type decoration search as the test.  returns the outermost
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static object ApplyDecorationIfNotPresent<T>(this object obj,
            Func<object, T> factory,
            string cakeName = null)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            //note we use an exact type search
            var face = obj.As<T>(true);
            if (face != null)
                return obj;

            face = factory(obj);
            return face;
        }
        /// <summary>
        /// if a cake constraint (aka IHasA) is declared anywhere in the stack
        /// we validate the current cake supports the constraint.  this is a topdown walk
        /// </summary>
        /// <param name="obj"></param>
        public static void ValidateIHasAConstraints(this object obj)
        {

            var cake = obj.GetAllDecorations();
            string cakeName = null;
            if (obj.IsADecoration())
            {
                cakeName = (obj as IDecoration).CakeName;
            }
            cake.WithEach(layer =>
            {
                if (layer is IHasA)
                {
                    //get all the interfaces it has, that derive from IHasDecoration
                    var layerType = layer.GetType();

                    var interfaces = layerType.GetInterfaces();
                    foreach (var interfaceType in interfaces)
                    {
                        if (!interfaceType.Name.Contains("IHasA`"))
                            continue;

                        var requiredDecorations = interfaceType.GetGenericArguments();

                        foreach (Type each in requiredDecorations)
                        {
                            if (!obj.Has(each, cakeName, false))
                                throw new InvalidOperationException(string.Format("required decoration {0} not found in cake", each.Name));
                        }
                    }
                }
            });
        }
    }

}
