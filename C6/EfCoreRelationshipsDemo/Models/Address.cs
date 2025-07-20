namespace EfCoreRelationshipsDemo.Models;

// Dependant entity in One to One relationship with Contact
public class Address
{
    public Guid    Id        { get; set; }
    public string  Street    { get; set; } = string.Empty;
    public string  City      { get; set; } = string.Empty;
    public string  State     { get; set; } = string.Empty;
    public string  ZipCode   { get; set; } = string.Empty;
    public string  Country   { get; set; } = string.Empty;
    public Guid    ContactId { get; set; }  // implies that Address is the dependant entity since it has the foreign key property
    public Contact Contact   { get; set; }
}
