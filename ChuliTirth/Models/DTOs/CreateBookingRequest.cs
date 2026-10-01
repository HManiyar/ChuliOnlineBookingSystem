namespace ChuliTirth.Models.DTOs;

public class CreateBookingRequest
{
    public int RoomTypeId { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int RoomsNeeded { get; set; } = 1;
    public int Adults { get; set; } = 1;
    public int Children { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; } = "India";
    public string? SpecialRequirements { get; set; }
    public string? IdType { get; set; }
    public string? IdNumber { get; set; }

    public int? UserId { get; set; }
    public bool AcceptedRules { get; set; }
}
