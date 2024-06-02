namespace APBD_REST4.Models.DTOs;

public class TripsListDto
{
    public int PageNum { get; set; }
    public int PageSize { get; set; }
    public int AllPages { get; set; }
    public string Name { get; set; } = null;
    public string Description { get; set; } = null;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public ICollection<CountryDto> Countries { get; set; } = new List<CountryDto>();
    public ICollection<ClientDto> Clients { get; set; } = new List<ClientDto>();
}