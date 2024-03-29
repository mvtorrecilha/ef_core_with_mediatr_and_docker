﻿
using Library.Application.Commands;
using Library.Application.Handlers;
using Library.Domain;
using Library.Domain.Repositories;
using Library.Infra.ResponseNotifier;
using Library.Infra.ResponseNotifier.Common;
using Library.Repository;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Library.IoC;

public class NativeInjectorBootStrapper
{
    public static void RegisterServices(IServiceCollection services)
    {
        //Repositories
        services.AddScoped(typeof(IBookRepository), typeof(BookRepository));
        services.AddScoped(typeof(IStudentRepository), typeof(StudentRepository));
        services.AddScoped(typeof(ICourseRepository), typeof(CourseRepository));
        services.AddScoped(typeof(IBorrowHistoryRepository), typeof(BorrowHistoryRepository));  

        //Services
        services.AddScoped(typeof(INotifier), typeof(Notifier));

        //ResponseFormatter
        services.AddScoped(typeof(IResponseFormatter), typeof(ResponseFormatter));

        //Mediatr
        services.AddScoped<IRequestHandler<BorrowBookCommand, Unit>, BorrowBookCommandHandler>();

        //Unit of Work
        services.AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork));
    }
}
