using System.Threading.Tasks;

namespace BlazorApp.DAL.Interfaces;

public interface IDatabaseInitializer
{
    Task SeedAsync();
}