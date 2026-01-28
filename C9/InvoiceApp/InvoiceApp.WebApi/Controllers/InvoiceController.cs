using InvoiceApp.WebApi.Data;
using InvoiceApp.WebApi.Enums;
using InvoiceApp.WebApi.Interfaces;
using InvoiceApp.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApp.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoiceController(InvoiceDbContext dbContext, IEmailService emailService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Invoice>>> GetInvoicesAsync(InvoiceStatus? status = null, 
                                                                    int page = 1,
                                                                    int pageSize = 10)
    {
        var invoices = await dbContext.Invoices
                                      .Include(x => x.Contact)
                                      .Where(x => status == null || x.Status == status)
                                      .OrderByDescending(i => i.InvoiceDate)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Invoice>> GetInvoiceAsync(Guid id)
    {
        var invoice = await dbContext.Invoices
                                     .Include(x => x.Contact)
                                     .SingleOrDefaultAsync(x => x.Id == id);
        if (invoice == null)
        {
            return NotFound();
        }

        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<Invoice>> CreateInvoiceAsync(Invoice invoice)
    {
        invoice.Id = Guid.NewGuid();
        invoice.InvoiceNumber = GenerateInvoiceNumber();
        invoice.InvoiceDate = DateTimeOffset.UtcNow;
        invoice.DueDate = invoice.InvoiceDate.AddDays(30);
        invoice.Status = InvoiceStatus.Draft;
        invoice.InvoiceItems.ForEach(x => x.Amount = x.UnitPrice * x.Quantity);
        invoice.Amount = invoice.InvoiceItems.Sum(x => x.Amount);

        var contact = await dbContext.Contacts.FindAsync(invoice.ContactId);
        if (contact == null)
        {
            return BadRequest("Contact not found.");
        }
        invoice.Contact = contact;

        await dbContext.Invoices.AddAsync(invoice);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction("GetInvoice", new { id = invoice.Id }, invoice);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInvoiceAsync(Guid id, Invoice invoice)
    {
        var existingInvoice = await dbContext.Invoices
                                             .SingleOrDefaultAsync(x => x.Id == id);
        if (existingInvoice == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(invoice.Id.ToString()) 
         && invoice.Id != Guid.Empty 
         && invoice.Id != id)
        {
            return BadRequest("Invoice Id cannot be changed.");
        }

        invoice.Id = id;
        dbContext.Entry(existingInvoice)
                 .CurrentValues
                 .SetValues(invoice);

        existingInvoice.InvoiceItems.ForEach(x => x.Amount = x.UnitPrice * x.Quantity);
        existingInvoice.Amount = existingInvoice.InvoiceItems.Sum(x => x.Amount);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoiceAsync(Guid id)
    {
        var existingInvoice = await dbContext.Invoices
            .SingleOrDefaultAsync(x => x.Id == id);
        if (existingInvoice == null)
        {
            return NotFound();
        }
        dbContext.Invoices.Remove(existingInvoice);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateInvoiceStatusAsync(Guid id, InvoiceStatus status)
    {
        var existingInvoice = await dbContext.Invoices
                                             .SingleOrDefaultAsync(x => x.Id == id);
        if (existingInvoice == null)
        {
            return NotFound();
        }

        existingInvoice.Status = status;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendInvoiceAsync(Guid id)
    {
        var existingInvoice = await dbContext.Invoices
                                             .Include(x => x.Contact)
                                             .SingleOrDefaultAsync(x => x.Id == id);
        if (existingInvoice == null)
        {
            return NotFound();
        }

        var (to, subject, body) = emailService.GenerateInvoiceEmail(existingInvoice);
        await emailService.SendEmailAsync(to, subject, body);
        
        existingInvoice.Status = InvoiceStatus.AwaitPayment;
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private string GenerateInvoiceNumber()
    {
        var random = new Random();
        return $"INV-{random.Next(0, 1000000):000000}";
    }

}
