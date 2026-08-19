using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JellyPremiere.Models;

namespace JellyPremiere.Services
{
    public interface IAnnouncementRepository
    {
        Task<IReadOnlyList<Announcement>> GetAllAnnouncementsAsync();
        Task<Announcement?> GetAnnouncementByIdAsync(Guid id);
        Task SaveAnnouncementAsync(Announcement announcement);
        Task DeleteAnnouncementAsync(Guid id);

        Task<IReadOnlyList<UserAcknowledgment>> GetAcknowledgmentsForUserAsync(Guid userId);
        Task<IReadOnlyList<UserAcknowledgment>> GetAcknowledgmentsForAnnouncementAsync(Guid announcementId);
        Task RecordAcknowledgmentAsync(Guid announcementId, Guid userId);

        Task<IReadOnlyList<Announcement>> GetActiveAnnouncementsForUserAsync(Guid userId, bool isUserAdmin);
    }
}
