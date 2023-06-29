using System;
using AutoMapper;
using BlazorApp.Domain.Entities;

namespace BlazorApp.Common.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TodoModel, Todo<Guid>>();
        CreateMap<UserProfileModel, UserProfile<Guid>>();
        CreateMap<ApiLogItemModel, ApiLogItem<Guid>>();
        CreateMap<MessageModel, Message<Guid>>();
        CreateMap<DbLogModel, DbLog>();

        CreateMap<Todo<Guid>, TodoModel>();
        CreateMap<UserProfile<Guid>, UserProfileModel>();
        CreateMap<ApiLogItem<Guid>, ApiLogItemModel>();
        CreateMap<Message<Guid>, MessageModel>();
        CreateMap<DbLog, DbLogModel>();

        CreateMap<Guid, string>().ConvertUsing(o => o == Guid.Empty ? null : o.ToString());
        CreateMap<string, Guid>()
            .ConvertUsing(s => !string.IsNullOrEmpty(s) ? Guid.Parse(s) : Guid.Empty);
    }
}