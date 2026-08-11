namespace MerxoSell.API.DTOs.Auth;

public class LoginResponseDto
{
    public string AccessToken       { get; set; } = string.Empty;
    public int    ExpiresIn         { get; set; }   // seconds
    public int    UserId            { get; set; }
    public string FullName          { get; set; } = string.Empty;
    public string Email             { get; set; } = string.Empty;
    public string Role              { get; set; } = string.Empty;
    public int?   SellerId          { get; set; }
    public string PreferredCurrency { get; set; } = string.Empty;
}
