using System.Text.Json.Serialization;

namespace EfCoreRelationshipsDemo.Models;

// Dependant/Child entity to Invoice
public class InvoiceItem
{
    public Guid     Id          { get; set; }
    public string   Name        { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public decimal  UnitPrice   { get; set; }
    public decimal  Quantity    { get; set; }
    public decimal  Amount      { get; set; }
    public Guid     InvoiceId   { get; set; }   // foreign key property

    [JsonIgnore]    // exclude this from JSON responses to avoid cyclic reference
    public Invoice? Invoice     { get; set; }   // REFERENCE navigation property
}
