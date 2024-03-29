﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Domain.Entities;

[Table("BorrowHistory")]
public class BorrowHistory : BaseEntity
{
    public Guid BookId { get; set; }

    public Book Book { get; set; }

    public Guid StudentId { get; set; }

    public Student Student { get; set; }

    public DateTime BorrowDate { get; set; }

    public DateTime? ReturnDate { get; set; }
}
