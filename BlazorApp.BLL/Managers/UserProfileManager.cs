using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Constants;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.DAL.Interfaces;
using BlazorApp.Domain.Entities;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.BLL.Managers;

public class UserProfileManager : IUserProfileManager
{
    private readonly IApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public UserProfileManager(IApplicationDbContext db, IHttpContextAccessor httpContextAccessor, IMapper mapper)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<ApiResponse> GetUserProfileAsync()
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirst(JwtClaimTypes.Subject).Value;
        return await GetUserProfileAsync(userId);
    }

    public async Task<ApiResponse> GetUserProfileAsync(string userId)
    {
        var profileQuery = from userProf in _db.UserProfiles
            where userProf.UserId == Guid.Parse((ReadOnlySpan<char>)userId)
            select userProf;

        UserProfile<Guid> userProfile;
        if (!await profileQuery.AnyAsync())
            userProfile = new UserProfile<Guid>
            {
                UserId = Guid.Parse((ReadOnlySpan<char>)userId)
            };
        else
            userProfile = await profileQuery.FirstAsync();

        return new ApiResponse(StatusCodes.Status200OK, "Retrieved User Profile",
            _mapper.Map<UserProfileModel>(userProfile));
    }

    public async Task<ApiResponse> UpsertUserProfileAsync(UserProfileModel userProfile)
    {
        try
        {
            var profileQuery = from prof in _db.UserProfiles
                where prof.UserId == Guid.Parse((ReadOnlySpan<char>)userProfile.UserId)
                select prof;
            if (profileQuery.Any())
            {
                var profile = profileQuery.First();
                profile.LastUpdatedDate = DateTime.Now;
                profile.LastPageVisited = userProfile.LastPageVisited;
                profile.IsNavMinified = userProfile.IsNavMinified;
                profile.IsNavOpen = userProfile.IsNavOpen;
                profile.Count = userProfile.Count;
                _db.UserProfiles.Update(profile);
            }
            else
            {
                var profile = new UserProfile<Guid>
                {
                    LastUpdatedDate = DateTime.Now,
                    LastPageVisited = userProfile.LastPageVisited,
                    IsNavMinified = userProfile.IsNavMinified,
                    IsNavOpen = userProfile.IsNavOpen,
                    Count = userProfile.Count,
                    UserId = Guid.Parse((ReadOnlySpan<char>)userProfile.UserId)
                };
                await _db.UserProfiles.AddAsync(profile);
            }

            await _db.SaveChangesAsync();

            return new ApiResponse(StatusCodes.Status200OK, "Updated User Profile");
        }
        catch (Exception)
        {
            return new ApiResponse(StatusCodes.Status400BadRequest, "Failed to Retrieve User Profile");
        }
    }

    public async Task<ApiResponse> GetLastPageVisitedAsync(string userName)
    {
        var lastPageVisited = CommonConstants.DefaultPageVisited;

        var userProfile = await (from userProf in _db.UserProfiles
            join user in _db.Users on userProf.UserId equals user.Id
            where user.UserName == userName
            select userProf).FirstOrDefaultAsync();
        if (userProfile != null)
            lastPageVisited = !string.IsNullOrEmpty(userProfile.LastPageVisited)
                ? userProfile.LastPageVisited
                : lastPageVisited;

        return new ApiResponse(StatusCodes.Status200OK, "Last Page Visited", lastPageVisited);
    }
}