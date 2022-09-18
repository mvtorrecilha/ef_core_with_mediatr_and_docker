﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Core.Models;

[Table("Course")]
public class Course : BaseEntity
{
    public string Name { get; set; }

    public ICollection<Student> Students { get; set; }

    public ICollection<CourseCategory> CourseCategories { get; set; }

}
