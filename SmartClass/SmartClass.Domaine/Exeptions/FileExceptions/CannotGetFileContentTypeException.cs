using SmartClass.Domain.Resources;
using System.Runtime.Serialization;

namespace SmartClass.Domain.Exceptions.FileExceptions
{
    [Serializable]
    public class CannotGetFileContentTypeException : FileException
    {
        public CannotGetFileContentTypeException()
            : base(ErrorMessages.CannotGetFileContentType) { }

        public CannotGetFileContentTypeException(Exception innerException)
            : base(ErrorMessages.CannotGetFileContentType, innerException) { }

        public CannotGetFileContentTypeException(string path)
            : base(ErrorMessages.CannotGetFileContentType, path) { }

        protected CannotGetFileContentTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
