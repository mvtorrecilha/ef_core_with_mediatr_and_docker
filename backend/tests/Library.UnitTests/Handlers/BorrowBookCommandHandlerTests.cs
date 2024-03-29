﻿using FluentAssertions;
using Library.Application.Commands;
using Library.Application.Handlers;
using Library.Application.Notifications;
using Library.Domain.Entities;
using Library.Infra.ResponseNotifier;
using Library.Infra.ResponseNotifier.Common;
using Library.UnitTests.Mocks;
using Library.UnitTests.Mocks.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Library.UnitTests.Handlers;

public class BorrowBookCommandHandlerTests
{
    protected MockUnitOfWork _mockUnitOfWork;
    protected MockBookRepository _mockBookRepository;
    protected MockStudentRepository _mockStudentRepository;
    protected MockBorrowHistoryRepository _mockBorrowHistoryRepository;
    protected ResponseFormatter _responseFormatterMock;
    protected Notifier _notifier;
    protected MockMediatr _mockMediatr;
    private readonly BorrowBookCommandHandler _handler;

    public BorrowBookCommandHandlerTests()
    {
        _mockUnitOfWork = new MockUnitOfWork();
        _mockBookRepository = new MockBookRepository();
        _mockStudentRepository = new MockStudentRepository();
        _mockBorrowHistoryRepository = new MockBorrowHistoryRepository();
        _notifier = new Notifier();
        _mockMediatr = new MockMediatr();

        _handler = new BorrowBookCommandHandler(
            _mockMediatr.Object,
            _mockUnitOfWork.Object,
            _notifier);
    }

    [Fact]
    public async Task HandleBorrowBookCommand_EmptyEmail_ShouldHasError()
    {
        // Arrange
        var command = new BorrowBookCommand
        {
            BookId = new Guid("762141A7-ACA3-4919-9ACD-3FC86815F05B")
        };

        //Act
        await _handler.Handle(command, default);

        // Assert
        string expectedError = Errors.EmailCannotBeEmpty;
        _notifier.HasError.Should().BeTrue();
        _notifier.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);
    }

    [Fact]
    public async Task HandleBorrowBookCommand_StudentNotFoundByEmail_ShouldHasError()
    {
        // Arrange
        var command = new BorrowBookCommand
        {
            BookId = new Guid("762141A7-ACA3-4919-9ACD-3FC86815F05B"),
            StudentEmail = "test@gmail.com"
        };

        _mockStudentRepository.MockGetStudentRegisteredByEmailAsync(command.StudentEmail, null);
        _mockUnitOfWork.MockStudents(_mockStudentRepository);

        //Act
        await _handler.Handle(command, default);

        // Assert
        string expectedError = Errors.StudentNotFound;
        _notifier.HasError.Should().BeTrue();
        _notifier.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);
    }

    [Fact]
    public async Task HandleBorrowBookCommand_BookDoesntBelongToStudentCourse_ShouldHasError()
    {
        // Arrange
        var command = new BorrowBookCommand
        {
            BookId = new Guid("762141A7-ACA3-4919-9ACD-3FC86815F05B"),
            StudentEmail = "jr@gmail.com"
        };

        var student = new Student
        {
            Id = Guid.NewGuid(),
            Email = "test@gmail.com"
        };

        _mockStudentRepository.MockGetStudentRegisteredByEmailAsync(command.StudentEmail, student);
        _mockUnitOfWork.MockStudents(_mockStudentRepository);

        _mockBookRepository.MockBookBelongToTheCourseCategoryAsync(command.BookId, command.StudentEmail, null);
        _mockUnitOfWork.MockBooks(_mockBookRepository);
        
        //Act
        await _handler.Handle(command, default);

        // Assert
        string expectedError = Errors.TheBookDoesNotBelongToTheCourseCategory;
        _notifier.HasError.Should().BeTrue();
        _notifier.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);
    }

    [Fact]
    public async Task HandleBorrowBookCommand_BookIsLent_ShouldHasError()
    {
        // Arrange
        var command = new BorrowBookCommand
        {
            BookId = new Guid("762141A7-ACA3-4919-9ACD-3FC86815F05B"),
            StudentEmail = "jr@gmail.com"
        };

        var student = new Student
        {
            Id = Guid.NewGuid(),
            Email = "test@gmail.com"
        };

        _mockStudentRepository.MockGetStudentRegisteredByEmailAsync(command.StudentEmail, student);
        _mockUnitOfWork.MockStudents(_mockStudentRepository);

        var lentBook = new Book
        {
            Id = Guid.NewGuid(),
            IsLent = true
        };

        _mockBookRepository.MockBookBelongToTheCourseCategoryAsync(command.BookId, command.StudentEmail, lentBook);
        _mockUnitOfWork.MockBooks(_mockBookRepository);

        //Act
        await _handler.Handle(command, default);

        // Assert
        string expectedError = Errors.BookAlreadyLent;
        _notifier.HasError.Should().BeTrue();
        _notifier.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);
    }

    [Fact]
    public async Task HandleBorrowBookCommand_ValidBookAndEmail_ShouldBeSuccess()
    {
        // Arrange
        var command = new BorrowBookCommand
        {
            BookId = Guid.Parse("762141A7-ACA3-4919-9ACD-3FC86815F05B"),
            StudentEmail = "domain@domain.com"
        };

        var student = new Student
        {
            Id = Guid.Parse("162141A7-ACA3-4919-9ACD-3FC86815F05B"),
            Email = command.StudentEmail
        };

        _mockStudentRepository.MockGetStudentRegisteredByEmailAsync(command.StudentEmail, student);
        _mockUnitOfWork.MockStudents(_mockStudentRepository);

        var bookToBorrow = new Book
        {
            Id = command.BookId,
            IsLent = false
        };

        _mockBorrowHistoryRepository.MockAddAsync();
        _mockUnitOfWork.MockBorrowHistories(_mockBorrowHistoryRepository);
        _mockBookRepository.MockBookBelongToTheCourseCategoryAsync(command.BookId, command.StudentEmail, bookToBorrow);
        _mockBookRepository.MockGetByIdAsync(command.BookId, bookToBorrow);
        _mockBookRepository.MockUpdate(bookToBorrow);
        _mockUnitOfWork.MockBooks(_mockBookRepository);
        _mockUnitOfWork.MockComplete(1);
        _mockMediatr.MockPublish<BorrowedBookNotification>();

        //Act
        await _handler.Handle(command, default);

        // Assert
        _notifier.HasError.Should().BeFalse();
        _mockBookRepository
            .VerifyBookBelongToTheCourseCategoryAsync(command.BookId, command.StudentEmail, Times.Once());
        _mockBookRepository
           .VerifyUpdate(bookToBorrow, Times.Once());
        _mockMediatr.VerifyPublish<BorrowedBookNotification>(Times.Once());
    }
}
