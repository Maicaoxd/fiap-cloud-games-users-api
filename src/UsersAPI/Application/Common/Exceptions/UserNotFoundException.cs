namespace UsersAPI.Application.Common.Exceptions
{
    public sealed class UserNotFoundException : Exception
    {
        public UserNotFoundException()
            : base(ApplicationMessages.User.NotFound)
        {
        }
    }
}
