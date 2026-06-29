namespace AidDashboard.Application.Abstractions;

/// <summary>Abstraction over password hashing so the algorithm can be swapped centrally.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
