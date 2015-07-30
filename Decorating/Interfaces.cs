using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arith.Decorating
{
    /// <summary>
    /// most basic definition of a decoration.  Something that wraps something else
    /// </summary>
    public interface IDecoration
    {
        /// <summary>
        /// provides discriminator to group layers together
        /// </summary>
        string DecorationName { get; }

        /// <summary>
        /// the immediate thing we are decorating
        /// </summary>
        object Decorated { get; }

        /// <summary>
        /// the innermost decorated thing
        /// </summary>
        object Inner { get; }

        /// <summary>
        /// Essentially is a clone mechanism.  Allow the current decoration to recreate an instance like itself when
        /// provided with a thing to decorate - think of this as a ctor with only one arg (the thing) and all other args
        /// coming from the current instance.
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        IDecoration ApplyThisDecorationTo(object thing);
    }

    /// <summary>
    /// a decoration that is aware of who is decorating it
    /// </summary>
    public interface IDecoratorAwareDecoration : IDecoration
    {
        /// <summary>
        /// the thing decorating this
        /// </summary>
        object Decorator { get; set; }
        /// <summary>
        /// the outermost decoration in the stack
        /// </summary>
        object Outer { get; }
    }

    /// <summary>
    /// marker interface. If present on a decoration it prevents other decorations from decorating it
    /// </summary>
    public interface ISealedDecoration
    {

    }
    public interface ITogglingDecoration : IDecoration
    {
        bool IsDecorationEnabled { get; set; }
    }

    /// <summary>
    /// specifies that T is contained in the decoration layer cake
    /// and provides strongly typed property to pull this from the cake
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDecorationOf<T> : IDecoration, IIsA<T>
    {
        T DecoratedOf { get; }
    }

    //a bunch of marker interfaces to use as placeholders, type constraints 
    //indicating that the decoration cake contains the specified type,
    //ie. that an Is<T> operation will succeed

    public interface IIsA { }
    public interface IIsA<T1> : IIsA { }
    public interface IIsA<T1, T2> : IIsA { }
    public interface IIsA<T1, T2, T3> : IIsA { }
    public interface IIsA<T1, T2, T3, T4> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> : IIsA { }
    public interface IIsA<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> : IIsA { }

    
}
