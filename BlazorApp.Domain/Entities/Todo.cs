using BlazorApp.Domain.Interfaces;

namespace BlazorApp.Domain.Entities;

public class Todo<T> : Entity<T>, IAuditable, ISoftDelete where T : struct
{
    public string Title { get; set; }

    public bool IsCompleted { get; set; }
}