namespace WebApi.Tests.Filters
{
    public class TestException : Exception
    {
        public TestException()
        {
        }

        public TestException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public TestException(string message) : base(message)
        {
        }
    }
}