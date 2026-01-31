using InvoiceApp.UnitTests.Fixtures;
using InvoiceApp.WebApi.Controllers;
using InvoiceApp.WebApi.Enums;
using InvoiceApp.WebApi.Interfaces;
using Moq;

namespace InvoiceApp.UnitTests.Tests;

[Collection("TransactionalTests")]
public class TransactionalInvoiceControllerTests : IDisposable
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
        var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        var emailServiceMock = new Mock<IEmailService>();

        var controller = new InvoiceController(invoiceRepositoryMock.Object, emailServiceMock.Object);

        // Act
        var invoices = await invoiceRepositoryMock.Object.GetInvoicesAsync(InvoiceStatus.AwaitPayment);
        var invoice = invoices.First();
        await controller.UpdateInvoiceStatusAsync(invoice.Id, InvoiceStatus.Paid);

        // Assert
        //dbContext.ChangeTracker.Clear();

        var updatedInvoice = await invoiceRepositoryMock.Object.GetInvoiceAsync(invoice.Id);
        Assert.NotNull(updatedInvoice);
        Assert.Equal(InvoiceStatus.Paid, updatedInvoice.Status);
    }

    public void Dispose()
    {
        _fixture.Cleanup();
    }
}
