using EfCoreRelationshipsDemo.Data;
using EfCoreRelationshipsDemo.Enums;
using EfCoreRelationshipsDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EfCoreRelationshipsDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly SampleDbContext _context;

    public InvoiceController(SampleDbContext context)
    {
        _context = context;
    }

    // GET: api/Invoices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices(int page = 1,
                                                                      int pageSize = 10,
                                                                      InvoiceStatus? status = null)
    {
        if (_context.Invoices == null)
            return NotFound();

        return await _context.Invoices
                             .AsQueryable() // convert DbSet<Invoice> to IQueryable<Invoice>
                             .Where(x => status == null || x.Status == status) // filter the data
                             .OrderByDescending(x => x.InvoiceDate)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync(); // executes the query from the IQueryable object and returns the result
    }

    // GET: api/Invoices/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Invoice>> GetInvoice(Guid id)
    {
        if (_context.Invoices == null)
            return NotFound();

        var invoice = await _context.Invoices.FindAsync(id);

        if (invoice == null)
            return NotFound();
        

        return invoice;
    }

    // PUT: api/Invoices/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutInvoice(Guid id, Invoice invoice)
    {
        if (id != invoice.Id)
            return BadRequest();
        

        _context.Entry(invoice).State = EntityState.Modified;

        try
        {
            var invoiceToUpdate = await _context.Invoices.FindAsync(id);
            if (invoiceToUpdate == null)
                return NotFound();

            // Update only the properties that have changed _context
            _context.Entry(invoiceToUpdate).CurrentValues.SetValues(invoice);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!InvoiceExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Invoices
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Invoice>> PostInvoice(Invoice invoice)
    {
        if (_context.Invoices == null)
        {
            return Problem("Entity set 'InvoiceDbContext.Invoices' is null.");
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetInvoice", new { id = invoice.Id }, invoice);
    }

    // DELETE: api/Invoices/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoice(Guid id)
    {
        if (_context.Invoices == null)
            return NotFound();

        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
            return NotFound();
        

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool InvoiceExists(Guid id)
    {
        return _context.Invoices.Any(e => e.Id == id);
    }
}
