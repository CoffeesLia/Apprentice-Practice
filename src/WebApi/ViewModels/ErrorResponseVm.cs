namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class ErrorResponseVm
    {
        public int Code { get; private set; }
        public string Message { get; private set; }
        public ICollection<string>? Errors { get { return _Errors; } }
        private readonly List<string> _Errors = [];

        public ErrorResponseVm(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public ErrorResponseVm(int code, string message, ICollection<string> errors)
        {
            Code = code;
            Message = message;
            _Errors.AddRange(errors);
        }
    }
}
