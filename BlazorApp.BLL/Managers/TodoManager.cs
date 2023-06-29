using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.DAL.Interfaces;
using BlazorApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.BLL.Managers;

public class TodoManager : ITodoManager
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public TodoManager(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<ApiResponse> GetTodoListAsync()
    {
        return new ApiResponse(StatusCodes.Status200OK, "Retrieved Todo List",
            _mapper.Map<List<TodoModel>>(await _db.Todos.ToListAsync()));
    }

    public async Task<ApiResponse> GetTodoAsync(string id)
    {
        var todo = await _db.Todos.FirstOrDefaultAsync(t => t.Id == Guid.Parse(id));
        return todo != null
            ? new ApiResponse(StatusCodes.Status200OK, "Retrieved Todo", _mapper.Map<TodoModel>(todo))
            : new ApiResponse(StatusCodes.Status400BadRequest, "Failed to Retrieve Todo");
    }

    public async Task<ApiResponse> CreateTodoAsync(TodoModel todo)
    {
        var dTodo = _mapper.Map<Todo<Guid>>(todo);
        await _db.Todos.AddAsync(dTodo);
        await _db.SaveChangesAsync();

        todo.Id = dTodo.Id.ToString();
        return new ApiResponse(StatusCodes.Status200OK, "Created Todo", todo);
    }

    public async Task<ApiResponse> UpdateTodoAsync(TodoModel todo)
    {
        var dbTodo = _db.Todos.FirstOrDefault(t => t.Id == Guid.Parse(todo.Id));
        if (dbTodo != null)
        {
            dbTodo.Title = todo.Title;
            dbTodo.IsCompleted = todo.IsCompleted;

            _db.Todos.Update(dbTodo);
            await _db.SaveChangesAsync();
            return new ApiResponse(StatusCodes.Status200OK, "Updated Todo", _mapper.Map<TodoModel>(dbTodo));
        }

        return new ApiResponse(StatusCodes.Status400BadRequest, "Failed to update Todo");
    }

    public async Task<ApiResponse> DeleteTodoAsync(string id)
    {
        var dbTodo = _db.Todos.FirstOrDefault(t => t.Id == Guid.Parse(id));
        if (dbTodo != null)
        {
            _db.Todos.Remove(dbTodo);
            await _db.SaveChangesAsync();
            return new ApiResponse(StatusCodes.Status200OK, "Soft Delete Todo");
        }

        return new ApiResponse(StatusCodes.Status400BadRequest, "Failed to Delete Todo");
    }
}