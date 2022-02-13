using Lookif.Layers.Core.Infrastructure.Base;
using Lookif.Layers.Core.Infrastructure.Base.DataInitializer; 
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lookif.Layers.Data.Repositories
{
    public class DataBaseService : IDataBaseService  , ISingletonDependency
    {
        protected readonly ApplicationDbContext  DbContext; //ToDo Make it generic

        public DataBaseService(ApplicationDbContext  dbContext)
        {
            DbContext = dbContext;
        }
        public void RefreshDatabase(List<IDataInitializer> dataInitializers, bool Do_not_use_Migrations = false)
        {
            if (Do_not_use_Migrations)
            {
                //Do not use Migrations, just Create Database with latest changes
                DbContext.Database.EnsureCreated();
            }
            else
            {
                //Applies any pending migrations for the context to the database like (Update-Database)
                DbContext.Database.Migrate();
            }

            foreach (var dataInitializer in dataInitializers.OrderBy(x => x.order))
                dataInitializer.InitializeData();

            

        }

    }
}
