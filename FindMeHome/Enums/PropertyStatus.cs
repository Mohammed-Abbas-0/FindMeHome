namespace FindMeHome.Enums
{
    public enum PropertyStatus
    {
        Active = 1,
        Pending = 2,           // For new listings waiting approval
        PendingApproval = 3,   // For edits waiting approval
        PendingDeletion = 4,   // For delete requests
        Deleted = 5,           // Soft deleted
        Sold = 6,
        Rejected = 7,
        Expired = 8
    }
}
