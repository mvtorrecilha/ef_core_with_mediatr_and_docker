﻿using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Library.Repository.EntityConfigurations;

public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        ArgumentNullException.ThrowIfNull(nameof(builder));

        builder.ToTable("Category");

        builder
           .HasKey(bc => bc.Id)
           .IsClustered(false)
           .HasName("PK_Category");

        builder.Property(s => s.Name)
            .HasColumnType("nvarchar(150)")
            .IsRequired(true);

        builder.HasMany(c => c.Books)
            .WithOne(p => p.Category);
    }
}
