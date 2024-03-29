﻿using Library.Api.ViewModels;
using Library.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public StudentController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Route("/api/students")]
    public async Task<IActionResult> Get()
    {
        var students = await _unitOfWork.Students.GetAllStudentsWithCourseAsync();

        return Ok(students.Select(s => new StudentViewModel()
        {
            Name = s.Name,
            Email = s.Email,
            Course = s.Course.Name
        }));
    }
}
