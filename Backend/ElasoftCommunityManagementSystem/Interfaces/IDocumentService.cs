namespace ElasoftCommunityManagementSystem.Interfaces
{
    public interface IDocumentService
    {
        Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType);
        Task<(Stream FileStream, string ContentType, string FileName)> GetDocumentAsync(string documentId);
        Task<bool> DeleteDocumentAsync(string documentId);
        Task<List<string>> GetDocumentsByUserIdAsync(int userId);
        Task<bool> AssignDocumentToEntityAsync(string documentId, string entityType, int entityId);
    }
} 