using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JellyPremiere.Models;
using JellyPremiere.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JellyPremiere.Tests
{
    public class JsonAnnouncementRepositoryTests : IDisposable
    {
        private readonly string _testFolderPath;

        public JsonAnnouncementRepositoryTests()
        {
            _testFolderPath = Path.Combine(Path.GetTempPath(), "JellyPremiereTests_" + Guid.NewGuid().ToString("N"));
        }

        public void Dispose()
        {
            if (Directory.Exists(_testFolderPath))
            {
                try
                {
                    Directory.Delete(_testFolderPath, recursive: true);
                }
                catch { }
            }
        }

        [Fact]
        public async Task SaveAndGetAnnouncement_ShouldPersistCorrectly()
        {
            // Arrange
            var repo = new JsonAnnouncementRepository(_testFolderPath, NullLogger<JsonAnnouncementRepository>.Instance);
            var announcement = new Announcement
            {
                Title = "Alien: Romulus",
                Description = "Estreno este viernes a las 22:00",
                Type = AnnouncementType.Banner,
                IsEnabled = true
            };

            // Act
            await repo.SaveAnnouncementAsync(announcement);
            var retrieved = await repo.GetAnnouncementByIdAsync(announcement.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Alien: Romulus", retrieved.Title);
            Assert.Equal("Estreno este viernes a las 22:00", retrieved.Description);
            Assert.Equal(AnnouncementType.Banner, retrieved.Type);
        }

        [Fact]
        public async Task GetActiveAnnouncementsForUser_ShouldFilterExpiredOrDisabled()
        {
            // Arrange
            var repo = new JsonAnnouncementRepository(_testFolderPath, NullLogger<JsonAnnouncementRepository>.Instance);
            var userId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var activeBanner = new Announcement
            {
                Title = "Active Banner",
                IsEnabled = true,
                Type = AnnouncementType.Banner
            };

            var expiredBanner = new Announcement
            {
                Title = "Expired Banner",
                IsEnabled = true,
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(-1)
            };

            var disabledBanner = new Announcement
            {
                Title = "Disabled Banner",
                IsEnabled = false
            };

            await repo.SaveAnnouncementAsync(activeBanner);
            await repo.SaveAnnouncementAsync(expiredBanner);
            await repo.SaveAnnouncementAsync(disabledBanner);

            // Act
            var activeList = await repo.GetActiveAnnouncementsForUserAsync(userId, isUserAdmin: false);

            // Assert
            Assert.Single(activeList);
            Assert.Equal("Active Banner", activeList[0].Title);
        }

        [Fact]
        public async Task RecordAcknowledgment_ShouldHideMandatoryNoticeFromUser()
        {
            // Arrange
            var repo = new JsonAnnouncementRepository(_testFolderPath, NullLogger<JsonAnnouncementRepository>.Instance);
            var userId = Guid.NewGuid();
            var notice = new Announcement
            {
                Title = "Mantenimiento Sábado",
                Description = "El servidor estará en mantenimiento de 2 a 4 AM.",
                Type = AnnouncementType.MandatoryNotice,
                IsEnabled = true
            };

            await repo.SaveAnnouncementAsync(notice);

            // Before ack
            var activeBefore = await repo.GetActiveAnnouncementsForUserAsync(userId, isUserAdmin: false);
            Assert.Single(activeBefore);

            // Act - Record acknowledgment
            await repo.RecordAcknowledgmentAsync(notice.Id, userId);

            // After ack
            var activeAfter = await repo.GetActiveAnnouncementsForUserAsync(userId, isUserAdmin: false);

            // Assert
            Assert.Empty(activeAfter);
        }

        [Fact]
        public async Task DeleteAnnouncement_ShouldRemoveAnnouncementAndAcks()
        {
            // Arrange
            var repo = new JsonAnnouncementRepository(_testFolderPath, NullLogger<JsonAnnouncementRepository>.Instance);
            var userId = Guid.NewGuid();
            var announcement = new Announcement
            {
                Title = "Aviso a Eliminar",
                Type = AnnouncementType.ImportantNotice
            };

            await repo.SaveAnnouncementAsync(announcement);
            await repo.RecordAcknowledgmentAsync(announcement.Id, userId);

            // Act
            await repo.DeleteAnnouncementAsync(announcement.Id);

            // Assert
            var retrieved = await repo.GetAnnouncementByIdAsync(announcement.Id);
            Assert.Null(retrieved);

            var acks = await repo.GetAcknowledgmentsForAnnouncementAsync(announcement.Id);
            Assert.Empty(acks);
        }
    }
}
