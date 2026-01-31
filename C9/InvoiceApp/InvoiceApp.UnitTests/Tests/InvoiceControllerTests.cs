using FluentAssertions;
using InvoiceApp.UnitTests.Fixtures;
using InvoiceApp.WebApi.Controllers;
using InvoiceApp.WebApi.Enums;
using InvoiceApp.WebApi.Interfaces;
using InvoiceApp.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace InvoiceApp.UnitTests.Tests;
public class InvoiceControllerTests 
           : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public InvoiceControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetInvoices_ShouldReturnInvoices()
    {
        // Arrange
        var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        var emailServiceMock = new Mock<IEmailService>();

        var controller = new InvoiceController(invoiceRepositoryMock.Object, emailServiceMock.Object);

        // Act
        var actionResult = await controller.GetInvoicesAsync();
        // Assert
        var result = actionResult.Result as OkObjectResult;
        Assert.NotNull(result);

        var returnResult = Assert.IsAssignableFrom<List<Invoice>>(result.Value);
        //Assert.NotNull(returnResult);
        //Assert.Equal(2, returnResult.Count);
        //Assert.Contains(returnResult, i => i.InvoiceNumber == "INV-001");
        //Assert.Contains(returnResult, i => i.InvoiceNumber == "INV-002");

        returnResult.Should().NotBeNull();

        //returnResult.Should().HaveCount(2);
        returnResult.Count.Should().Be(2, "The number of invoices should be 2");

        returnResult.Should().Contain(x => x.InvoiceNumber == "INV-001");
        returnResult.Should().Contain(x => x.InvoiceNumber == "INV-002");
    }

    [Theory]
    [InlineData(InvoiceStatus.AwaitPayment)]
    [InlineData(InvoiceStatus.Draft)]
    public async Task GetInvoicesByStatus_ShouldReturnInvoices(InvoiceStatus status)
    {
        // Arrange
        var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        var emailServiceMock = new Mock<IEmailService>();

        var controller = new InvoiceController(invoiceRepositoryMock.Object, emailServiceMock.Object);

        // Act
        var actionResult = await controller.GetInvoicesAsync(status);

        // Assert
        var result = actionResult.Result as OkObjectResult;
        Assert.NotNull(result);

        var returnResult = Assert.IsAssignableFrom<List<Invoice>>(result.Value);

        Assert.NotNull(returnResult);
        Assert.Single(returnResult);
        Assert.Equal(status, returnResult.First().Status);
    }

    [Fact]
    public async Task CreateInvoice_ShouldCreateInvoice()
    {
        // Arrange
        var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        var emailServiceMock = new Mock<IEmailService>();

        var controller = new InvoiceController(invoiceRepositoryMock.Object, emailServiceMock.Object);

        // Act
        var invoices = await invoiceRepositoryMock.Object.GetInvoicesAsync();
        var contactId = invoices.First().Id;

        var invoice = new Invoice
        {
            DueDate = DateTimeOffset.Now.AddDays(30),
            ContactId = contactId,
            Status = InvoiceStatus.Draft,
            InvoiceDate = DateTimeOffset.Now,
            InvoiceItems =
            {
                new()
                {
                    Id          = Guid.NewGuid(),
                    Description = "Item 1",
                    Quantity    = 1,
                    UnitPrice   = 100,
                },
                new()
                {
                    Id          = Guid.NewGuid(),
                    Description = "Item 2",
                    Quantity    = 2,
                    UnitPrice   = 200,
                }
            }
        };

        var actionResult = await controller.CreateInvoiceAsync(invoice);

        // Assert
        var result = actionResult.Result as CreatedAtActionResult;
        Assert.NotNull(result);

        var returnResult = Assert.IsAssignableFrom<Invoice>(result.Value);
        var invoiceCreated = await invoiceRepositoryMock.Object.GetInvoiceAsync(returnResult.Id);

        Assert.NotNull(invoiceCreated);
        Assert.Equal(InvoiceStatus.Draft, invoiceCreated.Status);
        Assert.Equal(500, invoiceCreated.Amount);
        Assert.Equal(3, (await invoiceRepositoryMock.Object.GetInvoicesAsync()).Count);
        Assert.Equal(contactId, invoiceCreated.ContactId);
        // You can add more assertions here

        // Clean up
        await invoiceRepositoryMock.Object.DeleteInvoiceAsync(invoiceCreated.Id);
    }

    [Fact]
    public async Task UpdateInvoice_ShouldUpdateInvoice()
    {
        // Arrange
        var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        var emailServiceMock = new Mock<IEmailService>();

        var controller = new InvoiceController(invoiceRepositoryMock.Object, emailServiceMock.Object);

        // Act
        // Start a transaction to prevent the changes from being saved to the database
        var invoices = await invoiceRepositoryMock.Object.GetInvoicesAsync();
        var invoice = invoices.First();
        invoice.Status      = InvoiceStatus.Paid;
        invoice.Description = "Updated description";
        invoice.InvoiceItems.ForEach(x =>
        {
            x.Description = "Updated description";
            x.UnitPrice  += 100;
        });

        var expectedAmount = invoice.InvoiceItems.Sum(x => x.UnitPrice * x.Quantity);

        await controller.UpdateInvoiceAsync(invoice.Id, invoice);

        // Assert
        // Explicitly lear the change tracker, it's used to track the changes made to the entities
        // If we don't clear the change tracker, we'll get the tracked entities
        // instead of querying the DB
        //dbContext.ChangeTracker.Clear();

        //var invoiceUpdated = await dbContext.Invoices.SingleAsync(x => x.Id == invoice.Id);
        var invoiceUpdated = await invoiceRepositoryMock.Object.GetInvoiceAsync(invoice.Id);
        //Assert.Equal(InvoiceStatus.Paid, invoiceUpdated.Status);
        //Assert.Equal("Updated description", invoiceUpdated.Description);
        //Assert.Equal(expectedAmount, invoiceUpdated.Amount);
        //Assert.Equal(2, dbContext.Invoices.Count());
        invoiceUpdated.Status.Should().Be(InvoiceStatus.Paid);
        invoiceUpdated.Description.Should().Be("Updated description");
        invoiceUpdated.Amount.Should().Be(expectedAmount);
        invoiceUpdated.InvoiceItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInvoices_ShouldReturnInvoices2()
    {
        // Arrange
        var repositoryMock = new Mock<IInvoiceRepository>();

        repositoryMock.Setup(x => x.GetInvoicesAsync(It.IsAny<InvoiceStatus>(),
                                                     It.IsAny<int>(),
                                                     It.IsAny<int>()))
                      .ReturnsAsync((int page, int pageSize, InvoiceStatus? status) =>
                          _fixture.Invoices
                                  .Where(x => status == null || x.Status == status)
                                  .OrderByDescending(x => x.InvoiceDate)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList());

        var emailServiceMock = new Mock<IEmailService>();
        var controller = new InvoiceController(repositoryMock.Object, emailServiceMock.Object);

        // Act
        var actionResult = await controller.GetInvoicesAsync();

        // Assert
        var result = actionResult.Result as OkObjectResult;
        Assert.NotNull(result);

        var returnResult = Assert.IsAssignableFrom<List<Invoice>>(result.Value);
        Assert.NotNull(returnResult);
        Assert.Equal(2, returnResult.Count);
        Assert.Contains(returnResult, i => i.InvoiceNumber == "INV-001");
        Assert.Contains(returnResult, i => i.InvoiceNumber == "INV-002");
    }

    [Fact]
    public async Task GetInvoice_ShouldReturnInvoice()
    {
        // Arrange
        var repositoryMock = new Mock<IInvoiceRepository>();
        repositoryMock.Setup(x => x.GetInvoiceAsync(It.IsAny<Guid>()))
                      .ReturnsAsync((Guid x) => _fixture.Invoices
                                                        .FirstOrDefault(y => y.Id == x));

        var emailServiceMock = new Mock<IEmailService>();

        var controller = new InvoiceController(repositoryMock.Object, emailServiceMock.Object);

        // Act
        var invoice = _fixture.Invoices.First();
        var actionResult = await controller.GetInvoiceAsync(invoice.Id);

        // Assert
        var result = actionResult.Result as OkObjectResult;
        Assert.NotNull(result);

        var returnResult = Assert.IsAssignableFrom<Invoice>(result.Value);
        Assert.NotNull(returnResult);
        Assert.Equal(invoice.Id, returnResult.Id);
        Assert.Equal(invoice.InvoiceNumber, returnResult.InvoiceNumber);
    }

    [Fact]
    public async Task GetInvoice_ShouldReturnNotFound()
    {
        // Arrange
        var repositoryMock = new Mock<IInvoiceRepository>();
        repositoryMock.Setup(x => x.GetInvoiceAsync(It.IsAny<Guid>()))
                      .ReturnsAsync((Guid x) => _fixture.Invoices
                                                        .FirstOrDefault(y => y.Id == x));

        var emailServiceMock = new Mock<IEmailService>();

        var controller = new InvoiceController(repositoryMock.Object, emailServiceMock.Object);

        // Act
        var actionResult = await controller.GetInvoiceAsync(Guid.NewGuid());

        // Assert
        var result = actionResult.Result as NotFoundResult;
        Assert.NotNull(result);
    }
}
