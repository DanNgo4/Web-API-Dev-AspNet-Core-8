using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcDemo.Protos;

namespace GrpcDemo.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        var updateInvoiceDueDateRequest = new UpdateInvoiceDueDateRequest
        {
            InvoiceId = Guid.Parse("3193C36C-2AAB-49A7-A0B1-6BDB3B69DEA1").ToString(),
            DueDate = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(30)),
            GracePeriod = Duration.FromTimeSpan(TimeSpan.FromDays(10))
        };

        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}
