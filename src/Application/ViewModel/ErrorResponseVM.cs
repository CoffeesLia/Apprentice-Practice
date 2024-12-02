namespace Application.ViewModel
{
    public class ErrorResponseVM
    {
        public int Code { get; private set; }
        public string Message { get; private set; }
        public List<string>? Errors { get; private set; }

        public ErrorResponseVM(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public ErrorResponseVM(int code, string message, Exception ex)
        {
            Code = code;
            Message = message;
            Errors = [];

            while (ex != null)
            {
                Errors.Add(ex.Message);
            }
        }

        public ErrorResponseVM(int code, string message, List<string> errors)
        {
            Code = code;
            Message = message;
            Errors = errors;
        }
    }
}
