using System.Text.Json.Serialization;

namespace Stellantis.ProjectName.Application.Models
{
    /// <summary>
    /// Represents the result of an operation, including its status, message, and any errors.
    /// </summary>
    public class OperationResult
    {
        [JsonIgnore]
        public bool Success => Status == OperationStatus.Success;

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
        public IReadOnlyList<string> Errors { get; private set; }

        private OperationResult(OperationStatus status, string message, IReadOnlyList<string> errors)
        {
            Status = status;
            Message = message;
            Errors = errors;
        }

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing an error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>An <see cref="OperationResult"/> with error status.</returns>
        public static OperationResult Error(string message)
        {
            return new OperationResult(OperationStatus.Error, message, [message]);
        }

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing a successful operation.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <returns>An <see cref="OperationResult"/> with success status.</returns>
        public static OperationResult Complete(string message = "")
        {
            return new OperationResult(OperationStatus.Success, message, []);
        }

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing a conflict.
        /// </summary>
        /// <param name="message">The conflict message.</param>
        /// <returns>An <see cref="OperationResult"/> with conflict status.</returns>
        public static OperationResult Conflict(string message)
        {
            return new OperationResult(OperationStatus.Conflict, message, [message]);
        }

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing invalid data.
        /// </summary>
        /// <param name="message">The invalid data message.</param>
        /// <returns>An <see cref="OperationResult"/> with invalid data status.</returns>
        public static OperationResult InvalidData(string message)
        {
            return new OperationResult(OperationStatus.InvalidData, message, [message]);
        }

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing a not found error.
        /// </summary>
        /// <param name="message">The not found message.</param>
        /// <returns>An <see cref="OperationResult"/> with not found status.</returns>
        public static OperationResult NotFound(string message)
        {
            return new OperationResult(OperationStatus.NotFound, message, [message]);
        }
    }
}
