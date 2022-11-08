namespace phat.Exceptions
{
    internal class ConsoleException : Exception
    {
        public override string? StackTrace => null;

        internal ConsoleException(String message) : base(message) { }




    }
}
