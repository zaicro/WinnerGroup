namespace FunEvents.Domain.Entities;

public class User
{
    public User()
    {
    }

    public User(
        int id,
        string username,
        string name,
        string email,
        string phone,
        string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException(
                "User username is required.",
                nameof(username));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "User name is required.",
                nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "User email is required.",
                nameof(email));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException(
                "User password is required.",
                nameof(password));

        var hasher = new PasswordHasher<User>();

        Id = id; 
        Username = username;
        Name = name;
        Email = email;
        Phone = phone;
        PasswordHash = hasher.HashPassword(this, password);
    }

    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
}