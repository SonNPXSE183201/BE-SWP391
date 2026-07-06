using System;
using MediatR;

namespace MangaPublishingSystem.Application.Events.AI
{
    public class DraftPageUploadedEvent : INotification
    {
        public Guid PageId { get; }
        public string ImageUrl { get; }

        public DraftPageUploadedEvent(Guid pageId, string imageUrl)
        {
            PageId = pageId;
            ImageUrl = imageUrl;
        }
    }
}
