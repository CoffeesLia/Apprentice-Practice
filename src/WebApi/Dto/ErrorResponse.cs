namespace Stellantis.ProjectName.WebApi.Dto
{
    internal class ErrorResponse(string error, string message)
    {
        public string Error { get; } = error;
        public string Message { get; } = message;

        internal static ErrorResponse BadRequest(string message)
        {
            return new ErrorResponse("Bad Request", message);
        }

        public override bool Equals(object? obj)
        {
            if (obj is ErrorResponse errorResponse)
                return Error == errorResponse.Error && Message == errorResponse.Message;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine<string>($"{Error}:{Message}");
        }
    }
}
