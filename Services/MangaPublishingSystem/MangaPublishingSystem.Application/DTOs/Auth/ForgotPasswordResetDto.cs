namespace MangaPublishingSystem.Application.DTOs.Auth
{
    public class ForgotPasswordResetDto
    {
        public string Email { get; set; } = null!;
        public string VerificationCode { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
