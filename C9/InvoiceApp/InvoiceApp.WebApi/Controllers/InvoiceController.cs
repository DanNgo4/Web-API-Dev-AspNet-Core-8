using InvoiceApp.WebApi.Enums;
using InvoiceApp.WebApi.Interfaces;
using InvoiceApp.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceApp.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IEmailService _emailService;

    public InvoiceController(IInvoiceRepository invoiceRepository, 
                             IEmailService emailService)
    {
        _invoiceRepository = invoiceRepository;
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Invoice>>> GetInvoicesAsync(InvoiceStatus? status = null, 
                                                                    int page = 1,
                                                                    int pageSize = 10)
    {
        var invoices = await _invoiceRepository.GetInvoicesAsync(status, page, pageSize);
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Invoice>> GetInvoiceAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetInvoiceAsync(id);

        return invoice == null ? NotFound() 
                               : Ok(invoice);
        
    }

    [HttpPost]
    public async Task<ActionResult<Invoice>> CreateInvoiceAsync(Invoice invoice)
    {
        invoice.Id = Guid.NewGuid();
        invoice.InvoiceNumber = GenerateInvoiceNumber;
        invoice.InvoiceDate = DateTimeOffset.UtcNow;
        invoice.DueDate = invoice.InvoiceDate.AddDays(30);
        invoice.Status = InvoiceStatus.Draft;
        invoice.InvoiceItems.ForEach(x => x.Amount = x.UnitPrice * x.Quantity);
        invoice.Amount = invoice.InvoiceItems.Sum(x => x.Amount);

        invoice = await _invoiceRepository.CreateInvoiceAsync(invoice);

        return CreatedAtAction("GetInvoice", new { id = invoice.Id }, invoice);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInvoiceAsync(Guid id, Invoice invoice)
    {
        var existingInvoice = await _invoiceRepository.GetInvoiceAsync(id);
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

        existingInvoice.InvoiceItems.ForEach(x => x.Amount = x.UnitPrice * x.Quantity);
        existingInvoice.Amount = existingInvoice.InvoiceItems.Sum(x => x.Amount);

        await _invoiceRepository.UpdateInvoiceAsync(existingInvoice);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoiceAsync(Guid id)
    {
        await _invoiceRepository.DeleteInvoiceAsync(id);

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateInvoiceStatusAsync(Guid id, InvoiceStatus status)
    {
        var existingInvoice = await _invoiceRepository.GetInvoiceAsync(id);
        if (existingInvoice == null)
        {
            return NotFound();
        }

        existingInvoice.Status = status;

        await _invoiceRepository.UpdateInvoiceAsync(existingInvoice);

        return NoContent();
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendInvoiceAsync(Guid id)
    {
        var existingInvoice = await _invoiceRepository.GetInvoiceAsync(id);
        if (existingInvoice == null)
        {
            return NotFound();
        }

        var (to, subject, body) = _emailService.GenerateInvoiceEmail(existingInvoice);
        await _emailService.SendEmailAsync(to, subject, body);
        
        existingInvoice.Status = InvoiceStatus.AwaitPayment;
        await _invoiceRepository.UpdateInvoiceAsync(existingInvoice);

        return NoContent();
    }

    private static string GenerateInvoiceNumber
    {
        get
        {
            var random = new Random();
            return $"INV-{random.Next(0, 1000000):000000}";
        }
    }
}
