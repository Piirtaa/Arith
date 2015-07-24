﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arith.DataStructures
{

    public interface ILinkedListNode<T>
    {
        T Value { get; }
        ILinkedListNode<T> NextNode { get; set; }
        ILinkedListNode<T> PreviousNode { get; set; }
        ILinkedList<T> ParentList { get; }
    }

    public interface ILinkedList<T>
    {
        Func<T, ILinkedListNode<T>> NodeBuildingStrategy { get; set; }
        ILinkedListNode<T> FirstNode { get; }
        ILinkedListNode<T> LastNode { get; }

        bool Contains(T val);
        bool Contains(ILinkedListNode<T> item);
        ILinkedListNode<T> InsertNode(ILinkedListNode<T> node, ILinkedListNode<T> before, ILinkedListNode<T> after);
        ILinkedList<T> Remove(ILinkedListNode<T> item);
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

    public static class ILinkedListExtensions
    {
        public static bool IsEmpty<T>(this ILinkedList<T> obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return obj.FirstNode == null;
        }
        public static ILinkedListNode<T> AddFirst<T>(this ILinkedList<T> obj, T val)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return obj.Insert(val, null, obj.FirstNode);
        }
        public static ILinkedListNode<T> AddLast<T>(this ILinkedList<T> obj, T val)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            
            return obj.Insert(val, obj.LastNode, null);
        }
        public static ILinkedListNode<T> Insert<T>(this ILinkedList<T> obj, 
            T val, ILinkedListNode<T> before, ILinkedListNode<T> after)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            ILinkedListNode<T> node = null;
            if (obj.NodeBuildingStrategy != null)
            {
                node = obj.NodeBuildingStrategy(val);
            }
            else
            {
                node = new LinkedListNode<T>(val, obj);
            }
            return obj.InsertNode(node, before, after);
        }
        public static void RemoveLast<T>(this ILinkedList<T> obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if(obj.LastNode != null)
                obj.Remove(obj.LastNode);
        }
        public static void RemoveFirst<T>(this ILinkedList<T> obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (obj.FirstNode != null)
                obj.Remove(obj.FirstNode);
        }

        /// <summary>
        /// a method to iterate thru the list either forwards or backwards
        /// </summary>
        /// <param name="action"></param>
        /// <param name="fromFirstToLast"></param>
        public static void Iterate<T>(this ILinkedList<T> obj, 
            Action<ILinkedListNode<T>> action, 
            bool fromFirstToLast)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (action == null)
                throw new ArgumentNullException("action");

            if (fromFirstToLast)
            {
                ILinkedListNode<T> node = obj.FirstNode;

                while (node != null)
                {
                    action(node);

                    if (node.IsLast())
                        break;

                    node = node.NextNode;
                }
            }
            else
            {
                ILinkedListNode<T> node = obj.LastNode;

                while (node != null)
                {
                    action(node);

                    if (node.IsFirst())
                        break;

                    node = node.PreviousNode;
                }
            }
        }

        /// <summary>
        /// performs Iterate, but on 2 lists, such that each are traversed in the same
        /// steps, in parallel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        /// <param name="fromFirstToLast"></param>
        public static void ParallelIterate<T>(this ILinkedList<T> obj,
            ILinkedList<T> list2,
            Action<ILinkedListNode<T>, ILinkedListNode<T>> action,
            bool fromFirstToLast)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (action == null)
                throw new ArgumentNullException("action");

            if (fromFirstToLast)
            {
                ILinkedListNode<T> node = obj.FirstNode;
                ILinkedListNode<T> node2 = list2.FirstNode;

                while (node != null)
                {
                    action(node, node2);

                    if (node.IsLast())
                        break;

                    node = node.NextNode;

                    if(node2 != null)
                        node2 = node2.NextNode;
                }
            }
            else
            {
                ILinkedListNode<T> node = obj.LastNode;
                ILinkedListNode<T> node2 = list2.LastNode;

                while (node != null)
                {
                    action(node, node2);

                    if (node.IsFirst())
                        break;

                    node = node.PreviousNode;

                    if (node2 != null)
                        node2 = node2.PreviousNode;
                }
            }
        }
        /// <summary>
        /// iterates from first to last and returns item from a positive filter.
        /// demonstrates good practice for iterating the list
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ILinkedListNode<T> Filter<T>(this ILinkedList<T> obj,
            Func<ILinkedListNode<T>, bool> filter,
            bool fromFirstToLast)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (filter == null)
                throw new ArgumentNullException("filter");

            if (fromFirstToLast)
            {
                ILinkedListNode<T> node = obj.FirstNode;

                while (node != null)
                {
                    if (filter(node))
                        break;

                    if (node.IsLast())
                    {
                        node = null;
                        break;
                    }
                    node = node.NextNode;
                }
                return node;
            }
            else
            {
                ILinkedListNode<T> node = obj.LastNode;

                while (node != null)
                {
                    if (filter(node))
                        break;

                    if (node.IsFirst())
                    {
                        node = null;
                        break;
                    }
                    node = node.PreviousNode;
                }
                return node;
            }
        }

    }
}
