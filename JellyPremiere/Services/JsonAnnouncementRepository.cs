using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JellyPremiere.Models;
using Microsoft.Extensions.Logging;

namespace JellyPremiere.Services
{
    public class JsonAnnouncementRepository : IAnnouncementRepository
    {
        private readonly string _announcementsFilePath;
        private readonly string _acknowledgmentsFilePath;
        private readonly ILogger<JsonAnnouncementRepository> _logger;
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public JsonAnnouncementRepository(string storageFolderPath, ILogger<JsonAnnouncementRepository> logger)
        {
            if (string.IsNullOrEmpty(storageFolderPath))
            {
                throw new ArgumentNullException(nameof(storageFolderPath));
            }

            Directory.CreateDirectory(storageFolderPath);
            _announcementsFilePath = Path.Combine(storageFolderPath, "jellypremiere_announcements.json");
            _acknowledgmentsFilePath = Path.Combine(storageFolderPath, "jellypremiere_acknowledgments.json");
            _logger = logger;
        }

        public async Task<IReadOnlyList<Announcement>> GetAllAnnouncementsAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                return await LoadAnnouncementsInternalAsync();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<Announcement?> GetAnnouncementByIdAsync(Guid id)
        {
            await _fileLock.WaitAsync();
            try
            {
                var announcements = await LoadAnnouncementsInternalAsync();
                return announcements.FirstOrDefault(a => a.Id == id);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task SaveAnnouncementAsync(Announcement announcement)
        {
            await _fileLock.WaitAsync();
            try
            {
                var announcements = (await LoadAnnouncementsInternalAsync()).ToList();
                var index = announcements.FindIndex(a => a.Id == announcement.Id);
                announcement.UpdatedAt = DateTimeOffset.UtcNow;

                if (index >= 0)
                {
                    announcements[index] = announcement;
                }
                else
                {
                    announcements.Add(announcement);
                }

                await SaveAnnouncementsInternalAsync(announcements);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task DeleteAnnouncementAsync(Guid id)
        {
            await _fileLock.WaitAsync();
            try
            {
                var announcements = (await LoadAnnouncementsInternalAsync()).ToList();
                announcements.RemoveAll(a => a.Id == id);
                await SaveAnnouncementsInternalAsync(announcements);

                var acks = (await LoadAcknowledgmentsInternalAsync()).ToList();
                acks.RemoveAll(a => a.AnnouncementId == id);
                await SaveAcknowledgmentsInternalAsync(acks);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<IReadOnlyList<UserAcknowledgment>> GetAcknowledgmentsForUserAsync(Guid userId)
        {
            await _fileLock.WaitAsync();
            try
            {
                var acks = await LoadAcknowledgmentsInternalAsync();
                return acks.Where(a => a.UserId == userId).ToList();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<IReadOnlyList<UserAcknowledgment>> GetAcknowledgmentsForAnnouncementAsync(Guid announcementId)
        {
            await _fileLock.WaitAsync();
            try
            {
                var acks = await LoadAcknowledgmentsInternalAsync();
                return acks.Where(a => a.AnnouncementId == announcementId).ToList();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task RecordAcknowledgmentAsync(Guid announcementId, Guid userId)
        {
            await _fileLock.WaitAsync();
            try
            {
                var acks = (await LoadAcknowledgmentsInternalAsync()).ToList();
                if (!acks.Any(a => a.AnnouncementId == announcementId && a.UserId == userId))
                {
                    acks.Add(new UserAcknowledgment
                    {
                        AnnouncementId = announcementId,
                        UserId = userId,
                        AcknowledgedAt = DateTimeOffset.UtcNow
                    });
                    await SaveAcknowledgmentsInternalAsync(acks);
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<IReadOnlyList<Announcement>> GetActiveAnnouncementsForUserAsync(Guid userId, bool isUserAdmin)
        {
            await _fileLock.WaitAsync();
            try
            {
                var now = DateTimeOffset.UtcNow;
                var announcements = await LoadAnnouncementsInternalAsync();
                var acks = await LoadAcknowledgmentsInternalAsync();

                var userAckIds = acks
                    .Where(a => a.UserId == userId)
                    .Select(a => a.AnnouncementId)
                    .ToHashSet();

                var activeList = new List<Announcement>();

                foreach (var a in announcements)
                {
                    if (!a.IsActive(now))
                    {
                        continue;
                    }

                    if (a.TargetUserIds != null && a.TargetUserIds.Count > 0 && !a.TargetUserIds.Contains(userId))
                    {
                        continue;
                    }

                    // For Mandatory & Important notices, hide if user has already acknowledged them
                    if ((a.Type == AnnouncementType.MandatoryNotice || a.Type == AnnouncementType.ImportantNotice)
                        && userAckIds.Contains(a.Id))
                    {
                        continue;
                    }

                    activeList.Add(a);
                }

                return activeList;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task<List<Announcement>> LoadAnnouncementsInternalAsync()
        {
            if (!File.Exists(_announcementsFilePath))
            {
                return new List<Announcement>();
            }

            try
            {
                using var stream = File.OpenRead(_announcementsFilePath);
                var list = await JsonSerializer.DeserializeAsync<List<Announcement>>(stream, _jsonOptions);
                return list ?? new List<Announcement>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read announcements JSON from {FilePath}", _announcementsFilePath);
                return new List<Announcement>();
            }
        }

        private async Task SaveAnnouncementsInternalAsync(List<Announcement> announcements)
        {
            var tempPath = _announcementsFilePath + ".tmp";
            using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, announcements, _jsonOptions);
            }

            File.Move(tempPath, _announcementsFilePath, overwrite: true);
        }

        private async Task<List<UserAcknowledgment>> LoadAcknowledgmentsInternalAsync()
        {
            if (!File.Exists(_acknowledgmentsFilePath))
            {
                return new List<UserAcknowledgment>();
            }

            try
            {
                using var stream = File.OpenRead(_acknowledgmentsFilePath);
                var list = await JsonSerializer.DeserializeAsync<List<UserAcknowledgment>>(stream, _jsonOptions);
                return list ?? new List<UserAcknowledgment>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read acknowledgments JSON from {FilePath}", _acknowledgmentsFilePath);
                return new List<UserAcknowledgment>();
            }
        }

        private async Task SaveAcknowledgmentsInternalAsync(List<UserAcknowledgment> acknowledgments)
        {
            var tempPath = _acknowledgmentsFilePath + ".tmp";
            using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, acknowledgments, _jsonOptions);
            }

            File.Move(tempPath, _acknowledgmentsFilePath, overwrite: true);
        }
    }
}
