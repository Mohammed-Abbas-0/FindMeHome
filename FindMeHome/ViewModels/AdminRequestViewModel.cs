namespace FindMeHome.ViewModels
{
    public class AdminRequestViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RequestType Type { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public enum RequestType
    {
        SellerRegistration,
        Verification
    }
}
