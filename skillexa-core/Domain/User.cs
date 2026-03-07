namespace Skillexa.Core.Domain;

public class User : IEntity
{
    public long Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    public ICollection<Job> Jobs { get; set; } = [];
}
