using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GATUtils.Utilities
{
    public static class EnumComparer
    {
        public static EnumComparer<TEnum> For<TEnum>()
            where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            return EnumComparer<TEnum>.s_Instance;
        }
    }

    public sealed class EnumComparer<TEnum> : IEqualityComparer<TEnum>, IComparer<TEnum>
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        #region Singleton
        /// <summary>
        /// The singleton accessor.
        /// </summary>
        public static readonly EnumComparer<TEnum> s_Instance;

        /// <summary>
        /// Initializes the <see cref="EnumComparer{TEnum}"/> class
        /// by generating the GetHashCode and Equals methods.
        /// </summary>
        static EnumComparer()
        {
            s_supportedTypes =
                new[]
                {
                    typeof (byte), typeof (sbyte), typeof (short), typeof (ushort),
                    typeof (int), typeof (uint), typeof (long), typeof (ulong)
                };
            _AssertTypeIsEnum();
            _AssertUnderlyingTypeIsSupported();
            s_getHashCode = _GenerateGetHashCode();
            s_equals = _GenerateEquals();
            s_compare = _GenerateCompare();

            s_Instance = new EnumComparer<TEnum>();
        }

        /// <summary>
        /// A private constructor to prevent user instantiation.
        /// </summary>
        private EnumComparer()
        {

        }
        #endregion

        // Public IEqualityComparer methods
        // ------------------
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <typeparamref name="TEnum"/> 
        /// to compare.</param>
        /// <param name="y">The second object of type <typeparamref name="TEnum"/> 
        /// to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(TEnum x, TEnum y)
        {
            // call the generated method
            return s_equals(x, y);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> 
        /// for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The type of <paramref name="obj"/> is a reference type and 
        /// <paramref name="obj"/> is null.
        /// </exception>
        public int GetHashCode(TEnum obj)
        {
            // call the generated method
            return s_getHashCode(obj);
        }

        /// <summary>
        ///   Compares two objects and returns a value indicating whether one is less than, equal to, 
        ///   or greater than the other.
        /// </summary>
        /// <param name = "x">The first object to compare.</param>
        /// <param name = "y">The second object to compare.</param>
        /// <returns>
        ///   Value Condition Less than zero <paramref name = "x" /> is less than <paramref name = "y" />.
        ///   Zero <paramref name = "x" /> equals <paramref name = "y" />.
        ///   Greater than zero <paramref name = "x" /> is greater than <paramref name = "y" />.
        /// </returns>
        public int Compare(TEnum x, TEnum y)
        {
            // call the generated method
            return s_compare(x, y);
        }

        // Private methods
        // ------------------
        private static void _AssertTypeIsEnum()
        {
            if (typeof(TEnum).IsEnum)
                return;

            var message =
                string.Format("The type parameter {0} is not an Enum. EnumComparer supports Enums only.", typeof(TEnum));
            throw new NotSupportedException(message);
        }

        private static void _AssertUnderlyingTypeIsSupported()
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

            if (s_supportedTypes.Contains(underlyingType))
                return;

            var message =
              string.Format("The underlying type of the type parameter {0} is {1}. " +
                            "LcgEnumComparer only supports Enums with underlying type of " +
                            "byte, sbyte, short, ushort, int, uint, long, or ulong.",
                            typeof(TEnum), underlyingType);
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Generates a comparison method similar to this:
        /// <code>
        /// bool Equals(TEnum x, TEnum y)
        /// {
        ///     return x == y;
        /// }
        /// </code>
        /// </summary>
        /// <returns>The generated method.</returns>
        private static Func<TEnum, TEnum, bool> _GenerateEquals()
        {
            var xParam = Expression.Parameter(typeof(TEnum), "x");
            var yParam = Expression.Parameter(typeof(TEnum), "y");
            var equalExpression = Expression.Equal(xParam, yParam);
            var expression = Expression.Lambda<Func<TEnum, TEnum, bool>>(equalExpression, new[] { xParam, yParam });
            return expression.Compile();
        }

        /// <summary>
        /// Generates a GetHashCode method similar to this:
        /// <code>
        /// int GetHashCode(TEnum obj)
        /// {
        ///     return ((int)obj).GetHashCode();
        /// }
        /// </code>
        /// </summary>
        /// <returns>The generated method.</returns>
        private static Func<TEnum, int> _GenerateGetHashCode()
        {
            var objParam = Expression.Parameter(typeof(TEnum), "obj");
            var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            var convertExpression = Expression.Convert(objParam, underlyingType);
            var getHashCodeMethod = underlyingType.GetMethod("GetHashCode");
            var getHashCodeExpression = Expression.Call(convertExpression, getHashCodeMethod);
            var expression = Expression.Lambda<Func<TEnum, int>>(getHashCodeExpression, new[] { objParam });
            return expression.Compile();
        }

        /// <summary>
        ///   Generates a Compare method similar to this:
        ///   <code>
        ///     int Compare(TEnum x, TEnum y)
        ///     {
        ///       return ((int)x).CompareTo(int(y));
        ///     }
        ///   </code>
        /// </summary>
        /// <returns>The generated method.</returns>
        private static Func<TEnum, TEnum, int> _GenerateCompare()
        {
            // This implementation calls CompareTo on underlying type: x.CompareTo(y)

            Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            ParameterExpression xParameter = Expression.Parameter(typeof(TEnum), "x");
            ParameterExpression yParameter = Expression.Parameter(typeof(TEnum), "y");
            UnaryExpression xCastedToUnderlyingType = Expression.Convert(xParameter, underlyingType);
            UnaryExpression yCastedToUnderlyingType = Expression.Convert(yParameter, underlyingType);
            var compareToMethod = underlyingType.GetMethod("CompareTo", new[] { underlyingType });
            var compareToExpression = Expression.Call(xCastedToUnderlyingType, compareToMethod, yCastedToUnderlyingType);
            var expression = Expression.Lambda<Func<TEnum, TEnum, int>>(compareToExpression, xParameter, yParameter);
            return expression.Compile();
        }

        // Private variables
        // ------------------
        private static readonly Func<TEnum, TEnum, bool> s_equals;
        private static readonly Func<TEnum, int> s_getHashCode;
        private static readonly Func<TEnum, TEnum, int> s_compare;
        private static readonly ICollection<Type> s_supportedTypes;
    }
}
