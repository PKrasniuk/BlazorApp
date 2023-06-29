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

public class MessageManager : IMessageManager
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public MessageManager(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<ApiResponse> CreateMessageAsync(MessageModel message)
    {
        var dMessage = _mapper.Map<Message<Guid>>(message);
        await _db.Messages.AddAsync(dMessage);
        await _db.SaveChangesAsync();

        message.Id = dMessage.Id.ToString();
        return new ApiResponse(StatusCodes.Status200OK, "Created Message", message);
    }

    public async Task<ApiResponse> GetMessagesAsync()
    {
        return new ApiResponse(StatusCodes.Status200OK, "Retrieved Messages",
            _mapper.Map<List<MessageModel>>(await _db.Messages.OrderBy(m => m.When).ToListAsync()));
    }

    public async Task<ApiResponse> DeleteMessageAsync(string id)
    {
        var dbMessage = _db.Messages.FirstOrDefault(m => m.Id == Guid.Parse((ReadOnlySpan<char>)id));
        if (dbMessage != null)
        {
            _db.Messages.Remove(dbMessage);
            await _db.SaveChangesAsync();
            return new ApiResponse(StatusCodes.Status200OK, "Soft Delete Message");
        }

        return new ApiResponse(StatusCodes.Status400BadRequest, "Failed to Delete Message");
    }
}