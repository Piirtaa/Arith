using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Arith.DataStructures.Decorations;

namespace Arith.DataStructures
{
    /// <summary>
    /// a linked list that links the last and first items in a loop
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularLinkedList<T> : CircularLinkedListDecoration<T>,
        ICircularLinkedList<T>
    {
        #region Ctor
        public CircularLinkedList(params T[] items)
            : base(LinkedList<T>.New().GetCircularityCake<T>())
        {
            //^^note that hooks are a required layer for the circular decoration
            //which implements IHasDecoration<IHasHooks<T>>, or it will 
            //fail runtime validation.  so we decorate with hooks in the ctor.

            this.OuterNodeBuildingList.AddNodes(items);
        }
        #endregion
    }



}
