﻿using Library.Domain.Entities;
using Library.Domain.Repositories;
using Library.Repository.Context;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.Repository;

public class StudentRepository : BaseRepository<Student>, IStudentRepository
{
    public StudentRepository(LibraryContext context) : base(context)
    {
    }

    public Task<IEnumerable<Student>> GetAllStudentsWithCourseAsync()
    {
        return GetAllWithIncludes(s => s.Course);
    }

    public Task<Student> GetStudentRegisteredByEmailAsync(string studentEmail)
    {
        return FindByAsync(s => s.Email == studentEmail);
    }
}
