using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace WebApi.Tests.Controllers
{
    /// <summary>
    /// A generic equality comparer that compares all public properties of two objects of type <typeparamref name="T"/>.
    /// Review: transforma this classe em um biblioteca de testes para reparoveitar em outros projetos.
    /// </summary>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    internal class GeneralEqualityComparer<T> : GeneralEqualityComparer, IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y)
        {
            return base.Equals(x, y);
        }

        public int GetHashCode([DisallowNull] T obj)
        {
            return base.GetHashCode(obj);
        }
    }

    /// <summary>
    /// A generic equality comparer that compares all public properties of two objects of object type.
    /// </summary>
    internal class GeneralEqualityComparer : IEqualityComparer
    {
        /// <summary>
        /// Determines whether two enumerables are equal by comparing their elements.
        /// </summary>
        /// <param name="xEnumerable">The first enumerable to compare.</param>
        /// <param name="yEnumerable">The second enumerable to compare.</param>
        /// <returns><c>true</c> if the enumerables are equal; otherwise, <c>false</c>.</returns>
        private static bool EnumerablesAreEqual(IEnumerable xEnumerable, IEnumerable yEnumerable)
        {
            IEnumerator xEnumerator = xEnumerable.GetEnumerator();
            IEnumerator yEnumerator = yEnumerable.GetEnumerator();
            GeneralEqualityComparer comparer = new();
            while (xEnumerator.MoveNext())
            {
                if (!yEnumerator.MoveNext())
                {
                    return false;
                }

                if (!comparer.Equals(xEnumerator.Current, yEnumerator.Current))
                {
                    return false;
                }
            }

            return !yEnumerator.MoveNext();
        }

        /// <summary>
        /// Determines whether the specified objects are equal by comparing all their public properties.
        /// </summary>
        /// <param name="x">The first object of type <typeparamref name="T"/> to compare.</param>
        /// <param name="y">The second object of type <typeparamref name="T"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the specified objects are equal; otherwise, <c>false</c>.
        /// </returns>
        public new bool Equals(object? x, object? y)
        {
            if (x is null)
            {
                return y is null;
            }

            if (y is null)
            {
                return false;
            }

            Type xType = x.GetType();
            if (xType != y.GetType())
            {
                return false;
            }

            PropertyInfo[] properties = GetProperties(xType);
            foreach (PropertyInfo property in properties)
            {
                object? xValue = property.GetValue(x);
                object? yValue = property.GetValue(y);

                if (!object.Equals(xValue, yValue))
                {
                    if (xValue is IEnumerable xEnumerable && yValue is IEnumerable yEnumerable)
                    {
                        if (!EnumerablesAreEqual(xEnumerable, yEnumerable))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a hash code for the specified object by combining the hash codes of all its public properties.
        /// </summary>
        /// <param name="obj">The object for which to get the hash code.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the specified object is <c>null</c>.</exception>
        public int GetHashCode(object obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            PropertyInfo[] properties = GetProperties(obj.GetType());
            int hash = 17;
            foreach (PropertyInfo property in properties)
            {
                object? value = property.GetValue(obj);
                hash = (hash * 23) + (value?.GetHashCode() ?? 0);
            }
            return hash;
        }

        /// <summary>
        /// Gets all public instance properties of the type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>An array of <see cref="PropertyInfo"/> objects representing the public instance properties of the type <typeparamref name="T"/>.</returns>
        private static PropertyInfo[] GetProperties(Type xType)
        {
            return xType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }
    }
}
