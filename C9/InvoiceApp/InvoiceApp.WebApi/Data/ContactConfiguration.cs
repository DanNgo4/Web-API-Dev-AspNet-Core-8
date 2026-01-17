using InvoiceApp.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceApp.WebApi.Data;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName(nameof(Contact.Id));
        builder.Property(x => x.FirstName).HasColumnName(nameof(Contact.FirstName)).HasColumnType("varchar(64)").IsRequired();
        builder.Property(x => x.LastName).HasColumnName(nameof(Contact.LastName)).HasColumnType("varchar(64)").IsRequired();
        builder.Property(x => x.Email).HasColumnName(nameof(Contact.Email)).HasColumnType("varchar(128)").IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Phone).HasColumnName(nameof(Contact.Phone)).HasColumnType("varchar(32)");
        builder.Property(x => x.Company).HasColumnName(nameof(Contact.Company)).HasColumnType("varchar(128)");
        builder.Property(x => x.Address).HasColumnName(nameof(Contact.Address)).HasColumnType("varchar(128)");
        builder.Property(x => x.City).HasColumnName(nameof(Contact.City)).HasColumnType("varchar(64)");
        builder.Property(x => x.State).HasColumnName(nameof(Contact.State)).HasColumnType("varchar(64)");
        builder.Property(x => x.Zip).HasColumnName(nameof(Contact.Zip)).HasColumnType("varchar(16)");
        builder.Property(x => x.Country).HasColumnName(nameof(Contact.Country)).HasColumnType("varchar(64)");
        builder.Property(x => x.Notes).HasColumnName(nameof(Contact.Notes)).HasColumnType("varchar(256)");
    }
}
