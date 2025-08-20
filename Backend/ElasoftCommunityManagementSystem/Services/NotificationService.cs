using ElasoftCommunityManagementSystem.Dtos;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasoftCommunityManagementSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateNotificationAsync(int userId, string title, string message, string type, int? entityId = null)
        {
            var notification = new NotificationModel
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                EntityId = entityId,
                Read = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification.NotificationId;
        }

        public async Task CreateNotificationForMultipleUsersAsync(List<int> userIds, string title, string message, string type, int? entityId = null)
        {
            var notifications = userIds.Select(userId => new NotificationModel
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                EntityId = entityId,
                Read = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task<UnreadNotificationsResponseDto> GetUnreadNotificationsAsync(int userId, int limit = 10)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.Read)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .Select(n => new NotificationResponseDto
                {
                    Id = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    EntityId = n.EntityId,
                    Read = n.Read,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.Read);

            return new UnreadNotificationsResponseDto
            {
                Notifications = notifications,
                Count = count
            };
        }

        public async Task<List<NotificationResponseDto>> GetNotificationsAsync(int userId, int page = 1, int limit = 10)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(n => new NotificationResponseDto
                {
                    Id = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    EntityId = n.EntityId,
                    Read = n.Read,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return false;
            }

            notification.Read = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.Read)
                .ToListAsync();

            if (!unreadNotifications.Any())
            {
                return 0;
            }

            foreach (var notification in unreadNotifications)
            {
                notification.Read = true;
            }

            await _context.SaveChangesAsync();
            return unreadNotifications.Count;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return false;
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 