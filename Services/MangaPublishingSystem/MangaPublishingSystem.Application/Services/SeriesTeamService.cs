using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;

namespace MangaPublishingSystem.Application.Services
{
    public class SeriesTeamService : ISeriesTeamService
    {
        private const int AssistantRoleId = 5;

        private readonly ISeriesRepository _seriesRepository;
        private readonly ISeriesAssistantRepository _seriesAssistantRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IUnitOfWork _unitOfWork;

        public SeriesTeamService(
            ISeriesRepository seriesRepository,
            ISeriesAssistantRepository seriesAssistantRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            IUnitOfWork unitOfWork)
        {
            _seriesRepository = seriesRepository;
            _seriesAssistantRepository = seriesAssistantRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SeriesAssistantDto>> GetTeamMembersAsync(int seriesId, int userId, bool isMangaka)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện.");
            }

            if (isMangaka)
            {
                if (series.MangakaId != userId)
                {
                    throw new ForbiddenException("Bạn không có quyền xem nhóm của bộ truyện này.");
                }

                var members = await _seriesAssistantRepository.GetBySeriesIdAsync(seriesId);
                return members.Select(MapToDto);
            }

            var membership = await _seriesAssistantRepository.GetMembershipAsync(seriesId, userId);
            if (membership == null)
            {
                throw new ForbiddenException("Bạn không thuộc nhóm của bộ truyện này.");
            }

            return new[] { MapToDto(membership) };
        }

        public async Task<IEnumerable<SeriesAssistantDto>> GetActiveTeamForAssignmentAsync(int seriesId, int mangakaId)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không có quyền giao việc cho bộ truyện này.");
            }

            var members = await _seriesAssistantRepository.GetBySeriesIdAsync(seriesId, "Active");
            return members.Select(MapToDto);
        }

        public async Task<SeriesAssistantDto> InviteAssistantAsync(int seriesId, int mangakaId, InviteSeriesAssistantDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RoleInTeam))
            {
                throw new BadRequestException("Vui lòng xác định vai trò trong nhóm (RoleInTeam).");
            }

            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Chỉ Mangaka sở hữu series mới có thể mời trợ lý.");
            }

            var assistant = await _userRepository.GetByIdAsync(dto.AssistantId);
            if (assistant == null || assistant.RoleId != AssistantRoleId)
            {
                throw new NotFoundException("Không tìm thấy tài khoản Trợ lý hợp lệ.");
            }

            if (assistant.Status != UserStatus.Active)
            {
                throw new ConflictException("Trợ lý chưa được duyệt hoặc đang bị khóa.");
            }

            var existing = await _seriesAssistantRepository.GetMembershipAsync(seriesId, dto.AssistantId);
            if (existing != null)
            {
                var newRole = dto.RoleInTeam.Trim();
                if (existing.Status is "Active" or "Pending")
                {
                    var currentRoles = existing.RoleInTeam
                        .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => r.Trim())
                        .ToList();

                    var newRoles = dto.RoleInTeam
                        .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => r.Trim())
                        .ToList();

                    var rolesToAdd = newRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();

                    if (!rolesToAdd.Any())
                    {
                        throw new ConflictException("Trợ lý này đã có tất cả các vai trò này trong nhóm.");
                    }

                    currentRoles.AddRange(rolesToAdd);
                    existing.RoleInTeam = string.Join(", ", currentRoles);
                    existing.UpdateAt = DateTime.UtcNow;
                    _seriesAssistantRepository.Update(existing);
                }
                else
                {
                    existing.RoleInTeam = dto.RoleInTeam.Trim();
                    existing.Status = "Pending";
                    existing.JoinedDate = null;
                    existing.UpdateAt = DateTime.UtcNow;
                    _seriesAssistantRepository.Update(existing);
                }
            }
            else
            {
                var invite = new SeriesAssistant
                {
                    SeriesId = seriesId,
                    AssistantId = dto.AssistantId,
                    RoleInTeam = dto.RoleInTeam.Trim(),
                    Status = "Pending",
                    CreateAt = DateTime.UtcNow,
                };
                await _seriesAssistantRepository.AddAsync(invite);
                existing = invite;
            }

            var notif = new Notification
            {
                UserId = dto.AssistantId,
                Content = $"Bạn được mời tham gia nhóm dự án \"{series.Title}\" với vai trò {dto.RoleInTeam.Trim()}.",
                Type = "Series_Team_Invite",
                IsRead = false,
            };
            await _notificationRepository.AddAsync(notif);
            await _unitOfWork.SaveChangesAsync();

            var payload = new NotificationPayload
            {
                Id = notif.Id,
                Title = "Lời mời tham gia dự án",
                Message = notif.Content,
                Link = $"/assistant/series-invites/{seriesId}",
                Type = notif.Type,
                CreateAt = notif.CreateAt,
            };
            await _notificationPublisher.PublishNotificationPayloadAsync(dto.AssistantId, payload);
            await _unitOfWork.SaveChangesAsync();

            existing.Assistant = assistant;
            return MapToDto(existing);
        }

        public async Task<SeriesAssistantDto> RespondToInviteAsync(int seriesId, int assistantId, RespondSeriesInviteDto dto)
        {
            var membership = await _seriesAssistantRepository.GetMembershipAsync(seriesId, assistantId);
            if (membership == null || membership.Status != "Pending")
            {
                throw new NotFoundException("Không tìm thấy lời mời đang chờ phản hồi.");
            }

            if (!dto.Accept)
            {
                membership.Status = "Removed";
                membership.UpdateAt = DateTime.UtcNow;
                _seriesAssistantRepository.Update(membership);

                var declineNotif = new Notification
                {
                    UserId = membership.Series.MangakaId,
                    Content = $"Trợ lý {membership.Assistant.FullName} đã từ chối lời mời tham gia \"{membership.Series.Title}\".",
                    Type = "Series_Team_Declined",
                    IsRead = false,
                };
                await _notificationRepository.AddAsync(declineNotif);
                await _unitOfWork.SaveChangesAsync();
                return MapToDto(membership);
            }

            membership.Status = "Active";
            membership.JoinedDate = DateTime.UtcNow;
            membership.UpdateAt = DateTime.UtcNow;
            _seriesAssistantRepository.Update(membership);

            var acceptNotif = new Notification
            {
                UserId = membership.Series.MangakaId,
                Content = $"Trợ lý {membership.Assistant.FullName} đã chấp nhận tham gia nhóm dự án \"{membership.Series.Title}\".",
                Type = "Series_Team_Accepted",
                IsRead = false,
            };
            await _notificationRepository.AddAsync(acceptNotif);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(membership);
        }

        public async System.Threading.Tasks.Task RemoveMemberAsync(int seriesId, int mangakaId, int assistantId, string? roleToRemove = null)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Chỉ Mangaka sở hữu series mới có thể gỡ thành viên.");
            }

            var membership = await _seriesAssistantRepository.GetMembershipAsync(seriesId, assistantId);
            if (membership == null || membership.Status is "Removed")
            {
                throw new NotFoundException("Thành viên không tồn tại trong nhóm.");
            }

            if (!string.IsNullOrWhiteSpace(roleToRemove))
            {
                var currentRoles = membership.RoleInTeam
                    .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .ToList();

                currentRoles.RemoveAll(r => r.Equals(roleToRemove.Trim(), StringComparison.OrdinalIgnoreCase));

                if (currentRoles.Any())
                {
                    membership.RoleInTeam = string.Join(", ", currentRoles);
                    membership.UpdateAt = DateTime.UtcNow;
                    _seriesAssistantRepository.Update(membership);
                    await _unitOfWork.SaveChangesAsync();
                    return;
                }
            }

            membership.Status = "Removed";
            membership.UpdateAt = DateTime.UtcNow;
            _seriesAssistantRepository.Update(membership);
            await _unitOfWork.SaveChangesAsync();
        }

        private static SeriesAssistantDto MapToDto(SeriesAssistant sa) => new()
        {
            SeriesId = sa.SeriesId,
            AssistantId = sa.AssistantId,
            AssistantName = sa.Assistant?.FullName,
            AssistantEmail = sa.Assistant?.Email,
            RoleInTeam = sa.RoleInTeam,
            JoinedDate = sa.JoinedDate,
            Status = sa.Status,
            CreateAt = sa.CreateAt,
        };
    }
}
