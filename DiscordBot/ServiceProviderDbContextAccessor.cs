using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot;

public sealed class ServiceProviderDbContextAccessor : IDbContextAccessor, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private  IServiceScope _currentScope;
    
    public ServiceProviderDbContextAccessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public TContext? ResolveContext<TContext>() where TContext : DbContext
    {
        _currentScope?.Dispose();
        _currentScope = _serviceProvider.CreateScope();
        return _currentScope.ServiceProvider.GetService<TContext>();
    }
    
    public void Dispose()
    {
        _currentScope?.Dispose();
    }
}

public interface IDbContextAccessor
{
    TContext ResolveContext<TContext>() where TContext : DbContext;
}