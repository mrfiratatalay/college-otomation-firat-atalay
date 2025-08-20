using ElasoftCommunityManagementSystem.Dtos.ClubMembershipoDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/memberships")]
    [ApiController]
    public class ClubMembershipController : ControllerBase
    {
        private readonly IClubMembershipService _membershipService;
        private readonly IAuthorizationService _authorizationService;

        public ClubMembershipController(IClubMembershipService membershipService, IAuthorizationService authorizationService)
        {
            _membershipService = membershipService;
            _authorizationService = authorizationService;
        }

        [HttpPost("basvur")]
        [Authorize]
        public async Task<IActionResult> ApplyToClub([FromBody] ClubMembershipDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await _membershipService.ApplyToClubAsync(userId, dto);
                return Ok(new { message = "Başvurunuz alındı, onay bekliyor." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my-club/basvurular")]
        [Authorize]
        public async Task<IActionResult> GetMyClubApplications()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var apps = await _membershipService.GetMyClubApplicationsAsync(userId);
                return Ok(apps);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{membershipId}/basvuruonaylama")]
        [Authorize]
        public async Task<IActionResult> ApproveMembership(int membershipId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower();
                await _membershipService.ApproveMembershipAsync(membershipId, userId, role);
                return Ok(new { message = "Başvuru onaylandı." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{membershipId}/basvurureddet")]
        [Authorize]
        public async Task<IActionResult> RejectMembership(int membershipId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower();
                await _membershipService.RejectMembershipAsync(membershipId, userId, role);
                return Ok(new { message = "Başvuru reddedildi." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("whoami")]
        [Authorize]
        public async Task<IActionResult> WhoAmIInClubs()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _membershipService.GetUserRolesInClubsAsync(userId);
            return Ok(result);
        }

        [HttpGet("my-led-clubs/pending-applications")]
        [Authorize]
        public async Task<IActionResult> GetPendingApplicationsForMyLedClubs()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var applications = await _membershipService.GetPendingApplicationsForMyLedClubsAsync(userId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                // Consider more specific exception handling and logging
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("advisor/pending-applications")]
        [Authorize]
        public async Task<IActionResult> GetPendingApplicationsForAdvisor()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var applications = await _membershipService.GetPendingApplicationsForAdvisorAsync(userId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("club/{clubId}/members")]
        [Authorize]
        public async Task<IActionResult> GetClubMembers(int clubId)
        {
            try
            {
                var members = await _membershipService.GetClubMembersAsync(clubId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("approved-member/{membershipId}")]
        [Authorize]
        public async Task<IActionResult> DeleteApprovedMember(int membershipId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _membershipService.DeleteApprovedMemberAsync(membershipId, userId);
                return Ok(new { message = "Üye başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("member-details/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetClubMemberUserDetails(int userId)
        {
            try
            {
                var userDetails = await _membershipService.GetClubMemberUserDetailsAsync(userId);
                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("approved-member/{membershipId}/leave")]
        [Authorize]
        public async Task<IActionResult> LeaveClub(int membershipId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _membershipService.LeaveClubAsync(membershipId, userId);
                return Ok(new { message = "Topluluktan başarıyla ayrıldınız." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Sets a club member as the club leader (president). Only admins and advisors can perform this action.
        /// Ensures only one president per club by demoting the current president to member role.
        /// </summary>
        /// <param name="membershipId">The membership ID of the user to be promoted to president</param>
        /// <returns>Success message or error</returns>
        [HttpPut("set-leader/{membershipId}")]
        [Authorize]
        public async Task<IActionResult> SetClubLeader(int membershipId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _membershipService.SetClubLeaderAsync(membershipId, userId);
                return Ok(new { message = "Kulüp başkanı başarıyla atandı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Validates that a club has exactly one president. Useful for data integrity checks.
        /// </summary>
        /// <param name="clubId">The ID of the club to validate</param>
        /// <returns>Validation result</returns>
        [HttpGet("validate-president/{clubId}")]
        [Authorize]
        public async Task<IActionResult> ValidateClubPresident(int clubId)
        {
            try
            {
                var isValid = await _membershipService.ValidateClubPresidentUniquenessAsync(clubId);
                var president = await _membershipService.GetClubPresidentAsync(clubId);

                return Ok(new
                {
                    isValid = isValid,
                    hasPresident = president != null,
                    president = president != null ? new
                    {
                        president.UserId,
                        president.User.Name,
                        president.User.Surname,
                        president.Role,
                        president.JoinedAt
                    } : null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("club/{clubId}/change-president")]
        [Authorize]
        public async Task<IActionResult> ChangeClubPresident(int clubId, [FromBody] ChangePresidentDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower();

                await _membershipService.ChangeClubPresidentAsync(clubId, dto.NewPresidentUserId, userId, role);
                return Ok(new { message = "Topluluk başkanı başarıyla değiştirildi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
