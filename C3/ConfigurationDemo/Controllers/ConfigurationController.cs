using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ConfigurationDemo.Models;

namespace ConfigurationDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfigurationController(IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    [Route("my-key")]
    public ActionResult GetMyKey()
    {
        var myKey = configuration["MyKey"];
        return Ok(myKey);
    }

    [HttpGet]
    [Route("database-configuration")]
    public ActionResult GetDatabaseConfiguration()
    {
        var type = configuration["Database:Type"];
        var connectionString = configuration["Database:ConnectionString"];
        return Ok(new { Type = type, ConnectionString = connectionString });
    }

    [HttpGet]
    [Route("database-configuration-with-bind")]
    public ActionResult GetDatabaseConfigurationWithBind()
    {
        var databaseOption = new DatabaseOption();

        // The SectionName is defined in the DatabaseOption class, which shows the section name in the appsettings.json file
        configuration.GetSection(DatabaseOption.SectionName).Bind(databaseOption);

        // alternative implementation:
        // configuration.Bind(DatabaseOption.SectionName, databaseOption);

        return Ok(new { databaseOption.Type, databaseOption.ConnectionString });
    }

    [HttpGet]
    [Route("database-configuration-with-ioptions")]
    public ActionResult GetDatabaseConfigurationWithIOptions([FromServices] IOptions<DatabaseOption> options)
    {
        var databaseOption = options.Value;

        return Ok(new { databaseOption.Type, databaseOption.ConnectionString });
    }
}
