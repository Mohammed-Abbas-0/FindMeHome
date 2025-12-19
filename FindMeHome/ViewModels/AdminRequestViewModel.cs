namespace FindMeHome.ViewModels
{
    public class AdminRequestViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RequestType Type { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int? PropertyId { get; set; }
        public string? PropertyTitle { get; set; }
        public DateTime? RequestDate { get; set; }
    }

    public enum RequestType
    {
        SellerRegistration,
        Verification,
        PropertyEdit,
        PropertyDeletion
    }
}
