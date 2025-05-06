namespace Stellantis.ProjectName.Application.Models
{
    public class ErrorResponseVm
    {
        public int Code { get; private set; }
        public string Message { get; private set; }
        public ICollection<string>? Errors { get { return this._Errors; } }
        private readonly List<string> _Errors = [];

        public ErrorResponseVm(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public ErrorResponseVm(int code, string message, Exception ex)
        {
            Code = code;
            Message = message;

            while (ex != null)
            {
                _Errors.Add(ex.Message);
            }
        }

        public ErrorResponseVm(int code, string message, ICollection<string> errors)
        {
            Code = code;
            Message = message;
            _Errors.AddRange(errors);
        }
    }
}
