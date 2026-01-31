using InvoiceApp.WebApi.Enums;
using InvoiceApp.WebApi.Models;

namespace InvoiceApp.WebApi.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?>             GetInvoiceAsync(Guid id);
    Task<IList<Invoice>>       GetInvoicesAsync(InvoiceStatus? status = null,
                                                int currentPage = 1,
                                                int pageSize = 10);
    Task<IList<Invoice>>       GetInvoicesByContactIdAsync(Guid contactId,
                                                           InvoiceStatus? status = null,
                                                           int currentPage = 1,
                                                           int pageSize = 10);
    Task<Invoice>              CreateInvoiceAsync(Invoice invoice);
    Task<Invoice?>             UpdateInvoiceAsync(Invoice invoice);
    Task                       DeleteInvoiceAsync(Guid id);
}
