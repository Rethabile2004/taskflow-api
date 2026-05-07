using TaskFlowAPI.Models;

namespace TaskFlowAPI.Services
{
    public interface ITokenService
    {
        // takes a user and returns a signed JWT string
        string GenerateToken(User user);
    }
}
