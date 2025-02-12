using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.WebApi.Dto;
using System.Collections;

namespace WebApi.Tests.Controllers
{
    internal static class AssertHelper
    {

        /// <summary>
        /// Asserts that the result is a BadRequestObjectResult with the expected message.
        /// </summary>
        /// <param name="result">Action result.</param>
        /// <param name="message">Expected message</param>
        internal static void IsBadRequest(IActionResult result, string message)
        {
            var expected = ErrorResponse.BadRequest(message);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal(expected, errorResponse);
        }

        internal static void EqualsProperties<TEntity>(TEntity item, object itemVm)
        {
            typeof(TEntity).GetProperties().ToList()
                .ForEach(xProperty =>
                {
                    var xValue = xProperty.GetValue(item);
                    var yProperty = itemVm.GetType().GetProperty(xProperty.Name);
                    if (yProperty != null)
                    {
                        object? yValue = yProperty!.GetValue(itemVm);
                        if (xValue is IEnumerable xEnumerable && yValue is IEnumerable yEnumerable)
                            EnumerablesAreEqual(xEnumerable, yEnumerable);
                        else
                            Assert.Equal(xValue, yValue);
                    }
                });
        }

        /// <summary>
        /// Determines whether two enumerables are equal by comparing their elements.
        /// </summary>
        /// <param name="xEnumerable">The first enumerable to compare.</param>
        /// <param name="yEnumerable">The second enumerable to compare.</param>
        /// <returns><c>true</c> if the enumerables are equal; otherwise, <c>false</c>.</returns>
        private static void EnumerablesAreEqual(IEnumerable xEnumerable, IEnumerable yEnumerable)
        {
            var xEnumerator = xEnumerable.GetEnumerator();
            var yEnumerator = yEnumerable.GetEnumerator();
            while (xEnumerator.MoveNext())
            {
                if (!yEnumerator.MoveNext())
                    Assert.Fail();
                EqualsProperties(xEnumerator.Current, yEnumerator.Current);
            }
            Assert.False(yEnumerator.MoveNext());
        }
    }
}
