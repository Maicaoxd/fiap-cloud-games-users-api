namespace UsersAPI.Application.Common
{
    public static class ApplicationMessages
    {
        public static class User
        {
            public const string PasswordConfirmationDoesNotMatch = "As senhas não conferem.";
            public const string EmailAlreadyRegistered = "E-mail já cadastrado.";
            public const string CpfAlreadyRegistered = "CPF já cadastrado.";
            public const string NotFound = "Usuário não encontrado.";
        }

        public static class Authentication
        {
            public const string InvalidCredentials = "E-mail ou senha inválidos.";
            public const string InactiveUser = "Usuário inativo.";
        }

        public static class PasswordRecovery
        {
            public const string InvalidRecoveryData = "Os dados de recuperação de senha são inválidos.";
        }

        public static class Conflict
        {
            public const string UniqueConstraintViolation = "Já existe um registro com os mesmos dados.";
        }
    }
}
