using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.DTOs.User;
using MangaPublishingSystem.Application.Common.Security;
using BuildingBlocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.Services
{
    public class UserService : IUserService
    {
        private const int RoleIdSystemAdmin = 1;
        private const int RoleIdTantouEditor = 2;
        private const int RoleIdEditorialBoard = 3;
        private const int RoleIdMangaka = 4;
        private const int RoleIdAssistant = 5;

        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponseDto> CreateUserByAdminAsync(CreateUserByAdminDto dto)
        {
            if (await _userRepository.ExistsByEmailAsync(dto.Email))
            {
                throw new ConflictException("Email đã tồn tại trên hệ thống.");
            }

            if (await _userRepository.ExistsByUserNameAsync(dto.UserName))
            {
                throw new ConflictException("Tên đăng nhập đã tồn tại trên hệ thống.");
            }

            if (dto.RoleId == RoleIdMangaka && !string.IsNullOrWhiteSpace(dto.PenName))
            {
                if (await _userRepository.ExistsByPenNameAsync(dto.PenName))
                {
                    throw new ConflictException("Bút danh đã tồn tại trên hệ thống.");
                }
            }

            var randomPassword = GenerateRandomPassword();
            var passwordHash = _passwordHasher.HashPassword(randomPassword);

            var user = new User
            {
                RoleId = dto.RoleId,
                UserName = dto.UserName,
                PasswordHash = passwordHash,
                Email = dto.Email,
                FullName = dto.FullName,
                PenName = dto.PenName,
                PortfolioUrl = dto.PortfolioUrl,
                Skills = dto.Skills,
                Status = UserStatus.Active
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                if (dto.RoleId == RoleIdMangaka)
                {
                    var wallet = new Wallet
                    {
                        UserId = user.Id,
                        SetupFundBalance = 0.00m,
                        WithdrawableBalance = 0.00m,
                        LockedFund = 0.00m,
                        LockedWithdrawable = 0.00m
                    };

                    await _walletRepository.AddAsync(wallet);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            try
            {
                await _emailService.SendAccountInfoAsync(
                    dto.Email,
                    dto.UserName,
                    randomPassword
                );
            }
            catch (Exception)
            {
                // Gửi email thất bại không rollback DB vì tài khoản đã tạo hợp lệ.
            }

            return MapUserResponse(user, "Tạo tài khoản thành công và đã gửi thông tin đăng nhập qua email.");
        }

        public async Task<List<AssistantResponseDto>> GetPendingAssistantsAsync()
        {
            var assistants = await _userRepository.GetPendingAssistantsAsync();
            return assistants.Select(MapAssistant).ToList();
        }

        public async Task<UserResponseDto> ApproveUserAsync(int id)
        {
            var user = await GetUserOrThrowAsync(id);
            ValidatePendingAssistant(user, "phê duyệt");

            user.Status = UserStatus.Active;
            await _unitOfWork.SaveChangesAsync();

            return MapUserResponse(user, "Phê duyệt tài khoản trợ lý thành công.");
        }

        public async Task<UserResponseDto> RejectUserAsync(int id)
        {
            var user = await GetUserOrThrowAsync(id);
            ValidatePendingAssistant(user, "từ chối phê duyệt");

            user.Status = UserStatus.Rejected;
            await _unitOfWork.SaveChangesAsync();

            return MapUserResponse(user, "Từ chối phê duyệt tài khoản trợ lý thành công.");
        }

        public async Task<UserResponseDto> LockUserAsync(int id)
        {
            var user = await GetUserOrThrowAsync(id);

            if (user.RoleId == RoleIdSystemAdmin)
            {
                throw new BadRequestException("Không thể khóa tài khoản Quản trị viên hệ thống.");
            }

            if (user.Status != UserStatus.Active)
            {
                throw new BadRequestException("Chỉ có thể khóa tài khoản đang hoạt động.");
            }

            user.Status = UserStatus.Locked;
            await _unitOfWork.SaveChangesAsync();

            return MapUserResponse(user, "Khóa tài khoản thành công.");
        }

        private async Task<User> GetUserOrThrowAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng trên hệ thống.");
            }

            return user;
        }

        private static void ValidatePendingAssistant(User user, string action)
        {
            if (user.RoleId != RoleIdAssistant)
            {
                throw new BadRequestException($"Chỉ có thể {action} tài khoản Trợ lý.");
            }

            if (user.Status != UserStatus.Pending)
            {
                throw new BadRequestException("Tài khoản không ở trạng thái chờ duyệt.");
            }
        }

        private static UserResponseDto MapUserResponse(User user, string message)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                Status = user.Status.ToString(),
                PenName = user.PenName,
                IsOnLeave = user.IsOnLeave,
                Message = message
            };
        }

        private static AssistantResponseDto MapAssistant(User user)
        {
            return new AssistantResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Status = user.Status.ToString(),
                PortfolioUrl = user.PortfolioUrl,
                Skills = user.Skills
            };
        }

        private string GenerateRandomPassword()
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()_+=-[]{}|;:',./<>?";

            var random = new Random();
            var chars = new char[10];

            chars[0] = upper[random.Next(upper.Length)];
            chars[1] = lower[random.Next(lower.Length)];
            chars[2] = digits[random.Next(digits.Length)];
            chars[3] = special[random.Next(special.Length)];

            string allPossible = upper + lower + digits + special;
            for (int i = 4; i < 10; i++)
            {
                chars[i] = allPossible[random.Next(allPossible.Length)];
            }

            return new string(chars.OrderBy(x => random.Next()).ToArray());
        }
    }
}
