using InvoiceApp.WebApi.Enums;
using InvoiceApp.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceApp.WebApi.Data;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName(nameof(Invoice.Id));
        builder.Property(x => x.InvoiceNumber).HasColumnName(nameof(Invoice.InvoiceNumber)).HasColumnType("varchar(32)").IsRequired();
        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.Property(x => x.ContactId).HasColumnName(nameof(Invoice.ContactId)).IsRequired();
        builder.Property(x => x.Description).HasColumnName(nameof(Invoice.Description)).HasMaxLength(256);
        builder.Property(x => x.Amount).HasColumnName(nameof(Invoice.Amount)).HasPrecision(18, 2);
        builder.Property(x => x .InvoiceDate).HasColumnName(nameof(Invoice.InvoiceDate)).HasColumnType("datetimeoffset").IsRequired();
        builder.Property(x => x.DueDate).HasColumnName(nameof(Invoice.DueDate)).HasColumnType("datetimeoffset").IsRequired();
        builder.Property(x => x.Status).HasColumnName(nameof(Invoice.Status)).HasMaxLength(16).HasConversion(x => x.ToString(),
                                                                                                             x => (InvoiceStatus)Enum.Parse(typeof(InvoiceStatus), x));

        //Use the owned type to configure the InvoiceItems collection
        builder.OwnsMany(x => x.InvoiceItems, a =>
        {
            a.WithOwner().HasForeignKey(x => x.InvoiceId);
            a.ToTable("InvoiceItems");
            a.Property(x => x.Id).HasColumnName(nameof(InvoiceItem.Id));
            a.Property(x => x.Name).HasColumnName(nameof(InvoiceItem.Name)).HasMaxLength(64).IsRequired();
            a.Property(x => x.Description).HasColumnName(nameof(InvoiceItem.Description)).HasMaxLength(256);
            a.Property(x => x.UnitPrice).HasColumnName(nameof(InvoiceItem.UnitPrice)).HasPrecision(8, 2);
            a.Property(x => x.Quantity).HasColumnName(nameof(InvoiceItem.Quantity)).HasPrecision(8, 2);
            a.Property(x => x.Amount).HasColumnName(nameof(InvoiceItem.Amount)).HasPrecision(18, 2);
            a.Property(x => x.InvoiceId).HasColumnName(nameof(InvoiceItem.InvoiceId));
        });

        builder.HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactId);
    }
}
