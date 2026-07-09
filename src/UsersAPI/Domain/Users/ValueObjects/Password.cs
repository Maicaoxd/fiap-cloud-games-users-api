using UsersAPI.Domain.Shared;

namespace UsersAPI.Domain.Users.ValueObjects
{
    public sealed class Password : ValueObject
    {
        private const int MinimumLength = 8;

        public string Value { get; }

        private Password(string value)
        {
            Value = value;
        }

        public static Password Create(string value)
        {
            EnsureIsRequired(value);
            EnsureHasNoWhiteSpace(value);
            EnsureMinimumLength(value);
            EnsureHasLetter(value);
            EnsureHasNumber(value);
            EnsureHasSpecialCharacter(value);

            return new Password(value);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        private static void EnsureIsRequired(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(DomainMessages.Password.Required);
        }

        private static void EnsureHasNoWhiteSpace(string value)
        {
            if (value.Any(char.IsWhiteSpace))
                throw new ArgumentException(DomainMessages.Password.WhiteSpaceNotAllowed);
        }

        private static void EnsureMinimumLength(string value)
        {
            if (value.Length < MinimumLength)
                throw new ArgumentException(DomainMessages.Password.MinimumLength);
        }

        private static void EnsureHasLetter(string value)
        {
            if (!value.Any(char.IsLetter))
                throw new ArgumentException(DomainMessages.Password.LetterRequired);
        }

        private static void EnsureHasNumber(string value)
        {
            if (!value.Any(char.IsDigit))
                throw new ArgumentException(DomainMessages.Password.NumberRequired);
        }

        private static void EnsureHasSpecialCharacter(string value)
        {
            if (!value.Any(character => !char.IsLetterOrDigit(character)))
                throw new ArgumentException(DomainMessages.Password.SpecialCharacterRequired);
        }
    }
}
