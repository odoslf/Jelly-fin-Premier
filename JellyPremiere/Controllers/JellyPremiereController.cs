using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Data.Entities;
using Jellyfin.Data.Enums;
using JellyPremiere.Models;
using JellyPremiere.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JellyPremiere.Controllers
{
    [ApiController]
    [Route("JellyPremiere")]
    [Produces("application/json")]
    public class JellyPremiereController : ControllerBase
    {
        private readonly IAnnouncementRepository _repository;
        private readonly IUserManager _userManager;
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<JellyPremiereController> _logger;

        public JellyPremiereController(
            IAnnouncementRepository repository,
            IUserManager userManager,
            ILibraryManager libraryManager,
            ILogger<JellyPremiereController> logger)
        {
            _repository = repository;
            _userManager = userManager;
            _libraryManager = libraryManager;
            _logger = logger;
        }

        private User? GetAuthenticatedUser()
        {
            // 1. Check ClaimsPrincipal
            var userIdClaim = User.FindFirst("UserId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var claimGuid))
            {
                var userFromClaim = _userManager.GetUserById(claimGuid);
                if (userFromClaim != null)
                {
                    return userFromClaim;
                }
            }

            // 2. Fallback to HttpContext.Items["User"] or HttpContext.User
            if (HttpContext.Items.TryGetValue("User", out var itemUser) && itemUser is User u)
            {
                return u;
            }

            return null;
        }

        private bool IsAdmin(User user)
        {
            return user.HasPermission(PermissionKind.IsAdministrator);
        }

        /// <summary>
        /// Serves the client overlay script for Jellyfin Web / WebView.
        /// </summary>
        [HttpGet("ClientScript.js")]
        [AllowAnonymous]
        [Produces("application/javascript")]
        public IActionResult GetClientScript()
        {
            var config = JellyPremierePlugin.Instance?.Configuration;
            if (config != null && !config.EnableClientInjection)
            {
                return Content("/* JellyPremiere client injection is disabled in configuration */", "application/javascript");
            }

            var assembly = typeof(JellyPremierePlugin).Assembly;
            var resourceName = "JellyPremiere.Web.jellypremiere.js";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return NotFound("/* jellypremiere.js resource not found */");
            }

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var scriptContent = reader.ReadToEnd();
            return Content(scriptContent, "application/javascript", Encoding.UTF8);
        }

        /// <summary>
        /// Retrieves active announcements for the currently logged in user.
        /// </summary>
        [HttpGet("Active")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<Announcement>>> GetActiveAnnouncements()
        {
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized();
            }

            var isAdmin = IsAdmin(user);
            var active = await _repository.GetActiveAnnouncementsForUserAsync(user.Id, isAdmin);
            return Ok(active);
        }

        /// <summary>
        /// Acknowledges an announcement for the current user.
        /// </summary>
        [HttpPost("Acknowledge/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcknowledgeAnnouncement(Guid id)
        {
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized();
            }

            var announcement = await _repository.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            await _repository.RecordAcknowledgmentAsync(id, user.Id);
            return Ok(new { success = true });
        }

        /// <summary>
        /// Gets all announcements (Admin only).
        /// </summary>
        [HttpGet("Admin/Announcements")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Announcement>>> GetAllAnnouncements()
        {
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized();
            }

            if (!IsAdmin(user))
            {
                return Forbid();
            }

            var announcements = await _repository.GetAllAnnouncementsAsync();
            return Ok(announcements);
        }

        /// <summary>
        /// Creates a new announcement (Admin only).
        /// </summary>
        [HttpPost("Admin/Announcements")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Announcement>> CreateAnnouncement([FromBody] Announcement announcement)
        {
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized();
            }

            if (!IsAdmin(user))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(announcement.Title))
            {
                return BadRequest("Title is required.");
            }

            if (announcement.Id == Guid.Empty)
            {
                announcement.Id = Guid.NewGuid();
            }

            announcement.CreatedAt = DateTimeOffset.UtcNow;
            announcement.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.SaveAnnouncementAsync(announcement);
            return Ok(announcement);
        }

        /// <summary>
        /// Updates an existing announcement (Admin only).
        /// </summary>
        [HttpPut("Admin/Announcements/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Announcement>> UpdateAnnouncement(Guid id, [FromBody] Announcement announcement)
        {
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized();
            }

            if (!IsAdmin(user))
            {
                return Forbid();
            }

            var existing = await _repository.GetAnnouncementByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            announcement.Id = id;
            announcement.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.SaveAnnouncementAsync(announcement);
            return Ok(announcement);
        }

        /// <summary>
        /// Deletes an announcement (Admin only).
        /// </summary>
        [HttpDelete("Admin/Announcements/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAnnouncement(Guid id)
        {
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized();
            }

            if (!IsAdmin(user))
            {
                return Forbid();
            }

            var existing = await _repository.GetAnnouncementByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            await _repository.DeleteAnnouncementAsync(id);
            return Ok(new { success = true });
        }

        /// <summary>
        /// Gets confirmation statistics for a given announcement (Admin only).
        /// </summary>
        [HttpGet("Admin/Stats/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AnnouncementStats>> GetStats(Guid id)
        {
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized();
            }

            if (!IsAdmin(user))
            {
                return Forbid();
            }

            var announcement = await _repository.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            var allUsers = _userManager.Users.ToList();
            var acks = await _repository.GetAcknowledgmentsForAnnouncementAsync(id);
            var ackMap = acks.ToDictionary(a => a.UserId, a => a.AcknowledgedAt);

            var userStatuses = new List<UserStatusInfo>();
            foreach (var u in allUsers)
            {
                // Filter by target users if configured
                if (announcement.TargetUserIds != null && announcement.TargetUserIds.Count > 0 && !announcement.TargetUserIds.Contains(u.Id))
                {
                    continue;
                }

                var hasAck = ackMap.TryGetValue(u.Id, out var ackDate);
                userStatuses.Add(new UserStatusInfo
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Acknowledged = hasAck,
                    AcknowledgedAt = hasAck ? ackDate : null
                });
            }

            var stats = new AnnouncementStats
            {
                AnnouncementId = id,
                TotalUsers = userStatuses.Count,
                SeenCount = userStatuses.Count(s => s.Acknowledged),
                PendingCount = userStatuses.Count(s => !s.Acknowledged),
                UserStatuses = userStatuses
            };

            return Ok(stats);
        }

        /// <summary>
        /// Fetches metadata for a library item (Movie/Series) by ID (Admin only or authenticated).
        /// </summary>
        [HttpGet("Library/Item/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetLibraryItemMetadata(Guid id)
        {
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized();
            }

            var item = _libraryManager.GetItemById(id);
            if (item == null)
            {
                return NotFound();
            }

            var meta = new MediaMetadata
            {
                Title = item.Name,
                Overview = item.Overview,
                ItemType = item.GetType().Name,
                PosterUrl = item.HasImage(MediaBrowser.Model.Entities.ImageType.Primary)
                    ? $"/Items/{item.Id}/Images/Primary"
                    : null,
                BackdropUrl = item.HasImage(MediaBrowser.Model.Entities.ImageType.Backdrop)
                    ? $"/Items/{item.Id}/Images/Backdrop/0"
                    : null
            };

            return Ok(new
            {
                id = item.Id,
                title = meta.Title,
                overview = meta.Overview,
                itemType = meta.ItemType,
                posterUrl = meta.PosterUrl,
                backdropUrl = meta.BackdropUrl,
                actionUrl = $"#/details?id={item.Id}"
            });
        }
    }
}
