namespace backend.Contracts.Auth;

public class AuthTokenRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
