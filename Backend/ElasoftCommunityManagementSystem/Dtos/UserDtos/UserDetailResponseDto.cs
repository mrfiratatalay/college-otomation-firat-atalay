using System;

namespace ElasoftCommunityManagementSystem.Dtos
{
    public class UserDetailResponseDto : UserResponseDto
    {
        public int CommunityCount { get; set; }
        public int EventCount { get; set; }
        public int CommentCount { get; set; }
    }
} 