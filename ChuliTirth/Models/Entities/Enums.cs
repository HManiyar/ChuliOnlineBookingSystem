namespace ChuliTirth.Models.Entities;

public enum UserRole
{
    Guest = 0,
    Staff = 1,
    Manager = 2,
    Admin = 3
}

public enum RoomStatus
{
    Available = 0,
    Occupied = 1,
    Blocked = 2,
    Maintenance = 3,
    Inactive = 4
}

public enum BookingStatus
{
    Pending = 0,
    PaymentPending = 1,
    Confirmed = 2,
    CheckedIn = 3,
    CheckedOut = 4,
    Cancelled = 5,
    Completed = 6,
    NoShow = 7,
    Rejected = 8
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3,
    NotRequired = 4
}

public enum AnnouncementPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
