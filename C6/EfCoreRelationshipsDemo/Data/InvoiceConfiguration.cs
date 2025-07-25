﻿using EfCoreRelationshipsDemo.Models;
using EfCoreRelationshipsDemo.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace EfCoreRelationshipsDemo.Data;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);
        builder.Property(p => p.Id).HasColumnName("Id");
        builder.Property(p => p.InvoiceNumber).HasColumnName("InvoiceNumber").HasColumnType("varchar(32)").IsRequired();
        builder.HasIndex(p => p.InvoiceNumber).IsUnique();
        builder.Property(p => p.ContactName).HasColumnName("ContactName").HasMaxLength(32).IsRequired();
        builder.Property(p => p.Description).HasColumnName("Description").HasMaxLength(256);
        builder.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
        builder.Property(p => p.InvoiceDate).HasColumnName("InvoiceDate").HasColumnType("datetimeoffset").IsRequired();
        builder.Property(p => p.DueDate).HasColumnName("DueDate").HasColumnType("datetimeoffset").IsRequired();
        builder.Property(p => p.Status).HasColumnName("Status").HasMaxLength(16).HasConversion
        (
            v => v.ToString(),
            v => (InvoiceStatus)Enum.Parse(typeof(InvoiceStatus), v)
        );

        // Explicitly configure One to Many relationship
        // only need to define One to Many (or vice versa) on 1 table
        //builder.HasMany(i => i.InvoiceItems)
        //       .WithOne(i => i.Invoice)
        //       .HasForeignKey(i => i.InvoiceId);

        // Use the own type to configure the InvoiceItems collection
        // With this, we wouldn't need to configure the relationship on the InvoiceItem side,
        // InvoiceItem wouldn't need to have its own DbSet<InvoiceItem>,
        // and it will always be included in Invoice (doesn't have to use .Include(x => x.InvoiceItems)
        //
        // However, use this approach only when the dependent entity only exists inside the principal entity's context,
        // and doesn't have its own lifecycle (can't be queried independently),
        // Stick with the other approach for full control over relationships and delete behaviour
        /*builder.OwnsMany
        (
            p => p.InvoiceItems,
            a =>
            {
                a.WithOwner(x => x.Invoice).HasForeignKey(x => x.InvoiceId);
                a.ToTable("InvoiceItems");
            }
        );*/
    }
}
