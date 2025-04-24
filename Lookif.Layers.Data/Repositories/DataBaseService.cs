using Lookif.Layers.Core.Infrastructure.Base;
using Lookif.Layers.Core.Infrastructure.Base.DataInitializer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lookif.Layers.Data.Repositories;

public class DataBaseService : IDataBaseService, ISingletonDependency
{
    protected readonly ApplicationDbContext DbContext; //ToDo Make it generic

    public DataBaseService(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
    }
    public async Task RefreshDatabaseAsync(List<IDataInitializer> dataInitializers, bool useMigration = true)
    {
        if (useMigration)
        {
            if (DbContext.Database.IsRelational())
            {
                //Applies any pending migrations for the context to the database like (Update-Database)
                var pedingMigraions = await DbContext.Database.GetPendingMigrationsAsync();
                if (pedingMigraions.Any())
                    await DbContext.Database.MigrateAsync();


            }
           
        }
        else
        {
            //Do not use Migrations, just Create Database with latest changes
            await DbContext.Database.EnsureCreatedAsync();
        }

        foreach (var dataInitializer in dataInitializers.OrderBy(x => x.order))
            await dataInitializer.InitializeDataAsync();



    }

}
