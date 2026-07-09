using UsersAPI.Domain.Shared;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Domain.Users
{
    public class User : Entity
    {
        public string Name { get; private set; }
        public Email Email { get; private set; }
        public Cpf Cpf { get; private set; }
        public DateOnly BirthDate { get; private set; }
        public PasswordHash PasswordHash { get; private set; }
        public UserRole Role { get; private set; }

        private User()
        {
            Name = null!;
            Email = null!;
            Cpf = null!;
            PasswordHash = null!;
        }

        private User(
            string name,
            Email email,
            Cpf cpf,
            DateOnly birthDate,
            PasswordHash passwordHash,
            UserRole role,
            Guid? createdBy)
            : base(createdBy)
        {
            Name = name;
            Email = email;
            Cpf = cpf;
            BirthDate = birthDate;
            PasswordHash = passwordHash;
            Role = role;
        }

        public static User Create(
            string name,
            Email email,
            Cpf cpf,
            DateOnly birthDate,
            PasswordHash passwordHash,
            Guid? createdBy = null)
        {
            EnsureNameIsRequired(name);
            EnsureEmailIsRequired(email);
            EnsureCpfIsRequired(cpf);
            EnsureBirthDateIsRequired(birthDate);
            EnsurePasswordHashIsRequired(passwordHash);

            return new User(name, email, cpf, birthDate, passwordHash, UserRole.User, createdBy);
        }

        public void Activate(Guid activatedBy)
        {
            MarkAsActivated(activatedBy);
        }

        public void Deactivate(Guid deactivatedBy)
        {
            MarkAsDeactivated(deactivatedBy);
        }

        public void ChangeName(string name, Guid updatedBy)
        {
            EnsureNameIsRequired(name);

            Name = name;
            MarkAsUpdated(updatedBy);
        }

        public void ChangeEmail(Email email, Guid updatedBy)
        {
            EnsureEmailIsRequired(email);

            Email = email;
            MarkAsUpdated(updatedBy);
        }

        public void ChangePassword(PasswordHash passwordHash, Guid updatedBy)
        {
            EnsurePasswordHashIsRequired(passwordHash);

            PasswordHash = passwordHash;
            MarkAsUpdated(updatedBy);
        }

        public void UpdateProfile(string name, Email email, Guid updatedBy)
        {
            EnsureNameIsRequired(name);
            EnsureEmailIsRequired(email);

            Name = name;
            Email = email;
            MarkAsUpdated(updatedBy);
        }

        public void UpdateProfile(string name, Email email, Cpf cpf, DateOnly birthDate, Guid updatedBy)
        {
            EnsureNameIsRequired(name);
            EnsureEmailIsRequired(email);
            EnsureCpfIsRequired(cpf);
            EnsureBirthDateIsRequired(birthDate);

            Name = name;
            Email = email;
            Cpf = cpf;
            BirthDate = birthDate;
            MarkAsUpdated(updatedBy);
        }

        public bool MatchesRecoveryData(Cpf cpf, DateOnly birthDate)
        {
            return Cpf.Equals(cpf) && BirthDate == birthDate;
        }

        private static void EnsureNameIsRequired(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(DomainMessages.User.NameRequired);
        }

        private static void EnsureEmailIsRequired(Email email)
        {
            if (email is null)
                throw new ArgumentException(DomainMessages.Email.Required);
        }

        private static void EnsureCpfIsRequired(Cpf cpf)
        {
            if (cpf is null)
                throw new ArgumentException(DomainMessages.Cpf.Required);
        }

        private static void EnsureBirthDateIsRequired(DateOnly birthDate)
        {
            if (birthDate == default)
                throw new ArgumentException(DomainMessages.User.BirthDateRequired);

            if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException(DomainMessages.User.BirthDateCannotBeInFuture);
        }

        private static void EnsurePasswordHashIsRequired(PasswordHash passwordHash)
        {
            if (passwordHash is null)
                throw new ArgumentException(DomainMessages.PasswordHash.Required);
        }
    }
}
