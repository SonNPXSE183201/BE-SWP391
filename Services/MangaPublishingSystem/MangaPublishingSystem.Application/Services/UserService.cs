using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;
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
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public UserService(
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<UserResponseDto> CreateUserByAdminAsync(CreateUserByAdminDto dto)
        {
            if (await _userRepository.ExistsByEmailAsync(dto.Email))
                throw new Exception("Email already exists");

            if (await _userRepository.ExistsByUserNameAsync(dto.UserName))
                throw new Exception("Username already exists");

            var randomPassword = GenerateRandomPassword();

            var user = new User
            {
                RoleId = dto.RoleId,
                UserName = dto.UserName,
                PasswordHash = HashPassword(randomPassword),
                Email = dto.Email,
                FullName = dto.FullName,
                PenName = dto.PenName,
                PortfolioUrl = dto.PortfolioUrl,
                Skills = dto.Skills,
                Status = UserStatus.Active
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Chỉ Mangaka mới được tạo ví khi Admin tạo tài khoản
            if (dto.RoleId == 2)
            {
                var wallet = new Wallet
                {
                    UserId = user.Id,
                    SetupFundBalance = 0,
                    WithdrawableBalance = 0,
                    LockedFund = 0,
                    LockedWithdrawable = 0
                };

                await _walletRepository.AddAsync(wallet);
                await _unitOfWork.SaveChangesAsync();
            }

            await _emailService.SendAccountInfoAsync(
                dto.Email,
                dto.UserName,
                randomPassword
            );

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
            if (await _userRepository.ExistsByEmailAsync(dto.Email))
                throw new Exception("Email already exists");

            if (await _userRepository.ExistsByUserNameAsync(dto.UserName))
                throw new Exception("Username already exists");

            var user = new User
            {
                RoleId = 4,
                UserName = dto.UserName,
                PasswordHash = "",
                Email = dto.Email,
                FullName = dto.FullName,
                PortfolioUrl = dto.PortfolioUrl,
                Skills = dto.Skills,
                Status = UserStatus.Pending
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Assistant vẫn có ví
            var wallet = new Wallet
            {
                UserId = user.Id,
                SetupFundBalance = 0,
                WithdrawableBalance = 0,
                LockedFund = 0,
                LockedWithdrawable = 0
            };

            await _walletRepository.AddAsync(wallet);
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

            user.Status = UserStatus.Active;

            await _unitOfWork.SaveChangesAsync();

            var response = MapAssistant(user);
            response.Message = "Assistant approved successfully.";

            return response;
        }

        public async Task<AssistantResponseDto> RejectAssistantAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new Exception("Assistant not found");

            user.Status = UserStatus.Rejected;

            await _unitOfWork.SaveChangesAsync();

            var response = MapAssistant(user);
            response.Message = "Assistant rejected successfully.";

            return response;
        }

        private AssistantResponseDto MapAssistant(User user)
        {
            return new AssistantResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Status = user.Status.ToString(),
                PortfolioUrl = user.PortfolioUrl,
                Skills = user.Skills,
                Message = "Assistant registration submitted successfully. Please wait for admin approval"
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