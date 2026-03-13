using System.Diagnostics.CodeAnalysis;

namespace Utils.Shared
{
    public struct  Error:IEquatable<Error>
    {
        public Error(string errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;

        }
        public string ErrorCode { get; }
        public string ErrorMessage { get; }
        public bool Equals(Error other)
        {
            return ErrorCode == other.ErrorCode &&
                ErrorMessage == other.ErrorMessage;
        }
        public override bool Equals(object @object) => 
            @object is Error other && Equals(other);
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
