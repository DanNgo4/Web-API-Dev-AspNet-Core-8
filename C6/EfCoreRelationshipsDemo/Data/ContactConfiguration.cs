using EfCoreRelationshipsDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreRelationshipsDemo.Data;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FirstName).IsRequired();
        builder.Property(c => c.LastName).IsRequired();
        builder.Property(c => c.Email).IsRequired();
        builder.Property(c => c.Phone).IsRequired();

        // Only need to define relationship on 1 side (preferrably on the dependant/child entity that has foreign key property)
        //builder.HasOne(c => c.Address)
        //       .WithOne(a => a.Contact)
        //       .HasForeignKey<Address>(a => a.ContactId);
    }
}
