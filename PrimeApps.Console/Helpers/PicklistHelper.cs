using System.Collections.Generic;
using System.Linq;
using PrimeApps.Console.Models;
using PrimeApps.Console.Models.ViewModel.Crm.Picklist;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;

namespace PrimeApps.Console.Helpers
{
    public static class PicklistHelper
    {
        public static Picklist CreateEntity(PicklistBindingModel picklistModel)
        {
            var picklist = new Picklist
            {
                SystemType = SystemType.Custom,
                LabelEn = picklistModel.LabelEn,
                LabelTr = picklistModel.LabelTr,
                Items = new List<PicklistItem>()
            };

            foreach (var picklistItemModel in picklistModel.Items)
            {
                var picklistItem = new PicklistItem
                {
                    LabelEn = picklistItemModel.LabelEn,
                    LabelTr = picklistItemModel.LabelTr,
                    Value = picklistItemModel.Value,
                    Value2 = picklistItemModel.Value2,
                    Value3 = picklistItemModel.Value3,
                    Order = picklistItemModel.Order,
                    Inactive = false
                };

                picklist.Items.Add(picklistItem);
            }

            return picklist;
        }

        public static void UpdateEntity(PicklistBindingModel picklistModel, Picklist picklist)
        {
            picklist.LabelEn = picklistModel.LabelEn;
            picklist.LabelTr = picklistModel.LabelTr;

            foreach (var picklistItem in picklist.Items)
            {
                var picklistItemModel = picklistModel.Items.FirstOrDefault(x => x.Id == picklistItem.Id);

                if (picklistItemModel == null)
                    continue;

                picklistItem.LabelEn = picklistItemModel.LabelEn;
                picklistItem.LabelTr = picklistItemModel.LabelTr;
                picklistItem.Value = picklistItemModel.Value;
                picklistItem.Value2 = picklistItemModel.Value2;
                picklistItem.Value3 = picklistItemModel.Value3;
                picklistItem.Order = picklistItemModel.Order;
                picklistItem.Inactive = picklistItemModel.Inactive;
            }

            foreach (var picklistItemModel in picklistModel.Items)
            {
                var picklistItem = picklist.Items.FirstOrDefault(x => x.Id == picklistItemModel.Id);

                if (picklistItem == null)
                {
                    picklistItem = new PicklistItem
                    {
                        LabelEn = picklistItemModel.LabelEn,
                        LabelTr = picklistItemModel.LabelTr,
                        Value = picklistItemModel.Value,
                        Value2 = picklistItemModel.Value2,
                        Value3 = picklistItemModel.Value3,
                        Order = picklistItemModel.Order,
                        Inactive = false
                    };

                    picklist.Items.Add(picklistItem);
                }
            }
        }

        public static PicklistViewModel MapToViewModel(Picklist picklist)
        {
            var picklistViewModel = new PicklistViewModel
            {
                Id = picklist.Id,
                LabelEn = picklist.LabelEn,
                LabelTr = picklist.LabelTr,
                Items = new List<PicklistItemViewModel>()
            };

            foreach (var picklistItem in picklist.Items)
            {
                var picklistItemViewModel = new PicklistItemViewModel
                {
                    Id = picklistItem.Id,
                    LabelEn = picklistItem.LabelEn,
                    LabelTr = picklistItem.LabelTr,
                    Value = picklistItem.Value,
                    Value2 = picklistItem.Value2,
                    Value3 = picklistItem.Value3,
                    SystemCode = picklistItem.SystemCode,
                    Order = picklistItem.Order,
                    Inactive = picklistItem.Inactive
                };

                picklistViewModel.Items.Add(picklistItemViewModel);
            }

            return picklistViewModel;
        }

        public static List<PicklistViewModel> MapToViewModel(ICollection<Picklist> picklists)
        {
            return picklists.Select(MapToViewModel).ToList();
        }
    }
}