using ElasoftCommunityManagementSystem.Models;
using ElasoftCommunityManagementSystem.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services.Authorization
{
    public class ClubExpenseAuthorizationService : BaseAuthorizationService
    {
        public ClubExpenseAuthorizationService(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> IsUserAuthorizedForExpense(int userId, string userRole, int clubId)
        {
            // Check admin permissions
            if (await IsAdmin(userRole))
                return true;

            // Validate club status
            await ValidateClubStatus(clubId);

            return true;
        }
    }
}