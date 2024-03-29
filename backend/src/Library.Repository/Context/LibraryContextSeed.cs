﻿using Library.Domain.Entities;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Repository.Context;

public class LibraryContextSeed
{
    public async Task SeedAsync(
       LibraryContext context,
       ILogger<LibraryContextSeed> logger)
    {
        var policy = CreatePolicy(logger, nameof(LibraryContextSeed));

        await policy.ExecuteAsync(async () =>
        {
            if (!context.Courses.Any()) { await SeedDefaultCourseCategory(context); }
            if (!context.Students.Any()) { await SeedDefaultStudents(context); }
            if (!context.Books.Any()) { await SeedDefaultBooks(context); }
        });
    }

    private static async Task SeedDefaultCourseCategory(LibraryContext context)
    {
        string[,] categoryCourses = new string[2, 4];
        categoryCourses[0, 0] = "a2c6f987-d83f-4fb3-9982-68553965b421";
        categoryCourses[0, 1] = "Systems Analysis";
        categoryCourses[0, 2] = "IT";
        categoryCourses[0, 3] = "6031d727-7fb5-47fa-a6d5-6e5cfeceff21";

        categoryCourses[1, 0] = "7ecb8a32-4452-43a0-b78a-ca1552303304";
        categoryCourses[1, 1] = "Civil Engineering";
        categoryCourses[1, 2] = "Civil Engineering";
        categoryCourses[1, 3] = "20efaba1-64bd-4b7f-82f4-c1d05550e305";

        for (int i = 0; i < 2; i++)
        {
            var newCourse = new Course
            {
                Id = new(categoryCourses[i, 0].ToString()),
                Name = categoryCourses[i, 1].ToString(),
                CourseCategories = new List<CourseCategory>()
            };

            var newCategory = new Category
            {
                Id = new(categoryCourses[i, 3].ToString()),
                Name = categoryCourses[i, 2].ToString(),
                CourseCategories = new List<CourseCategory>()
            };

            var categoryCourse = new CourseCategory
            {
                Category = newCategory,
                Course = newCourse
            };

            newCourse.CourseCategories.Add(categoryCourse);
            newCategory.CourseCategories.Add(categoryCourse);

            await context.AddAsync(newCourse);
            await context.AddAsync(newCategory);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDefaultStudents(LibraryContext context)
    {
        await context.Students.AddAsync(new Student
        {
            Id = new("1673a9fd-191a-479c-a41f-3dc5611aa98e"),
            Name = "Student One",
            Email = "student_one@domain.com",
            CourseId = new("7ecb8a32-4452-43a0-b78a-ca1552303304")
        });

        await context.Students.AddAsync(new Student
        {
            Id = new("2c4833c4-5138-48d7-80a7-60abe82a5c6c"),
            Name = "Student Two",
            Email = "student_two@domain.com",
            CourseId = new("a2c6f987-d83f-4fb3-9982-68553965b421")
        });

        await context.Students.AddAsync(new Student
        {
            Id = new("3a2ecda5-e160-4a42-898b-8f4a73989688"),
            Name = "Student Three",
            Email = "student_three@domain.com",
            CourseId = new("a2c6f987-d83f-4fb3-9982-68553965b421")
        });

        await context.Students.AddAsync(new Student
        {
            Id = new("4f35054b-7a3a-4dce-8355-cf81b8b223d1"),
            Name = "Student Four",
            Email = "student_four@domain.com",
            CourseId = new("7ecb8a32-4452-43a0-b78a-ca1552303304")
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedDefaultBooks(LibraryContext context)
    {
        await context.Books.AddAsync(new Book
        {
            Title = "Clean Code",
            Author = "Uncle Bob",
            Pages = 429,
            Publisher = "Atlas",
            CategoryId = new("6031d727-7fb5-47fa-a6d5-6e5cfeceff21"),
            IsLent = false
        });

        await context.Books.AddAsync(new Book
        {
            Title = "Pragmatic Programmer 2",
            Author = "Andrew Hunt",
            Pages = 352,
            Publisher = "Addison-Wesley Professional",
            CategoryId = new("6031d727-7fb5-47fa-a6d5-6e5cfeceff21"),
            IsLent = false
        });

        await context.Books.AddAsync(new Book
        {
            Title = "O edifício até sua cobertura",
            Author = "Hélio Alves de Azeredo",
            Pages = 193,
            Publisher = "Blucher",
            CategoryId = new("20efaba1-64bd-4b7f-82f4-c1d05550e305"),
            IsLent = false
        });

        await context.Books.AddAsync(new Book
        {
            Title = "Concreto armado: eu te amo",
            Author = "Manoel Henrique Campos",
            Pages = 652,
            Publisher = "Blucher",
            CategoryId = new("20efaba1-64bd-4b7f-82f4-c1d05550e305"),
            IsLent = false
        });

        await context.SaveChangesAsync();
    }

    private static AsyncRetryPolicy CreatePolicy(
           ILogger<LibraryContextSeed> logger,
           string prefix,
           int retries = 3)
    {
        return Policy.Handle<SqlException>().
            WaitAndRetryAsync(
                retryCount: retries,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(
                        exception,
                        "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}",
                        prefix,
                        exception.GetType().Name,
                        exception.Message,
                        retry,
                        retries);
                }
            );
    }
}
