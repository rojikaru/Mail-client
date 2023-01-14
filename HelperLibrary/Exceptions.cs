namespace HelperLibrary
{
    public class MailServerException : ApplicationException
    {
        public MailServerException() : base() { }
        public MailServerException(string? Message) : base(Message) { }
    }
    public class UserCredentialsException : ApplicationException
    {
        public UserCredentialsException() : base() { }
        public UserCredentialsException(string? Message) : base(Message) { }
    }
}
