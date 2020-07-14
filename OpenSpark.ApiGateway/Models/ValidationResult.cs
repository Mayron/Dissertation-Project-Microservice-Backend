namespace OpenSpark.ApiGateway.Models
{
    public class ValidationResult
    {
        public static ValidationResult Success = new ValidationResult(true, "Success");

        public bool IsValid { get; }
        public string Message { get; }

        public ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }
    }
}
