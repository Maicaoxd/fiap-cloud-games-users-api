namespace UsersAPI.Application.Common.Exceptions
{
    public sealed class CpfAlreadyRegisteredException : Exception
    {
        public CpfAlreadyRegisteredException()
            : base(ApplicationMessages.User.CpfAlreadyRegistered)
        {
        }
    }
}
