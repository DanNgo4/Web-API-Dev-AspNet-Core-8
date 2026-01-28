using InvoiceApp.UnitTests.Fixtures;
using InvoiceApp.WebApi.Controllers;
using InvoiceApp.WebApi.Enums;
using InvoiceApp.WebApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace InvoiceApp.UnitTests.Tests;

[Collection("TransactionalTests")]
public class TransactionalInvoiceControllerTests 
           : IDisposable
{
    private readonly TransactionalTestDatabaseFixture _fixture;

    public TransactionalInvoiceControllerTests(TransactionalTestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UpdateInvoiceStatusAsync_ShouldUpdateStatus()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();

        var emailServiceMock = new Mock<IEmailService>();

        var controller = new InvoiceController(dbContext, emailServiceMock.Object);

        // Act
        var invoice = await dbContext.Invoices.FirstAsync(x => x.Status == InvoiceStatus.AwaitPayment);
        await controller.UpdateInvoiceStatusAsync(invoice.Id, InvoiceStatus.Paid);

        // Assert
        dbContext.ChangeTracker.Clear();

        var updatedInvoice = await dbContext.Invoices.FindAsync(invoice.Id);
        Assert.NotNull(updatedInvoice);
        Assert.Equal(InvoiceStatus.Paid, updatedInvoice.Status);
    }

    public void Dispose()
    {
        _fixture.Cleanup();
    }
}
