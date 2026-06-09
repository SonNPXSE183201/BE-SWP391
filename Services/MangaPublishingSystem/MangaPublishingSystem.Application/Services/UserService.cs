using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.DTOs.User;
using System.Security.Cryptography;
using System.Text;

namespace MangaPublishingSystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public UserService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<UserResponseDto> CreateUserByAdminAsync(CreateUserByAdminDto dto)
        {
            var randomPassword = GenerateRandomPassword();

            var user = new User
            {
                RoleId = dto.RoleId,
                UserName = dto.UserName,
                PasswordHash = HashPassword(randomPassword),
                Email = dto.Email,
                FullName = dto.FullName,
                Status = "Active"
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await _emailService.SendAccountInfoAsync(
                    dto.Email,
                    dto.UserName,
                    randomPassword
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("SEND MAIL ERROR: " + ex.Message);
            }

            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId
            };
        }

        public async Task<AssistantResponseDto> RegisterAssistantAsync(AssistantRegisterDto dto)
        {
            var user = new User
            {
                RoleId = 4,
                UserName = dto.UserName,
                PasswordHash = "",
                Email = dto.Email,
                FullName = dto.FullName,
                PortfolioUrl = dto.PortfolioUrl,
                Skills = dto.Skills,
                Status = "Pending"
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return MapAssistant(user);
        }

        public async Task<List<AssistantResponseDto>> GetPendingAssistantsAsync()
        {
            var assistants = await _userRepository.GetPendingAssistantsAsync();

            return assistants.Select(MapAssistant).ToList();
        }

        public async Task<AssistantResponseDto> ApproveAssistantAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new Exception("Assistant not found");

            user.Status = "Active";

            await _unitOfWork.SaveChangesAsync();

            return MapAssistant(user);
        }

        public async Task<AssistantResponseDto> RejectAssistantAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new Exception("Assistant not found");

            user.Status = "Rejected";

            await _unitOfWork.SaveChangesAsync();

            return MapAssistant(user);
        }

        private AssistantResponseDto MapAssistant(User user)
        {
            return new AssistantResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Status = user.Status,
                PortfolioUrl = user.PortfolioUrl,
                Skills = user.Skills
            };
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            var random = new Random();

            return new string(Enumerable.Repeat(chars, 8)
                .Select(x => x[random.Next(x.Length)])
                .ToArray());
        }
    }
}