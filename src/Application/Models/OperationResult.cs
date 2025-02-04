using System.Text.Json.Serialization;

namespace Stellantis.ProjectName.Application.Models
{
    public class OperationResult
    {
        [JsonIgnore]
        public bool Success { get; internal set; }

        public string Message { get; internal set; } = string.Empty;

        public IEnumerable<string> Errors { get; internal set; } = [];

        public static OperationResult Error(string message)
        {
            return new OperationResult { Success = false, Errors = [message], Message = message };
        }

        public static OperationResult Complete(string message = "")
        {
            return new OperationResult { Success = true, Message = message };
        }
    }
}
