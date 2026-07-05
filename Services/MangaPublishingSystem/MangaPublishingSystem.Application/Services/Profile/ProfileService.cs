using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Profile;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices.Profile;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAssistantProfileRepository _assistantProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileService(
            IUserRepository userRepository,
            IAssistantProfileRepository assistantProfileRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _assistantProfileRepository = assistantProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProfileResponseDto> GetMyProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdWithDetailsAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("Tài khoản không tồn tại.");
            }

            return new ProfileResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = user.Role?.RoleName ?? "",
                PenName = user.PenName,
                PortfolioUrl = user.PortfolioUrl,
                Skills = user.Skills,
                SpecialtyTags = user.AssistantProfile?.SpecialtyTags,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl
            };
        }

        public async Task<ProfileResponseDto> UpdateMyProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdWithDetailsAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("Tài khoản không tồn tại.");
            }

            var roleName = user.Role?.RoleName ?? "";

            // Cập nhật các trường chung cho tất cả role
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;

            // Xử lý cập nhật tùy theo Role
            if (roleName == "Assistant")
            {
                if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
                if (dto.PortfolioUrl != null) user.PortfolioUrl = dto.PortfolioUrl;
                if (dto.Skills != null) user.Skills = dto.Skills;

                if (user.AssistantProfile == null)
                {
                    user.AssistantProfile = new AssistantProfile
                    {
                        AssistantId = userId,
                        SpecialtyTags = dto.SpecialtyTags,
                        TotalCompletedTasks = 0,
                        OnTimeRate = 0,
                        DisputeRate = 0,
                        CurrentActiveTasks = 0,
                        AverageRating = 0
                    };
                    await _assistantProfileRepository.AddAsync(user.AssistantProfile);
                }
                else
                {
                    if (dto.SpecialtyTags != null) user.AssistantProfile.SpecialtyTags = dto.SpecialtyTags;
                    _assistantProfileRepository.Update(user.AssistantProfile);
                }
            }
            else if (roleName == "Mangaka")
            {
                if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
                if (dto.PenName != null) user.PenName = dto.PenName;
            }
            else
            {
                // Cho các role còn lại (Admin, Board, Editor, Customer,...)
                if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
            }

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return new ProfileResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = roleName,
                PenName = user.PenName,
                PortfolioUrl = user.PortfolioUrl,
                Skills = user.Skills,
                SpecialtyTags = user.AssistantProfile?.SpecialtyTags,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl
            };
        }
    }
}
