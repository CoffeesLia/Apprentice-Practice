
namespace Application.Helpers
{
    public static class EntityComparison
    {
        /// <summary>
        /// Verify if the property in the origin is the same on the result.
        /// </summary>
        /// <typeparam name="TOrigin">Type of origin.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        /// <param name="origin">object origin.</param>
        /// <param name="result">object result.</param>
        /// <param name="propertyName">Property name to be compared (Id is compared if not set).</param>
        /// <returns>Returns if the prop is the same in the origin and result.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool PropDiffer<TOrigin, TResult>(TOrigin origin, TResult result, string propertyName = "Id")
        {
            if (origin == null || result == null)
                throw new InvalidOperationException("Objetos inválidos para comparação");

            var typeOfOrign = typeof(TOrigin);
            var typeOfResult = typeof(TResult);

            var propOrigin = typeOfOrign.GetProperty(propertyName);
            var propResult = typeOfResult.GetProperty(propertyName);

            if (propOrigin == null || propResult == null)
                throw new InvalidOperationException("Objetos inválidos para comparação");

            return propOrigin.GetValue(origin) != propResult.GetValue(result);
        }

        /// <summary>
        /// Return the props on origin there are not found on result
        /// </summary>
        /// <typeparam name="TOrigin">Type of object on OriginList.</typeparam>
        /// <typeparam name="TResult">Type of object on ResultList.</typeparam>
        /// <param name="originList">List of objects Origin.</param>
        /// <param name="resultList">List of objects Result.</param>
        /// <param name="propertyName">Property name to be compared (Id is compared if not set).</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IEnumerable<int> PropsDiffer<TOrigin, TResult>(IEnumerable<TOrigin> originList, IEnumerable<TResult> resultList, string propertyName = "Id")
        {
            if (originList == null || resultList == null)
                throw new InvalidOperationException("Objetos inválidos para comparação");

            var response = new List<int>();

            var typeOfOrign = typeof(TOrigin);
            var typeOfResult = typeof(TResult);

            var propOrigin = typeOfOrign.GetProperty(propertyName);
            var propResult = typeOfResult.GetProperty(propertyName);

            if (propOrigin == null || propResult == null)
                throw new InvalidOperationException("Objetos inválidos para comparação");

            foreach (var origin in originList)
            {
                var idValue = propOrigin.GetValue(origin);

                if (idValue != null && !resultList.Any(f => propResult.GetValue(f) == idValue))
                    response.Add((Int32)idValue);
            }

            return response;
        }
    }
}
