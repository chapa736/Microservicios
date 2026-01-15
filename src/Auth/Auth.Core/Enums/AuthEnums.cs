namespace Auth.Core.Enums
{
    public enum UserStatus
    {
        Inactivo = 0,
        Activo = 1
    }

    public enum RoleType
    {
        Admin = 1,
        Cliente = 2
    }

    public enum TokenType
    {
        AccessToken = 1,
        RefreshToken = 2
    }
}