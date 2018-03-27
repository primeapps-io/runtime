using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;


namespace PrimeApps.Model.Context
{
    public class PostgreHistoryContext : NpgsqlHistoryRepository
	{
		public PostgreHistoryContext(
		IDatabaseCreator databaseCreator, IRawSqlCommandBuilder rawSqlCommandBuilder,
		NpgsqlRelationalConnection connection, IDbContextOptions options,
        IMigrationsModelDiffer modelDiffer,
        IMigrationsSqlGenerator migrationsSqlGenerator,
		IRelationalAnnotationProvider annotations,
		ISqlGenerationHelper sqlGenerationHelper)
        : base(databaseCreator, rawSqlCommandBuilder, connection, options,
            modelDiffer, migrationsSqlGenerator, annotations, sqlGenerationHelper)
    {
    }

		protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
		{
			base.ConfigureTable(history);
			history.ToTable("_migration_history", "public");
			history.Property(h => h.MigrationId).HasColumnName("migration_id");
			history.Property(h => h.ProductVersion).HasColumnName("product_version");
		}
    }
}
