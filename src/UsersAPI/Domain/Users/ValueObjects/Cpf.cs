using UsersAPI.Domain.Shared;

namespace UsersAPI.Domain.Users.ValueObjects
{
    public sealed class Cpf : ValueObject
    {
        private const int Length = 11;

        public string Value { get; }

        private Cpf(string value)
        {
            Value = value;
        }

        public static Cpf Create(string value)
        {
            EnsureIsRequired(value);

            var normalizedValue = Normalize(value);

            EnsureHasValidLength(normalizedValue);
            EnsureIsNotRepeatedDigits(normalizedValue);
            EnsureHasValidCheckDigits(normalizedValue);

            return new Cpf(normalizedValue);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        private static string Normalize(string value)
        {
            return new string(value.Where(char.IsDigit).ToArray());
        }

        private static void EnsureIsRequired(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(DomainMessages.Cpf.Required);
        }

        private static void EnsureHasValidLength(string value)
        {
            if (value.Length != Length)
                throw new ArgumentException(DomainMessages.Cpf.InvalidFormat);
        }

        private static void EnsureIsNotRepeatedDigits(string value)
        {
            if (value.Distinct().Count() == 1)
                throw new ArgumentException(DomainMessages.Cpf.InvalidFormat);
        }

        private static void EnsureHasValidCheckDigits(string value)
        {
            var expectedDigits = CalculateCheckDigits(value);

            if (!value.EndsWith(expectedDigits, StringComparison.Ordinal))
                throw new ArgumentException(DomainMessages.Cpf.InvalidFormat);
        }

        private static string CalculateCheckDigits(string value)
        {
            var firstDigit = CalculateCheckDigit(value[..9], initialMultiplier: 10);
            var secondDigit = CalculateCheckDigit(value[..9] + firstDigit, initialMultiplier: 11);

            return string.Concat(firstDigit, secondDigit);
        }

        private static int CalculateCheckDigit(string value, int initialMultiplier)
        {
            var sum = 0;

            for (var index = 0; index < value.Length; index++)
            {
                sum += (value[index] - '0') * (initialMultiplier - index);
            }

            var remainder = sum % 11;

            return remainder < 2 ? 0 : 11 - remainder;
        }
    }
}
