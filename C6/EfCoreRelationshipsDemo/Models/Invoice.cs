using EfCoreRelationshipsDemo.Enums;
namespace EfCoreRelationshipsDemo.Models;

// Principal/Parent entity to InvoiceItem
public class Invoice
{
    public Guid              Id            { get; set; }    // primary key property (principal key)
    public string            InvoiceNumber { get; set; } = string.Empty;
    public string            ContactName   { get; set; } = string.Empty;
    public string?           Description   { get; set; }
    public decimal           Amount        { get; set; }
    public DateTimeOffset    InvoiceDate   { get; set; }
    public DateTimeOffset    DueDate       { get; set; }
    public InvoiceStatus     Status        { get; set; }
    public List<InvoiceItem> InvoiceItems  { get; set; } = new();   // COLLECTION navigation property
}
