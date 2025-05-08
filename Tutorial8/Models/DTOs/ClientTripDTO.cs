namespace Tutorial8.Models.DTOs;
public class ClientTripDTO
{
    public int TripId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public DateTime DateRangeFrom { get; set; }
    public DateTime DateRangeTo { get; set; }

    public int MaxPeople { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}

