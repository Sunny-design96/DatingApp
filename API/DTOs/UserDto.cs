namespace API.DTOs;
public class UserDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public string? ImageUrl { get; set; } //Need to check for ImageUrl before using it
    public string? Token {get; set;}
}