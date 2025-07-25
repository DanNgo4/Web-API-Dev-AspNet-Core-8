using System.ComponentModel.DataAnnotations;

namespace ConcurrencyConflictDemo.Models;

public class Product
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string  Description { get; set; } = string.Empty;
    public decimal Price       { get; set; } = 0;
    public int     Inventory   { get; set; }    // stores number of products in stock

    /*[Timestamp]
    public byte[]  RowVersion  { get; set; }*/    // to use the native database-generated concurrency token in SQL server

    [ConcurrencyCheck]
    public Guid    Version     { get; set; }    // application managed concurrency token
}
