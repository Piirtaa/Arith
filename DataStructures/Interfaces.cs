using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arith.DataStructures
{

    public interface ILinkedListNode<T>
    {
        T NodeValue { get; }
        ILinkedListNode<T> NextNode { get; set; }
        ILinkedListNode<T> PreviousNode { get; set; }
        ILinkedList<T> ParentList { get; }
    }

    public interface ILinkedList<T>
    {
        ILinkedListNode<T> FirstNode { get; }
        ILinkedListNode<T> LastNode { get; }

        bool Contains(T val);
        bool Contains(ILinkedListNode<T> item);
    }
    public static class ILinkedListExtensions
    {
        public static bool IsEmpty<T>(this ILinkedList<T> list)
        {
            return list.FirstNode == null;
        }
    }
    public static class ILinkedListNodeExtensions
    {
        public static bool IsFirst<T> (this ILinkedListNode<T> node)
        {
            return object.ReferenceEquals(node.ParentList.FirstNode, node);
        }
        public static bool IsLast<T>(this ILinkedListNode<T> node)
        {
            return object.ReferenceEquals(node.ParentList.LastNode, node);
        }
        /// <summary>
        /// given any node in the linked list, walks to the end
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static ILinkedListNode<T> GetLastByWalkingList<T>(this ILinkedListNode<T> node)
        {
            while (node != null && node.IsLast() == false)
            {
                node = node.NextNode;
            }

            return node;
        }
        /// <summary>
        /// given any node in the linked list, walks to the beginning
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static ILinkedListNode<T> GetFirstByWalkingList<T>(this ILinkedListNode<T> node)
        {
            while (node != null && node.IsFirst() == false)
            {
                node = node.PreviousNode;
            }

            return node;
        }
        /// <summary>
        /// validates the after node follows the before node
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public static bool IsPreceding<T>(this ILinkedListNode<T> before, ILinkedListNode<T> after)
        {
            if (before == null && after == null)
            {
                return false;
            }

            if (before == null)
            {
                return after.PreviousNode == null;
            }

            if (after == null)
            {
                return before.NextNode == null;
            }

            if (!object.ReferenceEquals(before.NextNode, after))
                return false;

            if (!object.ReferenceEquals(after.PreviousNode, before))
                return false;

            return true;
        }
    }


}
