using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lookif.Layers.Core.Infrastructure;
using Lookif.Layers.Core.MainCore.Base;
using Lookif.Layers.Core.MainCore.Identities;
using Lookif.Library.Common.Utilities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Lookif.Layers.Data;

public abstract class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
    public static Assembly CoreLayerAssembly { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.RegisterAllEntities<IEntity, ITemporal>(CoreLayerAssembly);
        modelBuilder.RegisterAllEntities<IEntity<Guid>, ITemporal>(CoreLayerAssembly);
        modelBuilder.RegisterEntityTypeConfiguration(CoreLayerAssembly);
        modelBuilder.AddRestrictDeleteBehaviorConvention();
        modelBuilder.AddSequentialGuidForIdConvention();
        modelBuilder.AddPluralizingTableNameConvention();
    }

    public override int SaveChanges()
    {
        _cleanString();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        _cleanString();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        _cleanString();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {

        _cleanString();
        return base.SaveChangesAsync(cancellationToken);
    }
    private void SoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                //case EntityState.Added:
                //    entry.CurrentValues["IsDeleted"] = false;
                //    // iterate over each nav. prop to performe cascade soft delete = false

                //    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.CurrentValues["IsDeleted"] = true;
                    // iterate over each nav. prop to performe cascade soft delete = true
                    IterateThroughNavigationProperties(entry);
                    break;
            }
        }
    }
    private void IterateThroughNavigationProperties(EntityEntry entry)
    {
        foreach (var navigationEntry in entry.Navigations.Where(n => !((INavigation)n.Metadata).IsOnDependent))
        {
            if (navigationEntry is CollectionEntry collectionEntry)
            {
                foreach (var dependentEntry in collectionEntry.CurrentValue)
                {
                    HandleDependent(Entry(dependentEntry));
                }
            }
            else
            {
                var dependentEntry = navigationEntry.CurrentValue;
                if (dependentEntry != null)
                {
                    HandleDependent(Entry(dependentEntry));
                }
            }
        }
    }
    private void HandleDependent(EntityEntry entry)
    {
        entry.CurrentValues["IsDeleted"] = true;
    }
    private void _cleanString()
    {
        var changedEntities = ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);
        foreach (var item in changedEntities)
        {
            if (item.Entity == null)
                continue;

            var properties = item.Entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(string));

            foreach (var property in properties)
            {
                var val = (string)property.GetValue(item.Entity, null);

                if (val.HasValue())
                {
                    var newVal = val.Fa2En().FixPersianChars();
                    if (newVal == val)
                        continue;
                    property.SetValue(item.Entity, newVal, null);
                }
            }
        }
    }
}
