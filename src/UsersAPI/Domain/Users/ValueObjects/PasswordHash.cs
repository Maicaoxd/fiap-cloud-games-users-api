using UsersAPI.Domain.Shared;

namespace UsersAPI.Domain.Users.ValueObjects
{
    public sealed class PasswordHash : ValueObject
    {
        public string Value { get; }

        private PasswordHash(string value)
        {
            Value = value;
        }

        public static PasswordHash Create(string value)
        {
            EnsureIsRequired(value);

            return new PasswordHash(value);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        private static void EnsureIsRequired(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(DomainMessages.PasswordHash.Required);
        }
    }
}
