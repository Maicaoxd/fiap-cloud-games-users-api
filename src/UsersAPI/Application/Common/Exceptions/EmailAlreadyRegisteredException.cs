namespace UsersAPI.Application.Common.Exceptions
{
    public sealed class EmailAlreadyRegisteredException : Exception
    {
        public EmailAlreadyRegisteredException()
            : base(ApplicationMessages.User.EmailAlreadyRegistered)
        {
        }
    }
}
