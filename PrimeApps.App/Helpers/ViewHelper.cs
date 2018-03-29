using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.App.Models;
using PrimeApps.App.Models.ViewModel.Crm;
using PrimeApps.App.Models.ViewModel.Crm.View;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Helpers
{
    public static class ViewHelper
    {
        public static async Task<View> CreateEntity(ViewBindingModel viewModel, IUserRepository userRepository)
        {
            var view = new View
            {
                ModuleId = viewModel.ModuleId,
                SystemType = SystemType.Custom,
                LabelEn = viewModel.Label,
                LabelTr = viewModel.Label,
                SharingType = viewModel.SharingType != ViewSharingType.NotSet ? viewModel.SharingType : ViewSharingType.Me,
                FilterLogic = viewModel.FilterLogic,
                Fields = new List<ViewField>(),
                Filters = new List<ViewFilter>()
            };

            await CreateViewRelations(viewModel, view, userRepository);

            return view;
        }

        public static async Task UpdateEntity(ViewBindingModel viewModel, View view, IUserRepository userRepository)
        {
            view.LabelEn = viewModel.Label;
            view.LabelTr = viewModel.Label;
            view.SharingType = viewModel.SharingType != ViewSharingType.NotSet ? viewModel.SharingType : ViewSharingType.Me;
            view.FilterLogic = viewModel.FilterLogic;

            await CreateViewRelations(viewModel, view, userRepository);
        }

        public static ViewViewModel MapToViewModel(View view)
        {
            var viewViewModel = new ViewViewModel
            {
                Id = view.Id,
                ModuleId = view.ModuleId,
                SystemType = view.SystemType,
                LabelEn = view.LabelEn,
                LabelTr = view.LabelTr,
                SharingType = view.SharingType,
                Active = view.Active,
                FilterLogic = view.FilterLogic,
                CreatedBy = view.CreatedById,
                Fields = new List<ViewFieldViewModel>(),
                Filters = new List<ViewFilterViewModel>()
            };

            foreach (var viewField in view.Fields)
            {
                var viewFieldViewModel = new ViewFieldViewModel
                {
                    Field = viewField.Field,
                    Order = viewField.Order
                };

                viewViewModel.Fields.Add(viewFieldViewModel);
            }

            foreach (var viewFilter in view.Filters)
            {
                var viewFilterViewModel = new ViewFilterViewModel
                {
                    Field = viewFilter.Field,
                    Operator = viewFilter.Operator,
                    Value = viewFilter.Value,
                    No = viewFilter.No
                };

                viewViewModel.Filters.Add(viewFilterViewModel);
            }

            if (view.Shares != null && view.Shares.Count > 0)
            {
                viewViewModel.Shares = new List<UserBasicViewModel>();

                foreach (var user in view.Shares)
                {
                    viewViewModel.Shares.Add(new UserBasicViewModel { Id = user.UserId, FullName = user.User.FullName });
                }
            }

            return viewViewModel;
        }

        public static List<ViewViewModel> MapToViewModel(ICollection<View> views)
        {
            return views.Select(MapToViewModel).ToList();
        }

        public static ViewState CreateEntityViewState(ViewStateBindingModel viewStateModel, int userId)
        {
            var viewState = new ViewState
            {
                ModuleId = viewStateModel.ModuleId,
                UserId = userId,
                ActiveView = viewStateModel.ActiveView,
                SortField = viewStateModel.SortField,
                SortDirection = viewStateModel.SortDirection,
                RowPerPage = viewStateModel.RowPerPage
            };

            return viewState;
        }

        public static void UpdateEntityViewState(ViewStateBindingModel viewStateModel, ViewState viewState)
        {
            viewState.ActiveView = viewStateModel.ActiveView;
            viewState.SortField = viewStateModel.SortField;
            viewState.SortDirection = viewStateModel.SortDirection;
            viewState.RowPerPage = viewStateModel.RowPerPage;
        }

        public static async Task<View> CreateDefaultViewAllRecords(Module module, IModuleRepository moduleRepository)
        {
            var view = new View
            {
                ModuleId = module.Id,
                SystemType = SystemType.System,
                LabelEn = "All " + module.LabelEnPlural,
                LabelTr = "Tüm " + module.LabelTrPlural,
                Active = true,
                SharingType = ViewSharingType.Everybody,
                Fields = new List<ViewField>()
            };

            var primaryField = module.Fields.Single(x => x.Primary);
            var viewFieldPrimary = new ViewField { Field = primaryField.Name, Order = 1 };

            view.Fields.Add(viewFieldPrimary);

            var i = 2;

            foreach (var field in module.Fields)
            {
                if (i > 3 || field.Name == primaryField.Name || field.Name == "created_at" || field.Name == "owner")
                    continue;

                var viewField = new ViewField { Field = field.Name, Order = i };

                view.Fields.Add(viewField);

                if (field.DataType == DataType.Lookup)
                {
                    if (field.LookupType != "users")
                    {
                        var lookupModule = await moduleRepository.GetByName(field.LookupType);
                        var lookupModulePrimaryField = lookupModule.Fields.Single(x => x.Primary);
                        var viewFieldLookupPrimary = new ViewField { Field = field.Name + "." + field.LookupType + "." + lookupModulePrimaryField.Name + ".primary", Order = i };
                        view.Fields.Add(viewFieldLookupPrimary);
                    }
                    else
                    {
                        var viewFieldUserPrimary = new ViewField { Field = field.Name + ".users.full_name.primary", Order = i };
                        view.Fields.Add(viewFieldUserPrimary);
                    }
                }

                i++;
            }

            var viewFieldOwner = new ViewField { Field = "owner", Order = 4 };
            var viewFieldOwnerFullName = new ViewField { Field = "owner.users.full_name.primary", Order = 4 };
            var viewFieldCretedAt = new ViewField { Field = "created_at", Order = 5 };

            view.Fields.Add(viewFieldOwner);
            view.Fields.Add(viewFieldOwnerFullName);
            view.Fields.Add(viewFieldCretedAt);

            return view;
        }

        public static View CreateDefaultViewMyRecords(Module module)
        {
            var view = new View
            {
                ModuleId = module.Id,
                SystemType = SystemType.System,
                LabelEn = "My " + module.LabelEnPlural,
                LabelTr = "Bana Ait " + module.LabelTrPlural,
                SharingType = ViewSharingType.Everybody,
                Fields = new List<ViewField>(),
                Filters = new List<ViewFilter>()
            };

            var primaryField = module.Fields.Single(x => x.Primary);
            var viewFieldPrimary = new ViewField { Field = primaryField.Name, Order = 1 };

            view.Fields.Add(viewFieldPrimary);

            var i = 2;

            foreach (var field in module.Fields)
            {
                if (i > 3 || field.Name == primaryField.Name || field.Name == "created_at" || field.Name == "owner")
                    continue;

                var viewField = new ViewField { Field = field.Name, Order = i };

                view.Fields.Add(viewField);

                i++;
            }

            var viewFieldCretedAt = new ViewField { Field = "created_at", Order = 4 };

            view.Fields.Add(viewFieldCretedAt);

            var viewFilterOwnerMe = new ViewFilter { Field = "owner", Operator = Operator.Equals, Value = "[me]", No = 1 };
            view.Filters.Add(viewFilterOwnerMe);

            return view;
        }


        public static View CreateViewPendingProcessRecords(Module module)
        {
            var view = new View
            {
                ModuleId = module.Id,
                SystemType = SystemType.System,
                LabelEn = "Pending " + module.LabelEnPlural,
                LabelTr = "Onay Bekleyen " + module.LabelTrPlural,
                SharingType = ViewSharingType.Everybody,
                Fields = new List<ViewField>(),
                Filters = new List<ViewFilter>()
            };

            var primaryField = module.Fields.Single(x => x.Primary && !x.Deleted);
            var viewFieldPrimary = new ViewField { Field = primaryField.Name, Order = 1 };

            view.Fields.Add(viewFieldPrimary);

            var i = 2;

            foreach (var field in module.Fields)
            {
                if (i > 3 || field.Name == primaryField.Name || field.Name == "created_at" || field.Name == "owner")
                    continue;

                var viewField = new ViewField { Field = field.Name, Order = i };

                view.Fields.Add(viewField);

                i++;
            }

            var viewFieldCretedAt = new ViewField { Field = "created_at", Order = 4 };

            view.Fields.Add(viewFieldCretedAt);

            var viewFilterOwnerMe = new ViewFilter { Field = "process.process_requests.process_status", Operator = Operator.Equals, Value = "1", No = 1 };
            view.Filters.Add(viewFilterOwnerMe);

            return view;
        }

        public static View CreateViewApprovedProcessRecords(Module module)
        {
            var view = new View
            {
                ModuleId = module.Id,
                SystemType = SystemType.System,
                LabelEn = "Approved " + module.LabelEnPlural,
                LabelTr = "Onaylanan " + module.LabelTrPlural,
                SharingType = ViewSharingType.Everybody,
                Fields = new List<ViewField>(),
                Filters = new List<ViewFilter>()
            };

            var primaryField = module.Fields.Single(x => x.Primary && !x.Deleted);
            var viewFieldPrimary = new ViewField { Field = primaryField.Name, Order = 1 };

            view.Fields.Add(viewFieldPrimary);

            var i = 2;

            foreach (var field in module.Fields)
            {
                if (i > 3 || field.Name == primaryField.Name || field.Name == "created_at" || field.Name == "owner")
                    continue;

                var viewField = new ViewField { Field = field.Name, Order = i };

                view.Fields.Add(viewField);

                i++;
            }

            var viewFieldCretedAt = new ViewField { Field = "created_at", Order = 4 };

            view.Fields.Add(viewFieldCretedAt);

            var viewFilterOwnerMe = new ViewFilter { Field = "process.process_requests.process_status", Operator = Operator.Equals, Value = "2", No = 1 };
            view.Filters.Add(viewFilterOwnerMe);

            return view;
        }

        public static View CreateViewRejectedProcessRecords(Module module)
        {
            var view = new View
            {
                ModuleId = module.Id,
                SystemType = SystemType.System,
                LabelEn = "Rejected " + module.LabelEnPlural,
                LabelTr = "Reddedilen " + module.LabelTrPlural,
                SharingType = ViewSharingType.Everybody,
                Fields = new List<ViewField>(),
                Filters = new List<ViewFilter>()
            };

            var primaryField = module.Fields.Single(x => x.Primary && !x.Deleted);
            var viewFieldPrimary = new ViewField { Field = primaryField.Name, Order = 1 };

            view.Fields.Add(viewFieldPrimary);

            var i = 2;

            foreach (var field in module.Fields)
            {
                if (i > 3 || field.Name == primaryField.Name || field.Name == "created_at" || field.Name == "owner")
                    continue;

                var viewField = new ViewField { Field = field.Name, Order = i };

                view.Fields.Add(viewField);

                i++;
            }

            var viewFieldCretedAt = new ViewField { Field = "created_at", Order = 4 };

            view.Fields.Add(viewFieldCretedAt);

            var viewFilterOwnerMe = new ViewFilter { Field = "process.process_requests.process_status", Operator = Operator.Equals, Value = "3", No = 1 };
            view.Filters.Add(viewFilterOwnerMe);

            return view;
        }

        public static View CreateViewPendingFromMeProcessRecords(Module module, Process process)
        {
            var view = new View
            {
                ModuleId = module.Id,
                SystemType = SystemType.System,
                LabelEn = "Pending " + module.LabelEnPlural + " From Me",
                LabelTr = "Benden Onay Bekleyen " + module.LabelTrPlural,
                SharingType = ViewSharingType.Everybody,
                Fields = new List<ViewField>(),
                Filters = new List<ViewFilter>()
            };

            var primaryField = module.Fields.Single(x => x.Primary && !x.Deleted);
            var viewFieldPrimary = new ViewField { Field = primaryField.Name, Order = 1 };

            view.Fields.Add(viewFieldPrimary);

            var i = 2;

            foreach (var field in module.Fields)
            {
                if (i > 3 || field.Name == primaryField.Name || field.Name == "created_at" || field.Name == "owner")
                    continue;

                var viewField = new ViewField { Field = field.Name, Order = i };

                view.Fields.Add(viewField);

                i++;
            }

            var viewFieldCretedAt = new ViewField { Field = "created_at", Order = 4 };

            view.Fields.Add(viewFieldCretedAt);

            var viewFilterProcessStatus = new ViewFilter { Field = "process.process_requests.process_status", Operator = Operator.Equals, Value = "1", No = 1 };
            view.Filters.Add(viewFilterProcessStatus);

            if (!string.IsNullOrEmpty(process.ApproverField))
            {
                var viewFilterProcessApproverCustom = new ViewFilter { Field = "custom_approver", Operator = Operator.Equals, Value = "[me.email]", No = 2 };
                view.Filters.Add(viewFilterProcessApproverCustom);

                var viewFilterProcessStatusOrder = new ViewFilter { Field = "process.process_requests.process_status_order", Operator = Operator.Equals, Value = "1", No = 3 };
                view.Filters.Add(viewFilterProcessStatusOrder);

                var viewFilterProcessApproverCustom2 = new ViewFilter { Field = "custom_approver_2", Operator = Operator.Equals, Value = "[me.email]", No = 4 };
                view.Filters.Add(viewFilterProcessApproverCustom2);

                var viewFilterProcessStatusOrder2 = new ViewFilter { Field = "process.process_requests.process_status_order", Operator = Operator.Equals, Value = "2", No = 5 };
                view.Filters.Add(viewFilterProcessStatusOrder2);

                view.FilterLogic = "(1 and ((2 and 3) or (4 and 5)))";
            }
            else
            {
                var viewFilterProcessApprover = new ViewFilter { Field = "process.process_approvers.user_id", Operator = Operator.Equals, Value = "[me]", No = 2 };
                view.Filters.Add(viewFilterProcessApprover);
            }

            return view;
        }

        private static async Task CreateViewRelations(ViewBindingModel viewModel, View view, IUserRepository userRepository)
        {
            foreach (var viewFieldModel in viewModel.Fields)
            {
                var viewField = new ViewField
                {
                    Field = viewFieldModel.Field,
                    Order = viewFieldModel.Order
                };

                view.Fields.Add(viewField);
            }

            foreach (var viewFilterModel in viewModel.Filters)
            {
                var viewFilter = new ViewFilter
                {
                    Field = viewFilterModel.Field,
                    Operator = viewFilterModel.Operator,
                    Value = viewFilterModel.Value.ToString(),
                    No = viewFilterModel.No
                };

                view.Filters.Add(viewFilter);
            }

            if (viewModel.Shares != null && viewModel.Shares.Count > 0)
            {
                view.Shares = new List<TenantUser>();

                foreach (var userId in viewModel.Shares)
                {
                    var sharedUser = await userRepository.GetById(userId);

                    if (sharedUser != null)
                        view.Shares.Add(sharedUser);
                }
            }
        }
    }
}