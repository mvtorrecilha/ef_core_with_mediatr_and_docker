﻿using Library.Api.ViewModels;
using Library.Application.Commands;
using Library.Domain;
using Library.Infra.ResponseNotifier;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class BookController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IResponseFormatter _responseFormatter;
    private readonly IMediator _mediator;

    public BookController(IUnitOfWork unitOfWork,
                          IResponseFormatter responseFormatter,
                          IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _responseFormatter = responseFormatter;
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Route("/api/books")]
    public async Task<IActionResult> Get()
    {
        var books = await _unitOfWork.Books.GetAllBooksNotLentAsync();

        return Ok(books.Select(b => new BookViewModel()
        {
            Author = b.Author,
            Id = b.Id,
            Pages = b.Pages,
            Publisher = b.Publisher,
            Title = b.Title
        }));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]   
    [Route("/api/books/{bookId:guid}/student/{studentEmail}/borrow")]
    public async Task<IActionResult> Post([FromRoute] BorrowBookCommand command)
    {
        await _mediator.Send(command);
        return _responseFormatter.Format();
    }
}
