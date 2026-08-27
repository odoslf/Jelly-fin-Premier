using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JellyPremiere.Services;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Querying;

namespace JellyPremiere.Channels;

public sealed class PremiereChannel : IChannel
{
    private readonly IAnnouncementRepository _repository;

    public PremiereChannel(IAnnouncementRepository repository)
    {
        _repository = repository;
    }

    public string Name => "Estrenos";
    public string Description => "Estrenos y próximos contenidos publicados por JellyPremiere.";
    public string DataVersion => "1.0.1";
    public string HomePageUrl => string.Empty;
    public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

    public InternalChannelFeatures GetChannelFeatures() => new()
    {
        ContentTypes = new List<ChannelMediaContentType> { ChannelMediaContentType.Movie, ChannelMediaContentType.TVShow },
        MediaTypes = new List<ChannelMediaType> { ChannelMediaType.Video },
        SupportsContentDownloading = false
    };

    public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
    {
        var announcements = await _repository.GetAllAnnouncementsAsync().ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;
        var items = announcements
            .Where(a => a.IsActive && (!a.ExpiresAt.HasValue || a.ExpiresAt.Value > now))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ChannelItemInfo
            {
                Id = a.Id.ToString("N"),
                Name = a.Title,
                Overview = a.Message,
                Type = ChannelItemType.Media,
                MediaType = ChannelMediaType.Video,
                ContentType = ChannelMediaContentType.Movie,
                IsLiveStream = false
            })
            .ToList();

        return new ChannelItemResult
        {
            Items = items,
            TotalRecordCount = items.Count
        };
    }

    public Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(string id, CancellationToken cancellationToken)
        => Task.FromResult<IEnumerable<MediaSourceInfo>>(Array.Empty<MediaSourceInfo>());
}
