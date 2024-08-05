using Microsoft.Extensions.DependencyInjection;
namespace GhostOfJoe;

public interface IBot
{
    Task StartAsync(ServiceProvider services);

    Task StopAsync();
}
