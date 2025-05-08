namespace Tutorial8.Models.DTOs;

public class TripDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DateRangeFrom { get; set; }
    public int DateRangeTo { get; set; }
    public int MaxPeople { get; set; }
    
    public List<CountryDTO> Countries { get; set; }
}