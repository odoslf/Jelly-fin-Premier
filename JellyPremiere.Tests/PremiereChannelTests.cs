using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JellyPremiere.Channels;
using JellyPremiere.Models;
using JellyPremiere.Services;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JellyPremiere.Tests;

public sealed class PremiereChannelTests
{
    [Fact]
    public async Task Channel_exposes_only_active_playable_library_announcements()
    {
        var libraryId = Guid.NewGuid();
        var linked = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = "Próximo estreno",
            Description = "Disponible próximamente",
            IsEnabled = true,
            LibraryItemId = libraryId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var unlinked = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = "Aviso sin vídeo",
            Description = "No debe parecer reproducible",
            IsEnabled = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var repo = new FakeRepository(linked, unlinked);
        var library = new Mock<ILibraryManager>();
        library.Setup(l => l.GetItemById(libraryId)).Returns(new Movie { Id = libraryId, Name = "Película" });
        var media = new Mock<IMediaSourceManager>();
        var channel = new PremiereChannel(repo, library.Object, media.Object, Mock.Of<ILogger<PremiereChannel>>());

        var result = await channel.GetChannelItems(new InternalChannelItemQuery(), CancellationToken.None);

        Assert.Equal("Estrenos", channel.Name);
        Assert.Single(result.Items);
        Assert.Equal("Próximo estreno", result.Items[0].Name);
        Assert.Equal("Disponible próximamente", result.Items[0].Overview);
        Assert.Equal(linked.Id.ToString("N"), result.Items[0].Id);
    }

    [Fact]
    public async Task Playback_callback_uses_the_linked_Jellyfin_library_item()
    {
        var libraryId = Guid.NewGuid();
        var announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = "Estreno",
            IsEnabled = true,
            LibraryItemId = libraryId
        };
        var movie = new Movie { Id = libraryId, Name = "Película" };
        var repo = new FakeRepository(announcement);
        var library = new Mock<ILibraryManager>();
        library.Setup(l => l.GetItemById(libraryId)).Returns(movie);
        var expected = new List<MediaSourceInfo> { new() { Id = "library-source" } };
        var media = new Mock<IMediaSourceManager>();
        media.Setup(m => m.GetPlaybackMediaSources(movie, null!, false, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var channel = new PremiereChannel(repo, library.Object, media.Object, Mock.Of<ILogger<PremiereChannel>>());

        var sources = await channel.GetChannelItemMediaInfo(announcement.Id.ToString("N"), CancellationToken.None);

        Assert.Single(sources);
        media.Verify(m => m.GetPlaybackMediaSources(movie, null!, false, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class FakeRepository : IAnnouncementRepository
    {
        private readonly IReadOnlyList<Announcement> _announcements;

        public FakeRepository(params Announcement[] announcements)
        {
            _announcements = announcements;
        }

        public Task<IReadOnlyList<Announcement>> GetAllAnnouncementsAsync() => Task.FromResult(_announcements);
        public Task<IReadOnlyList<Announcement>> GetActiveAnnouncementsForUserAsync(Guid userId, bool isUserAdmin) => GetAllAnnouncementsAsync();
        public Task<Announcement?> GetAnnouncementByIdAsync(Guid id) => Task.FromResult<Announcement?>(_announcements.FirstOrDefault(a => a.Id == id));
        public Task SaveAnnouncementAsync(Announcement announcement) => Task.CompletedTask;
        public Task DeleteAnnouncementAsync(Guid id) => Task.CompletedTask;
        public Task RecordAcknowledgmentAsync(Guid announcementId, Guid userId) => Task.CompletedTask;
        public Task<IReadOnlyList<UserAcknowledgment>> GetAcknowledgmentsForAnnouncementAsync(Guid announcementId)
            => Task.FromResult<IReadOnlyList<UserAcknowledgment>>(Array.Empty<UserAcknowledgment>());
        public Task<IReadOnlyList<UserAcknowledgment>> GetAcknowledgmentsForUserAsync(Guid userId)
            => Task.FromResult<IReadOnlyList<UserAcknowledgment>>(Array.Empty<UserAcknowledgment>());
    }
}
