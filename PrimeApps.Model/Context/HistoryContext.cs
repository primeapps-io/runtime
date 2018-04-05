using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace PrimeApps.Model.Context
{
    public class PostgreHistoryContext : NpgsqlHistoryRepository
	{
		public PostgreHistoryContext(
			IRelationalDatabaseCreator databaseCreator,
			IRawSqlCommandBuilder rawSqlCommandBuilder,
			IRelationalConnection connection,
			IDbContextOptions options,
			IMigrationsModelDiffer modelDiffer,
			IMigrationsSqlGenerator migrationsSqlGenerator,
			ISqlGenerationHelper sqlGenerationHelper,
			ICoreConventionSetBuilder coreConventionSetBuilder,
			IEnumerable<IConventionSetBuilder> conventionSetBuilders)
        : base(new HistoryRepositoryDependencies(databaseCreator, rawSqlCommandBuilder, connection, options, modelDiffer, migrationsSqlGenerator, sqlGenerationHelper, coreConventionSetBuilder, conventionSetBuilders))
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
