using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JellyPremiere.Channels;
using JellyPremiere.Models;
using JellyPremiere.Services;
using MediaBrowser.Model.Channels;
using Xunit;

namespace JellyPremiere.Tests;

public sealed class PremiereChannelTests
{
    [Fact]
    public async Task Channel_exposes_active_announcements()
    {
        var repo = new FakeRepository();
        var channel = new PremiereChannel(repo);
        var result = await channel.GetChannelItems(new InternalChannelItemQuery(), CancellationToken.None);

        Assert.Equal("Estrenos", channel.Name);
        Assert.Single(result.Items);
        Assert.Equal("Próximo estreno", result.Items[0].Name);
    }

    private sealed class FakeRepository : IAnnouncementRepository
    {
        public Task<IReadOnlyList<Announcement>> GetAllAnnouncementsAsync() => Task.FromResult<IReadOnlyList<Announcement>>(new[]
        {
            new Announcement { Id = Guid.NewGuid(), Title = "Próximo estreno", Message = "Disponible próximamente", IsActive = true, CreatedAt = DateTimeOffset.UtcNow }
        });

        public Task<IReadOnlyList<Announcement>> GetActiveAnnouncementsForUserAsync(Guid userId, bool isAdmin) => GetAllAnnouncementsAsync();
        public Task<Announcement?> GetAnnouncementByIdAsync(Guid id) => Task.FromResult<Announcement?>(null);
        public Task SaveAnnouncementAsync(Announcement announcement) => Task.CompletedTask;
        public Task DeleteAnnouncementAsync(Guid id) => Task.CompletedTask;
        public Task RecordAcknowledgmentAsync(Guid announcementId, Guid userId) => Task.CompletedTask;
        public Task<IReadOnlyList<AnnouncementAcknowledgment>> GetAcknowledgmentsForAnnouncementAsync(Guid announcementId) => Task.FromResult<IReadOnlyList<AnnouncementAcknowledgment>>(Array.Empty<AnnouncementAcknowledgment>());
    }
}
