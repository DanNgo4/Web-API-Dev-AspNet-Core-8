using BasicEfCoreDemo.Enums;

namespace BasicEfCoreDemo.Models;

public class Invoice
{
    public Guid           Id            { get; set; }
    public string         InvoiceNumber { get; set; } = string.Empty;
    public string         ContactName   { get; set; } = string.Empty;
    public string?        Description   { get; set; }
    public decimal        Amount        { get; set; }
    public DateTimeOffset InvoiceDate   { get; set; }
    public DateTimeOffset DueDate       { get; set; }
    public InvoiceStatus  status        { get; set; }
}
