namespace ElasoftCommunityManagementSystem.Dtos.ClubDtos
{
    public class CreateClubDto
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public int AdvisorId { get; set; }
        public int? CategoryId { get; set; }
        public byte[] Image { get; set; }
        public byte[] Document { get; set; }
        public int CreatorUserId { get; set; }
    }
}
