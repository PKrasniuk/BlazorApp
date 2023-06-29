using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Helpers;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorApp.BLL.Managers;

public class DbLogManager : IDbLogManager
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<DbLogManager> _logger;
    private readonly IMapper _mapper;

    public DbLogManager(IApplicationDbContext db, ILogger<DbLogManager> logger, IMapper mapper)
    {
        _db = db;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ApiResponse> GetAsync(int pageSize, int page, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _db.Logs.AsNoTracking().OrderByDescending(x => x.TimeStamp)
                .Skip(pageSize * page).Take(pageSize).ToArrayAsync(cancellationToken);
            var count = await _db.Logs.CountAsync(cancellationToken);
            var syncPoint = Guid.NewGuid();
            if (data.Length == 0)
                return new ApiResponse(StatusCodes.Status204NoContent,
                    $"No results for request; pageSize={pageSize}; page={page}");

            foreach (var item in data)
                item.LogProperties = RegexUtilities.DirtyXmlPropertyParser(item.Properties);

            return new ApiResponse(
                StatusCodes.Status200OK,
                $"Retrieved Type=DbLogModel;pageSize={pageSize};page={page}",
                _mapper.Map<List<DbLogModel>>(data),
                paginationDetails: new PaginationDetails<Guid>
                {
                    CollectionSize = count,
                    PageIndex = page,
                    PageSize = 25,
                    SyncPointReference = syncPoint
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation Error while retrieving Db Logs");
            return new ApiResponse(StatusCodes.Status500InternalServerError,
                "Operation Error while retrieving Db Logs");
        }
    }
}