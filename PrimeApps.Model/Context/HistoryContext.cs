using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;


namespace PrimeApps.Model.Context
{
    public class PostgreHistoryContext : HistoryContext
    {
        public PostgreHistoryContext(DbConnection dbConnection, string defaultSchema) : base(dbConnection, "public") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<HistoryRow>().ToTable(tableName: "_migration_history", schemaName: "public");
            modelBuilder.Entity<HistoryRow>().Property(p => p.MigrationId).HasColumnName("migration_id");
            modelBuilder.Entity<HistoryRow>().Property(p => p.ContextKey).HasColumnName("context_key");
            modelBuilder.Entity<HistoryRow>().Property(p => p.Model).HasColumnName("model");
            modelBuilder.Entity<HistoryRow>().Property(p => p.ProductVersion).HasColumnName("product_version");
        }
    }
}
