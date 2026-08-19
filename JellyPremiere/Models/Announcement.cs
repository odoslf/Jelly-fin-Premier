using System;

namespace JellyPremiere.Models
{
    public enum AnnouncementType
    {
        Banner = 0,
        ImportantNotice = 1,
        MandatoryNotice = 2
    }

    public class MediaMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string? Overview { get; set; }
        public string? PosterUrl { get; set; }
        public string? BackdropUrl { get; set; }
        public string? ItemType { get; set; }
    }

    public class Announcement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AnnouncementType Type { get; set; } = AnnouncementType.Banner;

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool IsEnabled { get; set; } = true;

        public Guid? LibraryItemId { get; set; }
        public MediaMetadata? MediaMetadata { get; set; }

        public string? ActionUrl { get; set; }
        public string? ButtonText { get; set; }

        public System.Collections.Generic.List<Guid> TargetUserIds { get; set; } = new();

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public bool IsActive(DateTimeOffset now)
        {
            if (!IsEnabled)
            {
                return false;
            }

            if (StartDate.HasValue && now < StartDate.Value)
            {
                return false;
            }

            if (EndDate.HasValue && now > EndDate.Value)
            {
                return false;
            }

            return true;
        }
    }

    public class UserAcknowledgment
    {
        public Guid AnnouncementId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset AcknowledgedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class UserStatusInfo
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Acknowledged { get; set; }
        public DateTimeOffset? AcknowledgedAt { get; set; }
    }

    public class AnnouncementStats
    {
        public Guid AnnouncementId { get; set; }
        public int TotalUsers { get; set; }
        public int SeenCount { get; set; }
        public int PendingCount { get; set; }
        public System.Collections.Generic.List<UserStatusInfo> UserStatuses { get; set; } = new();
    }
}
