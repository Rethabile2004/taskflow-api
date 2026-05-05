namespace TaskFlowAPI.DTOs
{
    // what the client recieves after successful registration or login
    public class AuthResponseDto
    {
        // the JWT token the client will use for all subsequent request
        public string Token { get; set; } = string.Empty;
        // When the token expires
        public DateTime ExpiresAt { get; set; }
        // basic user info so the client can display it
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
