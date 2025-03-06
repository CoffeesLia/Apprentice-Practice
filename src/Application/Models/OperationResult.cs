using FluentValidation.Results;
using System.Text.Json.Serialization;

namespace Stellantis.ProjectName.Application.Models
{
    /// <summary>
    /// Represents the result of an operation, including its status, message, and any errors.
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Gets the status of the operation.
        /// </summary>
        [JsonIgnore]
        public OperationStatus Status { get; private set; }

        /// <summary>
        /// Gets the message associated with the operation result.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the errors associated with the operation result.
        /// </summary>
        public IEnumerable<string> Errors { get; private set; }

        private OperationResult(OperationStatus status, string message, IEnumerable<string> errors)
        {
            Status = status;
            Message = message;
            Errors = errors;
        }

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing a successful operation.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <returns>An <see cref="OperationResult"/> with success status.</returns>
        public static OperationResult Complete(string message = "")
        {
            return new OperationResult(OperationStatus.Success, message, new List<string>());
        }

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing a conflict.
        /// </summary>
        /// <param name="message">The conflict message.</param>
        /// <returns>An <see cref="OperationResult"/> with conflict status.</param>
        public static OperationResult Conflict(string message)
        {
            return new OperationResult(OperationStatus.Conflict, message, new List<string> { message });
        }

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing invalid data.
        /// </summary>
        /// <param name="result">The validation result.</param>
        /// <returns>An <see cref="OperationResult"/> with invalid data status.</returns>
        internal static OperationResult InvalidData(string v, ValidationResult result)
        {
            return new OperationResult(OperationStatus.InvalidData, "", result.Errors.Select(e => e.ErrorMessage));
        }

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing a not found error.
        /// </summary>
        /// <param name="message">The not found message.</param>
        /// <returns>An <see cref="OperationResult"/> with not found status.</returns>
        public static OperationResult NotFound(string message)
        {
            return new OperationResult(OperationStatus.NotFound, message, new List<string> { message });
        }
    }
}
