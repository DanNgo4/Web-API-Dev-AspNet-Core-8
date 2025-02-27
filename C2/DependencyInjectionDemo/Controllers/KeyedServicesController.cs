using DependencyInjectionDemo.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DependencyInjectionDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class KeyedServicesController : ControllerBase
{
    [HttpGet("sql")]
    public ActionResult GetSqlData([FromKeyedServices("sqlDatabaseService")] IDataService dataService)
        => Content(dataService.GetData());

    [HttpGet("cosmos")]
    public ActionResult GetCosmosData([FromKeyedServices("cosmosDatabaseService")] IDataService dataService)
        => Content(dataService.GetData());
}
