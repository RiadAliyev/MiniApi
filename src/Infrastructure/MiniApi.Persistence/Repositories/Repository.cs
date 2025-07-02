using Microsoft.EntityFrameworkCore;
using MiniApi.Domain.Entities;
using System.Linq.Expressions;
using System.Linq;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Persistence.Contexts;

namespace MiniApi.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity, new()
{
    private readonly MiniApiDbContext _context;
    private readonly DbSet<T> Table;

    public Repository(MiniApiDbContext context)
    {
        _context = context;
        Table = _context.Set<T>();
    }

    public async Task AddAsync(T entity)
    {
        await Table.AddAsync(entity);
    }

    public void Update(T entity)
    {
        Table.Update(entity);
    }

    public void Delete(T entity)
    {
        Table.Remove(entity);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await Table.FindAsync(id);
    }

    public IQueryable<T> GetByFiltered(
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>[]? include = null,
        bool IsTracking = false)
    {
        IQueryable<T> query = Table;

        if (predicate is not null)
            query = query.Where(predicate);

        if (include is not null && include.Length > 0)
        {
            foreach (var includeExpression in include)
                query = query.Include(includeExpression);
        }

        if (!IsTracking)
            query = query.AsNoTracking();

        return query;
    }

    public IQueryable<T> GetAll(bool IsTracking = false)
    {
        return !IsTracking ? Table.AsNoTracking() : Table;
    }

    public IQueryable<T> GetAllFiltered(
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>[]? include = null,
        Expression<Func<T, object>>? orderBy = null,
        bool IsOrderByAsc = true,
        bool IsTracking = false)
    {
        IQueryable<T> query = Table;

        if (predicate is not null)
            query = query.Where(predicate);

        if (include is not null && include.Length > 0)
        {
            foreach (var includeExpression in include)
                query = query.Include(includeExpression);
        }

        if (orderBy is not null)
        {
            query = IsOrderByAsc
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);
        }

        if (!IsTracking)
            query = query.AsNoTracking();

        return query;
    }

    public async Task SaveChangeAsync()
    {
        await _context.SaveChangesAsync();
    }
}

