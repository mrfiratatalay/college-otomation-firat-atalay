 
using System.IO;  
using System.Threading.Tasks;  
using System.Collections.Generic;  
using ElasoftCommunityManagementSystem.Interfaces;  

namespace ElasoftCommunityManagementSystem.Services  
{  
    public class DocumentService : IDocumentService  
    {  
        public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType)  
        {  
            // Implementation for uploading a document  
            return "documentId";  
        }  

        public async Task<(Stream FileStream, string ContentType, string FileName)> GetDocumentAsync(string documentId)
        {
            // Belgeler wwwroot/uploads altında tutuluyor varsayımıyla
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(uploadsPath, documentId);

            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var contentType = GetContentType(filePath);
            var fileName = Path.GetFileName(filePath);

            return (fileStream, contentType, fileName);
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };
        }

        public async Task<bool> DeleteDocumentAsync(string documentId)  
        {  
            // Implementation for deleting a document  
            return true;  
        }  

        public async Task<List<string>> GetDocumentsByUserIdAsync(int userId)  
        {  
            // Implementation for retrieving documents by user ID  
            return new List<string>();  
        }  

        public async Task<bool> AssignDocumentToEntityAsync(string documentId, string entityType, int entityId)  
        {  
            // Implementation for assigning a document to an entity  
            return true;  
        }  
    }  
}  
