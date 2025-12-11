using SmartClass.Domain.Resources;
using System.Runtime.Serialization;

namespace SmartClass.Domain.Exceptions.FileExceptions
{
    [Serializable]
    public class FileNotFoundException : FileException
    {
        public FileNotFoundException()
            : base(ErrorMessages.FileNotFound) { }

        public FileNotFoundException(Exception innerException)
            : base(ErrorMessages.FileNotFound, innerException) { }

        public FileNotFoundException(string path)
            : base(ErrorMessages.FileNotFound, path) { }

        protected FileNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
