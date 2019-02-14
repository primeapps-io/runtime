using System;
using Aspose.Words.MailMerging;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Net;
using Aspose.Words;
using Aspose.Words.Tables;
using Microsoft.Extensions.Configuration;
using SkiaSharp;

namespace PrimeApps.App.Helpers
{
    public class MailMergeDataSource : IMailMergeDataSource
    {
        private int row { get; set; }

        private Dictionary<string, JArray> relatedModules { get; set; }
        private JObject record { get; set; }
        private string tableName { get; set; }
        private bool isChild { get; set; }
        private Module module { get; set; }

        private ICollection<Note> notes { get; set; }

        public MailMergeDataSource(JObject record, string tableName, Module module, Dictionary<string, JArray> relatedModules, bool isChild = false, ICollection<Note> notes = null)
        {
            this.record = record;
            this.tableName = tableName;
            this.isChild = isChild;
            this.relatedModules = relatedModules;
            this.module = module;
            this.notes = notes;
        }

        public string TableName
        {
            get { return tableName; }
        }

        public IMailMergeDataSource GetChildDataSource(string tableName)
        {
            switch (tableName)
            {
                case "notes":
                    return new MailMergeNoteDataSource(this.notes, tableName, false);
                default:
                    if (relatedModules.Keys.Contains(tableName))
                    {
                        return new MailMergeRelationalModuleDataSource(relatedModules[tableName], tableName);
                    }

                    foreach (var property in record)
                    {
                        if (property.Key.Contains($"{tableName}."))
                        {
                            return new MailMergeDataSource(this.record, tableName, module, relatedModules, true);
                        }
                    }
                    return null;
            }
        }

        public bool GetValue(string fieldName, out object fieldValue)
        {
            if (fieldName.Contains("html__"))
                fieldName = fieldName.Remove(0, 6);
            else if (fieldName.Contains("img__"))
                fieldName = fieldName.Remove(0, 5);

            fieldValue = isChild ? record[tableName + "." + fieldName] : record[fieldName];

            return fieldValue != null;
        }

        public bool MoveNext()
        {
            row++;
            return isChild || row < 2;
        }
    }

    public class MailMergeRelationalModuleDataSource : IMailMergeDataSource
    {
        private int row { get; set; }
        private JArray records { get; set; }
        private string tableName { get; set; }
        private bool isChild { get; set; }

        public MailMergeRelationalModuleDataSource(JArray records, string tableName, bool isChild = false, int currentRow = -1)
        {
            this.tableName = tableName;
            this.records = records;
            this.isChild = isChild;

            if (currentRow > -1)
                row = currentRow;
        }

        public string TableName
        {
            get { return tableName; }
        }

        public IMailMergeDataSource GetChildDataSource(string tableName)
        {
            if (tableName.Contains("_records"))
                return new MailMergeRelationalSecondLevelModuleDataSource((JArray)this.records[row - 1][tableName], tableName);

            return new MailMergeRelationalModuleDataSource(this.records, tableName, true, row - 1);
        }

        public bool GetValue(string fieldName, out object fieldValue)
        {
            if (fieldName.Contains("html__"))
                fieldName = fieldName.Remove(0, 6);
            else if (fieldName.Contains("img__"))
                fieldName = fieldName.Remove(0, 5);

            if (isChild)
            {
                string[] propertyParts;
                fieldValue = null;

                foreach (JProperty property in records[row - 1])
                {
                    propertyParts = property.Name.Split('.');

                    if (propertyParts.Length < 3)
                        continue;

                    if (propertyParts[0] == tableName && propertyParts[2] == fieldName)
                    {
                        fieldValue = property.Value;
                        break;
                    }
                }
            }
            else
            {
                fieldValue = records[row - 1][fieldName];

                if (fieldName == "discount_percent" && !string.IsNullOrEmpty(fieldValue.ToString()))
                {
                    fieldValue = "%" + records[row - 1]["discount_percent"];
                }
            }

            return fieldValue != null;
        }

        public bool MoveNext()
        {
            row++;
            return row <= records.Count;
        }
    }

    public class MailMergeRelationalSecondLevelModuleDataSource : IMailMergeDataSource
    {
        private int row { get; set; }
        private JArray records { get; set; }
        private string tableName { get; set; }
        private bool isChild { get; set; }

        public MailMergeRelationalSecondLevelModuleDataSource(JArray records, string tableName, bool isChild = false, int currentRow = -1)
        {
            this.tableName = tableName;
            this.records = records;
            this.isChild = isChild;

            if (currentRow > -1)
                row = currentRow;
        }

        public string TableName
        {
            get { return tableName; }
        }

        public IMailMergeDataSource GetChildDataSource(string tableName)
        {
            return new MailMergeRelationalModuleDataSource(this.records, tableName, true, row - 1);
        }

        public bool GetValue(string fieldName, out object fieldValue)
        {
            if (fieldName.Contains("html__"))
                fieldName = fieldName.Remove(0, 6);
            else if (fieldName.Contains("img__"))
                fieldName = fieldName.Remove(0, 5);

            if (isChild)
            {
                string[] propertyParts;
                fieldValue = null;

                foreach (JProperty property in records[row - 1])
                {
                    propertyParts = property.Name.Split('.');

                    if (propertyParts.Length < 3)
                        continue;

                    if (propertyParts[0] == tableName && propertyParts[2] == fieldName)
                    {
                        fieldValue = property.Value;
                        break;
                    }
                }
            }
            else
            {
                fieldValue = records[row - 1][fieldName];
            }

            return fieldValue != null ? true : false;
        }

        public bool MoveNext()
        {
            row++;
            return row <= records.Count;
        }
    }

    public class MailMergeNoteDataSource : IMailMergeDataSource
    {
        private int row { get; set; }
        private string tableName { get; set; }
        private bool isChild { get; set; }
        private Note[] notes { get; set; }

        public MailMergeNoteDataSource(ICollection<Note> notes, string tableName, bool isChild = false)
        {
            this.notes = notes.ToArray();
            this.isChild = isChild;
        }

        public string TableName
        {
            get { return tableName; }
        }

        public IMailMergeDataSource GetChildDataSource(string tableName)
        {
            return null;
        }

        public bool GetValue(string fieldName, out object fieldValue)
        {
            switch (fieldName)
            {
                case "text":
                    fieldValue = notes[row - 1].Text;
                    break;
                case "first_name":
                    fieldValue = notes[row - 1].CreatedBy.FirstName;
                    break;
                case "last_name":
                    fieldValue = notes[row - 1].CreatedBy.LastName;
                    break;
                case "full_name":
                    fieldValue = notes[row - 1].CreatedBy.FullName;
                    break;
                case "email":
                    fieldValue = notes[row - 1].CreatedBy.Email;
                    break;
                case "created_at":
                    fieldValue = notes[row - 1].CreatedAt.ToString();
                    break;
                default:
                    fieldValue = null;
                    break;
            }

            return fieldValue != null;
        }

        public bool MoveNext()
        {
            row++;
            return row <= this.notes.Count();
        }
    }

    public class FieldMergingCallback : IFieldMergingCallback
    {
        private Guid instanceId { get; set; }
        private DocumentBuilder _documentBuilder;
        private IConfiguration _configuration;

        public FieldMergingCallback(Guid instanceId, IConfiguration configuration)
        {
            this.instanceId = instanceId;
            _configuration = configuration;
        }

        public void FieldMerging(FieldMergingArgs args)
        {
            if (_documentBuilder == null)
                _documentBuilder = new DocumentBuilder(args.Document);

            var row = (Row)args.Field.Start.GetAncestor(NodeType.Row);
            var allTables = args.Document.GetChildNodes(NodeType.Table, true);

            if (args.DocumentFieldName.Contains("html__") && args.FieldValue != null)
            {
                var builder = new DocumentBuilder(args.Document);
                builder.MoveToMergeField(args.DocumentFieldName);
                builder.InsertHtml(args.FieldValue.ToString(), true);
                args.Text = "";
            } else if (args.DocumentFieldName.Contains("img__") && args.FieldValue != null && !string.IsNullOrWhiteSpace(args.FieldValue.ToString()))
            {
                var url = _configuration.GetSection("AppSettings")["StorageUrl"] + "/record-detail-" + instanceId + "/" + args.FieldValue;
                var wc = new WebClient();
                var bytes = wc.DownloadData(url);
                var ms = new MemoryStream(bytes);
                var img = SKBitmap.Decode(ms);
                
                var builder = new DocumentBuilder(args.Document);
                builder.MoveToMergeField(args.DocumentFieldName);
                builder.InsertImage(img);
                args.Text = "";
            }

            if (args.FieldValue != null && args.FieldValue.ToString().Contains("-product_separator_separator"))
            {
                args.Text = args.FieldValue.ToString().Split('-')[0].ToString();
                              
                for (var i = 0; i < row.Cells.Count; i++)
                {
                    _documentBuilder.MoveToCell(allTables.IndexOf(row.ParentTable), row.ParentTable.IndexOf(row), i, 0);
                    _documentBuilder.CellFormat.Shading.BackgroundPatternColor = Color.LightGray;
                    _documentBuilder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
                   
                }

                MergeCells(row.FirstCell, row.LastCell);
            }
        }

        public void ImageFieldMerging(ImageFieldMergingArgs args)
        {
        }

        public static void MergeCells(Cell startCell, Cell endCell)
        {
            Table parentTable = startCell.ParentRow.ParentTable;

            // Find the row and cell indices for the start and end cell.
            Point startCellPos = new Point(startCell.ParentRow.IndexOf(startCell), parentTable.IndexOf(startCell.ParentRow));
            Point endCellPos = new Point(endCell.ParentRow.IndexOf(endCell), parentTable.IndexOf(endCell.ParentRow));
            // Create the range of cells to be merged based off these indices. Inverse each index if the end cell if before the start cell. 
            Rectangle mergeRange = new Rectangle(Math.Min(startCellPos.X, endCellPos.X), Math.Min(startCellPos.Y, endCellPos.Y),
                Math.Abs(endCellPos.X - startCellPos.X) + 1, Math.Abs(endCellPos.Y - startCellPos.Y) + 1);

            foreach (Row row in parentTable.Rows)
            {
                
                foreach (Cell cell in row.Cells)
                {
                    Point currentPos = new Point(row.IndexOf(cell), parentTable.IndexOf(row));
                    // Check if the current cell is inside our merge range then merge it.
                    if (mergeRange.Contains(currentPos))
                    {
                        if (currentPos.X == mergeRange.X)
                            cell.CellFormat.HorizontalMerge = CellMerge.First;
                        else
                            cell.CellFormat.HorizontalMerge = CellMerge.Previous;

                        if (currentPos.Y == mergeRange.Y)
                            cell.CellFormat.VerticalMerge = CellMerge.First;
                        else
                            cell.CellFormat.VerticalMerge = CellMerge.Previous;
                    }
                }
            }
        }
    }
}