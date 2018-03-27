 using System.Data.Entity.Migrations;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public sealed class Configuration : DbMigrationsConfiguration<Context.TenantDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Model.Context.DatabaseContext";
            MigrationsDirectory = @"Migrations\TenantDB";
        }

        protected override void Seed(Context.TenantDBContext context)
        {
            //Create custom functions
            context.Database.ExecuteSqlCommand(ModuleHelper.ArrayLowerCaseFunction);
        }
    }
}
