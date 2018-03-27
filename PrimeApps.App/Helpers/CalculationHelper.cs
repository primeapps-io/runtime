using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using Elmah;
using ElmahCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;

namespace PrimeApps.App.Helpers
{
    public static class CalculationHelper
    {
        public static async Task Calculate(int recordId, Module module, UserItem appUser, Warehouse warehouse, OperationType operationType)
        {
            try
            {
                using (var databaseContext = new TenantDBContext(appUser.TenantId))
                {
                    using (var moduleRepository = new ModuleRepository(databaseContext))
                    {
                        using (var picklistRepository = new PicklistRepository(databaseContext))
                        {
                            using (var recordRepository = new RecordRepository(databaseContext, warehouse))
                            {
                                moduleRepository.UserId = appUser.TenantId;
                                recordRepository.UserId = appUser.TenantId;
                                picklistRepository.UserId = appUser.TenantId;
                                warehouse.DatabaseName = appUser.WarehouseDatabaseName;
                                var record = recordRepository.GetById(module, recordId, true, null, true);

                                switch (module.Name)
                                {
                                    case "petty_cash_requisition":
                                    case "expenditure":
                                        var pettyCashModule = await moduleRepository.GetByName("petty_cash");
                                        var pettyCashRecord = recordRepository.GetById(pettyCashModule, (int)record["related_petty_cash_2"], false, null, true);
                                        var pettyCashUpdateRecord = new JObject();
                                        if (module.Name == "petty_cash_requisition")
                                        {
                                            var pettyCashRequisitionModule = await moduleRepository.GetByName("petty_cash_requisition");
                                            var pettyCashRequisitionPicklist = pettyCashRequisitionModule.Fields.Single(x => x.Name == "status");
                                            var pettyCashRequisitionPicklistItem = await picklistRepository.FindItemByLabel(pettyCashRequisitionPicklist.PicklistId.Value, (string)record["status"], appUser.TenantLanguage);
                                          
                                            if (pettyCashRequisitionPicklistItem.Value == "paid" && !record["paid_amount"].IsNullOrEmpty() && !record["paid_by"].IsNullOrEmpty())
                                            {
                                                var findRequestPettyCashRequisition = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_petty_cash_2", Operator = Operator.Equals, Value = (int)record["related_petty_cash_2"], No = 1 } }, Limit = 9999 };
                                                var pettyCashRequisitionRecords = recordRepository.Find(module.Name, findRequestPettyCashRequisition);
                                                var currencyFieldRequisition = pettyCashRequisitionModule.Fields.Single(x => x.Name == "currency_2");
                                                var currencyPicklistRequisition = await picklistRepository.GetById(currencyFieldRequisition.PicklistId.Value);

                                                decimal totalIncomeTry = 0;
                                                decimal totalIncomeEur = 0;
                                                decimal totalIncomeUsd = 0;
                                                decimal totalIncomeGbp = 0;

                                                foreach (var requisitionRecordItem in pettyCashRequisitionRecords)
                                                {
                                                    var amount = !requisitionRecordItem["paid_amount"].IsNullOrEmpty() ? (decimal)requisitionRecordItem["paid_amount"] : 0;
                                                    var currency = currencyPicklistRequisition.Items.Single(x => x.LabelEn == (string)requisitionRecordItem["currency_2"]).SystemCode;

                                                    switch (currency)
                                                    {
                                                        case "try":
                                                            totalIncomeTry += amount;
                                                            break;
                                                        case "eur":
                                                            totalIncomeEur += amount;
                                                            break;
                                                        case "usd":
                                                            totalIncomeUsd += amount;
                                                            break;
                                                        case "gbp":
                                                            totalIncomeGbp += amount;
                                                            break;
                                                    }
                                                }
                                                
                                                pettyCashUpdateRecord["id"] = (int)record["related_petty_cash_2"];
                                                pettyCashUpdateRecord["try_income"] = totalIncomeTry;
                                                pettyCashUpdateRecord["eur_income"] = totalIncomeEur;
                                                pettyCashUpdateRecord["usd_income"] = totalIncomeUsd;
                                                pettyCashUpdateRecord["gbp_income"] = totalIncomeGbp;
                                                pettyCashUpdateRecord["try_balance"] = totalIncomeTry - (pettyCashRecord["try_expenditure"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["try_expenditure"]);
                                                pettyCashUpdateRecord["eur_balance"] = totalIncomeEur - (pettyCashRecord["eur_expenditure"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["eur_expenditure"]);
                                                pettyCashUpdateRecord["usd_balance"] = totalIncomeUsd - (pettyCashRecord["usd_expenditure"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["usd_expenditure"]);
                                                pettyCashUpdateRecord["gbp_balance"] = totalIncomeGbp - (pettyCashRecord["gbp_expenditure"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["gbp_expenditure"]);
                                                pettyCashUpdateRecord["updated_by"] = (int)record["updated_by"];
                                            }
                                        }
                                        if(module.Name == "expenditure")
                                        {
                                            var expenditureModule = await moduleRepository.GetByName("expenditure");

                                            if (!record["amount"].IsNullOrEmpty())
                                            {
                                                var findRequestExpenditure = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_petty_cash_2", Operator = Operator.Equals, Value = (int)record["related_petty_cash_2"], No = 1 } }, Limit = 9999 };
                                                var expenditureRecords = recordRepository.Find(module.Name, findRequestExpenditure);
                                                var currencyFieldExpenditure = expenditureModule.Fields.Single(x => x.Name == "currency_c");
                                                var currencyPicklistExpenditure = await picklistRepository.GetById(currencyFieldExpenditure.PicklistId.Value);

                                                decimal totalExpenditureTry = 0;
                                                decimal totalExpenditureEur = 0;
                                                decimal totalExpenditureUsd = 0;
                                                decimal totalExpenditureGbp = 0;

                                                foreach (var expenditureRecordItem in expenditureRecords)
                                                {
                                                    var amount = !expenditureRecordItem["amount"].IsNullOrEmpty() ? (decimal)expenditureRecordItem["amount"] : 0;
                                                    var currency = currencyPicklistExpenditure.Items.Single(x => x.LabelEn == (string)expenditureRecordItem["currency_c"]).SystemCode;

                                                    switch (currency)
                                                    {
                                                        case "try":
                                                            totalExpenditureTry += amount;
                                                            break;
                                                        case "eur":
                                                            totalExpenditureEur += amount;
                                                            break;
                                                        case "usd":
                                                            totalExpenditureUsd += amount;
                                                            break;
                                                        case "gbp":
                                                            totalExpenditureGbp += amount;
                                                            break;
                                                    }
                                                }

                                                
                                                pettyCashUpdateRecord["id"] = (int)record["related_petty_cash_2"];
                                                pettyCashUpdateRecord["try_expenditure"] = totalExpenditureTry;
                                                pettyCashUpdateRecord["eur_expenditure"] = totalExpenditureEur;
                                                pettyCashUpdateRecord["usd_expenditure"] = totalExpenditureUsd;
                                                pettyCashUpdateRecord["gbp_expenditure"] = totalExpenditureGbp;
                                                pettyCashUpdateRecord["try_balance"] = (pettyCashRecord["try_income"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["try_income"]) - totalExpenditureTry;
                                                pettyCashUpdateRecord["eur_balance"] = (pettyCashRecord["eur_income"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["eur_income"]) - totalExpenditureEur;
                                                pettyCashUpdateRecord["usd_balance"] = (pettyCashRecord["usd_income"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["usd_income"]) - totalExpenditureUsd;
                                                pettyCashUpdateRecord["gbp_balance"] = (pettyCashRecord["gbp_income"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["gbp_income"]) - totalExpenditureGbp;
                                                pettyCashUpdateRecord["updated_by"] = (int)record["updated_by"];
                                            }
                                        }
                                        await recordRepository.Update(pettyCashUpdateRecord, pettyCashModule);
                                        break;

                                    case "expense_sheet":
                                    case "invoices":

                                        var expenseModule = await moduleRepository.GetByName("expense_sheet");
                                        var expenseTypePicklist = expenseModule.Fields.Single(x => x.Name == "expense_type");
                                        var expenseTypePicklistItem = await picklistRepository.FindItemByLabel(expenseTypePicklist.PicklistId.Value, (string)record["expense_type"], appUser.TenantLanguage);

                                        var approvalWorkflowModule = await moduleRepository.GetByName("approval_workflow");
                                        var approvalTypePicklistId = approvalWorkflowModule.Fields.Single(x => x.Name == "approval_type").PicklistId.Value;
                                        var approvalTypePicklist = await picklistRepository.GetById(approvalTypePicklistId);


                                        if (module.Name == "expense_sheet")
                                        {
                                            JArray approverUserRecord = null;
                                            if (expenseTypePicklistItem.SystemCode == "project_expense")
                                            {
                                                var approvalTypePicklistItem = approvalTypePicklist.Items.Single(x => x.SystemCode == "nonbillable");
                                                var findRequestApprovalWorkflow = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_project", Operator = Operator.Equals, Value = (int)record["project_code"], No = 1 }, new Filter { Field = "approval_type", Operator = Operator.Equals, Value = appUser.TenantLanguage == "tr" ? approvalTypePicklistItem.LabelTr : approvalTypePicklistItem.LabelEn, No = 2 } }, Limit = 9999 };
                                                var approvalWorkflowRecord = recordRepository.Find("approval_workflow", findRequestApprovalWorkflow);
                                                var findRequestHumanResources = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["first_approver"], No = 1 } }, Limit = 9999 };
                                                var humanResourcesRecord = recordRepository.Find("human_resources", findRequestHumanResources);
                                                var findApproverUser = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = humanResourcesRecord.First()["e_mail1"], No = 1 } }, Limit = 9999 };
                                                approverUserRecord = recordRepository.Find("users", findApproverUser);
                                                if (!humanResourcesRecord.IsNullOrEmpty())
                                                {
                                                    record["custom_approver"] = humanResourcesRecord.First()["e_mail1"];
                                                    if (!record["shared_users_edit"].IsNullOrEmpty())
                                                    {
                                                        int id = (int)approverUserRecord.First()["id"];
                                                        var sharedUsers = (JArray)record["shared_users_edit"];
                                                        foreach (var item in sharedUsers.ToList())
                                                        {
                                                            if ((int)item != id)
                                                            {
                                                                sharedUsers.Add(approverUserRecord.First()["id"]);
                                                                record["shared_users_edit"] = sharedUsers;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var sharedUsers = new JArray();
                                                        sharedUsers.Add(approverUserRecord.First()["id"]);
                                                        record["shared_users_edit"] = sharedUsers;
                                                    }
                                                }
                                            }
                                            else if (expenseTypePicklistItem.SystemCode == "overhead")
                                            {
                                                var findRequestHumanResources = new FindRequest { Filters = new List<Filter> { new Filter { Field = "e_mail1", Operator = Operator.Equals, Value = appUser.Email, No = 1 } }, Limit = 9999 };
                                                var humanResourcesRecord = recordRepository.Find("human_resources", findRequestHumanResources);
                                                var approvalTypePicklistItem = approvalTypePicklist.Items.Single(x => x.SystemCode == "management");
                                                var findRequestApprovalWorkflow = new FindRequest { Filters = new List<Filter> { new Filter { Field = "staff", Operator = Operator.Equals, Value = (int)humanResourcesRecord.First()["id"], No = 1 }, new Filter { Field = "approval_type", Operator = Operator.Equals, Value = appUser.TenantLanguage == "tr" ? approvalTypePicklistItem.LabelTr : approvalTypePicklistItem.LabelEn, No = 2 } }, Limit = 9999 };
                                                var approvalWorkflowRecord = recordRepository.Find("approval_workflow", findRequestApprovalWorkflow);
                                                var findApproverRecord = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["first_approver"], No = 1 } }, Limit = 9999 };
                                                var approverRecord = recordRepository.Find("human_resources", findApproverRecord);
                                                var findApproverUser = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 9999 };
                                                approverUserRecord = recordRepository.Find("users", findApproverUser);
                                                if (!approverRecord.IsNullOrEmpty())
                                                {
                                                    record["custom_approver"] = approverRecord.First()["e_mail1"];
                                                    if (!record["shared_users_edit"].IsNullOrEmpty())
                                                    {
                                                        var sharedUsers = (JArray)record["shared_users_edit"];
                                                        sharedUsers.Add(approverUserRecord.First()["id"]);
                                                        record["shared_users_edit"] = sharedUsers;
                                                    }
                                                    else
                                                    {
                                                        var sharedUsers = new JArray();
                                                        sharedUsers.Add(approverUserRecord.First()["id"]);
                                                        record["shared_users_edit"] = sharedUsers;
                                                    }
                                                }

                                            }

                                            if (!approverUserRecord.IsNullOrEmpty())
                                            {
                                                var expenseFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "expense_sheet", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
                                                var expenses = recordRepository.Find("expenses", expenseFindRequest);
                                                if (expenses.Count > 0)
                                                {
                                                    var expModule = await moduleRepository.GetByName("expenses");
                                                    foreach (JObject expense in expenses)
                                                    {
                                                        if (!expense["shared_users_edit"].IsNullOrEmpty())
                                                        {
                                                            var expenseSharedUsers = (JArray)expense["shared_users_edit"];
                                                            expenseSharedUsers.Add(approverUserRecord.First()["id"]);
                                                            expense["shared_users_edit"] = expenseSharedUsers;
                                                        }
                                                        else
                                                        {
                                                            var expenseSharedUsers = new JArray();
                                                            expenseSharedUsers.Add(approverUserRecord.First()["id"]);
                                                            expense["shared_users_edit"] = expenseSharedUsers;
                                                        }

                                                        await recordRepository.Update(expense, expModule);
                                                    }
                                                }
                                            }

                                            using (var userGroupRepository = new UserGroupRepository(databaseContext))
                                            {
                                                var financeUserGroup = await userGroupRepository.GetByName("finance-expense");

                                                if (financeUserGroup != null)
                                                {
                                                    var sharedUserGroups = new JArray();

                                                    if (!record["shared_user_groups_edit"].IsNullOrEmpty())
                                                        sharedUserGroups = (JArray)record["shared_user_groups_edit"];

                                                    if (!sharedUserGroups.Any(x => (int)x == financeUserGroup.Id))
                                                        sharedUserGroups.Add(financeUserGroup.Id);

                                                    record["shared_user_groups_edit"] = sharedUserGroups;

                                                    var expenseFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "expense_sheet", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
                                                    var expenses = recordRepository.Find("expenses", expenseFindRequest);
                                                    if (expenses.Count > 0)
                                                    {
                                                        var expModule = await moduleRepository.GetByName("expenses");
                                                        foreach (JObject expense in expenses)
                                                        {
                                                            if (!expense["shared_user_groups_edit"].IsNullOrEmpty())
                                                            {
                                                                var expenseSharedUsers = (JArray)expense["shared_user_groups_edit"];
                                                                if (!expenseSharedUsers.Any(x => (int)x == financeUserGroup.Id))
                                                                {
                                                                    expenseSharedUsers.Add(financeUserGroup.Id);
                                                                    expense["shared_user_groups_edit"] = expenseSharedUsers;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                var expenseSharedUsers = new JArray();
                                                                expenseSharedUsers.Add(financeUserGroup.Id);
                                                                expense["shared_user_groups_edit"] = expenseSharedUsers;
                                                            }

                                                            await recordRepository.Update(expense, expModule);
                                                        }
                                                    }
                                                }
                                            }

                                            await recordRepository.Update(record, expenseModule);
                                        }
                                        else if (module.Name == "invoices")
                                        {
                                            var invoiceModule = await moduleRepository.GetByName("invoices");
                                            var invoiceTypePicklist = invoiceModule.Fields.Single(x => x.Name == "invoice_type");
                                            var invoiceTypePicklistItem = await picklistRepository.FindItemByLabel(invoiceTypePicklist.PicklistId.Value, (string)record["invoice_type"], appUser.TenantLanguage);
                                            var invoiceApproverPicklist = invoiceModule.Fields.Single(x => x.Name == "approver");
                                            var invoiceApproverPicklistItem = await picklistRepository.FindItemByLabel(invoiceApproverPicklist.PicklistId.Value, (string)record["approver"], appUser.TenantLanguage);

                                            if (invoiceTypePicklistItem.SystemCode == "project_expense")
                                            {
                                                var approvalTypePicklistItem = approvalTypePicklist.Items.Single(x => x.SystemCode == "nonbillable");
                                                var findRequestApprovalWorkflow = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_project", Operator = Operator.Equals, Value = (int)record["project"], No = 1 }, new Filter { Field = "approval_type", Operator = Operator.Equals, Value = appUser.TenantLanguage == "tr" ? approvalTypePicklistItem.LabelTr : approvalTypePicklistItem.LabelEn, No = 2 } }, Limit = 9999 };
                                                var approvalWorkflowRecord = recordRepository.Find("approval_workflow", findRequestApprovalWorkflow);

                                                if (invoiceApproverPicklistItem.SystemCode == "project_director")
                                                {
                                                    var findApproverRecord = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["first_approver"], No = 1 } }, Limit = 9999 };
                                                    var approverRecord = recordRepository.Find("human_resources", findApproverRecord);
                                                    var findApproverUser = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 9999 };
                                                    var approverUserRecord = recordRepository.Find("users", findApproverUser);

                                                    if (!approverRecord.IsNullOrEmpty())
                                                    {
                                                        record["custom_approver"] = approverRecord.First()["e_mail1"];
                                                        if (!record["shared_users_edit"].IsNullOrEmpty())
                                                        {
                                                            var sharedUsers = (JArray)record["shared_users_edit"];
                                                            sharedUsers.Add(approverUserRecord.First()["id"]);
                                                            record["shared_users_edit"] = sharedUsers;
                                                        }
                                                        else
                                                        {
                                                            var sharedUsers = new JArray();
                                                            sharedUsers.Add(approverUserRecord.First()["id"]);
                                                            record["shared_users_edit"] = sharedUsers;
                                                        }
                                                    }
                                                }
                                                else if (invoiceApproverPicklistItem.SystemCode == "project_officer")
                                                {
                                                    var findApproverRecord = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["project_officer_staff"], No = 1 } }, Limit = 9999 };
                                                    var approverRecord = recordRepository.Find("human_resources", findApproverRecord);
                                                    var findApproverUser = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 9999 };
                                                    var approverUserRecord = recordRepository.Find("users", findApproverUser);

                                                    if (!approverRecord.IsNullOrEmpty())
                                                    {
                                                        record["custom_approver"] = approverRecord.First()["e_mail1"];
                                                        if (!record["shared_users_edit"].IsNullOrEmpty())
                                                        {
                                                            var sharedUsers = (JArray)record["shared_users_edit"];
                                                            sharedUsers.Add(approverUserRecord.First()["id"]);
                                                            record["shared_users_edit"] = sharedUsers;
                                                        }
                                                        else
                                                        {
                                                            var sharedUsers = new JArray();
                                                            sharedUsers.Add(approverUserRecord.First()["id"]);
                                                            record["shared_users_edit"] = sharedUsers;
                                                        }
                                                    }
                                                }

                                            }
                                            else if (invoiceTypePicklistItem.SystemCode == "overhead")
                                            {
                                                var approvalTypePicklistItem = approvalTypePicklist.Items.Single(x => x.SystemCode == "management");
                                                var findRequestHumanResources = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)record["related_staff"], No = 1 } }, Limit = 9999 };
                                                var humanResourcesRecord = recordRepository.Find("human_resources", findRequestHumanResources);
                                                var findApproverUser = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = humanResourcesRecord.First()["e_mail1"], No = 1 } }, Limit = 9999 };
                                                var approverUserRecord = recordRepository.Find("users", findApproverUser);

                                                if (!humanResourcesRecord.IsNullOrEmpty())
                                                {
                                                    record["custom_approver"] = humanResourcesRecord.First()["e_mail1"];
                                                    if (!record["shared_users_edit"].IsNullOrEmpty())
                                                    {
                                                        var sharedUsers = (JArray)record["shared_users_edit"];
                                                        sharedUsers.Add(approverUserRecord.First()["id"]);
                                                        record["shared_users_edit"] = sharedUsers;
                                                    }
                                                    else
                                                    {
                                                        var sharedUsers = new JArray();
                                                        sharedUsers.Add(approverUserRecord.First()["id"]);
                                                        record["shared_users_edit"] = sharedUsers;
                                                    }
                                                }
                                            }

                                            using (var userGroupRepository = new UserGroupRepository(databaseContext))
                                            {
                                                var financeUserGroup = await userGroupRepository.GetByName("finance-expense");

                                                if (financeUserGroup != null)
                                                {
                                                    var sharedUserGroups = new JArray();

                                                    if (!record["shared_user_groups_edit"].IsNullOrEmpty())
                                                        sharedUserGroups = (JArray)record["shared_user_groups_edit"];

                                                    if (!sharedUserGroups.Any(x => (int)x == financeUserGroup.Id))
                                                        sharedUserGroups.Add(financeUserGroup.Id);

                                                    record["shared_user_groups_edit"] = sharedUserGroups;
                                                }
                                            }

                                            await recordRepository.Update(record, invoiceModule);
                                        }

                                        break;
                                    case "order_products":
                                    case "purchase_order_products":
                                        var isApproved = false;
                                        var prodMod = await moduleRepository.GetByName("products");
                                        var prodItem = recordRepository.GetById(prodMod, (int)record["product"], false);
                                        if (module.Name == "order_products")
                                        {
                                            var salesOrderModule = await moduleRepository.GetByName("sales_orders");
                                            var salesOrderItem = recordRepository.GetById(salesOrderModule, (int)record["sales_order"], false);
                                            var salesStagePicklist = salesOrderModule.Fields.Single(x => x.Name == "order_stage");
                                            var salesStagePicklistItem = await picklistRepository.FindItemByLabel(salesStagePicklist.PicklistId.Value, (string)salesOrderItem["order_stage"], appUser.TenantLanguage);
                                            if (salesStagePicklistItem.SystemCode == "confirmed_order_stage" && !(bool)salesOrderItem["approved"] && (bool)prodItem["using_stock"])
                                                isApproved = true;
                                        }
                                        else if (module.Name == "purchase_order_products")
                                        {
                                            var purchaseOrderModule = await moduleRepository.GetByName("purchase_orders");
                                            var purchaseOrderItem = recordRepository.GetById(purchaseOrderModule, (int)record["purchase_order"], false);
                                            var purchaseStagePicklist = purchaseOrderModule.Fields.Single(x => x.Name == "order_stage");
                                            var purchaseStagePicklistItem = await picklistRepository.FindItemByLabel(purchaseStagePicklist.PicklistId.Value, (string)purchaseOrderItem["order_stage"], appUser.TenantLanguage);
                                            if (purchaseStagePicklistItem.SystemCode == "confirmed_purchase_order_stage" && !(bool)purchaseOrderItem["approved"] && (bool)prodItem["using_stock"])
                                                isApproved = true;
                                        }

                                        if (isApproved)
                                        {
                                            var modelStateTransaction = new ModelStateDictionary();
                                            var stockModule = await moduleRepository.GetByName("stock_transactions");
                                            var transactionTypeField = stockModule.Fields.Single(x => x.Name == "stock_transaction_type");
                                            var transactionTypes = await picklistRepository.GetById(transactionTypeField.PicklistId.Value);

                                            var stock = new JObject();
                                            stock["owner"] = appUser.Id;
                                            stock["product"] = record["product"];
                                            stock["quantity"] = record["quantity"];
                                            stock["transaction_date"] = DateTime.UtcNow.Date;

                                            if (module.Name == "order_products")
                                            {
                                                stock["stock_transaction_type"] = transactionTypes.Items.Single(x => x.SystemCode == "stock_output").Id;
                                                stock["sales_order"] = (int)record["sales_order"];

                                            }
                                            else if (module.Name == "purchase_order_products")
                                            {
                                                stock["stock_transaction_type"] = transactionTypes.Items.Single(x => x.SystemCode == "stock_input").Id;
                                                stock["purchase_order"] = (int)record["purchase_order"];
                                            }

                                            var transactionBeforeCreate = await RecordHelper.BeforeCreateUpdate(stockModule, stock, modelStateTransaction, appUser.TenantLanguage, moduleRepository, picklistRepository);
                                            if (transactionBeforeCreate < 0 && !modelStateTransaction.IsValid)
                                            {
                                                ErrorLog.GetDefault(null).Log(new Error(new Exception("Stock transaction can not be created")));
                                                return;
                                            }

                                            await recordRepository.Create(stock, stockModule);

                                            if (prodItem["stock_quantity"].IsNullOrEmpty())
                                                prodItem["stock_quantity"] = 0;

                                            if (module.Name == "order_products")
                                            {
                                                prodItem["stock_quantity"] = (decimal)prodItem["stock_quantity"] - (decimal)record["quantity"];
                                                var salOrMod = await moduleRepository.GetByName("sales_orders");
                                                var salOrItem = recordRepository.GetById(salOrMod, (int)record["sales_order"], false);
                                                salOrItem["approved"] = true;
                                                await recordRepository.Update(salOrItem, salOrMod);
                                            }
                                            else if (module.Name == "purchase_order_products")
                                            {
                                                prodItem["stock_quantity"] = (decimal)prodItem["stock_quantity"] + (decimal)record["quantity"];
                                                var purcOrMod = await moduleRepository.GetByName("purchase_order_products");
                                                var purcOrItem = recordRepository.GetById(purcOrMod, (int)record["purchase_order"], false);
                                                purcOrItem["approved"] = true;
                                                await recordRepository.Update(purcOrItem, purcOrMod);
                                            }

                                            await recordRepository.Update(prodItem, prodMod);
                                        }

                                        break;

                                    case "stock_transactions":
                                        var productModuleObj = await moduleRepository.GetByName("products");
                                        var product = recordRepository.GetById(productModuleObj, (int)record["product"], false);

                                        if (product["stock_quantity"].IsNullOrEmpty())
                                            product["stock_quantity"] = 0;

                                        var stockModuleObj = await moduleRepository.GetByName("stock_transactions");
                                        var transactionTypePicklist = stockModuleObj.Fields.Single(x => x.Name == "stock_transaction_type");
                                        var transactionTypePicklistItem = await picklistRepository.FindItemByLabel(transactionTypePicklist.PicklistId.Value, (string)record["stock_transaction_type"], appUser.TenantLanguage);
                                        if (transactionTypePicklistItem.Value2 == "customer_return" || transactionTypePicklistItem.Value2 == "stock_input")
                                        {
                                            product["stock_quantity"] = (decimal)product["stock_quantity"] + (decimal)record["quantity"];
                                        }
                                        else if (transactionTypePicklistItem.Value2 == "supplier_return" || transactionTypePicklistItem.Value2 == "stock_output")
                                        {
                                            product["stock_quantity"] = (decimal)product["stock_quantity"] - (decimal)record["quantity"];
                                        }

                                        await recordRepository.Update(product, productModuleObj);
                                        break;

                                    case "current_accounts":
                                        try
                                        {
                                            var currentTransactionType = (string)record["transaction_type_system"];
                                            var recordUpdate = new JObject();
                                            decimal balance;
                                            Module moduleUpdate;

                                            switch (currentTransactionType)
                                            {
                                                case "sales_invoice":
                                                case "collection":
                                                    var customerId = (int)record["customer"];
                                                    balance = recordRepository.CalculateBalance(currentTransactionType, customerId);
                                                    moduleUpdate = await moduleRepository.GetByName("accounts");
                                                    recordUpdate["id"] = customerId;
                                                    recordUpdate["balance"] = balance;
                                                    break;
                                                case "purchase_invoice":
                                                case "payment":
                                                    var supplierId = (int)record["supplier"];
                                                    balance = recordRepository.CalculateBalance(currentTransactionType, supplierId);
                                                    moduleUpdate = await moduleRepository.GetByName("suppliers");
                                                    recordUpdate["id"] = supplierId;
                                                    recordUpdate["balance"] = balance;
                                                    break;
                                                default:
                                                    throw new Exception("Record transaction_type_system must be sales_invoice, collection, purchase_invoice or payment.");
                                            }

                                            recordUpdate["updated_by"] = (int)record["updated_by"];

                                            var resultUpdate = await recordRepository.Update(recordUpdate, moduleUpdate);

                                            if (resultUpdate < 1)
                                                ErrorLog.GetDefault(null).Log(new Error(new Exception("Balance cannot be updated! Object: " + recordUpdate)));
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorLog.GetDefault(null).Log(new Error(ex));
                                        }
                                        break;
                                    case "project_indicators":
                                        var projectScopeModule = await moduleRepository.GetByName("project_scope");
                                        var projectScopeRecord = recordRepository.GetById(projectScopeModule, (int)record["related_result"]);
                                        var findRequestProjectIndicator = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_result", Operator = Operator.Equals, Value = (int)record["related_result"], No = 1 } }, Limit = 9999 };
                                        var projectIndicatorRecords = recordRepository.Find(module.Name, findRequestProjectIndicator);
                                        var modelState = new ModelStateDictionary();
                                        var weightScope = !projectScopeRecord["weight"].IsNullOrEmpty() ? (decimal)projectScopeRecord["weight"] : 0;
                                        decimal percentage = 0;
                                        decimal targetBudget = 0;
                                        decimal actualBudget = 0;

                                        //Update project scope percentage
                                        foreach (var projectIndicatorRecord in projectIndicatorRecords)
                                        {
                                            percentage += (!projectIndicatorRecord["weight2"].IsNullOrEmpty() ? (decimal)projectIndicatorRecord["weight2"] : 0) * (!projectIndicatorRecord["percentage"].IsNullOrEmpty() ? (decimal)projectIndicatorRecord["percentage"] : 0) / 100;
                                            targetBudget += !projectIndicatorRecord["target_budget"].IsNullOrEmpty() ? (decimal)projectIndicatorRecord["target_budget"] : 0;
                                            actualBudget += !projectIndicatorRecord["actual_budget"].IsNullOrEmpty() ? (decimal)projectIndicatorRecord["actual_budget"] : 0;
                                        }

                                        if (percentage > 100)
                                            percentage = 100;

                                        var projectScopeUpdateRecord = new JObject();
                                        projectScopeUpdateRecord["id"] = (int)projectScopeRecord["id"];
                                        projectScopeUpdateRecord["percentage"] = percentage;
                                        projectScopeUpdateRecord["effect"] = weightScope * percentage / 100;
                                        projectScopeUpdateRecord["budget"] = targetBudget;
                                        projectScopeUpdateRecord["actual_budget"] = actualBudget;
                                        projectScopeUpdateRecord["updated_by"] = (int)projectScopeRecord["updated_by"];

                                        var resultBeforeProjectScope = await RecordHelper.BeforeCreateUpdate(projectScopeModule, projectScopeUpdateRecord, modelState, appUser.TenantLanguage, moduleRepository, picklistRepository);

                                        if (resultBeforeProjectScope < 0 && !modelState.IsValid)
                                        {
                                            ErrorLog.GetDefault(null).Log(new Error(new Exception("ProjectScope cannot be updated! Object: " + projectScopeUpdateRecord + " ModelState: " + modelState.ToJsonString())));
                                            return;
                                        }

                                        try
                                        {
                                            var resultUpdateProjectScope = await recordRepository.Update(projectScopeUpdateRecord, projectScopeModule);

                                            if (resultUpdateProjectScope < 1)
                                            {
                                                ErrorLog.GetDefault(null).Log(new Error(new Exception("ProjectScope cannot be updated! Object: " + projectScopeUpdateRecord)));
                                                return;
                                            }

                                            await Calculate((int)projectScopeUpdateRecord["id"], projectScopeModule, appUser, warehouse, operationType);
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorLog.GetDefault(null).Log(new Error(ex));
                                            return;
                                        }

                                        //Update project indicator effect
                                        if (operationType != OperationType.delete)
                                        {
                                            var weightIndicator = !record["weight2"].IsNullOrEmpty() ? (decimal)record["weight2"] : 0;
                                            var percentageIndicator = !record["percentage"].IsNullOrEmpty() ? (decimal)record["percentage"] : 0;
                                            var projectIndicatorUpdateRecord = new JObject();
                                            projectIndicatorUpdateRecord["id"] = (int)record["id"];
                                            projectIndicatorUpdateRecord["effect"] = ((weightIndicator / 100) * (percentageIndicator / 100)) * weightScope;
                                            projectIndicatorUpdateRecord["updated_by"] = (int)record["updated_by"];

                                            var indicatorStatusField = module.Fields.Single(x => x.Name == "status");
                                            var indicatorStatusPicklist = await picklistRepository.GetById(indicatorStatusField.PicklistId.Value);
                                            var completedStatusPicklistItem = indicatorStatusPicklist.Items.Single(x => x.Value == "completed");
                                            var ongoingStatusPicklistItem = indicatorStatusPicklist.Items.Single(x => x.Value == "ongoing");
                                            var notStartedStatusPicklistItem = indicatorStatusPicklist.Items.Single(x => x.Value == "not_started");

                                            if (percentageIndicator >= 100)
                                                projectIndicatorUpdateRecord["status"] = completedStatusPicklistItem.Id;

                                            if (percentageIndicator < 100)
                                                projectIndicatorUpdateRecord["status"] = ongoingStatusPicklistItem.Id;

                                            if (percentageIndicator <= 0)
                                                projectIndicatorUpdateRecord["status"] = notStartedStatusPicklistItem.Id;

                                            var resultBeforeProjectIndicator = await RecordHelper.BeforeCreateUpdate(module, projectIndicatorUpdateRecord, modelState, appUser.TenantLanguage, moduleRepository, picklistRepository);

                                            if (resultBeforeProjectIndicator < 0 && !modelState.IsValid)
                                            {
                                                ErrorLog.GetDefault(null).Log(new Error(new Exception("ProjectIndicator cannot be updated! Object: " + projectScopeUpdateRecord + " ModelState: " + modelState.ToJsonString())));
                                                return;
                                            }

                                            try
                                            {
                                                var resultUpdateProjectIndicator = await recordRepository.Update(projectIndicatorUpdateRecord, module);

                                                if (resultUpdateProjectIndicator < 1)
                                                {
                                                    ErrorLog.GetDefault(null).Log(new Error(new Exception("ProjectIndicator cannot be updated! Object: " + projectScopeUpdateRecord)));
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                ErrorLog.GetDefault(null).Log(new Error(ex));
                                            }
                                        }

                                        break;
                                    case "project_scope":
                                        var projectModule = await moduleRepository.GetByName("projects");
                                        var projectRecord = recordRepository.GetById(projectModule, (int)record["project"]);
                                        var findRequestProjectScope = new FindRequest { Filters = new List<Filter> { new Filter { Field = "project", Operator = Operator.Equals, Value = (int)record["project"], No = 1 } }, Limit = 9999 };
                                        var projectScopeRecords = recordRepository.Find(module.Name, findRequestProjectScope);
                                        var modelStateScope = new ModelStateDictionary();
                                        decimal scope = 0;

                                        foreach (var projectScopeRecordItem in projectScopeRecords)
                                        {
                                            scope += !projectScopeRecordItem["effect"].IsNullOrEmpty() ? (decimal)projectScopeRecordItem["effect"] : 0;
                                        }

                                        if (scope > 100)
                                            scope = 100;

                                        var projectUpdateRecord = new JObject();
                                        projectUpdateRecord["id"] = (int)projectRecord["id"];
                                        projectUpdateRecord["scope"] = scope;
                                        projectUpdateRecord["updated_by"] = (int)projectRecord["updated_by"];

                                        var resultBeforeProject = await RecordHelper.BeforeCreateUpdate(projectModule, projectUpdateRecord, modelStateScope, appUser.TenantLanguage, moduleRepository, picklistRepository);

                                        if (resultBeforeProject < 0 && !modelStateScope.IsValid)
                                        {
                                            ErrorLog.GetDefault(null).Log(new Error(new Exception("Project cannot be updated! Object: " + projectUpdateRecord + " ModelState: " + modelStateScope.ToJsonString())));
                                            return;
                                        }

                                        try
                                        {
                                            var resultUpdateProject = await recordRepository.Update(projectUpdateRecord, projectModule);

                                            if (resultUpdateProject < 1)
                                            {
                                                ErrorLog.GetDefault(null).Log(new Error(new Exception("Project cannot be updated! Object: " + projectUpdateRecord)));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorLog.GetDefault(null).Log(new Error(ex));
                                        }
                                        break;
                                    case "expenses":
                                        var expenseSheetModule = await moduleRepository.GetByName("expense_sheet");
                                        var expensesModule = await moduleRepository.GetByName("expenses");
                                        var expenseSheetRecord = recordRepository.GetById(expenseSheetModule, (int)record["expense_sheet"]);
                                        var findRequestExpense = new FindRequest { Filters = new List<Filter> { new Filter { Field = "expense_sheet", Operator = Operator.Equals, Value = (int)record["expense_sheet"], No = 1 } }, Limit = 9999 };
                                        var expenseRecords = recordRepository.Find(module.Name, findRequestExpense);
                                        var currencyField = expensesModule.Fields.Single(x => x.Name == "currency");
                                        var currencyPicklist = await picklistRepository.GetById(currencyField.PicklistId.Value);
                                        var modelStateExpenseSheet = new ModelStateDictionary();
                                        decimal totalAmountTry = 0;
                                        decimal totalAmountEur = 0;
                                        decimal totalAmountUsd = 0;
                                        decimal totalAmountGbp = 0;

                                        foreach (var expenseRecordItem in expenseRecords)
                                        {
                                            var amount = !expenseRecordItem["amount"].IsNullOrEmpty() ? (decimal)expenseRecordItem["amount"] : 0;
                                            var currency = currencyPicklist.Items.Single(x => x.LabelEn == (string)expenseRecordItem["currency"]).SystemCode;

                                            switch (currency)
                                            {
                                                case "try":
                                                    totalAmountTry += amount;
                                                    break;
                                                case "eur":
                                                    totalAmountEur += amount;
                                                    break;
                                                case "usd":
                                                    totalAmountUsd += amount;
                                                    break;
                                                case "gbp":
                                                    totalAmountGbp += amount;
                                                    break;
                                            }
                                        }

                                        var expenseSheetUpdateRecord = new JObject();
                                        expenseSheetUpdateRecord["id"] = (int)expenseSheetRecord["id"];
                                        expenseSheetUpdateRecord["total_amount_try"] = totalAmountTry;
                                        expenseSheetUpdateRecord["total_amount_eur"] = totalAmountEur;
                                        expenseSheetUpdateRecord["total_amount_usd"] = totalAmountUsd;
                                        expenseSheetUpdateRecord["total_amount_gbp"] = totalAmountGbp;
                                        expenseSheetUpdateRecord["updated_by"] = (int)expenseSheetRecord["updated_by"];

                                        var resultBeforeExpenseSheet = await RecordHelper.BeforeCreateUpdate(expenseSheetModule, expenseSheetUpdateRecord, modelStateExpenseSheet, appUser.TenantLanguage, moduleRepository, picklistRepository);

                                        if (resultBeforeExpenseSheet < 0 && !modelStateExpenseSheet.IsValid)
                                        {
                                            ErrorLog.GetDefault(null).Log(new Error(new Exception("ExpenseSheet cannot be updated! Object: " + expenseSheetUpdateRecord + " ModelState: " + modelStateExpenseSheet.ToJsonString())));
                                            return;
                                        }

                                        try
                                        {
                                            var resultUpdateExpenseSheet = await recordRepository.Update(expenseSheetUpdateRecord, expenseSheetModule);

                                            if (resultUpdateExpenseSheet < 1)
                                            {
                                                ErrorLog.GetDefault(null).Log(new Error(new Exception("ExpenseSheet cannot be updated! Object: " + expenseSheetUpdateRecord)));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorLog.GetDefault(null).Log(new Error(ex));
                                        }
                                        break;
                                    case "projects":
                                        /*//Disabled temporarily
                                        if (module.Fields.Any(x => x.Name == "eoi_coordinator_and_rationale_writer"))
                                        {
                                            if (!record["sector"].IsNullOrEmpty())
                                            {
                                                var projectsModule = await moduleRepository.GetByName("projects");
                                                var valueArray = new JArray();
                                                valueArray.Add((string)record["sector"]);
                                                var findRequestEoiCoordinator = new FindRequest { Filters = new List<Filter> { new Filter { Field = "eoi_coordinator_and_rationale_writer_sectors", Operator = Operator.Contains, Value = valueArray, No = 1 } }, Limit = 1 };
                                                var eoiCoordinatorRecords = recordRepository.Find("human_resources", findRequestEoiCoordinator);
                                                var findRequestTpManager = new FindRequest { Filters = new List<Filter> { new Filter { Field = "tp_manager_and_strategy_writer_sector", Operator = Operator.Contains, Value = valueArray, No = 1 } }, Limit = 1 };
                                                var tpManagerRecords = recordRepository.Find("human_resources", findRequestTpManager);
                                                var modelStateprojects = new ModelStateDictionary();
                                                var projectsUpdateRecord = new JObject();
                                                projectsUpdateRecord["id"] = (int)record["id"];
                                                projectsUpdateRecord["updated_by"] = (int)record["updated_by"];

                                                if (!eoiCoordinatorRecords.IsNullOrEmpty())
                                                    projectsUpdateRecord["eoi_coordinator_and_rationale"] = eoiCoordinatorRecords[0]["id"];

                                                if (!tpManagerRecords.IsNullOrEmpty())
                                                    projectsUpdateRecord["tp_manager_and_strategy"] = tpManagerRecords[0]["id"];

                                                var resultBeforeProjects = await RecordHelper.BeforeCreateUpdate(projectsModule, projectsUpdateRecord, modelStateprojects, appUser.PicklistLanguage, moduleRepository, picklistRepository);

                                                if (resultBeforeProjects < 0 && !modelStateprojects.IsValid)
                                                {
                                                    ErrorLog.GetDefault(null).Log(new Error(new Exception("ExpenseSheet cannot be updated! Object: " + projectsUpdateRecord + " ModelState: " + modelStateprojects.ToJsonString())));
                                                    return;
                                                }

                                                try
                                                {
                                                    var resultUpdateProject = await recordRepository.Update(projectsUpdateRecord, projectsModule);

                                                    if (resultUpdateProject < 1)
                                                    {
                                                        ErrorLog.GetDefault(null).Log(new Error(new Exception("Project cannot be updated! Object: " + projectsUpdateRecord)));
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    ErrorLog.GetDefault(null).Log(new Error(ex));
                                                }
                                            }
                                        }
                                        */
                                        break;
                                    case "timesheet_item":
                                        var timesheetModule = await moduleRepository.GetByName("timesheet");
                                        var findRequestFields = await RecordHelper.GetAllFieldsForFindRequest("timesheet_item", moduleRepository);
                                        var findRequestTimesheetItems = new FindRequest { Fields = findRequestFields, Filters = new List<Filter> { new Filter { Field = "related_timesheet", Operator = Operator.Equals, Value = (int)record["related_timesheet"], No = 1 } }, Limit = 9999 };
                                        var timesheetItemsRecords = recordRepository.Find(module.Name, findRequestTimesheetItems, false);
                                        var statusField = timesheetModule.Fields.Single(x => x.Name == "status");
                                        var statusPicklist = await picklistRepository.GetById(statusField.PicklistId.Value);
                                        var approvedStatusPicklistItem = statusPicklist.Items.Single(x => x.Value == "approved_second");
                                        var approvedStatusPicklistItemLabel = appUser.TenantLanguage == "tr" ? approvedStatusPicklistItem.LabelTr : approvedStatusPicklistItem.LabelEn;
                                        var timesheetRecord = recordRepository.GetById(timesheetModule, (int)record["related_timesheet"]);
                                        var approved = true;

                                        if ((string)timesheetRecord["status"] == approvedStatusPicklistItemLabel)
                                            return;

                                        if (timesheetItemsRecords.Count == 0)
                                            return;

                                        if (!timesheetItemsRecords.IsNullOrEmpty() && timesheetItemsRecords.Count > 0)
                                        {
                                            foreach (var timesheetItemsRecord in timesheetItemsRecords)
                                            {
                                                if (!approved)
                                                    continue;

                                                var statusRecord = timesheetItemsRecord["status"].ToString();
                                                var statusPicklistItem = statusPicklist.Items.Single(x => x.LabelEn == statusRecord);

                                                if (statusPicklistItem.Value != "approved_second")
                                                {
                                                    approved = false;
                                                    break;
                                                }
                                            }
                                        }

                                        if (approved)
                                        {
                                            var timesheetRecordUpdate = new JObject();
                                            timesheetRecordUpdate["id"] = (int)record["related_timesheet"];
                                            timesheetRecordUpdate["status"] = approvedStatusPicklistItem.Id;
                                            timesheetRecordUpdate["updated_by"] = (int)record["updated_by"];

                                            var modelStateTimesheet = new ModelStateDictionary();
                                            var resultBefore = await RecordHelper.BeforeCreateUpdate(timesheetModule, timesheetRecordUpdate, modelStateTimesheet, appUser.TenantLanguage, moduleRepository, picklistRepository);

                                            if (resultBefore < 0 && !modelStateTimesheet.IsValid)
                                            {
                                                ErrorLog.GetDefault(null).Log(new Error(new Exception("Timesheet cannot be updated! Object: " + timesheetRecordUpdate + " ModelState: " + modelStateTimesheet.ToJsonString())));
                                                return;
                                            }

                                            try
                                            {
                                                var resultUpdate = await recordRepository.Update(timesheetRecordUpdate, timesheetModule);

                                                if (resultUpdate < 1)
                                                {
                                                    ErrorLog.GetDefault(null).Log(new Error(new Exception("Timesheet cannot be updated! Object: " + timesheetRecordUpdate)));
                                                    return;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                ErrorLog.GetDefault(null).Log(new Error(ex));
                                                return;
                                            }

                                            using (var userRepository = new UserRepository(databaseContext))
                                            {
                                                var timesheetOwner = await userRepository.GetById((int)record["owner"]);


                                                    var timesheetInfo = timesheetRecord["year"] + "-" + timesheetRecord["term"];
                                                    var timesheetMonth = int.Parse(timesheetRecord["term"].ToString()) - 1;
                                                    var body = "<!DOCTYPE html> <html> <head> <meta name=\"viewport\" content=\"width=device-width\"> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"> <title>Ofisim.com</title> <style type=\"text/css\"> @media only screen and (max-width: 620px) { table[class= body] h1 { font-size: 28px !important; margin-bottom: 10px !important; } table[class=body] p, table[class=body] ul, table[class=body] ol, table[class=body] td, table[class=body] span, table[class=body] a { font-size: 16px !important; } table[class=body] .wrapper, table[class=body] .article { padding: 10px !important; } table[class=body] .content { padding: 0 !important; } table[class=body] .container { padding: 0 !important; width: 100% !important; } table[class=body] .main { border-left-width: 0 !important; border-radius: 0 !important; border-right-width: 0 !important; } table[class=body] .btn table { width: 100% !important; } table[class=body] .btn a { width: 100% !important; } table[class=body] .img-responsive { height: auto !important; max-width: 100% !important; width: auto !important; }} @media all { .ExternalClass { width: 100%; } .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; } .apple-link a { color: inherit !important; font-family: inherit !important; font-size: inherit !important; font-weight: inherit !important; line-height: inherit !important; text-decoration: none !important; } .btn-primary table td:hover { background-color: #34495e !important; } .btn-primary a:hover { background - color: #34495e !important; border-color: #34495e !important; } } </style> </head> <body class=\"\" style=\"background-color:#f6f6f6;font-family:sans-serif;-webkit-font-smoothing:antialiased;font-size:14px;line-height:1.4;margin:0;padding:0;-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"body\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background-color:#f6f6f6;width:100%;\"> <tr> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;\">&nbsp;</td> <td class=\"container\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;display:block;max-width:580px;padding:10px;width:580px;Margin:0 auto !important;\"> <div class=\"content\" style=\"box-sizing:border-box;display:block;Margin:0 auto;max-width:580px;padding:10px;\"> <!-- START CENTERED WHITE CONTAINER --> <table class=\"main\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background:#fff;border-radius:3px;width:100%;\"> <!-- START MAIN CONTENT AREA --> <tr> <td class=\"wrapper\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;box-sizing:border-box;padding:20px;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;\"> <tr> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;\"> Dear " + timesheetOwner.FullName + ", <br><br>Your timesheet (" + timesheetInfo + ") is approved. <br><br><br><br><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"btn btn-primary\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;box-sizing:border-box;width:100%;\"> <tbody> <tr> <td align=\"left\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;padding-bottom:15px;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;width:auto;\"> <tbody> <tr> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;background-color:#ffffff;border-radius:5px;text-align:center;background-color:#3498db;\"> <a href=\"https://crm.ofisim.com/#/app/crm/timesheet?month=" + timesheetMonth + "\" target=\"_blank\" style=\"text-decoration:underline;background-color:#ffffff;border:solid 1px #3498db;border-radius:5px;box-sizing:border-box;color:#3498db;cursor:pointer;display:inline-block;font-size:14px;font-weight:bold;margin:0;padding:12px 25px;text-decoration:none;background-color:#3498db;border-color:#3498db;color:#ffffff;\">Go to Your Timesheet</a> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table></td> </tr> </table> </td> </tr> <!-- END MAIN CONTENT AREA --> </table> <!-- START FOOTER --> <div class=\"footer\" style=\"clear:both;padding-top:10px;text-align:center;width:100%;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;\"> <tr> <td class=\"content-block\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;color:#999999;font-size:12px;text-align:center;\"> <br><span class=\"apple-link\" style=\"color:#999999;font-size:12px;text-align:center;\">Ofisim.com</span> </td> </tr> </table> </div> <!-- END FOOTER --> <!-- END CENTERED WHITE CONTAINER --> </div> </td> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;\">&nbsp;</td> </tr> </table> </body> </html>";
                                                    var externalEmail = new Email("Timesheet (" + timesheetInfo + ") Approved", body);
                                                    externalEmail.AddRecipient(timesheetOwner.Email);
                                                    externalEmail.AddToQueue();


                                        }


                                            await CalculateTimesheet(timesheetItemsRecords, appUser, module, timesheetModule, recordRepository, moduleRepository, picklistRepository);
                                        }
                                        break;
                                    case "human_resources":
                                        var findRequestIzinlerCalisanPG = new FindRequest
                                        {
                                            Filters = new List<Filter> { new Filter { Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1 } },
                                            Limit = 99999,
                                            Offset = 0
                                        };

                                        var izinlerCalisanPG = recordRepository.Find("izin_turleri", findRequestIzinlerCalisanPG, false).First;
                                        await YillikIzinHesaplama((int)record["id"], (int)izinlerCalisanPG["id"], recordRepository, moduleRepository);
                                        break;
                                    case "calisanlar":

                                        //Yıllık izin ise calculationlar çalıştırılıyor.
                                        var findRequestIzinlerCalisan = new FindRequest
                                        {
                                            Filters = new List<Filter> { new Filter { Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1 } },
                                            Limit = 99999,
                                            Offset = 0
                                        };

                                        var izinlerCalisan = recordRepository.Find("izin_turleri", findRequestIzinlerCalisan, false).First;

                                        await YillikIzinHesaplama((int)record["id"], (int)izinlerCalisan["id"], recordRepository, moduleRepository);

                                        var rehberModule = await moduleRepository.GetByName("rehber");
                                        var recordRehber = new JObject();

                                        if (operationType == OperationType.update || operationType == OperationType.delete)
                                        {
                                            var findRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "calisan_id", Operator = Operator.Equals, Value = (int)record["id"] } } };
                                            var recordsRehber = recordRepository.Find("rehber", findRequest);

                                            if (recordsRehber.IsNullOrEmpty())
                                            {
                                                if (operationType == OperationType.update)
                                                    operationType = OperationType.insert;
                                                else
                                                    return;
                                            }
                                            else
                                            {
                                                recordRehber = (JObject)recordsRehber[0];
                                            }
                                        }

                                        if (operationType != OperationType.delete)
                                        {
                                            recordRehber["owner"] = record["owner"];
                                            recordRehber["ad"] = record["ad"];
                                            recordRehber["soyad"] = record["soyad"];
                                            recordRehber["ad_soyad"] = record["ad_soyad"];
                                            recordRehber["cep_telefonu"] = record["cep_telefonu"];
                                            recordRehber["e_posta"] = record["e_posta"];
                                            recordRehber["lokasyon"] = record["lokasyon"];
                                            recordRehber["sube"] = record["sube"];
                                            recordRehber["fotograf"] = record["fotograf"];
                                            recordRehber["departman"] = record["departman"];
                                            recordRehber["unvan"] = record["unvan"];

                                            if (!record["is_telefonu"].IsNullOrEmpty())
                                                recordRehber["is_telefonu"] = record["is_telefonu"];

                                            if (!record["ozel_cep_telefonu"].IsNullOrEmpty())
                                                recordRehber["ozel_cep_telefonu"] = record["ozel_cep_telefonu"];

                                            if (!record["yoneticisi"].IsNullOrEmpty())
                                            {
                                                var findRequestYonetici = new FindRequest { Filters = new List<Filter> { new Filter { Field = "calisan_id", Operator = Operator.Equals, Value = (int)record["yoneticisi"] } } };
                                                var recordsYonetici = recordRepository.Find("rehber", findRequestYonetici);

                                                if (!recordsYonetici.IsNullOrEmpty())
                                                    recordRehber["yoneticisi"] = recordsYonetici[0]["id"];
                                            }

                                            var modelStateRehber = new ModelStateDictionary();
                                            var resultBefore = await RecordHelper.BeforeCreateUpdate(rehberModule, recordRehber, modelStateRehber, appUser.TenantLanguage, moduleRepository, picklistRepository, convertPicklists: false);

                                            if (resultBefore < 0 && !modelStateRehber.IsValid)
                                            {
                                                ErrorLog.GetDefault(null).Log(new Error(new Exception("Rehber cannot be created or updated! Object: " + recordRehber + " ModelState: " + modelStateRehber.ToJsonString())));
                                                return;
                                            }

                                            if (operationType == OperationType.insert && recordRehber["id"].IsNullOrEmpty()) //create
                                            {
                                                recordRehber["calisan_id"] = record["id"];

                                                try
                                                {
                                                    var resultCreate = await recordRepository.Create(recordRehber, rehberModule);

                                                    if (resultCreate < 1)
                                                        ErrorLog.GetDefault(null).Log(new Error(new Exception("Rehber cannot be created! Object: " + recordRehber)));
                                                }
                                                catch (Exception ex)
                                                {
                                                    ErrorLog.GetDefault(null).Log(new Error(ex));
                                                }
                                            }
                                            else //update
                                            {
                                                try
                                                {
                                                    var resultUpdate = await recordRepository.Update(recordRehber, rehberModule);

                                                    if (resultUpdate < 1)
                                                        ErrorLog.GetDefault(null).Log(new Error(new Exception("Rehber cannot be updated! Object: " + recordRehber)));
                                                }
                                                catch (Exception ex)
                                                {
                                                    ErrorLog.GetDefault(null).Log(new Error(ex));
                                                }
                                            }
                                        }
                                        else//delete
                                        {
                                            try
                                            {
                                                var resultDelete = await recordRepository.Delete(recordRehber, rehberModule);

                                                if (resultDelete < 1)
                                                    ErrorLog.GetDefault(null).Log(new Error(new Exception("Rehber cannot be deleted! Object: " + recordRehber)));
                                            }
                                            catch (Exception ex)
                                            {
                                                ErrorLog.GetDefault(null).Log(new Error(ex));
                                            }
                                        }
                                        break;
                                    case "izinler":
                                        if (record["calisan"].IsNullOrEmpty())
                                            return;

                                        //Yıllık izin ise calculationlar çalıştırılıyor.
                                        var findRequestIzinler = new FindRequest
                                        {
                                            Filters = new List<Filter> { new Filter { Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1 } },
                                            Limit = 99999,
                                            Offset = 0
                                        };

                                        var izinler = recordRepository.Find("izin_turleri", findRequestIzinler, false).First;

                                        //await YillikIzinHesaplama((int)record["calisan"], izinTuru, recordRepository, moduleRepository);
                                        if (record["process_status"] != null)
                                        {
                                            if ((bool)izinler["yillik_izin"] && operationType == OperationType.update && !record["process_status"].IsNullOrEmpty() && (int)record["process_status"] == 2)
                                                await YillikIzinHesaplama((int)record["calisan"], (int)izinler["id"], recordRepository, moduleRepository);
                                            else if ((bool)izinler["yillik_izin"] && operationType == OperationType.delete && !record["process_status"].IsNullOrEmpty() && (int)record["process_status"] == 2)
                                                await DeleteAnnualLeave((int)record["calisan"], (int)izinler["id"], record, recordRepository, moduleRepository);
                                        }
                                        else
                                        {
                                            if (operationType == OperationType.delete)
                                            {
                                                await DeleteAnnualLeave((int)record["calisan"], (int)izinler["id"], record, recordRepository, moduleRepository);
                                            }
                                            else
                                            {
                                                await YillikIzinHesaplama((int)record["calisan"], (int)izinler["id"], recordRepository, moduleRepository);
                                            }
                                        }
                                        break;
                                    case "masraf_kalemleri":
                                        try
                                        {
                                            var recordUpdate = new JObject();
                                            var moduleUpdate = await moduleRepository.GetByName("masraflar");
                                            var masrafId = (int)record["masraf"];
                                            decimal totalAmount = 0;

                                            var findRequestMasrafKalemi = new FindRequest { Filters = new List<Filter> { new Filter { Field = "masraf", Operator = Operator.Equals, Value = masrafId, No = 1 } }, Limit = 9999 };
                                            var recordsMasrafKalemi = recordRepository.Find(module.Name, findRequestMasrafKalemi);

                                            foreach (JObject recordMasrafKalemi in recordsMasrafKalemi)
                                            {
                                                decimal amount = 0;

                                                if (!recordMasrafKalemi["tutar"].IsNullOrEmpty())
                                                    amount = (decimal)recordMasrafKalemi["tutar"];

                                                totalAmount += amount;
                                            }

                                            recordUpdate["id"] = masrafId;
                                            recordUpdate["toplam_tutar"] = totalAmount;
                                            recordUpdate["updated_by"] = (int)record["updated_by"];

                                            var resultUpdate = await recordRepository.Update(recordUpdate, moduleUpdate);

                                            if (resultUpdate < 1)
                                                ErrorLog.GetDefault(null).Log(new Error(new Exception("toplam_tutar cannot be updated! Object: " + recordUpdate)));
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorLog.GetDefault(null).Log(new Error(ex));
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.GetDefault(null).Log(new Error(ex));
            }
        }

        public static async Task<bool> YillikIzinHesaplama(int userId, int izinTuruId, RecordRepository recordRepository, ModuleRepository moduleRepository)
        {
            var calisanlarModule = await moduleRepository.GetByName("calisanlar");
            if (calisanlarModule == null)
            {
                calisanlarModule = await moduleRepository.GetByName("human_resources");
                if (calisanlarModule == null)
                    return false;
            }

            var calisanId = userId;

            var calisan = recordRepository.GetById(calisanlarModule, calisanId, false);

            if (calisan == null)
                return false;

            var izinTurleri = await moduleRepository.GetByName("izin_turleri");
            var iseBaslamaTarihi = (string)calisan["ise_baslama_tarihi"];

            var bugun = DateTime.UtcNow;

            if (string.IsNullOrEmpty(iseBaslamaTarihi))
                return false;

            var calismayaBasladigiZaman = DateTime.ParseExact(iseBaslamaTarihi, "MM/dd/yyyy h:mm:ss", null);

            var dayDiff = bugun - calismayaBasladigiZaman;
            var dayDiffMonth = ((bugun.Year - calismayaBasladigiZaman.Year) * 12) + bugun.Month - calismayaBasladigiZaman.Month;
            var dayDiffYear = dayDiff.Days / 365;

            var izinKurali = recordRepository.GetById(izinTurleri, izinTuruId, false);

            var ekIzin = 0.0;
            var toplamKalanIzin = 0.0;
            var hakedilenIzin = 0.0;

            var yasaGoreIzin = 0.0;
            var kidemeGoreIzin = 0.0;
            var kullanilanYillikIzin = 0.0;
            var devredenIzin = 0.0;

            if (!calisan["sabit_devreden_izin"].IsNullOrEmpty())
            {
                devredenIzin = (double)calisan["sabit_devreden_izin"];
            }

            if (dayDiffYear > 0)
            {
                #region Yıllık izine ek izin süresi tanımlanmışsa tanımlanan ek izin süresini çalışanın profiline tanımlıyoruz.
                if ((bool)izinKurali["yillik_izine_ek_izin_suresi_ekle"])
                    ekIzin = (double)izinKurali["yillik_izine_ek_izin_suresi_gun"];
                #endregion

                #region Yaşa göre asgari izin kuralı.
                if ((double)izinKurali["yasa_gore_asgari_izin_gun"] != 0)
                {
                    var dogumYili = (string)calisan["dogum_tarihi"];
                    if (!String.IsNullOrEmpty(dogumYili))
                    {
                        var calisanYasi = (bugun - DateTime.ParseExact(dogumYili, "MM/dd/yyyy h:mm:ss", null)).Days / 365;
                        if (calisanYasi < 18 || calisanYasi > 50)
                        {
                            yasaGoreIzin = (double)izinKurali["yasa_gore_asgari_izin_gun"];
                        }
                    }
                }
                #endregion

                #region Çalışanın çalıştığı yıl hesaplanarak kıdem izinleri hesaplanıyor.
                var findRequestKidemeGoreIzinler = new FindRequest
                {
                    Filters = new List<Filter>
                    {
                        new Filter { Field = "turu", Operator = Operator.Equals, Value = 1, No = 1 },
                        new Filter { Field = "calisilan_yil", Operator = Operator.LessEqual, Value = dayDiffYear}
                    },
                    Limit = 9999,
                    SortDirection = SortDirection.Desc
                };

                var kidemIzni = recordRepository.Find("kideme_gore_yillik_izin_artislari", findRequestKidemeGoreIzinler, false).First;


                if (kidemIzni != null)
                    kidemeGoreIzin = (double)kidemIzni["ek_yillik_izin"];
                #endregion

                #region Kıdem ve yaş izinleri kıyaslanarak uygun olan setleniyor.

                if (kidemeGoreIzin + (double)izinKurali["yillik_izin_hakki_gun"] > yasaGoreIzin)
                    hakedilenIzin = kidemeGoreIzin + (double)izinKurali["yillik_izin_hakki_gun"];
                else
                    hakedilenIzin = yasaGoreIzin + (double)izinKurali["yillik_izin_hakki_gun"];
                #endregion
            }

            /*#region İlk izin kullanımı hakediş zamanı çalışanın işe giriş tarihinden büyük ise kullanıcının izin hakları sıfır olarak kaydediliyor.
			if ((double)izinKurali["ilk_izin_kullanimi_hakedis_zamani_ay"] != 0 && dayDiffMonth < (int)izinKurali["ilk_izin_kullanimi_hakedis_zamani_ay"])
                return false;
            #endregion*/

            #region Bu yıl kullandığı toplam izinler hesaplanıyor.
            var totalUsed = 0.0;

            var year = DateTime.UtcNow.Year;
            var ts = new TimeSpan(0, 0, 0);
            calismayaBasladigiZaman = calismayaBasladigiZaman.Date + ts;

            if (new DateTime(DateTime.UtcNow.Year, calismayaBasladigiZaman.Month, calismayaBasladigiZaman.Day, 0, 0, 0) > DateTime.UtcNow)
                year--;

            var findRequestIzinler = new FindRequest
            {
                Fields = new List<string> { "hesaplanan_alinacak_toplam_izin", "process.process_requests.process_status" },
                Filters = new List<Filter>
                {
                    new Filter { Field = "calisan", Operator = Operator.Equals, Value = calisanId, No = 1 },
                    new Filter { Field = "baslangic_tarihi", Operator = Operator.GreaterEqual, Value = new DateTime(year, calismayaBasladigiZaman.Month, calismayaBasladigiZaman.Day, 0, 0, 0).ToString("yyyy-MM-dd h:mm:ss"), No = 2 },
                    new Filter { Field = "izin_turu", Operator = Operator.Equals, Value = izinTuruId, No = 3 },
                    new Filter { Field = "deleted", Operator = Operator.Equals, Value = false, No = 4 },
                    new Filter { Field = "process.process_requests.process_status", Operator = Operator.Equals, Value = 2, No = 5 }
                },
                Limit = 9999
            };

            var izinlerRecords = recordRepository.Find("izinler", findRequestIzinler, false);

            foreach (var izinlerRecord in izinlerRecords)
            {
                if (!izinlerRecord["hesaplanan_alinacak_toplam_izin"].IsNullOrEmpty())
                    totalUsed += (double)izinlerRecord["hesaplanan_alinacak_toplam_izin"];
            }

            kullanilanYillikIzin = totalUsed;
            #endregion

            #region Kullanılan izinler öncelikle devreden düşülüyor sonra kalan izninden düşülüyor.

            hakedilenIzin = hakedilenIzin + ekIzin;

            var devredenCounter = devredenIzin - kullanilanYillikIzin;

            devredenIzin = devredenCounter;

            if (devredenCounter < 0)
            {
                devredenIzin = 0;
                toplamKalanIzin = hakedilenIzin + devredenCounter;
            }
            else
            {
                toplamKalanIzin = hakedilenIzin + devredenIzin;
            }

            #endregion

            //var calisanlarModule = await moduleRepository.GetByName("calisanlar");
            try
            {
                var accountRecordUpdate = new JObject();
                accountRecordUpdate["id"] = calisanId;
                accountRecordUpdate["hakedilen_izin"] = hakedilenIzin;
                accountRecordUpdate["devreden_izin"] = devredenIzin;
                accountRecordUpdate["kalan_izin_hakki"] = toplamKalanIzin;


                accountRecordUpdate["updated_by"] = (int)calisan["updated_by"];
                var resultUpdate = await recordRepository.Update(accountRecordUpdate, calisanlarModule);

                if (resultUpdate < 1)
                {
                    ErrorLog.GetDefault(null).Log(new Error(new Exception("Account (IK) cannot be updated! Object: " + accountRecordUpdate)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.GetDefault(null).Log(new Error(ex));
                return false;
            }
            return true;
        }

        public static async Task<bool> DeleteAnnualLeave(int userId, int izinTuruId, JObject record, RecordRepository recordRepository, ModuleRepository moduleRepository)
        {
            var calisanlarModule = await moduleRepository.GetByName("calisanlar");
            if (calisanlarModule == null)
            {
                calisanlarModule = await moduleRepository.GetByName("human_resources");
                if (calisanlarModule == null)
                    return false;
            }

            var calisanId = userId;
            var calisan = recordRepository.GetById(calisanlarModule, calisanId, false);

            if (calisan == null)
                return false;

            try
            {
                var accountRecordUpdate = new JObject();
                accountRecordUpdate["id"] = calisanId;
                accountRecordUpdate["kalan_izin_hakki"] = (double)calisan["kalan_izin_hakki"] + (double)record["hesaplanan_alinacak_toplam_izin"];


                accountRecordUpdate["updated_by"] = (int)calisan["updated_by"];
                var resultUpdate = await recordRepository.Update(accountRecordUpdate, calisanlarModule);

                if (resultUpdate < 1)
                {
                    ErrorLog.GetDefault(null).Log(new Error(new Exception("Account (IK) cannot be updated! Object: " + accountRecordUpdate)));
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog.GetDefault(null).Log(new Error(ex));
                return false;
            }
        }

        private static async Task<bool> CalculateTimesheet(JArray timesheetItemsRecords, UserItem appUser, Module timesheetItemModule, Module timesheetModule, RecordRepository recordRepository, ModuleRepository moduleRepository, PicklistRepository picklistRepository)
        {
            var entryTypeField = timesheetItemModule.Fields.Single(x => x.Name == "entry_type");
            var entryTypePicklist = await picklistRepository.GetById(entryTypeField.PicklistId.Value);
            var chargeTypeField = timesheetItemModule.Fields.Single(x => x.Name == "charge_type");
            var chargeTypePicklist = await picklistRepository.GetById(chargeTypeField.PicklistId.Value);
            var placeOfPerformanceField = timesheetItemModule.Fields.Single(x => x.Name == "place_of_performance");
            var placeOfPerformancePicklist = await picklistRepository.GetById(placeOfPerformanceField.PicklistId.Value);
            var timesheetId = 0;
            var timesheetOwnerEmail = "";
            var totalDaysWorked = 0.0;
            var totalDaysWorkedTurkey = 0.0;
            var totalDaysWorkedNonTurkey = 0.0;
            var totalDaysWorkedPerDiem = 0.0;
            var projectTotals = new Dictionary<int, double>();

            //Calculate timesheet total days
            foreach (var timesheetItemRecord in timesheetItemsRecords)
            {
                timesheetId = (int)timesheetItemRecord["related_timesheet.timesheet.timesheet_id"];
                timesheetOwnerEmail = (string)timesheetItemRecord["owner.users.email"];

                if (timesheetItemRecord["entry_type"].IsNullOrEmpty())
                    continue;

                var entryTypePicklistItem = entryTypePicklist.Items.Single(x => x.LabelEn == timesheetItemRecord["entry_type"].ToString());

                if (entryTypePicklistItem == null)
                    continue;

                var value = double.Parse(entryTypePicklistItem.Value);

                if (entryTypePicklistItem.SystemCode != "per_diem_only")
                {
                    totalDaysWorked += value;

                    if (!timesheetItemRecord["place_of_performance"].IsNullOrEmpty())
                    {
                        var placeOfPerformancePicklistItem = placeOfPerformancePicklist.Items.Single(x => x.LabelEn == timesheetItemRecord["place_of_performance"].ToString());

                        if (placeOfPerformancePicklistItem.SystemCode == "other")
                            totalDaysWorkedNonTurkey += value;
                        else
                            totalDaysWorkedTurkey += value;
                    }

                    if (!timesheetItemRecord["per_diem"].IsNullOrEmpty() && bool.Parse(timesheetItemRecord["per_diem"].ToString()))
                        totalDaysWorkedPerDiem += value;
                }
                else
                {
                    totalDaysWorkedPerDiem += value;
                }

                var chargeTypePicklistItem = chargeTypePicklist.Items.Single(x => x.LabelEn == timesheetItemRecord["charge_type"].ToString());

                if (chargeTypePicklistItem == null)
                    continue;

                if (chargeTypePicklistItem.Value == "billable")
                {
                    var projectId = (int)timesheetItemRecord["selected_project.projects.id"];

                    if (projectTotals.ContainsKey(projectId))
                        projectTotals[projectId] += value;
                    else
                        projectTotals.Add(projectId, value);
                }
            }

            //Update timesheet total days
            var timesheetRecordUpdate = new JObject();
            timesheetRecordUpdate["id"] = timesheetId;
            timesheetRecordUpdate["total_days"] = totalDaysWorked;
            timesheetRecordUpdate["total_days_worked_turkey"] = totalDaysWorkedTurkey;
            timesheetRecordUpdate["total_days_worked_non_turkey"] = totalDaysWorkedNonTurkey;
            timesheetRecordUpdate["total_days_perdiem"] = totalDaysWorkedPerDiem;

            try
            {
                var resultUpdate = await recordRepository.Update(timesheetRecordUpdate, timesheetModule);

                if (resultUpdate < 1)
                {
                    ErrorLog.GetDefault(null).Log(new Error(new Exception("Timesheet cannot be updated! Object: " + timesheetRecordUpdate)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.GetDefault(null).Log(new Error(ex));
                return false;
            }

            //Update project teams billable timesheet total days
            var findRequestExpert = new FindRequest { Filters = new List<Filter> { new Filter { Field = "e_mail1", Operator = Operator.Is, Value = timesheetOwnerEmail, No = 1 } } };
            var expertRecords = recordRepository.Find("experts", findRequestExpert, false);

            if (expertRecords.IsNullOrEmpty() || expertRecords.Count < 1)
            {
                ErrorLog.GetDefault(null).Log(new Error(new Exception("Expert not found! FindRequest: " + findRequestExpert.ToJsonString())));
                return false;
            }

            var expertRecord = expertRecords[0];
            var projectTeamModule = await moduleRepository.GetByName("project_team");

            foreach (var projectTotal in projectTotals)
            {
                var findRequestProjectTeam = new FindRequest
                {
                    Filters = new List<Filter>
                    {
                        new Filter { Field = "expert", Operator = Operator.Equals, Value = (int)expertRecord["id"], No = 1 },
                        new Filter { Field = "project", Operator = Operator.Equals, Value = projectTotal.Key, No = 2 }
                    }
                };

                var projectTeamRecords = recordRepository.Find("project_team", findRequestProjectTeam, false);

                if (projectTeamRecords.IsNullOrEmpty() || projectTeamRecords.Count < 1)
                    continue;

                var projectTeamRecordUpdate = new JObject();
                projectTeamRecordUpdate["id"] = (int)projectTeamRecords[0]["id"];
                projectTeamRecordUpdate["timesheet_total_days"] = projectTotal.Value;

                try
                {
                    var resultUpdate = await recordRepository.Update(projectTeamRecordUpdate, projectTeamModule);

                    if (resultUpdate < 1)
                    {
                        ErrorLog.GetDefault(null).Log(new Error(new Exception("ProjectTeam cannot be updated! Object: " + projectTeamRecordUpdate)));
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog.GetDefault(null).Log(new Error(ex));
                    return false;
                }
            }

            return true;
        }
    }
}