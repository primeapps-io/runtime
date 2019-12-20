using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using PrimeApps.Model.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories
{
    public class ReportRepository : RepositoryBaseTenant, IReportRepository
    {
        public ReportRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<int> Count()
        {
            var count = await DbContext.Reports
                .Where(x => !x.Deleted).CountAsync();
            return count;
        }

        public async Task<ICollection<Report>> Find(PaginationModel paginationModel, bool withIncludes = true)
        {
            var reports = DbContext.Reports
                .Where(x => !x.Deleted)
                .Include(x => x.Category)
                .OrderByDescending(x => x.Id)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit);

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Report).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

                if (paginationModel.OrderType == "asc")
                {
                    reports = reports.OrderBy(x => propertyInfo.GetValue(x, null));
                }
                else
                {
                    reports = reports.OrderByDescending(x => propertyInfo.GetValue(x, null));
                }

            }

            return await reports.ToListAsync();
        }

        public async Task<JArray> ChartFilter(FindRequest request, string aggregationField, string currentCulture, string tenantLanguage, string moduleName, IConfiguration configuration, IPicklistRepository picklistRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, int timezoneOffset = 180, bool roleBasedEnabled = true, bool showDisplayValue = true)
        {
            var data = new JArray();
            
            var noneLabel = tenantLanguage == "tr" ? "(Boş)" : "(None)";
            
            var findRequest = new FindRequest
            {
                Fields = new List<string>(),
                SortField = request.SortField,
                SortDirection = request.SortDirection,
                GroupBy = request.GroupBy
            };

            string currencyFilterValue = null;

            findRequest.Fields.Add(aggregationField);
            findRequest.Fields.Add(request.GroupBy);

            if (request.Filters != null && request.Filters.Count > 0)
            {
                findRequest.Filters = new List<Filter>();

                foreach (var reportFilter in request.Filters)
                {
                    findRequest.Filters.Add(new Filter {Field = reportFilter.Field, Operator = reportFilter.Operator, Value = reportFilter.Value, No = reportFilter.No});

                    if (reportFilter.Field == "currency")
                        currencyFilterValue = reportFilter.Value.ToString();
                }
            }

            var records = recordRepository.Find(moduleName, findRequest, true, 180);

            var module = await moduleRepository.GetByName(moduleName);

            var lookupModules = await RecordHelper.GetLookupModules(module, moduleRepository, tenantLanguage: tenantLanguage);

            foreach (var record in records)
            {
                var dataItem = new JObject();
                dataItem["value"] = record.First().First();

                if (aggregationField != request.GroupBy)
                    record[aggregationField] = record.First().First();

                var recordFormatted = await RecordHelper.FormatRecordValues(module, (JObject) record, moduleRepository, picklistRepository, configuration, null, tenantLanguage, currentCulture, timezoneOffset,
                    lookupModules, currencyPicklistValue: currencyFilterValue, userLanguage: tenantLanguage);

                dataItem["label"] = !recordFormatted[request.GroupBy].IsNullOrEmpty() ? recordFormatted[request.GroupBy] : noneLabel;
                dataItem["valueFormatted"] = recordFormatted[aggregationField];

                if (showDisplayValue)
                {
                    dataItem["displayValue"] = recordFormatted[aggregationField];
                    dataItem["tooltext"] = dataItem["label"] + ", " + recordFormatted[aggregationField];
                }

                data.Add(dataItem);
            }

            return data;
        }

        public async Task<JArray> GetDashletReportData(int reportId, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IConfiguration configuration, UserItem appUser, string locale = "", int timezoneOffset = 180, bool roleBasedEnabled = true, bool showDisplayValue = true)
        {
            var data = new JArray();
            var parameters = new List<NpgsqlParameter>();
            var noneLabel = appUser.TenantLanguage == "tr" ? "(Boş)" : "(None)";

            var report = GetReportQuery(true)
                .FirstOrDefault(x => x.Deleted == false && x.Id == reportId);

            if (report == null)
                return data;

            if (report.ReportFeed == Enums.ReportFeed.Custom)
            {
                if (roleBasedEnabled)
                {
                    var sqlRoleBased = RecordHelper.GenerateRoleBasedSql(report.Module.Name, CurrentUser.UserId);
                    var roleBased = DbContext.Database.SqlQueryDynamic(sqlRoleBased).First();
                    var isAdmin = (bool)roleBased["has_admin_rights"];
                    var sharing = (int)roleBased["sharing"];

                    if (!isAdmin && sharing == 1)
                    {
                        var owners = (string)roleBased["owners"];
                        var ownerList = new List<int>();
                        var ownersArray = owners.Split(',');

                        if (!ownersArray.Contains(CurrentUser.UserId.ToString()))
                        {
                            ownerList.Add(CurrentUser.UserId);
                        }

                        foreach (var owner in ownersArray)
                        {
                            if (!string.IsNullOrEmpty(owner))
                                ownerList.Add(int.Parse(owner));
                        }

                        parameters.Add(new NpgsqlParameter { ParameterName = "owners", NpgsqlValue = ownerList, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });
                    }
                    else
                    {
                        roleBasedEnabled = false;
                        parameters.Add(new NpgsqlParameter { ParameterName = "owners", NpgsqlValue = (new List<int> { 0 }).ToArray(), NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });
                    }
                }

                parameters.Add(new NpgsqlParameter { ParameterName = "rolebasedenabled", NpgsqlValue = roleBasedEnabled, NpgsqlDbType = NpgsqlDbType.Boolean });

                if (report.ReportFeed == Enums.ReportFeed.Custom && report.UserId.HasValue)
                    parameters.Add(new NpgsqlParameter { ParameterName = "userid", NpgsqlValue = CurrentUser.UserId, NpgsqlDbType = NpgsqlDbType.Integer });

                data = DbContext.Database.FunctionQueryDynamic(report.SqlFunction, parameters);
            }
            else if (report.ReportFeed == Enums.ReportFeed.Module)
            {
                if (report.ReportType == Enums.ReportType.Single)
                {
                    var findRequest = new FindRequest
                    {
                        Fields = new List<string>()
                    };

                    var aggregation = report.Aggregations.Single();
                    var aggregationType = aggregation.AggregationType.ToString().ToLower();
                    var aggregationField = aggregation.Field != "id" ? aggregation.Field : "created_at";
                    var field = aggregationType + "(" + aggregationField + ")";
                    string currencyFilterValue = null;

                    findRequest.Fields.Add(field);

                    if (report.Filters != null && report.Filters.Count > 0)
                    {
                        findRequest.Filters = new List<Filter>();

                        foreach (var reportFilter in report.Filters)
                        {
                            findRequest.Filters.Add(new Filter { Field = reportFilter.Field, Operator = reportFilter.Operator, Value = reportFilter.Value, No = reportFilter.No });

                            if (reportFilter.Field == "currency")
                                currencyFilterValue = reportFilter.Value;
                        }
                    }

                    var records = recordRepository.Find(report.Module.Name, findRequest, roleBasedEnabled, timezoneOffset);
                    var dataItem = new JObject();

                    var currentCulture = appUser.Culture != null ? appUser.Culture : locale == "en" ? "en-US" : "tr-TR";

                    var lookupModules = await RecordHelper.GetLookupModules(report.Module, moduleRepository, tenantLanguage: appUser.TenantLanguage);

                    if (!records.IsNullOrEmpty() && !records[0].IsNullOrEmpty())
                    {
                        if (aggregationType != "count")
                        {
                            var record = new JObject();
                            record[aggregation.Field] = records[0].First().First();

                            var recordFormatted = await RecordHelper.FormatRecordValues(report.Module, record, moduleRepository, picklistRepository, configuration, appUser.TenantGuid, appUser.TenantLanguage, currentCulture, timezoneOffset, lookupModules, currencyPicklistValue: currencyFilterValue);

                            dataItem["value"] = !recordFormatted[aggregation.Field].IsNullOrEmpty() ? recordFormatted[aggregation.Field] : noneLabel;
                        }
                        else
                        {
                            dataItem["value"] = records[0].First().First();
                        }

                        dataItem["field"] = aggregation.Field;
                        dataItem["type"] = aggregation.AggregationType.ToString().ToLower();
                    }
                    else
                    {
                        dataItem["value"] = 0;
                        dataItem["field"] = aggregation.Field;
                        dataItem["type"] = aggregation.AggregationType.ToString().ToLower();
                    }

                    data.Add(dataItem);
                }
                else
                {
                    var findRequest = new FindRequest
                    {
                        Fields = new List<string>(),
                        SortField = report.SortField,
                        SortDirection = report.SortDirection,
                        GroupBy = report.GroupField
                    };

                    var aggregation = report.Aggregations.Single();
                    var aggregationFieldName = aggregation.Field != "id" ? aggregation.Field : "created_at";
                    var aggregationField = aggregation.AggregationType.ToString().ToLower() + "(" + aggregationFieldName + ")";
                    string currencyFilterValue = null;

                    findRequest.Fields.Add(aggregationField);
                    findRequest.Fields.Add(report.GroupField);

                    if (report.Filters != null && report.Filters.Count > 0)
                    {
                        findRequest.Filters = new List<Filter>();

                        foreach (var reportFilter in report.Filters)
                        {
                            findRequest.Filters.Add(new Filter { Field = reportFilter.Field, Operator = reportFilter.Operator, Value = reportFilter.Value, No = reportFilter.No });

                            if (reportFilter.Field == "currency")
                                currencyFilterValue = reportFilter.Value;
                        }
                    }

                    var records = recordRepository.Find(report.Module.Name, findRequest, roleBasedEnabled, timezoneOffset);

                    var currentCulture = appUser.Culture != null ? appUser.Culture : locale == "en" ? "en-US" : "tr-TR";

                    var lookupModules = await RecordHelper.GetLookupModules(report.Module, moduleRepository, tenantLanguage: appUser.TenantLanguage);

                    foreach (var record in records)
                    {
                        var dataItem = new JObject();
                        dataItem["value"] = record.First().First();

                        if (aggregation.Field != report.GroupField)
                            record[aggregation.Field] = record.First().First();

                        var recordFormatted = await RecordHelper.FormatRecordValues(report.Module, (JObject)record, moduleRepository, picklistRepository, configuration, appUser.TenantGuid, appUser.TenantLanguage, currentCulture, timezoneOffset, lookupModules, currencyPicklistValue: currencyFilterValue, userLanguage: appUser.Language);

                        dataItem["label"] = !recordFormatted[report.GroupField].IsNullOrEmpty() ? recordFormatted[report.GroupField] : noneLabel;
                        dataItem["valueFormatted"] = recordFormatted[aggregation.Field];

                        if (showDisplayValue)
                        {
                            dataItem["displayValue"] = recordFormatted[aggregation.Field];
                            dataItem["tooltext"] = dataItem["label"] + ", " + recordFormatted[aggregation.Field];
                        }

                        data.Add(dataItem);
                    }
                }
            }


            return data;
        }

        public async Task<JArray> GetDashletViewData(int viewId, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IConfiguration configuration, UserItem appUser, string locale = "", int timezoneOffset = 180, bool roleBasedEnabled = true)
        {
            var data = new JArray();

            var view = await DbContext.Views
                .Include(x => x.Fields)
                .Include(x => x.Filters)
                .Include(x => x.Module)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == viewId);

            if (view == null)
                return data;

            var findRequest = new FindRequest
            {
                Fields = new List<string>(),
            };

            findRequest.Fields.Add("total_count()");

            if (view.Filters != null && view.Filters.Count > 0)
            {
                findRequest.Filters = new List<Filter>();

                foreach (var viewFilter in view.Filters)
                {
                    if (viewFilter.Field.Contains("."))
                    {
                        findRequest.Fields.Add(viewFilter.Field);
                    }
                    
                    viewFilter.Value = viewFilter.Value.Replace("[me]", appUser.TenantId.ToString());
                    viewFilter.Value = viewFilter.Value.Replace("[me.email]", appUser.Email);

                    findRequest.Filters.Add(new Filter { Field = viewFilter.Field, Operator = viewFilter.Operator, Value = viewFilter.Value, No = viewFilter.No });
                }
            }

            if (!string.IsNullOrEmpty(view.FilterLogic))
                findRequest.FilterLogic = view.FilterLogic;

            var records = recordRepository.Find(view.Module.Name, findRequest, roleBasedEnabled, timezoneOffset);
            var dataItem = new JObject();

            if (!records.IsNullOrEmpty() && !records[0].IsNullOrEmpty())
            {
                dataItem["value"] = records[0]["total_count"];

            }
            else
            {
                dataItem["value"] = 0;
            }

            dataItem["modulename"] = view.Module.Name;

            data.Add(dataItem);

            return data;
        }

        public ICollection<Report> GetAllBasic()
        {
            var reports = GetReportQuery();

            return reports.ToList();
        }

        public async Task<Report> GetById(int id)
        {
            var report = await DbContext.Reports
                .Include(x => x.Fields)
                .Include(x => x.Filters)
                .Include(x => x.Aggregations)
                .Include(x => x.Shares).ThenInclude(y => y.TenantUser)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return report;
        }

        public async Task<int> Create(Report report)
        {
            DbContext.Reports.Add(report);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Report report, List<int> currentFieldIds, List<int> currentFilterIds, List<int> currentAggregationIds)
        {
            foreach (var fieldId in currentFieldIds)
            {
                var currentField = report.Fields.First(x => x.Id == fieldId);
                report.Fields.Remove(currentField);
                DbContext.ReportFields.Remove(currentField);
            }

            foreach (var filterId in currentFilterIds)
            {
                var currentFilter = report.Filters.First(x => x.Id == filterId);
                report.Filters.Remove(currentFilter);
                DbContext.ReportFilters.Remove(currentFilter);
            }

            foreach (var aggregationId in currentAggregationIds)
            {
                var currentAggregation = report.Aggregations.First(x => x.Id == aggregationId);
                report.Aggregations.Remove(currentAggregation);
                DbContext.ReportAggregations.Remove(currentAggregation);
            }

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(Report report)
        {
            report.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Report report)
        {
            DbContext.Reports.Remove(report);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteReportShare(ReportShares report, TenantUser user)
        {
            user.SharedReports.Remove(report);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<ReportCategory> GetCategoryById(int id)
        {
            var reportCategory = await DbContext.ReportCategories
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return reportCategory;
        }

        public ICollection<ReportCategory> GetCategories(int userId)
        {
            var reportCategories = DbContext.ReportCategories
                .Where(x => !x.Deleted && (!x.UserId.HasValue || x.UserId == userId))
                .OrderBy(x => x.Order);

            return reportCategories.ToList();
        }
        public async Task<ICollection<ReportCategory>> GetAllCategories()
        {
            var reportCategories = DbContext.ReportCategories
                .Where(x => !x.Deleted)
                .OrderBy(x => x.Order);
         
            return await reportCategories.ToListAsync();
        }

        public Task<int> CreateCategory(ReportCategory reportCategory)
        {
            DbContext.ReportCategories.Add(reportCategory);

            return DbContext.SaveChangesAsync();
        }

        public Task<int> UpdateCategory(ReportCategory reportCategory)
        {
            return DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoftCategory(ReportCategory reportCategory)
        {
            reportCategory.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHardCategory(ReportCategory reportCategory)
        {
            DbContext.ReportCategories.Remove(reportCategory);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<Chart> GetChartByReportId(int reportId)
        {
            var chart = await DbContext.Charts
                .Include(x => x.Report)
                .Include(x => x.Report.Aggregations)
                .SingleOrDefaultAsync(x => !x.Deleted && x.ReportId == reportId);

            return chart;
        }

        public Task<int> CreateChart(Chart chart)
        {
            DbContext.Charts.Add(chart);

            return DbContext.SaveChangesAsync();
        }

        public Task<int> UpdateChart(Chart chart)
        {
            return DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoftChart(Chart chart)
        {
            chart.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHardChart(Chart chart)
        {
            DbContext.Charts.Remove(chart);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<Widget> GetWidgetByReportId(int reportId)
        {
            var widget = await DbContext.Widgets
                .SingleOrDefaultAsync(x => !x.Deleted && x.ReportId == reportId);

            return widget;
        }

        public Task<int> CreateWidget(Widget widget)
        {
            DbContext.Widgets.Add(widget);

            return DbContext.SaveChangesAsync();
        }

        public Task<int> UpdateWidget(Widget widget)
        {
            return DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoftWidget(Widget widget)
        {
            widget.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHardWidget(Widget widget)
        {
            DbContext.Widgets.Remove(widget);

            return await DbContext.SaveChangesAsync();
        }

        private IQueryable<Report> GetReportQuery(bool includeModule = false)
        {
            IQueryable<Report> reports = DbContext.Reports
                .Include(x => x.Fields)
                .Include(x => x.Filters)
                .Include(x => x.Aggregations);

            if (includeModule)
            {
                reports = reports
                    .Include(x => x.Module)
                    .Include(x => x.Module.Fields);
            }

            reports = reports
                .Include(x => x.Shares)
                .ThenInclude(x => x.TenantUser)
                .Where(x => !x.Deleted)
                      .Where(x => x.SharingType == ReportSharingType.Everybody
                || x.CreatedBy.Id == CurrentUser.UserId
                || x.Shares.Any(j => j.UserId == CurrentUser.UserId));

            return reports;
        }
    }
}
