using Data.Contexts;
using Data.Interfaces;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Data.Repositories;

public abstract class BaseRepository<TEntity>(DataContext context) : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly DataContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    private IDbContextTransaction _transaction = null!;


    #region Transaction Management
    public virtual async Task BeginTransactionAsync()
    {
        _transaction ??= await _context.Database.BeginTransactionAsync();
    }

    public virtual async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null!;
        }
    }

    public virtual async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null!;
        }
    }
    #endregion


    // CREATE
    public virtual async Task<RepositoryResult<bool?>> AddAsync(TEntity entity)
    {
        if (entity == null)
            return RepositoryResult<bool?>.BadRequest("Entity cannot be null.");

        try
        {
            await _dbSet.AddAsync(entity);
            return RepositoryResult<bool?>.Ok();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return RepositoryResult<bool?>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }


    // READ
    public virtual async Task<RepositoryResult<IEnumerable<TEntity>>> GetAllAsync(bool orderByDescending = false, Expression<Func<TEntity, object>>? sortBy = null, Expression<Func<TEntity, bool>>? where = null, params Expression<Func<TEntity, object>>[] includes)
    {
        try
        {
            IQueryable<TEntity> query = _dbSet;

            if (where != null)
                query = query.Where(where);

            if (includes != null && includes.Length != 0)
                foreach (var include in includes)
                    query = query.Include(include);

            if (sortBy != null)
                query = orderByDescending
                    ? query.OrderByDescending(sortBy)
                    : query.OrderBy(sortBy);

            var entities = await query.ToListAsync();
            return RepositoryResult<IEnumerable<TEntity>>.Ok(entities);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return RepositoryResult<IEnumerable<TEntity>>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }

    public virtual async Task<RepositoryResult<IEnumerable<TEntity>>> GetByQueryAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryBuilder = null)
    {
        IQueryable<TEntity> query = _dbSet;

        if (queryBuilder != null)
            query = queryBuilder(query);

        var entities = await query.ToListAsync();
        return RepositoryResult<IEnumerable<TEntity>>.Ok(entities);
    }

    public virtual async Task<RepositoryResult<TEntity>> GetOneAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includes)
    {
        try
        {
            IQueryable<TEntity> query = _dbSet;

            if (includes != null && includes.Length != 0)
                foreach (var include in includes)
                    query = query.Include(include);


            var entity = await query.FirstOrDefaultAsync(where);
            if (entity == null)
                return RepositoryResult<TEntity>.NotFound("Entity not found.");

            return RepositoryResult<TEntity>.Ok(entity);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return RepositoryResult<TEntity>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }

    public virtual async Task<RepositoryResult<bool?>> ExistsAsync(Expression<Func<TEntity, bool>> expression)
    {
        try
        {
            if (expression == null)
                return RepositoryResult<bool?>.BadRequest("Expression cannot be null.");

            var exists = await _dbSet.AnyAsync(expression);
            return exists
                ? RepositoryResult<bool?>.NoContent()
                : RepositoryResult<bool?>.NotFound("Entity not found.");
        }
        catch (Exception ex)
        {
            // catch db connection failure etc
            Debug.WriteLine(ex.Message);
            return RepositoryResult<bool?>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }

    }


    // UPDATE
    public virtual RepositoryResult<bool?> Update(TEntity entity)
    {
        if (entity == null)
            return RepositoryResult<bool?>.BadRequest("Entity cannot be null.");

        try
        {
            _dbSet.Update(entity);
            return RepositoryResult<bool?>.Ok();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return RepositoryResult<bool?>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }


    // DELETE
    public virtual RepositoryResult<bool?> Delete(TEntity entity)
    {
        if (entity == null)
            return RepositoryResult<bool?>.BadRequest("Entity cannot be null.");

        try
        {
            _dbSet.Remove(entity);
            return RepositoryResult<bool?>.Ok();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return RepositoryResult<bool?>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }


    // SAVE CHANGES
    public virtual async Task<RepositoryResult<bool?>> SaveAsync()
    {
        try
        {
            var result = await _context.SaveChangesAsync() > 0;
            return RepositoryResult<bool?>.Ok();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return RepositoryResult<bool?>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }
}
