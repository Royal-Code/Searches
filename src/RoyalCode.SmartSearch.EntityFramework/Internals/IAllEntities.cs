﻿using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Abstractions;

#pragma warning disable S2326 // Unused type parameters should be removed

namespace RoyalCode.SmartSearch.EntityFramework.Internals;

/// <summary>
/// <para>
///     Represents a search for all entities of a specific type using the Entity Framework Core.
/// </para>
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IAllEntities<TDbContext, TEntity> : IAllEntities<TEntity>
    where TEntity : class
    where TDbContext : DbContext
{ }