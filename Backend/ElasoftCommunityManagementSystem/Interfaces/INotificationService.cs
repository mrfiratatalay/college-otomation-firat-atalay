using System.Threading.Tasks;
using ElasoftCommunityManagementSystem.Dtos;
using System.Collections.Generic;

namespace ElasoftCommunityManagementSystem.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// Kullanıcı için yeni bildirim oluşturur
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <param name="title">Bildirim başlığı</param>
        /// <param name="message">Bildirim mesajı</param>
        /// <param name="type">Bildirim tipi (event, announcement, invite vb.)</param>
        /// <param name="entityId">İlişkili öğe ID (opsiyonel)</param>
        /// <returns>Oluşturulan bildirim ID'si</returns>
        Task<int> CreateNotificationAsync(int userId, string title, string message, string type, int? entityId = null);
        
        /// <summary>
        /// Birden fazla kullanıcı için aynı bildirimi oluşturur
        /// </summary>
        /// <param name="userIds">Kullanıcı ID'leri listesi</param>
        /// <param name="title">Bildirim başlığı</param>
        /// <param name="message">Bildirim mesajı</param>
        /// <param name="type">Bildirim tipi</param>
        /// <param name="entityId">İlişkili öğe ID (opsiyonel)</param>
        /// <returns>İşlem durumu</returns>
        Task CreateNotificationForMultipleUsersAsync(List<int> userIds, string title, string message, string type, int? entityId = null);
        
        /// <summary>
        /// Kullanıcının okunmamış bildirimlerini getirir
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <param name="limit">Maksimum bildirim sayısı</param>
        /// <returns>Bildirimler listesi ve sayısı</returns>
        Task<UnreadNotificationsResponseDto> GetUnreadNotificationsAsync(int userId, int limit = 10);
        
        /// <summary>
        /// Kullanıcının tüm bildirimlerini sayfalı şekilde getirir
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <param name="page">Sayfa numarası</param>
        /// <param name="limit">Sayfa başına bildirim sayısı</param>
        /// <returns>Bildirimler listesi</returns>
        Task<List<NotificationResponseDto>> GetNotificationsAsync(int userId, int page = 1, int limit = 10);
        
        /// <summary>
        /// Bildirimi okundu olarak işaretler
        /// </summary>
        /// <param name="notificationId">Bildirim ID</param>
        /// <param name="userId">Kullanıcı ID</param>
        /// <returns>İşlem durumu</returns>
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        
        /// <summary>
        /// Kullanıcının tüm bildirimlerini okundu olarak işaretler
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <returns>İşaretlenen bildirim sayısı</returns>
        Task<int> MarkAllAsReadAsync(int userId);
        
        /// <summary>
        /// Bildirimi siler
        /// </summary>
        /// <param name="notificationId">Bildirim ID</param>
        /// <param name="userId">Kullanıcı ID</param>
        /// <returns>İşlem durumu</returns>
        Task<bool> DeleteNotificationAsync(int notificationId, int userId);
    }
} 