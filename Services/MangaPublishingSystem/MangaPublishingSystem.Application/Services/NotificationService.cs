using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.Services
{
    public class NotificationService : GenericService<Notification>, INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
            _notificationRepository = repository;
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsByUserIdAsync(int userId)
        {
            var notifs = await _notificationRepository.FindAsync(n => n.UserId == userId);
            return notifs
                .OrderByDescending(n => n.CreateAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Content = n.Content,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreateAt = n.CreateAt
                });
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            var notifs = await _notificationRepository.FindAsync(n => n.UserId == userId && !n.IsRead);
            return notifs.Count();
        }

        public async System.Threading.Tasks.Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notif = await _notificationRepository.GetByIdAsync(notificationId);
            if (notif == null || notif.UserId != userId)
            {
                throw new NotFoundException("Không tìm thấy thông báo hoặc bạn không có quyền.");
            }

            if (!notif.IsRead)
            {
                notif.IsRead = true;
                _notificationRepository.Update(notif);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async System.Threading.Tasks.Task MarkAllAsReadAsync(int userId)
        {
            var notifs = await _notificationRepository.FindAsync(n => n.UserId == userId && !n.IsRead);
            foreach (var notif in notifs)
            {
                notif.IsRead = true;
                _notificationRepository.Update(notif);
            }
            await _unitOfWork.SaveChangesAsync();
        }
    }
}