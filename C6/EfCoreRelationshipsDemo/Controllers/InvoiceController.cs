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
    public async Task<ActionResult<IList<Invoice>>> GetInvoices(int page = 1,
                                                                      int pageSize = 10,
                                                                      InvoiceStatus? status = null)
    {
        if (_context.Invoices == null)
            return NotFound();

        return await _context.Invoices
                             .Include(x => x.InvoiceItems)  
                             .Where(x => status == null || x.Status == status) // filter the data
                             .OrderByDescending(x => x.InvoiceDate)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .AsSplitQuery() // avoids Catesian explosion problem, which means the amount of duplicated data in the result is too large. This will query all the invoices, then query the invoice items
                             .ToListAsync(); // executes the query from the IQueryable object and returns the result

        // generated query:
        /* SELECT 
         * [i].[Id], 
         * [i].[Amount], 
         * [i].[ContactName], 
         * [i].[Description], 
         * [i].[DueDate], 
         * [i].[InvoiceDate], 
         * [i].[InvoiceNumber], 
         * [i].[Status]
         * FROM [Invoices] AS [i]
         * ORDER BY [i].[InvoiceDate] DESC, [i].[Id]
         * OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
         * 
         * SELECT 
         * [i0].[Id], 
         * [i0].[Amount], 
         * [i0].[Description], 
         * [i0].[InvoiceId], 
         * [i0].[Name], 
         * [i0].[Quantity], 
         * [i0].[UnitPrice], 
         * [t].[Id]      
         * FROM (
           * SELECT 
           * [i].[Id], 
           * [i].[InvoiceDate]
           * FROM [Invoices] AS [i]
           * ORDER BY [i].[InvoiceDate] DESC
           * OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
         * ) AS [t]      
         * INNER JOIN [InvoiceItems] AS [i0] 
         * ON [t].[Id] = [i0].[InvoiceId]
         * ORDER BY 
         * [t].[InvoiceDate] DESC, 
         * [t].[Id] */
    }

    // GET: api/Invoices/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Invoice>> GetInvoice(Guid id)
    {
        if (_context.Invoices == null)
            return NotFound();

        var invoice = await _context.Invoices
                                    .Include(x => x.InvoiceItems)
                                    .SingleOrDefaultAsync(x => x.Id == id);

        // generated query
        /* SELECT 
         * [i0].[Id], 
         * [i0].[Amount], 
         * [i0].[Description], 
         * [i0].[InvoiceId], 
         * [i0].[Name], 
         * [i0].[Quantity], 
         * [i0].[UnitPrice], 
         * [t].[Id]
         * FROM (
           * SELECT TOP(1) 
           * [i].[Id]
           * FROM [Invoices] AS [i]
           * WHERE [i].[Id] = @__id_0
         * ) AS [t]
         * INNER JOIN [InvoiceItems] AS [i0] 
         * ON [t].[Id] = [i0].[InvoiceId]
         * ORDER BY [t].[Id] */

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

        // generated query
        /* DELETE FROM [i]
         * FROM [Invoices] AS [i]
         * WHERE [i].[Id] = @__id_0 */

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool InvoiceExists(Guid id)
    {
        return _context.Invoices.Any(e => e.Id == id);
    }
}
