using System;
using System.Collections.Generic;
using System.Linq;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Helpers
{
    public static class ModuleHelper
    {
        private const string TableCreateTemplate = "CREATE TABLE \"{0}_d\" (\n{1}\n);";
        private const string TableAlterTemplate = "ALTER TABLE \"{0}_d\"\n{1};";
        private const string ColumnTemplate = "\"{0}\" {1} {2}";
        private const string ColumnAddTemplate = "ADD COLUMN \"{0}\" {1} {2}";
        private const string ColumnAlterTemplate = "ALTER COLUMN \"{0}\" TYPE {1} {2}";
        private const string ColumnDropTemplate = "DROP COLUMN \"{0}\"";
        private const string ForeignKeyTemplate = "REFERENCES {0}";
        private const string IndexCreateTemplate = "CREATE INDEX {0} ON \"{1}_d\" (\"{2}\") WHERE (\"deleted\" IS NOT TRUE);";
        private const string IndexCreateArrayTemplate = "CREATE INDEX {0} ON \"{1}_d\" USING GIN (\"{2}\") WHERE (\"deleted\" IS NOT TRUE);";
        private const string IndexCreateLowerTemplate = "CREATE INDEX {0} ON \"{1}_d\" (LOWER(\"{2}\")) WHERE (\"deleted\" IS NOT TRUE);";
        private const string IndexCreateArrayLowerTemplate = "CREATE INDEX {0} ON \"{1}_d\" USING GIN (ARRAY_LOWERCASE(\"{2}\")) WHERE (\"deleted\" IS NOT TRUE);";
        private const string IndexUniqueCreateTemplate = "CREATE UNIQUE INDEX \"{0}\" ON {1}_d ({2}) WHERE (\"deleted\" IS NOT TRUE);";
        private const string IndexCreateNotDeletedTemplate = "CREATE INDEX {0} ON \"{1}_d\" (\"{2}\");";
        private const string IndexDropTemplate = "DROP INDEX \"{0}\"";
        private const string TableDropTemplate = "DROP TABLE IF EXISTS \"{0}_d\" CASCADE;";
        private const string ViewCreateTemplate = "CREATE VIEW \"{0}\" AS\n{1};";
        private const string ViewDropTemplate = "DROP VIEW IF EXISTS \"{0}\";";
        private const string ViewTemplate = "\tSELECT\n{0}\n\tFROM\n\t\t\"{1}_d\"\n\tWHERE\n\t\t\"deleted\" IS NOT TRUE;\n\n";
        private const string ColumnViewTemplate = "\t\t\"{0}\" AS \"{1}\"";
        public const string ArrayLowerCaseFunction = "CREATE OR REPLACE FUNCTION array_lowercase(VARCHAR[]) RETURNS VARCHAR[] AS\n$BODY$\n\tSELECT ARRAY_AGG(arr.item) FROM (\n\tSELECT btrim(LOWER(UNNEST($1)))::VARCHAR AS item\n\t) AS arr;\n$BODY$\nLANGUAGE SQL IMMUTABLE;";

        public static List<string> SystemFields => new List<string>
        {
            "id",
            "deleted",
            "shared_users",
            "shared_user_groups",
            "shared_users_edit",
            "shared_user_groups_edit",
            "is_sample",
            "is_converted",
            "master_id",
            "migration_id",
            "import_id"
        };

        public static List<string> StandardFields => new List<string>
        {
            "created_by",
            "updated_by",
            "created_at",
            "updated_at"
        };

        public static List<string> SystemFieldsExtended => SystemFields.Concat(StandardFields).ToList();

        public static List<DataType> LowerDataTypes => new List<DataType>
        {
            DataType.TextSingle,
            DataType.TextMulti,
            DataType.Email,
            DataType.Picklist,
            DataType.Multiselect,
            DataType.Tag
        };

        public static List<string> ModuleSpecificFields(Module module)
        {
            var fields = new List<string>();

            switch (module.Name)
            {
                case "quotes":
                case "sales_orders":
                case "purchase_orders":
                    var hasCurrencyField = module.Fields.Any(x => x.Name == "currency");

                    if (hasCurrencyField)
                    {
                        fields.AddRange(new List<string>
                        {
                            "exchange_rate_try_usd",
                            "exchange_rate_try_eur",
                            "exchange_rate_usd_try",
                            "exchange_rate_usd_eur",
                            "exchange_rate_eur_try",
                            "exchange_rate_eur_usd"
                        });
                    }
                    break;
            }

            return fields;
        }

        public static string GenerateTableCreateSql(Module module, string language)
        {
            var columns = new List<string>();
            var columnsView = new List<string>();
            var labelsView = new List<string>();

            columns.Add(string.Format(ColumnTemplate, "id", "SERIAL PRIMARY KEY", "NOT NULL"));
            columnsView.Add(string.Format(ColumnViewTemplate, "id", "Id"));
            labelsView.Add("Id");

            foreach (var field in module.Fields)
            {
                if (StandardFields.Contains(field.Name))
                    AddViewColumn(columnsView, labelsView, field, language);

                if (SystemFieldsExtended.Contains(field.Name))
                    continue;

                List<string> options;
                var dataType = GetCloumnDataTypeAndOptions(field, out options);

                columns.Add(string.Format(ColumnTemplate, field.Name, dataType, string.Join(" ", options)));
                AddViewColumn(columnsView, labelsView, field, language);
            }

            if (module.Name == "opportunities")
            {
                columns.Add(string.Format(ColumnTemplate, "forecast_type", "VARCHAR (100)", ""));
                columns.Add(string.Format(ColumnTemplate, "forecast_category", "VARCHAR (100)", ""));
                columns.Add(string.Format(ColumnTemplate, "forecast_year", "NUMERIC", ""));
                columns.Add(string.Format(ColumnTemplate, "forecast_month", "NUMERIC", ""));
                columns.Add(string.Format(ColumnTemplate, "forecast_quarter", "NUMERIC", ""));
            }

            if (module.Name == "activities")
            {
                columns.Add(string.Format(ColumnTemplate, "activity_type_system", "VARCHAR (10)", ""));
            }

            if (module.Name == "current_accounts")
            {
                columns.Add(string.Format(ColumnTemplate, "transaction_type_system", "VARCHAR (30)", ""));
            }

            columns.Add(string.Format(ColumnTemplate, "shared_users", "INTEGER[]", ""));
            columns.Add(string.Format(ColumnTemplate, "shared_user_groups", "INTEGER[]", ""));
            columns.Add(string.Format(ColumnTemplate, "shared_users_edit", "INTEGER[]", ""));
            columns.Add(string.Format(ColumnTemplate, "shared_user_groups_edit", "INTEGER[]", ""));
            columns.Add(string.Format(ColumnTemplate, "is_sample", "BOOLEAN", "NOT NULL DEFAULT FALSE"));
            columns.Add(string.Format(ColumnTemplate, "is_converted", "BOOLEAN", "NOT NULL DEFAULT FALSE"));
            columns.Add(string.Format(ColumnTemplate, "master_id", "INTEGER", ""));
            columns.Add(string.Format(ColumnTemplate, "migration_id", "VARCHAR (100)", ""));
            columns.Add(string.Format(ColumnTemplate, "import_id", "INTEGER", ""));
            columns.Add(string.Format(ColumnTemplate, "created_by", "INTEGER", "NOT NULL REFERENCES users"));
            columns.Add(string.Format(ColumnTemplate, "updated_by", "INTEGER", "REFERENCES users"));
            columns.Add(string.Format(ColumnTemplate, "created_at", "TIMESTAMP", "NOT NULL"));
            columns.Add(string.Format(ColumnTemplate, "updated_at", "TIMESTAMP", ""));
            columns.Add(string.Format(ColumnTemplate, "deleted", "BOOLEAN", "NOT NULL DEFAULT FALSE"));

            var junctionTablesSql = "";

            if (module.Relations != null && module.Relations.Count > 0)
            {
                var junctionTableNames = new List<string>();

                foreach (var relation in module.Relations)
                {
                    if (relation.RelationType == RelationType.ManyToMany)
                    {
                        var relationFieldName1 = module.Name;
                        var relationFieldName2 = relation.RelatedModule;

                        if (relationFieldName1 == relationFieldName2)
                        {
                            relationFieldName1 = relationFieldName1 + "1";
                            relationFieldName2 = relationFieldName2 + "2";
                        }

                        var relationColumns = new List<string>();
                        relationColumns.Add(string.Format(ColumnTemplate, relationFieldName1 + "_id", "INTEGER", string.Format(ForeignKeyTemplate, module.Name + "_d")));
                        relationColumns.Add(string.Format(ColumnTemplate, relationFieldName2 + "_id", "INTEGER", string.Format(ForeignKeyTemplate, relation.RelatedModule + "_d")));

                        var junctionTableName = module.Name + "_" + relation.RelatedModule;

                        if (junctionTableNames.Contains(junctionTableName))
                            junctionTableName = junctionTableName + "_" + relation.Id;
                        else
                            junctionTableNames.Add(junctionTableName);

                        junctionTablesSql += string.Format(TableCreateTemplate, junctionTableName, string.Join(",\n", relationColumns));
                    }
                }
            }

            var tableCreateSql = string.Format(TableCreateTemplate, module.Name, string.Join(",\n", columns));
            var viewSql = string.Format(ViewTemplate, string.Join(",\n", columnsView), module.Name);
            var viewCreateSql = string.Format(ViewCreateTemplate, module.Name + "_v", viewSql);
            var sql = string.Join("\n\n", tableCreateSql, viewCreateSql, junctionTablesSql);

            return sql;
        }

        public static string GenerateTableAlterSql(Module module, ModuleChanges moduleChanges, string language)
        {
            var columns = new List<string>();
            var columnsView = new List<string>();
            var labelsView = new List<string>();
            var indexesCreate = new List<string>();
            var indexesDrop = new List<string>();
            var junctionTableNames = new List<string>();
            var tableAlterSql = string.Empty;
            var junctionTablesSql = string.Empty;
            var viewDropCreateSql = string.Empty;
            var viewCreateSql = string.Empty;

            if (moduleChanges.FieldsAdded.Count > 0)
            {
                foreach (var fieldAdded in moduleChanges.FieldsAdded)
                {
                    List<string> options;
                    var dataType = GetCloumnDataTypeAndOptions(fieldAdded, out options);

                    columns.Add(string.Format(ColumnAddTemplate, fieldAdded.Name, dataType, string.Join(" ", options)));
                }

                indexesCreate = GenerateIndexesCreateSql(moduleChanges.FieldsAdded, module.Name, module.Fields);
            }

            if (moduleChanges.RelationsAdded.Count > 0)
            {
                foreach (var relationAdded in moduleChanges.RelationsAdded)
                {
                    if (relationAdded.RelationType == RelationType.ManyToMany)
                    {
                        var relationFieldName1 = module.Name;
                        var relationFieldName2 = relationAdded.RelatedModule;

                        if (relationFieldName1 == relationFieldName2)
                        {
                            relationFieldName1 = relationFieldName1 + "1";
                            relationFieldName2 = relationFieldName2 + "2";
                        }

                        var relationColumns = new List<string>();
                        relationColumns.Add(string.Format(ColumnTemplate, relationFieldName1 + "_id", "INTEGER", string.Format(ForeignKeyTemplate, module.Name + "_d")));
                        relationColumns.Add(string.Format(ColumnTemplate, relationFieldName2 + "_id", "INTEGER", string.Format(ForeignKeyTemplate, relationAdded.RelatedModule + "_d")));

                        var junctionTableName = module.Name + "_" + relationAdded.RelatedModule;

                        if (junctionTableNames.Contains(junctionTableName))
                            junctionTableName = junctionTableName + "_" + relationAdded.Id;
                        else
                            junctionTableNames.Add(junctionTableName);

                        junctionTablesSql += string.Format(TableCreateTemplate, junctionTableName, string.Join(",\n", relationColumns));
                    }
                }
            }

            if (moduleChanges.ValidationsChanged.Count > 0)
            {
                foreach (var validationChanged in moduleChanges.ValidationsChanged)
                {
                    var field = module.Fields.Single(x => x.Name == validationChanged.Key);

                    if (validationChanged.Value == "maxlength")
                    {
                        List<string> options;
                        var dataType = GetCloumnDataTypeAndOptions(field, out options);
                        columns.Add(string.Format(ColumnAlterTemplate, field.Name, dataType, string.Join(" ", options)));
                    }

                    if (validationChanged.Value == "unique" && field.Validation.Unique.HasValue && !field.Validation.Unique.Value)
                    {
                        var indexFieldName = field.Name;

                        if (!string.IsNullOrWhiteSpace(field.UniqueCombine))
                            indexFieldName += "-" + field.UniqueCombine;

                        indexesDrop.Add(string.Format(IndexDropTemplate, module.Name + "_ix_unique_" + indexFieldName));
                    }
                }
            }

            if (columns.Count > 0)
            {
                tableAlterSql = string.Format(TableAlterTemplate, module.Name, string.Join(",\n", columns));

                foreach (var field in module.Fields)
                {
                    if (StandardFields.Contains(field.Name))
                        AddViewColumn(columnsView, labelsView, field, language);

                    if (SystemFieldsExtended.Contains(field.Name))
                        continue;

                    AddViewColumn(columnsView, labelsView, field, language);
                }

                viewDropCreateSql = string.Format(ViewDropTemplate, module.Name + "_v");
                var viewSql = string.Format(ViewTemplate, string.Join(",\n", columnsView), module.Name);
                viewCreateSql = string.Format(ViewCreateTemplate, module.Name + "_v", viewSql);
            }

            var indexesCreateSql = string.Join("\n", indexesCreate);
            var indexesDropSql = string.Join("\n", indexesDrop);
            var sql = string.Empty;

            if (!(string.IsNullOrEmpty(viewDropCreateSql) && string.IsNullOrEmpty(tableAlterSql) && string.IsNullOrEmpty(junctionTablesSql)
                && string.IsNullOrEmpty(viewCreateSql) && string.IsNullOrEmpty(indexesCreateSql) && string.IsNullOrEmpty(indexesDropSql)))
                sql = string.Join("\n\n", viewDropCreateSql, tableAlterSql, junctionTablesSql, viewCreateSql, indexesCreateSql, indexesDropSql);

            return sql;
        }

        public static string GenerateJunctionTableCreateSql(string moduleName, string relatedModuleName, ICollection<Relation> currentRelations)
        {
            var relationExists = currentRelations.Any(x => x.RelatedModule == relatedModuleName && x.RelationType == RelationType.ManyToMany);
            var relationFieldName1 = moduleName;
            var relationFieldName2 = relatedModuleName;

            if (relationFieldName1 == relationFieldName2)
            {
                relationFieldName1 = relationFieldName1 + "1";
                relationFieldName2 = relationFieldName2 + "2";
            }

            var relationColumns = new List<string>();
            relationColumns.Add(string.Format(ColumnTemplate, relationFieldName1 + "_id", "INTEGER", string.Format(ForeignKeyTemplate, moduleName + "_d")));
            relationColumns.Add(string.Format(ColumnTemplate, relationFieldName2 + "_id", "INTEGER", string.Format(ForeignKeyTemplate, relatedModuleName + "_d")));

            var junctionTableName = moduleName + "_" + relatedModuleName;

            if (relationExists)
            {
                var topRelationId = currentRelations.OrderByDescending(x => x.Id).Take(1).Single().Id;
                junctionTableName = junctionTableName + "_" + (topRelationId + 1);
            }

            var indexes = new List<string>();
            indexes.Add(string.Format(IndexCreateNotDeletedTemplate, junctionTableName + "_ix_" + relationFieldName1, junctionTableName, relationFieldName1 + "_id"));
            indexes.Add(string.Format(IndexCreateNotDeletedTemplate, junctionTableName + "_ix_" + relationFieldName2, junctionTableName, relationFieldName2 + "_id"));

            var junctionTableSql = string.Format(TableCreateTemplate, junctionTableName, string.Join(",\n", relationColumns));
            var indexesCreateSql = string.Join("\n", indexes);
            var sql = string.Join("\n\n", junctionTableSql, indexesCreateSql);

            return sql;
        }

        public static string GenerateIndexesCreateSql(Module module)
        {
            var indexes = GenerateIndexesCreateSql(module.Fields, module.Name, module.Fields);
            indexes.Add(string.Format(IndexCreateArrayTemplate, module.Name + "_ix_shared_users", module.Name, "shared_users"));
            indexes.Add(string.Format(IndexCreateArrayTemplate, module.Name + "_ix_shared_user_groups", module.Name, "shared_user_groups"));
            indexes.Add(string.Format(IndexCreateArrayTemplate, module.Name + "_ix_shared_users_edit", module.Name, "shared_users_edit"));
            indexes.Add(string.Format(IndexCreateArrayTemplate, module.Name + "_ix_shared_user_groups_edit", module.Name, "shared_user_groups_edit"));
            indexes.Add(string.Format(IndexCreateTemplate, module.Name + "_ix_is_sample", module.Name, "is_sample"));
            indexes.Add(string.Format(IndexCreateTemplate, module.Name + "_ix_master_id", module.Name, "master_id"));
            indexes.Add(string.Format(IndexCreateTemplate, module.Name + "_ix_migration_id", module.Name, "migration_id"));
            indexes.Add(string.Format(IndexCreateTemplate, module.Name + "_ix_import_id", module.Name, "import_id"));
            indexes.Add(string.Format(IndexCreateTemplate, module.Name + "_ix_created_by", module.Name, "created_by"));
            indexes.Add(string.Format(IndexCreateTemplate, module.Name + "_ix_updated_by", module.Name, "updated_by"));
            indexes.Add(string.Format(IndexCreateTemplate, module.Name + "_ix_created_at", module.Name, "created_at"));
            indexes.Add(string.Format(IndexCreateTemplate, module.Name + "_ix_updated_at", module.Name, "updated_at"));
            indexes.Add(string.Format(IndexCreateTemplate, module.Name + "_ix_deleted", module.Name, "deleted"));
            var indexesCreateSql = string.Join("\n", indexes);

            return indexesCreateSql;
        }

        public static string GenerateTableDropSql(Module module)
        {
            var tableDropSql = string.Format(TableDropTemplate, module.Name);

            return tableDropSql;
        }

        public static Module GetFakeUserModule()
        {
            var userModule = new Module();
            userModule.Name = "users";
            userModule.Fields = new List<Field>();
            userModule.Fields.Add(new Field { DataType = DataType.TextSingle, Name = "email" });
            userModule.Fields.Add(new Field { DataType = DataType.TextSingle, Name = "first_name" });
            userModule.Fields.Add(new Field { DataType = DataType.TextSingle, Name = "last_name" });
            userModule.Fields.Add(new Field { DataType = DataType.TextSingle, Name = "full_name", Primary = true });
            userModule.Fields.Add(new Field { DataType = DataType.Checkbox, Name = "is_active" });
            userModule.Fields.Add(new Field { DataType = DataType.Checkbox, Name = "is_subscriber" });

            return userModule;
        }

        private static string GetCloumnDataTypeAndOptions(Field field, out List<string> options)
        {
            var dataType = GetDataType(field);
            options = new List<string>();

            if (field.Combination == null)
            {
                if (field.Validation != null)
                {
                    if (field.Validation.MaxLength > 0 && field.DataType == DataType.TextSingle)
                        dataType = string.Format(dataType, field.Validation.MaxLength);
                    else
                        dataType = string.Format(dataType, 400);
                }
                else
                {
                    if (field.DataType == DataType.TextSingle)
                        dataType = string.Format(dataType, 400);
                }
            }
            else
            {
                dataType = string.Format(dataType, 850);
            }

            if (field.DataType == DataType.Checkbox)
                options.Add("NOT NULL DEFAULT FALSE");

            if (field.DataType == DataType.Lookup && field.LookupType != "relation")
                options.Add(string.Format(ForeignKeyTemplate, field.LookupType != "users" ? field.LookupType + "_d" : "users"));

            return dataType;
        }

        private static string GetDataType(Field field)
        {
            switch (field.DataType)
            {
                case DataType.TextSingle:
                    return "VARCHAR ({0})";
                case DataType.TextMulti:
                    return field.MultilineType == MultilineType.Large ? "TEXT" : "VARCHAR (2000)";
                case DataType.Number:
                    return "NUMERIC";
                case DataType.NumberAuto:
                    return "SERIAL";
                case DataType.NumberDecimal:
                    return "DECIMAL";
                case DataType.Currency:
                    return "MONEY";
                case DataType.Date:
                    return "DATE";
                case DataType.DateTime:
                case DataType.Time:
                    return "TIMESTAMP";
                case DataType.Email:
                    return "VARCHAR (100)";
                case DataType.Picklist:
                    return "VARCHAR (100)";
                case DataType.Multiselect:
                    return "VARCHAR (100) ARRAY";
                case DataType.Tag:
                    return "VARCHAR (400) ARRAY";
                case DataType.Lookup:
                    return "INTEGER";
                case DataType.Checkbox:
                    return "BOOLEAN";
                default:
                    return "VARCHAR(400)";
            }
        }

        private static List<string> GenerateIndexesCreateSql(IEnumerable<Field> fields, string moduleName, IEnumerable<Field> moduleFields)
        {
            var indexes = new List<string>();

            foreach (var field in fields)
            {
                if (SystemFieldsExtended.Contains(field.Name))
                    continue;

                if (!(field.DataType == DataType.TextMulti && field.MultilineType == MultilineType.Large))
                {
                    if (!LowerDataTypes.Contains(field.DataType))
                    {
                        indexes.Add(string.Format(IndexCreateTemplate, moduleName + "_ix_" + field.Name, moduleName, field.Name));
                    }
                    else
                    {
                        if (field.DataType == DataType.Multiselect || field.DataType == DataType.Tag)
                            indexes.Add(string.Format(IndexCreateArrayLowerTemplate, moduleName + "_ix_" + field.Name, moduleName, field.Name));
                        else
                            indexes.Add(string.Format(IndexCreateLowerTemplate, moduleName + "_ix_" + field.Name, moduleName, field.Name));
                    }
                }

                if (field.Validation != null && field.Validation.Unique.HasValue && field.Validation.Unique.Value)
                {
                    var indexColumns = $"\"{field.Name}\"";

                    if (LowerDataTypes.Contains(field.DataType))
                        indexColumns = $"LOWER(\"{field.Name}\")";


                    if (!string.IsNullOrWhiteSpace(field.UniqueCombine))
                    {
                        var uniqueCombinationField = moduleFields.Single(x => x.Name == field.UniqueCombine);

                        if (!LowerDataTypes.Contains(uniqueCombinationField.DataType))
                            indexColumns += $", \"{uniqueCombinationField.Name}\"";
                        else
                            indexColumns += $", LOWER(\"{uniqueCombinationField.Name}\")";
                    }

                    var indexFieldName = field.Name;

                    if (!string.IsNullOrWhiteSpace(field.UniqueCombine))
                        indexFieldName += "-" + field.UniqueCombine;

                    indexes.Add(string.Format(IndexUniqueCreateTemplate, moduleName + "_ix_unique_" + indexFieldName, moduleName, indexColumns));
                }
            }

            return indexes;
        }

        private static void AddViewColumn(List<string> columnsView, List<string> labelsView, Field field, string language)
        {
            var label = language == "tr" ? field.LabelTr : field.LabelEn;
            label = label.Replace("\"", "");

            if (labelsView.Contains(label))
                label += " " + field.Name;

            labelsView.Add(label);
            var columnViewSql = string.Format(ColumnViewTemplate, field.Name, label);
            columnsView.Add(columnViewSql);
        }
    }
}
