using System.Text.RegularExpressions;
using UsersAPI.Domain.Shared;

namespace UsersAPI.Domain.Users.ValueObjects
{
    public sealed class Email : ValueObject
    {
        private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$");

        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string value)
        {
            EnsureIsRequired(value);

            var normalizedValue = Normalize(value);

            EnsureValidFormat(normalizedValue);

            return new Email(normalizedValue);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        private static void EnsureIsRequired(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(DomainMessages.Email.Required);
        }

        private static string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        private static void EnsureValidFormat(string value)
        {
            if (!EmailRegex.IsMatch(value))
                throw new ArgumentException(DomainMessages.Email.InvalidFormat);
        }
    }
}
