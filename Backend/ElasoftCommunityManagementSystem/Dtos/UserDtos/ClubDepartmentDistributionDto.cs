namespace ElasoftCommunityManagementSystem.Dtos.UserDtos
{
    public class ClubDepartmentDistributionDto
    {
        public int ClubId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
    }
}
