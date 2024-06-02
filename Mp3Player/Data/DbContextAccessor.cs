using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mp3Player.Data;

public sealed class DbContextAccessor : IDbContextAccessor, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private IServiceScope? _currentScope;
    
    public DbContextAccessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public TContext ResolveContext<TContext>()
        where TContext : DbContext
    {
        _currentScope?.Dispose();
        _currentScope = _serviceProvider.CreateScope();
        return _currentScope.ServiceProvider.GetRequiredService<TContext>();
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