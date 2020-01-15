using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace PrimeApps.Model.Context
{
    public class HistoryRepository : NpgsqlHistoryRepository
	{
		public HistoryRepository(HistoryRepositoryDependencies dependencies): base(dependencies)
		{
		}

		protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
		{
			base.ConfigureTable(history);
			
			history.Property(h => h.MigrationId).HasColumnName("migration_id");
			history.Property(h => h.ProductVersion).HasColumnName("product_version");
		}
    }
}
