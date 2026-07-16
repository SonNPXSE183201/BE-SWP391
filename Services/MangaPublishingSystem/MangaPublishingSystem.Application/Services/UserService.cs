using DomainUser = MangaPublishingSystem.Domain.Entities.User;
using DomainWallet = MangaPublishingSystem.Domain.Entities.Wallet;
using MangaPublishingSystem.Domain.Enums;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.DTOs.User;
using MangaPublishingSystem.Application.Common.Security;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Web.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.Services
{
    public class UserService : IUserService
    {
        private const int RoleIdSystemAdmin = 1;
        private const int RoleIdMangaka = 4;
        private const int RoleIdAssistant = 5;

        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IBoardVotingService _boardVotingService;

        public UserService(
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IPasswordHasher passwordHasher,
            IBoardVotingService boardVotingService)
        {
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
            _boardVotingService = boardVotingService;
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

            if (dto.AssignedEditorId.HasValue)
            {
                if (dto.RoleId != RoleIdMangaka)
                {
                    throw new BadRequestException("Chỉ được phép gán Biên tập viên phụ trách cho tài khoản Tác giả (Mangaka).");
                }

                var editor = await _userRepository.GetByIdWithDetailsAsync(dto.AssignedEditorId.Value);
                if (editor == null || editor.Role?.RoleName != "Tantou Editor" || editor.Status != UserStatus.Active)
                {
                    throw new BadRequestException("Biên tập viên được gán không tồn tại hoặc không hoạt động.");
                }
            }

            var randomPassword = GenerateRandomPassword();
            var passwordHash = _passwordHasher.HashPassword(randomPassword);

            var user = new DomainUser
            {
                RoleId = dto.RoleId,
                UserName = dto.UserName,
                PasswordHash = passwordHash,
                Email = dto.Email,
                FullName = dto.FullName,
                PenName = dto.PenName,
                PortfolioUrl = dto.PortfolioUrl,
                Skills = dto.Skills,
                Status = UserStatus.Active,
                AssignedEditorId = dto.AssignedEditorId,
                CitizenId = dto.CitizenId,
                CitizenIdIssueDate = dto.CitizenIdIssueDate,
                CitizenIdIssuePlace = dto.CitizenIdIssuePlace
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                if (dto.RoleId == RoleIdMangaka)
                {
                    var wallet = new DomainWallet
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
                // Hỗ trợ dev/E2E khi SMTP chưa cấu hình — tương tự log OTP
                Console.WriteLine($"\n>>> [DEBUG ACCOUNT] Tài khoản mới: {dto.UserName} / {dto.Email} — mật khẩu: {randomPassword}\n");
                await _emailService.SendAccountInfoAsync(dto.Email, dto.UserName, randomPassword);
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

        public async Task<PagedResult<UserListItemDto>> GetUsersAsync(
            string? role,
            string? status,
            string? search,
            int pageNumber,
            int pageSize)
        {
            var pagedUsers = await _userRepository.GetUsersFilteredPagedAsync(role, status, search, pageNumber, pageSize);
            var items = pagedUsers.Items.Select(MapUserListItem).ToList();

            return new PagedResult<UserListItemDto>(
                items,
                pagedUsers.PageNumber,
                pagedUsers.PageSize,
                pagedUsers.TotalItems,
                pagedUsers.TotalPages);
        }

        public async Task<AssistantProfileResponseDto> ApproveAssistantAsync(int id, ApproveAssistantRequestDto dto)
        {
            if (!int.TryParse(dto.UserId, out var dtoUserId) || dtoUserId != id)
            {
                throw new BadRequestException("Mã người dùng trong yêu cầu không khớp với đường dẫn API.");
            }
            if (dto.Approved)
            {
                await ApproveUserAsync(id);
            }
            else
            {
                await RejectUserAsync(id);
            }

            var user = await _userRepository.GetByIdWithDetailsAsync(id)
                ?? throw new NotFoundException("Không tìm thấy người dùng trên hệ thống.");

            var profile = user.AssistantProfile;

            return new AssistantProfileResponseDto
            {
                Id = (profile?.Id ?? user.Id).ToString(),
                CreatedAt = (profile?.CreateAt ?? user.CreateAt).ToUniversalTime().ToString("o"),
                UpdatedAt = (profile?.UpdateAt ?? user.UpdateAt)?.ToUniversalTime().ToString("o"),
                UserId = user.Id.ToString(),
                PortfolioUrl = user.PortfolioUrl,
                SpecialtyTags = ParseSpecialtyTags(profile?.SpecialtyTags ?? user.Skills),
                TotalTasksCompleted = profile?.TotalCompletedTasks ?? 0,
                AverageRating = profile?.AverageRating ?? 0,
                AccountStatus = MapAccountStatus(user.Status)
            };
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
            await _boardVotingService.ClearChairIfUserDeactivatedAsync(id);
            await _boardVotingService.NotifyBoardMembershipChangedAsync(id);

            var message = "Khóa tài khoản thành công.";
            var config = await _boardVotingService.GetConfigAsync();
            if (user.RoleId == config.BoardRoleId)
            {
                var activeBoardMembersCount = (await _userRepository.FindAsync(u => 
                    u.RoleId == config.BoardRoleId && u.Status == UserStatus.Active)).Count();
                
                if (activeBoardMembersCount < 3)
                {
                    message += $" Cảnh báo: Hội đồng hiện chỉ còn {activeBoardMembersCount} thành viên hoạt động. Biểu quyết sẽ bị gián đoạn cho đến khi có đủ tối thiểu 3 thành viên.";
                }
            }

            return MapUserResponse(user, message);
        }

        public async Task<UserResponseDto> UnlockUserAsync(int id)
        {
            var user = await GetUserOrThrowAsync(id);

            if (user.Status != UserStatus.Locked)
            {
                throw new BadRequestException("Chỉ có thể mở khóa tài khoản đang bị khóa.");
            }

            user.Status = UserStatus.Active;
            await _unitOfWork.SaveChangesAsync();
            await _boardVotingService.NotifyBoardMembershipChangedAsync(id);

            return MapUserResponse(user, "Mở khóa tài khoản thành công.");
        }

        private async Task<DomainUser> GetUserOrThrowAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng trên hệ thống.");
            }

            return user;
        }

        private static void ValidatePendingAssistant(DomainUser user, string action)
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

        private static UserResponseDto MapUserResponse(DomainUser user, string message)
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
                AssignedEditorId = user.AssignedEditorId,
                AssignedEditorName = user.AssignedEditor?.FullName,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Message = message
            };
        }

        private static AssistantResponseDto MapAssistant(DomainUser user)
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

        private static UserListItemDto MapUserListItem(DomainUser user)
        {
            return new UserListItemDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                FullName = user.FullName,
                Role = MapRoleForFe(user.Role?.RoleName ?? string.Empty),
                Status = user.Status.ToString(),
                CreatedAt = user.CreateAt.ToUniversalTime().ToString("o"),
                AssignedEditorId = user.AssignedEditorId,
                AssignedEditorName = user.AssignedEditor?.FullName,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl
            };
        }

        private static string MapRoleForFe(string roleName)
        {
            return roleName switch
            {
                "Tantou Editor" => "Editor",
                "Editorial Board" => "Board",
                _ => roleName
            };
        }

        private static string MapAccountStatus(UserStatus status)
        {
            return status switch
            {
                UserStatus.Pending => "PendingApproval",
                UserStatus.Active => "Active",
                UserStatus.Rejected => "Deactivated",
                UserStatus.Locked => "Suspended",
                _ => status.ToString()
            };
        }

        private static List<string> ParseSpecialtyTags(string? tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return new List<string>();
            }

            return tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        public async Task<UserResponseDto> UpdateUserByAdminAsync(int id, UpdateUserByAdminDto dto)
        {
            var user = await _userRepository.GetByIdWithDetailsAsync(id);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng trên hệ thống.");
            }

            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _userRepository.ExistsByEmailAsync(dto.Email))
                {
                    throw new ConflictException("Email đã tồn tại trên hệ thống.");
                }
            }

            if (user.RoleId == RoleIdMangaka && !string.IsNullOrWhiteSpace(dto.PenName))
            {
                if (!string.Equals(user.PenName, dto.PenName, StringComparison.OrdinalIgnoreCase))
                {
                    if (await _userRepository.ExistsByPenNameAsync(dto.PenName))
                    {
                        throw new ConflictException("Bút danh đã tồn tại trên hệ thống.");
                    }
                }
            }

            if (dto.AssignedEditorId.HasValue)
            {
                if (user.RoleId != RoleIdMangaka)
                {
                    throw new BadRequestException("Chỉ được phép gán Biên tập viên phụ trách cho tài khoản Tác giả (Mangaka).");
                }

                var editor = await _userRepository.GetByIdWithDetailsAsync(dto.AssignedEditorId.Value);
                if (editor == null || editor.Role?.RoleName != "Tantou Editor" || editor.Status != UserStatus.Active)
                {
                    throw new BadRequestException("Biên tập viên được gán không tồn tại hoặc không hoạt động.");
                }
            }

            user.Email = dto.Email;
            user.FullName = dto.FullName;
            if (user.RoleId == RoleIdMangaka)
            {
                user.PenName = dto.PenName;
                user.AssignedEditorId = dto.AssignedEditorId;
            }
            user.PortfolioUrl = dto.PortfolioUrl;
            user.Skills = dto.Skills;
            
            if (dto.CitizenId != null) user.CitizenId = dto.CitizenId;
            if (dto.CitizenIdIssueDate.HasValue) user.CitizenIdIssueDate = dto.CitizenIdIssueDate.Value;
            if (dto.CitizenIdIssuePlace != null) user.CitizenIdIssuePlace = dto.CitizenIdIssuePlace;

            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var updatedUser = await _userRepository.GetByIdWithDetailsAsync(user.Id);
            return MapUserResponse(updatedUser!, "Cập nhật thông tin tài khoản thành công.");
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
