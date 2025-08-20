using System;
using System.Collections.Generic;

namespace ElasoftCommunityManagementSystem.Dtos
{
    // Tekil bildirim yanıtı
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public int? EntityId { get; set; }
        public bool Read { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Okunmamış bildirimler için yanıt
    public class UnreadNotificationsResponseDto
    {
        public List<NotificationResponseDto> Notifications { get; set; }
        public int Count { get; set; }
    }

    // Bildirim oluşturma isteği
    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public int? EntityId { get; set; }
    }
} 