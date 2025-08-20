using System.Net;

namespace ElasoftCommunityManagementSystem.Exceptions
{
    public class BusinessException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public BusinessException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class ResourceNotFoundException : BusinessException
    {
        public ResourceNotFoundException(string message) 
            : base(message, HttpStatusCode.NotFound)
        {
        }
    }

    public class UnauthorizedBusinessException : BusinessException
    {
        public UnauthorizedBusinessException(string message) 
            : base(message, HttpStatusCode.Forbidden)
        {
        }
    }

    public class ValidationException : BusinessException
    {
        public ValidationException(string message) 
            : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}