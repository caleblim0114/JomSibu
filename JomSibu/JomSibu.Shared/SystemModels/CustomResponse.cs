namespace JomSibu.Shared.SystemModels
{
    public class CustomResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }

    public static class CustomStatusCodes
    {
        public const int Ok = 0;
        public const int WrongEmailOrPassword = 1;
        public const int AccountRegistered = 2;
        public const int InvalidData = 3;
        public const int InvalidMediaType = 4;
        public const int InsufficientData = 5;
        public const int CurrentlyNotAvailable = 6;
        public const int EmailHavenConfirm = 6;
        public const int VendorHavenVerified = 6;
        public const int VendorHavenSubmitIdentity = 7;
        public const int UnknownError = 999;
    }
}
