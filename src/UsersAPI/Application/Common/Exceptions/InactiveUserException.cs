namespace UsersAPI.Application.Common.Exceptions
{
    public sealed class InactiveUserException : Exception
    {
        public InactiveUserException()
            : base(ApplicationMessages.Authentication.InactiveUser)
        {
        }
    }
}
