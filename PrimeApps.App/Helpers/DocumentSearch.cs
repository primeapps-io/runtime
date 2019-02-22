using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.Search;
using System.Configuration;
using Microsoft.Azure.Search.Models;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Enums;
using DataType = Microsoft.Azure.Search.Models.DataType;

namespace PrimeApps.App.Helpers
{
	/// <summary>
	/// Main class for Azure Search (FTS) engine. Contains indexing and searching on Azure Search SaaS (Lucene based)
	/// More information 
	/// </summary>
	public class DocumentSearch
	{
		private SearchServiceClient CreateSearchServiceClient(IConfiguration configuration)
		{
			string searchServiceName = configuration.GetValue("AppSettings:AzureSearch.Storage", string.Empty);
			string adminApiKey = configuration.GetValue("AppSettings:AzureSearch.AdminKey", string.Empty);
			SearchServiceClient serviceClient = null;
			if (!string.IsNullOrEmpty(searchServiceName) && !string.IsNullOrEmpty(adminApiKey))
			{
				serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
			}
			return serviceClient;
		}

		private SearchIndexClient CreateSearchIndexClient(string indexName, IConfiguration configuration)
		{
			string searchServiceName = configuration.GetValue("AppSettings:AzureSearch.Storage", string.Empty);
			string adminApiKey = configuration.GetValue("AppSettings:AzureSearch.AdminKey", string.Empty);
			SearchIndexClient indexClient = null;
			if (!string.IsNullOrEmpty(searchServiceName) && !string.IsNullOrEmpty(adminApiKey))
			{
				indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(adminApiKey));
			}
			return indexClient;
		}
		/// <summary>
		/// Create or Update Azure Search Datasource and index and indexer. Indexer based on datasource and index, with special parameter recordId to merge/separate with/from other records. If index and datasource exists, it runs indexer to index changes on search engine.
		/// </summary>
		/// <param name="instanceId"></param>
		/// <param name="moduleName"></param>
		/// <param name="uniqueRecordId"></param>
		/// <returns></returns>
		public string CreateOrUpdateIndexOnDocumentBlobStorage(string instanceId, string moduleName, IConfiguration configuration, bool renewIndex = false)
		{
			try
			{
				moduleName = moduleName.Replace("_", "-");
				var searchServiceClient = CreateSearchServiceClient(configuration);
				var dataSourceName = "blob-datasource-" + instanceId + "-" + moduleName;


				var dataSource = new DataSource();
				var azureStorageConnectionString = configuration.GetValue("AppSettings:AzureStorage.ConnectionString", string.Empty);
				if (!string.IsNullOrEmpty(azureStorageConnectionString))
				{
					dataSource.Credentials = new DataSourceCredentials(azureStorageConnectionString);
				}
				dataSource.Type = DataSourceType.AzureBlob;
				dataSource.Name = dataSourceName;


				string virtualDirectory = $"{instanceId}/{moduleName}";

				dataSource.Container = new DataContainer("module-documents", virtualDirectory);

				if (!searchServiceClient.DataSources.Exists(dataSourceName))
				{
					searchServiceClient.DataSources.CreateOrUpdate(dataSource);
				}


				var uniqueSearchIndexName = instanceId + "-" + moduleName;


				if (!searchServiceClient.Indexes.Exists(uniqueSearchIndexName) || renewIndex == true)
				{
					if (renewIndex)
					{
						searchServiceClient.Indexes.Delete(uniqueSearchIndexName);
					}

					SearchIndexClient searchIndexClient = CreateSearchIndexClient(uniqueSearchIndexName, configuration);

					List<Field> searchIndexFields = new List<Field>();
					Field fieldKey = new Field();
					fieldKey.Name = "id";
					fieldKey.Type = DataType.String;
					fieldKey.IsKey = true;
					fieldKey.IsSearchable = false;


					Field fieldContent = new Field();
					fieldContent.Name = "content";
					fieldContent.Type = DataType.String;
					fieldContent.IsSearchable = true;
					fieldContent.IsFilterable = true;
					fieldContent.IsSortable = false;


					Field fieldRecordId = new Field();
					fieldRecordId.Name = "recordid";
					fieldRecordId.Type = DataType.Int32;
					fieldRecordId.IsSearchable = false;
					fieldRecordId.IsFilterable = true;
					fieldRecordId.IsSortable = false;

					Field fieldModuleName = new Field();
					fieldModuleName.Name = "module";
					fieldModuleName.Type = DataType.String;
					fieldModuleName.IsSearchable = false;
					fieldModuleName.IsFilterable = true;
					fieldModuleName.IsSortable = true;

					Field fieldFullFileName = new Field();
					fieldFullFileName.Name = "fullfilename";
					fieldFullFileName.Type = DataType.String;
					fieldFullFileName.IsSearchable = false;
					fieldFullFileName.IsFilterable = false;
					fieldFullFileName.IsSortable = false;

					Field fieldViewFileName = new Field();
					fieldViewFileName.Name = "viewfilename";
					fieldViewFileName.Type = DataType.String;
					fieldViewFileName.IsSearchable = false;
					fieldViewFileName.IsFilterable = false;
					fieldViewFileName.IsSortable = false;

					searchIndexFields.Add(fieldKey);
					searchIndexFields.Add(fieldContent);
					searchIndexFields.Add(fieldRecordId);
					searchIndexFields.Add(fieldModuleName);
					searchIndexFields.Add(fieldFullFileName);
					searchIndexFields.Add(fieldViewFileName);


					searchServiceClient.Indexes.CreateOrUpdate(new Index()
					{
						Name = uniqueSearchIndexName,
						Fields = searchIndexFields

					});
				}
				createIndexer:

				if (!searchServiceClient.Indexers.Exists(uniqueSearchIndexName))
				{
					IndexingParameters indexingParameters = new IndexingParameters();
					indexingParameters.IndexFileNameExtensions(new string[]
					{
						".doc",".docx",".pdf",".txt"
					});


					searchServiceClient.Indexers.CreateOrUpdate(new Indexer()
					{
						Name = uniqueSearchIndexName,
						DataSourceName = dataSourceName,
						TargetIndexName = uniqueSearchIndexName,
						Parameters = indexingParameters,
						Schedule = new IndexingSchedule(TimeSpan.FromMinutes(5)) // 5 min auto index incremental change detection policy check which azure provided. May 2017 released!

					});
				}

				if (renewIndex)
				{
					if (!searchServiceClient.Indexers.Exists(uniqueSearchIndexName))
					{
						goto createIndexer;
					}
					else
					{
						searchServiceClient.Indexers.Reset(uniqueSearchIndexName);
						searchServiceClient.Indexers.Run(uniqueSearchIndexName);
					}

				}

				return uniqueSearchIndexName;
			}
			catch (Exception ex)
			{
				return ex.Message;
			}

		}

		public JArray SearchDocuments(List<string> ids, string searchIndexName, ICollection<Filter> filters, IConfiguration configuration)
		{
			searchIndexName = searchIndexName.Replace("_", "-");
			ISearchIndexClient client = CreateSearchIndexClient(searchIndexName, configuration);


			//Basic search for only one criteria search from module document type

			var parameters = new SearchParameters()
			{
				SearchFields = new[] { "content" },
				Select = new[] { "recordid", "module" },
				QueryType = QueryType.Full,
				SearchMode = SearchMode.All
			};

			var searchValue = "";
			var searchQuery = "";

			foreach (var filter in filters)
			{
				if (filter == filters.FirstOrDefault())
				{
					if (filter.Operator == Operator.StartsWith)
					{
						searchValue += filter.Value?.ToString() + "*";
					}
					else if (filter.Operator == Operator.Is)
					{
						searchValue += filter.Value?.ToString();
					}
					else
					{
						searchValue += filter.Value?.ToString();
					}
				}
				else
				{
					if (filter.Operator == Operator.StartsWith)
					{
						searchValue += " || " + filter.Value?.ToString() + "*";
					}
					else if (filter.Operator == Operator.Is)
					{
						searchValue += " || " + filter.Value?.ToString();
					}
					else
					{
						searchValue += " || " + filter.Value?.ToString();
					}
				}

			}
			searchQuery = "content:" + searchValue;

			var documents = client.Documents.Search(searchQuery, parameters);
			var results = new JArray();
			documents.Results.ToList().ForEach(x =>
			{
				var recordId = x.Document["recordid"].ToString().Trim();

				if (!results.Any(r => r.ToString() == recordId))
				{
					results.Add(recordId);
				}

			});
			return results;

		}

		public JObject AdvancedSearchDocuments(string searchIndexName, ICollection<DocumentFilter> filters, int maxResultCount, int startFrom, IConfiguration configuration)
		{
			//check index existence first
			var searchServiceClient = CreateSearchServiceClient(configuration);

			searchIndexName = searchIndexName.Replace("_", "-");

			if (!searchServiceClient.Indexes.Exists(searchIndexName))
			{
				return null;
			}
			else
			{
				ISearchIndexClient client = CreateSearchIndexClient(searchIndexName, configuration);


				var searchParameters = new SearchParameters()
				{
					SearchFields = new[] { "content" },
					Select = new[] { "recordid", "module", "viewfilename", "fullfilename" },
					QueryType = QueryType.Full,
					SearchMode = SearchMode.All,
					IncludeTotalResultCount = true,
					Top = maxResultCount,
					Skip = startFrom,
					HighlightFields = new[] { "content" },
					HighlightPreTag = "<mark>",
					HighlightPostTag = "</mark>",

				};

				string luceneQuery = "content:";
				foreach (var filter in filters)
				{
					if (filter == filters.FirstOrDefault())
					{
						if (filter.QueryOperator == DocumentFilterQueryOperator.StartsWith)
						{
							luceneQuery += filter.SearchText?.ToString() + "* ";
						}
						else if (filter.QueryOperator == DocumentFilterQueryOperator.Equals)
						{
							luceneQuery += "\"" + filter.SearchText?.ToString() + "\"";
						}

						if (filters.Count > 1)
						{
							luceneQuery += " " + filter.Operator.ToString().ToUpper() + " ";
						}
					}
					else
					{
						if (filter.QueryOperator == DocumentFilterQueryOperator.StartsWith)
						{
							luceneQuery += filter.SearchText?.ToString() + "* ";
						}
						else if (filter.QueryOperator == DocumentFilterQueryOperator.Equals)
						{
							luceneQuery += filter.SearchText?.ToString();
						}

						if (filter != filters.LastOrDefault())
						{
							luceneQuery += " " + filter.Operator.ToString().ToUpper() + " ";
						}
					}
				}

				var documents = client.Documents.Search(luceneQuery, searchParameters);

				var responseResult = new JObject();

				var results = new JArray();
				documents.Results.ToList().ForEach(x =>
				{
					var document = new JObject();
					var highlights = new JArray();

					//for purpose, content string removed from result set. for app based search, review here.
					x.Highlights.Select(v => v.Value).ToList().ForEach(c =>
					{
						c.ToList().ForEach(u =>
						{
							highlights.Add(u);
						});

					});



					document["highlights"] = highlights;
					document["meta"] = new JObject();
					x.Document.ToList().ForEach(f =>
					{
						var doc = new JObject();
						document["meta"][f.Key] = f.Value.ToString();
					});

					//no default azure search scoring implemented, (index creation from blob too slow with scoring function - so manual one)
					//count closing post tag for highlight
					int customscore = 0;
					highlights.ToList().ForEach(b =>
					{
						customscore = customscore + b.ToString().Split(new string[] { "</mark>" }, StringSplitOptions.None).Count();
					});

					document["score"] = customscore;//x.Score; for now, not using score unit, instead highlight count
					results.Add(document);

				});

				responseResult["totalCount"] = documents.Count;
				responseResult["data"] = results;

				return responseResult;
			}




		}

	}
}