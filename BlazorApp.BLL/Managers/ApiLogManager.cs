using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Interfaces;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.DAL;
using BlazorApp.DAL.Interfaces;
using BlazorApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BlazorApp.BLL.Managers;

public class ApiLogManager : IApiLogManager
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;
    private readonly IUserSession<Guid> _userSession;

    public ApiLogManager(IConfiguration configuration, IApplicationDbContext db, IMapper mapper,
        IUserSession<Guid> userSession)
    {
        _db = db;
        _mapper = mapper;
        _userSession = userSession;
        _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        _optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<ApiResponse> LogAsync(ApiLogItemModel apiLogItem)
    {
        await using var dbContext = new ApplicationDbContext(_optionsBuilder.Options, _userSession);
        await dbContext.ApiLogs.AddAsync(_mapper.Map<ApiLogItem<Guid>>(apiLogItem));
        await dbContext.SaveChangesAsync();

        return new ApiResponse(StatusCodes.Status200OK);
    }

    public async Task<ApiResponse> GetApiResponsesAsync()
    {
        return new ApiResponse(StatusCodes.Status200OK, "Retrieved Api Log",
            _mapper.Map<List<ApiLogItemModel>>(await _db.ApiLogs.ToListAsync()));
    }

    public async Task<ApiResponse> GetApiResponseByApplicationUserIdAsync(string applicationUserId)
    {
        return new ApiResponse(StatusCodes.Status200OK, "Retrieved Api Log",
            _mapper.Map<List<ApiLogItemModel>>(await _db.ApiLogs
                .Where(a => a.ApplicationUserId == Guid.Parse(applicationUserId))
                .ToListAsync()));
    }
}