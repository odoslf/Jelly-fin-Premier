using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JellyPremiere.Models;
using JellyPremiere.Services;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace JellyPremiere.Channels;

/// <summary>
/// Native Jellyfin channel for active premiere announcements linked to playable library items.
/// Plain notices remain a Web/WebView overlay and are never exposed as fake playable videos.
/// </summary>
public sealed class PremiereChannel : IChannel, IRequiresMediaInfoCallback
{
    private readonly IAnnouncementRepository _repository;
    private readonly ILibraryManager _libraryManager;
    private readonly IMediaSourceManager _mediaSourceManager;
    private readonly ILogger<PremiereChannel> _logger;

    public PremiereChannel(
        IAnnouncementRepository repository,
        ILibraryManager libraryManager,
        IMediaSourceManager mediaSourceManager,
        ILogger<PremiereChannel> logger)
    {
        _repository = repository;
        _libraryManager = libraryManager;
        _mediaSourceManager = mediaSourceManager;
        _logger = logger;
    }

    public string Name => "Estrenos";
    public string Description => "Estrenos activos vinculados a contenidos reproducibles de la biblioteca.";
    public string DataVersion => "1.0.1.0";
    public string HomePageUrl => string.Empty;
    public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

    public InternalChannelFeatures GetChannelFeatures() => new()
    {
        ContentTypes = new List<ChannelMediaContentType>
        {
            ChannelMediaContentType.Movie,
            ChannelMediaContentType.Episode
        },
        MediaTypes = new List<ChannelMediaType> { ChannelMediaType.Video },
        SupportsContentDownloading = false
    };

    public bool IsEnabledFor(string userId) => true;

    public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var announcements = await _repository.GetAllAnnouncementsAsync().ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;
        var items = new List<ChannelItemInfo>();

        foreach (var announcement in announcements)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!announcement.IsActive(now) || !announcement.LibraryItemId.HasValue)
            {
                continue;
            }

            if (query.UserId != Guid.Empty
                && announcement.TargetUserIds.Count > 0
                && !announcement.TargetUserIds.Contains(query.UserId))
            {
                continue;
            }

            var libraryItem = _libraryManager.GetItemById(announcement.LibraryItemId.Value);
            if (libraryItem is null || libraryItem.IsFolder || libraryItem.MediaType != MediaType.Video)
            {
                continue;
            }

            items.Add(new ChannelItemInfo
            {
                Id = announcement.Id.ToString("N"),
                Name = announcement.Title,
                Overview = announcement.Description,
                Type = ChannelItemType.Media,
                MediaType = ChannelMediaType.Video,
                ContentType = string.Equals(libraryItem.GetType().Name, "Episode", StringComparison.OrdinalIgnoreCase)
                    ? ChannelMediaContentType.Episode
                    : ChannelMediaContentType.Movie,
                IsLiveStream = false
            });
        }

        return new ChannelItemResult
        {
            Items = items,
            TotalRecordCount = items.Count
        };
    }

    public async Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Guid.TryParse(id, out var announcementId))
        {
            _logger.LogWarning("Invalid JellyPremiere announcement id {AnnouncementId}", id);
            return Array.Empty<MediaSourceInfo>();
        }

        var announcement = await _repository.GetAnnouncementByIdAsync(announcementId).ConfigureAwait(false);
        if (announcement?.LibraryItemId is not Guid libraryItemId)
        {
            return Array.Empty<MediaSourceInfo>();
        }

        var libraryItem = _libraryManager.GetItemById(libraryItemId);
        if (libraryItem is null || libraryItem.IsFolder || libraryItem.MediaType != MediaType.Video)
        {
            _logger.LogWarning("JellyPremiere library item {LibraryItemId} is missing or not directly playable", libraryItemId);
            return Array.Empty<MediaSourceInfo>();
        }

        return await _mediaSourceManager
            .GetPlaybackMediaSources(libraryItem, null!, false, false, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new DynamicImageResponse { HasImage = false });
    }

    public IEnumerable<ImageType> GetSupportedChannelImages() => Array.Empty<ImageType>();
}
