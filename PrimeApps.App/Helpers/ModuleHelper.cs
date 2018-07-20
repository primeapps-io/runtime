using System.Collections.Generic;
using System.Linq;
using PrimeApps.App.Models;
using PrimeApps.App.Services;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;

namespace PrimeApps.App.Helpers
{
    public interface IModuleHelper
	{
		Module CreateEntity(ModuleBindingModel moduleModel);
		ModuleChanges UpdateEntity(ModuleBindingModel moduleModel, Module moduleEntity);
		List<ViewField> DeleteViewField(ICollection<View> views, int id, List<FieldBindingModel> fields);
		Module RevertEntity(ModuleChanges moduleChanges, Module moduleEntity);
		void AfterCreate(UserItem appUser, Module module);
		void AfterUpdate(UserItem appUser, Module module);
		void AfterDelete(UserItem appUser, Module module);
		Relation CreateRelationEntity(RelationBindingModel relationModel, Module moduleEntity);
		void UpdateRelationEntity(RelationBindingModel relationModel, Relation relationEntity, Module moduleEntity);
		Dependency CreateDependencyEntity(DependencyBindingModel dependencyModel, Module moduleEntity);
		void UpdateDependencyEntity(DependencyBindingModel dependencyModel, Dependency dependencyEntity, Module moduleEntity);
		Section NewSectionEntity(SectionBindingModel sectionModel);
		Field NewFieldEntity(FieldBindingModel fieldModel);
		FieldValidation NewFieldValidationEntity(FieldBindingModel fieldModel);
		FieldCombination NewFieldCombinationEntity(FieldBindingModel fieldModel);
		Relation NewRelationEntity(RelationBindingModel relationModel);
		Dependency NewDependencyEntity(DependencyBindingModel dependencyModel);
		Calculation NewCalculationEntity(CalculationBindingModel calculationModel);
	}
    public class ModuleHelper : IModuleHelper
    {
	    private IAuditLogHelper _auditLogHelper;
	    public IBackgroundTaskQueue Queue { get; }

		public ModuleHelper(IAuditLogHelper auditLogHelper, IBackgroundTaskQueue queue)
	    {
		    _auditLogHelper = auditLogHelper;
		    Queue = queue;
	    }
        public Module CreateEntity(ModuleBindingModel moduleModel)
        {
            var moduleEntity = new Module
            {
                Name = moduleModel.Name,
                SystemType = SystemType.Custom,
                LabelEnSingular = moduleModel.LabelEnSingular,
                LabelEnPlural = moduleModel.LabelEnPlural,
                LabelTrSingular = moduleModel.LabelTrSingular,
                LabelTrPlural = moduleModel.LabelTrPlural,
                Order = moduleModel.Order,
                Display = moduleModel.Display,
                Sharing = moduleModel.Sharing,
                LocationEnabled = moduleModel.LocationEnabled,
                DisplayCalendar = moduleModel.DisplayCalendar,
                CalendarColorDark = moduleModel.CalendarColorDark,
                CalendarColorLight = moduleModel.CalendarColorLight,
                MenuIcon = moduleModel.MenuIcon,
                DetailViewType = moduleModel.DetailViewType,
                Sections = new List<Section>(),
                Fields = new List<Field>()
            };

            foreach (var sectionModel in moduleModel.Sections)
            {
                var sectionEntity = NewSectionEntity(sectionModel);

                if (sectionModel.Permissions != null && sectionModel.Permissions.Count > 0)
                {
                    sectionEntity.Permissions = new List<SectionPermission>();

                    foreach (var permissionModel in sectionModel.Permissions)
                    {
                        var permissionEntity = new SectionPermission
                        {
                            ProfileId = permissionModel.ProfileId,
                            Type = permissionModel.Type
                        };

                        sectionEntity.Permissions.Add(permissionEntity);
                    }
                }

                moduleEntity.Sections.Add(sectionEntity);
            }

            foreach (var fieldModel in moduleModel.Fields)
            {
                var fieldEntity = NewFieldEntity(fieldModel);

                if (fieldModel.Validation != null)
                {
                    var fieldValidationEntity = NewFieldValidationEntity(fieldModel);
                    fieldEntity.Validation = fieldValidationEntity;
                }

                if (fieldModel.Combination != null)
                {
                    var fieldCombinationEntity = NewFieldCombinationEntity(fieldModel);
                    fieldEntity.Combination = fieldCombinationEntity;
                }

                if (fieldModel.Permissions != null && fieldModel.Permissions.Count > 0)
                {
                    fieldEntity.Permissions = new List<FieldPermission>();

                    foreach (var permissionModel in fieldModel.Permissions)
                    {
                        var permissionEntity = new FieldPermission
                        {
                            ProfileId = permissionModel.ProfileId,
                            Type = permissionModel.Type
                        };

                        fieldEntity.Permissions.Add(permissionEntity);
                    }
                }

                moduleEntity.Fields.Add(fieldEntity);

                if (fieldModel.Encrypted)
                {
                    var encryptedField = new FieldBindingModel
                    {
                        Name = fieldModel.Name + "__encrypted",
                        LabelEn = fieldModel.Name + "__encrypted",
                        LabelTr = fieldModel.Name + "__encrypted",
                        DataType = DataType.TextSingle,
                        DisplayDetail = false,
                        DisplayForm = false,
                        DisplayList = false
                    };

                    var encryptedFieldEntity = NewFieldEntity(encryptedField);
                    moduleEntity.Fields.Add(encryptedFieldEntity);
                }
            }

            if (moduleModel.Relations != null && moduleModel.Relations.Count > 0)
            {
                moduleEntity.Relations = new List<Relation>();

                foreach (var relationModel in moduleModel.Relations)
                {
                    var relationEntity = NewRelationEntity(relationModel);
                    moduleEntity.Relations.Add(relationEntity);
                }
            }

            if (moduleModel.Dependencies != null && moduleModel.Dependencies.Count > 0)
            {
                moduleEntity.Dependencies = new List<Dependency>();

                foreach (var dependencyModel in moduleModel.Dependencies)
                {
                    var dependencyEntity = NewDependencyEntity(dependencyModel);
                    moduleEntity.Dependencies.Add(dependencyEntity);
                }
            }

            if (moduleModel.Calculations != null && moduleModel.Calculations.Count > 0)
            {
                moduleEntity.Calculations = new List<Calculation>();

                foreach (var calculationModel in moduleModel.Calculations)
                {
                    var calculationEntity = NewCalculationEntity(calculationModel);
                    moduleEntity.Calculations.Add(calculationEntity);
                }
            }

            if (moduleEntity.Name == "mails")
                moduleEntity.SystemType = SystemType.System;

            return moduleEntity;
        }

        public ModuleChanges UpdateEntity(ModuleBindingModel moduleModel, Module moduleEntity)
        {
            moduleEntity.Name = moduleModel.Name;
            moduleEntity.LabelEnSingular = moduleModel.LabelEnSingular;
            moduleEntity.LabelEnPlural = moduleModel.LabelEnPlural;
            moduleEntity.LabelTrSingular = moduleModel.LabelTrSingular;
            moduleEntity.LabelTrPlural = moduleModel.LabelTrPlural;
            moduleEntity.Order = moduleModel.Order;
            moduleEntity.Display = moduleModel.Display;
            moduleEntity.Sharing = moduleModel.Sharing;
            moduleEntity.MenuIcon = moduleModel.MenuIcon;
            moduleEntity.LocationEnabled = moduleModel.LocationEnabled;
            moduleEntity.DisplayCalendar = moduleModel.DisplayCalendar;
            moduleEntity.CalendarColorDark = moduleModel.CalendarColorDark;
            moduleEntity.CalendarColorLight = moduleModel.CalendarColorLight;
            moduleEntity.DetailViewType = moduleModel.DetailViewType;

            var moduleChanges = new ModuleChanges();

            if (moduleModel.Sections != null && moduleModel.Sections.Count > 0)
            {
                //New Sections
                foreach (var sectionModel in moduleModel.Sections)
                {
                    if (!sectionModel.Id.HasValue)
                    {
                        var sectionEntity = NewSectionEntity(sectionModel);
                        moduleEntity.Sections.Add(sectionEntity);

                        moduleChanges.SectionsAdded.Add(sectionEntity);
                    }
                }

                //Existing Sections
                foreach (var sectionEntity in moduleEntity.Sections)
                {
                    var sectionModel = moduleModel.Sections.FirstOrDefault(x => x.Id == sectionEntity.Id);
                    var newSection = moduleModel.Sections.FirstOrDefault(x => x.Name == sectionEntity.Name);

                    //Section Deleted
                    if (sectionModel == null && newSection == null && !sectionEntity.Deleted)
                    {
                        sectionEntity.Deleted = true;
                        moduleChanges.SectionsDeleted.Add(sectionEntity);
                        continue;
                    }

                    if (sectionModel == null)
                        continue;

                    sectionEntity.Name = sectionModel.Name;
                    sectionEntity.LabelEn = sectionModel.LabelEn;
                    sectionEntity.LabelTr = sectionModel.LabelTr;
                    sectionEntity.Order = sectionModel.Order;
                    sectionEntity.DisplayForm = sectionModel.DisplayForm;
                    sectionEntity.DisplayDetail = sectionModel.DisplayDetail;
                    sectionEntity.ColumnCount = sectionModel.ColumnCount;
                    sectionEntity.Deleted = sectionModel.Deleted;
                    sectionEntity.CustomLabel = sectionModel.CustomLabel;


                    if (sectionModel.Permissions != null && sectionModel.Permissions.Count > 0)
                    {
                        //New Permissions
                        foreach (var permissionModel in sectionModel.Permissions)
                        {
                            if (!permissionModel.Id.HasValue)
                            {
                                if (sectionEntity.Permissions == null)
                                    sectionEntity.Permissions = new List<SectionPermission>();

                                var permissionEntity = new SectionPermission
                                {
                                    ProfileId = permissionModel.ProfileId,
                                    Type = permissionModel.Type
                                };

                                sectionEntity.Permissions.Add(permissionEntity);
                            }
                        }

                        //Existing Permissions
                        foreach (var permissionEntity in sectionEntity.Permissions)
                        {
                            var permissionModel = sectionModel.Permissions.FirstOrDefault(x => x.Id == permissionEntity.Id);

                            if (permissionModel == null)
                                continue;

                            permissionEntity.Type = permissionModel.Type;
                        }
                    }

                }

            }

            if (moduleModel.Fields != null && moduleModel.Fields.Count > 0)
            {
                //New Fields
                foreach (var fieldModel in moduleModel.Fields)
                {
                    if (!fieldModel.Id.HasValue)
                    {
                        var fieldEntity = NewFieldEntity(fieldModel);

                        if (fieldModel.Validation != null)
                        {
                            var fieldValidationEntity = NewFieldValidationEntity(fieldModel);
                            fieldEntity.Validation = fieldValidationEntity;
                        }

                        if (fieldModel.Combination != null)
                        {
                            var fieldCombinationEntity = NewFieldCombinationEntity(fieldModel);
                            fieldEntity.Combination = fieldCombinationEntity;
                        }

                        moduleEntity.Fields.Add(fieldEntity);
                        moduleChanges.FieldsAdded.Add(fieldEntity);

                        if (fieldModel.Encrypted)
                        {
                            var encryptedField = new FieldBindingModel
                            {
                                Name = fieldModel.Name + "__encrypted",
                                LabelEn = fieldModel.Name + "__encrypted",
                                LabelTr = fieldModel.Name + "__encrypted",
                                DataType = DataType.TextSingle,
                                DisplayDetail = false,
                                DisplayForm = false,
                                DisplayList = false
                            };

                            var encryptedFieldEntity = NewFieldEntity(encryptedField);
                            moduleEntity.Fields.Add(encryptedFieldEntity);
                            moduleChanges.FieldsAdded.Add(encryptedFieldEntity);
                        }
                    }
                }

                //Existing Fields
                foreach (var fieldEntity in moduleEntity.Fields)
                {
                    var fieldModel = moduleModel.Fields.FirstOrDefault(x => x.Id == fieldEntity.Id);

                    if (fieldModel == null)
                        continue;

                    if (fieldModel.Deleted && !fieldEntity.Deleted)
                        moduleChanges.FieldsDeleted.Add(fieldEntity);

                    fieldEntity.Name = fieldModel.Name;
                    fieldEntity.LabelEn = fieldModel.LabelEn;
                    fieldEntity.LabelTr = fieldModel.LabelTr;
                    fieldEntity.Order = fieldModel.Order;
                    fieldEntity.DataType = fieldModel.DataType;
                    fieldEntity.DisplayList = fieldModel.DisplayList;
                    fieldEntity.DisplayForm = fieldModel.DisplayForm;
                    fieldEntity.DisplayDetail = fieldModel.DisplayDetail;
                    fieldEntity.Section = fieldModel.Section;
                    fieldEntity.SectionColumn = fieldModel.SectionColumn;
                    fieldEntity.Primary = fieldModel.Primary;
                    fieldEntity.DefaultValue = fieldModel.DefaultValue;
                    fieldEntity.InlineEdit = fieldModel.InlineEdit;
                    fieldEntity.ShowLabel = fieldModel.ShowLabel;
                    fieldEntity.Editable = fieldModel.Editable;
                    fieldEntity.Encrypted = fieldModel.Encrypted;
                    fieldEntity.EncryptionAuthorizedUsers = fieldModel.EncryptionAuthorizedUsers;
                    fieldEntity.AddressType = fieldModel.AddressType;
                    fieldEntity.MultilineType = fieldModel.MultilineType;
                    fieldEntity.MultilineTypeUseHtml = fieldModel.MultilineTypeUseHtml;
                    fieldEntity.PicklistId = fieldModel.PicklistId;
                    fieldEntity.PicklistSortorder = fieldModel.PicklistSortorder;
                    fieldEntity.LookupType = fieldModel.LookupType;
                    fieldEntity.LookupRelation = fieldModel.LookupRelation;
                    fieldEntity.DecimalPlaces = fieldModel.DecimalPlaces;
                    fieldEntity.Rounding = fieldModel.Rounding;
                    fieldEntity.CurrencySymbol = fieldModel.CurrencySymbol;
                    fieldEntity.AutoNumberPrefix = fieldModel.AutoNumberPrefix;
                    fieldEntity.AutoNumberSuffix = fieldModel.AutoNumberSuffix;
                    fieldEntity.Mask = fieldModel.Mask;
                    fieldEntity.Placeholder = fieldModel.Placeholder;
                    fieldEntity.UniqueCombine = fieldModel.UniqueCombine;
                    fieldEntity.ShowOnlyEdit = fieldModel.ShowOnlyEdit;
                    fieldEntity.StyleLabel = fieldModel.StyleLabel;
                    fieldEntity.StyleInput = fieldModel.StyleInput;
                    fieldEntity.CalendarDateType = fieldModel.CalendarDateType;
                    fieldEntity.DocumentSearch = fieldModel.DocumentSearch;
                    fieldEntity.PrimaryLookup = fieldModel.PrimaryLookup;
                    fieldEntity.CustomLabel = fieldModel.CustomLabel;
                    fieldEntity.Deleted = fieldModel.Deleted;
                    fieldEntity.ImageSizeList = fieldModel.ImageSizeList;
                    fieldEntity.ImageSizeDetail = fieldModel.ImageSizeDetail;
                    fieldEntity.ShowAsDropdown = fieldModel.ShowAsDropdown;
                    fieldEntity.ViewType = fieldModel.ViewType;
                    fieldEntity.Position = fieldModel.Position;

                    if (fieldModel.Validation != null)
                    {
                        if (fieldEntity.Validation == null)
                        {
                            fieldEntity.Validation = new FieldValidation();
                        }
                        else
                        {
                            if (fieldModel.Validation.MaxLength.HasValue && (!fieldEntity.Validation.MaxLength.HasValue || fieldModel.Validation.MaxLength > fieldEntity.Validation.MaxLength))
                                moduleChanges.ValidationsChanged.Add(fieldModel.Name, "maxlength");

                            if (fieldEntity.Validation.Unique != fieldModel.Validation.Unique)
                                moduleChanges.ValidationsChanged.Add(fieldModel.Name, "unique");
                        }

                        fieldEntity.Validation.Required = fieldModel.Validation.Required;
                        fieldEntity.Validation.Readonly = fieldModel.Validation.Readonly;
                        fieldEntity.Validation.MinLength = fieldModel.Validation.MinLength;
                        fieldEntity.Validation.MaxLength = fieldModel.Validation.MaxLength;
                        fieldEntity.Validation.Min = fieldModel.Validation.Min;
                        fieldEntity.Validation.Max = fieldModel.Validation.Max;
                        fieldEntity.Validation.Pattern = fieldModel.Validation.Pattern;
                        fieldEntity.Validation.Unique = fieldModel.Validation.Unique;
                    }

                    if (fieldModel.Combination != null)
                    {
                        fieldEntity.Combination.Field1 = fieldModel.Combination.Field1;
                        fieldEntity.Combination.Field2 = fieldModel.Combination.Field2;
                        fieldEntity.Combination.CombinationCharacter = fieldModel.Combination.CombinationCharacter;
                    }

                    if (fieldModel.Permissions != null && fieldModel.Permissions.Count > 0)
                    {
                        //New Permissions
                        foreach (var permissionModel in fieldModel.Permissions)
                        {
                            if (!permissionModel.Id.HasValue)
                            {
                                if (fieldEntity.Permissions == null)
                                    fieldEntity.Permissions = new List<FieldPermission>();

                                var permissionEntity = new FieldPermission
                                {
                                    ProfileId = permissionModel.ProfileId,
                                    Type = permissionModel.Type
                                };

                                fieldEntity.Permissions.Add(permissionEntity);
                            }
                        }

                        //Existing Permissions
                        foreach (var permissionEntity in fieldEntity.Permissions)
                        {
                            var permissionModel = fieldModel.Permissions.FirstOrDefault(x => x.Id == permissionEntity.Id);

                            if (permissionModel == null)
                                continue;

                            permissionEntity.Type = permissionModel.Type;
                        }
                    }
                }
            }

            if (moduleModel.Relations != null && moduleModel.Relations.Count > 0)
            {
                //New Relations
                foreach (var relationModel in moduleModel.Relations)
                {
                    if (!relationModel.Id.HasValue)
                    {
                        if (moduleEntity.Relations == null)
                            moduleEntity.Relations = new List<Relation>();

                        var relationEntity = NewRelationEntity(relationModel);
                        moduleEntity.Relations.Add(relationEntity);

                        if (relationModel.RelationType == RelationType.ManyToMany)
                            moduleChanges.RelationsAdded.Add(relationEntity);
                    }
                }

                //Existing Relations
                foreach (var relationEntity in moduleEntity.Relations)
                {
                    var relationModel = moduleModel.Relations.FirstOrDefault(x => x.Id == relationEntity.Id);

                    if (relationModel == null)
                        continue;

                    if (relationModel.Deleted && relationModel.RelationType == RelationType.ManyToMany)
                        moduleChanges.RelationsDeleted.Add(relationEntity);

                    relationEntity.RelatedModule = relationModel.RelatedModule;
                    relationEntity.RelationField = relationModel.RelationField;
                    relationEntity.RelationType = relationModel.RelationType;
                    relationEntity.DisplayFieldsArray = relationModel.DisplayFields;
                    relationEntity.LabelEnSingular = relationModel.LabelEnSingular;
                    relationEntity.LabelEnPlural = relationModel.LabelEnPlural;
                    relationEntity.LabelTrSingular = relationModel.LabelTrSingular;
                    relationEntity.LabelTrPlural = relationModel.LabelTrPlural;
                    relationEntity.Readonly = relationModel.Readonly;
                    relationEntity.Order = relationModel.Order;
                    relationEntity.Deleted = relationModel.Deleted;
                }
            }

            if (moduleModel.Dependencies != null && moduleModel.Dependencies.Count > 0)
            {
                //New Dependencies
                foreach (var dependencyModel in moduleModel.Dependencies)
                {
                    if (!dependencyModel.Id.HasValue)
                    {
                        if (moduleEntity.Dependencies == null)
                            moduleEntity.Dependencies = new List<Dependency>();

                        var dependencyEntity = NewDependencyEntity(dependencyModel);
                        moduleEntity.Dependencies.Add(dependencyEntity);
                    }
                }

                //Existing Dependencies
                foreach (var dependencyEntity in moduleEntity.Dependencies)
                {
                    var dependencyModel = moduleModel.Dependencies.FirstOrDefault(x => x.Id == dependencyEntity.Id);

                    if (dependencyModel == null)
                        continue;

                    dependencyEntity.ParentField = dependencyModel.ParentField;
                    dependencyEntity.ChildField = dependencyModel.ChildField;
                    dependencyEntity.ChildSection = dependencyModel.ChildSection;
                    dependencyEntity.ValuesArray = dependencyModel.Values;
                    dependencyEntity.FieldMapParent = dependencyModel.FieldMapParent;
                    dependencyEntity.FieldMapChild = dependencyModel.FieldMapChild;
                    dependencyEntity.ValueMap = dependencyModel.ValueMap;
                    dependencyEntity.Otherwise = dependencyModel.Otherwise;
                    dependencyEntity.Clear = dependencyModel.Clear;
                    dependencyEntity.Deleted = dependencyModel.Deleted;
                }
            }

            if (moduleModel.Calculations != null && moduleModel.Calculations.Count > 0)
            {
                //New Calculations
                foreach (var calculationModel in moduleModel.Calculations)
                {
                    if (!calculationModel.Id.HasValue)
                    {
                        if (moduleEntity.Calculations == null)
                            moduleEntity.Calculations = new List<Calculation>();

                        var calculationEntity = NewCalculationEntity(calculationModel);
                        moduleEntity.Calculations.Add(calculationEntity);
                    }
                }

                //Existing Calculations
                foreach (var calculationEntity in moduleEntity.Calculations)
                {
                    var calculationModel = moduleModel.Calculations.FirstOrDefault(x => x.Id == calculationEntity.Id);

                    if (calculationModel == null)
                        continue;

                    calculationEntity.ResultField = calculationModel.ResultField;
                    calculationEntity.Field1 = calculationModel.Field1;
                    calculationEntity.Field2 = calculationModel.Field2;
                    calculationEntity.CustomValue = calculationModel.CustomValue;
                    calculationEntity.Operator = calculationModel.Operator;
                    calculationEntity.Order = calculationModel.Order;
                    calculationEntity.Deleted = calculationModel.Deleted;
                }
            }

            if (moduleChanges.FieldsAdded.Count < 1 && moduleChanges.FieldsDeleted.Count < 1 &&
                moduleChanges.SectionsAdded.Count < 1 && moduleChanges.SectionsDeleted.Count < 1 &&
                moduleChanges.RelationsAdded.Count < 1 && moduleChanges.RelationsDeleted.Count < 1 &&
                moduleChanges.ValidationsChanged.Count < 1)
                return null;

            return moduleChanges;
        }

        public List<ViewField> DeleteViewField(ICollection<View> views, int id, List<FieldBindingModel> fields)
        {
            var deletedViewFields = new List<ViewField>();

            foreach (var field in fields)
            {
                if (field.Deleted)
                {
                    foreach (var view in views)
                    {
                        foreach (var viewField in view.Fields)
                        {
                            if (field.Name == viewField.Field && viewField.ViewId == view.Id)
                            {
                                var a = new ViewField();
                                a.ViewId = view.Id;
                                a.Field = field.Name;
                                deletedViewFields.Add(a);
                            }
                        }
                    }
                }
            }
            return deletedViewFields;
        }

        public Module RevertEntity(ModuleChanges moduleChanges, Module moduleEntity)
        {
            foreach (var sectionAdded in moduleChanges.SectionsAdded)
            {
                var section = moduleEntity.Sections.Single(x => x.Name == sectionAdded.Name);
                section.Deleted = true;
            }

            foreach (var sectionDeleted in moduleChanges.SectionsDeleted)
            {
                var section = moduleEntity.Sections.Single(x => x.Name == sectionDeleted.Name);
                section.Deleted = false;
            }

            foreach (var fieldAdded in moduleChanges.FieldsAdded)
            {
                var field = moduleEntity.Fields.Single(x => x.Name == fieldAdded.Name);
                field.Deleted = true;
            }

            foreach (var fieldDeleted in moduleChanges.FieldsDeleted)
            {
                var field = moduleEntity.Fields.Single(x => x.Name == fieldDeleted.Name);
                field.Deleted = false;
            }

            return moduleEntity;
        }

        public void AfterCreate(UserItem appUser, Module module)
        {
	        Queue.QueueBackgroundWorkItem(async token => _auditLogHelper.CreateLog(appUser, module.Id, string.Empty, AuditType.Setup, null, SetupActionType.ModuleCreated, null));
		}

        public void AfterUpdate(UserItem appUser, Module module)
        {
	        Queue.QueueBackgroundWorkItem(async token => _auditLogHelper.CreateLog(appUser, module.Id, string.Empty, AuditType.Setup, null, SetupActionType.ModuleUpdated, null));
		}

        public void AfterDelete(UserItem appUser, Module module)
        {
	        Queue.QueueBackgroundWorkItem(async token => _auditLogHelper.CreateLog(appUser, module.Id, string.Empty, AuditType.Setup, null, SetupActionType.ModuleDeleted, null));
		}

        public Relation CreateRelationEntity(RelationBindingModel relationModel, Module moduleEntity)
        {
            var relationEntity = NewRelationEntity(relationModel);
            relationEntity.ModuleId = moduleEntity.Id;

            return relationEntity;
        }

        public void UpdateRelationEntity(RelationBindingModel relationModel, Relation relationEntity, Module moduleEntity)
        {
            relationEntity.RelatedModule = relationModel.RelatedModule;
            relationEntity.RelationField = relationModel.RelationField;
            relationEntity.RelationType = relationModel.RelationType;
            relationEntity.DisplayFieldsArray = relationModel.DisplayFields;
            relationEntity.LabelEnSingular = relationModel.LabelEnSingular;
            relationEntity.LabelEnPlural = relationModel.LabelEnPlural;
            relationEntity.LabelTrSingular = relationModel.LabelTrSingular;
            relationEntity.LabelTrPlural = relationModel.LabelTrPlural;
            relationEntity.Readonly = relationModel.Readonly;
            relationEntity.Order = relationModel.Order;
            relationEntity.ModuleId = moduleEntity.Id;
            relationEntity.DetailViewType = relationModel.DetailViewType;
        }

        public Dependency CreateDependencyEntity(DependencyBindingModel dependencyModel, Module moduleEntity)
        {
            var dependencyEntity = NewDependencyEntity(dependencyModel);
            dependencyEntity.ModuleId = moduleEntity.Id;

            return dependencyEntity;
        }

        public void UpdateDependencyEntity(DependencyBindingModel dependencyModel, Dependency dependencyEntity, Module moduleEntity)
        {
            dependencyEntity.ParentField = dependencyModel.ParentField;
            dependencyEntity.ChildField = dependencyModel.ChildField;
            dependencyEntity.ChildSection = dependencyModel.ChildSection;
            dependencyEntity.ValuesArray = dependencyModel.Values;
            dependencyEntity.FieldMapParent = dependencyModel.FieldMapParent;
            dependencyEntity.FieldMapChild = dependencyModel.FieldMapChild;
            dependencyEntity.ValueMap = dependencyModel.ValueMap;
            dependencyEntity.Otherwise = dependencyModel.Otherwise;
            dependencyEntity.Clear = dependencyModel.Clear;
            dependencyEntity.ModuleId = moduleEntity.Id;
        }

	    public Section NewSectionEntity(SectionBindingModel sectionModel)
        {
            var sectionEntity = new Section
            {
                Name = sectionModel.Name,
                SystemType = SystemType.Custom,
                LabelEn = sectionModel.LabelEn,
                LabelTr = sectionModel.LabelTr,
                Order = sectionModel.Order,
                DisplayForm = sectionModel.DisplayForm,
                DisplayDetail = sectionModel.DisplayDetail,
                ColumnCount = sectionModel.ColumnCount,
                CustomLabel = sectionModel.CustomLabel,
            };

            return sectionEntity;
        }

	    public Field NewFieldEntity(FieldBindingModel fieldModel)
        {
            var fieldEntity = new Field
            {
                Name = fieldModel.Name,
                SystemType = SystemType.Custom,
                LabelEn = fieldModel.LabelEn,
                LabelTr = fieldModel.LabelTr,
                Order = fieldModel.Order,
                DataType = fieldModel.DataType,
                DisplayList = fieldModel.DisplayList,
                DisplayForm = fieldModel.DisplayForm,
                DisplayDetail = fieldModel.DisplayDetail,
                Section = fieldModel.Section,
                SectionColumn = fieldModel.SectionColumn,
                Primary = fieldModel.Primary,
                DefaultValue = fieldModel.DefaultValue,
                InlineEdit = fieldModel.InlineEdit,
                Editable = fieldModel.Editable,
                Encrypted = fieldModel.Encrypted,
                EncryptionAuthorizedUsers = fieldModel.EncryptionAuthorizedUsers,
                ShowLabel = fieldModel.ShowLabel,
                AddressType = fieldModel.AddressType,
                MultilineType = fieldModel.MultilineType,
                MultilineTypeUseHtml = fieldModel.MultilineTypeUseHtml,
                PicklistId = fieldModel.PicklistId,
                PicklistSortorder = fieldModel.PicklistSortorder,
                LookupType = fieldModel.LookupType,
                LookupRelation = fieldModel.LookupRelation,
                DecimalPlaces = fieldModel.DecimalPlaces,
                Rounding = fieldModel.Rounding,
                CurrencySymbol = fieldModel.CurrencySymbol,
                AutoNumberPrefix = fieldModel.AutoNumberPrefix,
                AutoNumberSuffix = fieldModel.AutoNumberSuffix,
                Mask = fieldModel.Mask,
                Placeholder = fieldModel.Placeholder,
                UniqueCombine = fieldModel.UniqueCombine,
                ShowOnlyEdit = fieldModel.ShowOnlyEdit,
                StyleLabel = fieldModel.StyleLabel,
                StyleInput = fieldModel.StyleInput,
                CustomLabel = fieldModel.CustomLabel,
                CalendarDateType = fieldModel.CalendarDateType,
                DocumentSearch = fieldModel.DocumentSearch,
                PrimaryLookup = fieldModel.PrimaryLookup,
                ImageSizeList = fieldModel.ImageSizeList,
                ImageSizeDetail = fieldModel.ImageSizeDetail,
                ShowAsDropdown = fieldModel.ShowAsDropdown,
                ViewType = fieldModel.ViewType,
                Position = fieldModel.Position,
            };
            return fieldEntity;
        }

	    public FieldValidation NewFieldValidationEntity(FieldBindingModel fieldModel)
        {
            var fieldValidationEntity = new FieldValidation
            {
                Required = fieldModel.Validation.Required,
                Readonly = fieldModel.Validation.Readonly,
                MinLength = fieldModel.Validation.MinLength,
                MaxLength = fieldModel.Validation.MaxLength,
                Min = fieldModel.Validation.Min,
                Max = fieldModel.Validation.Max,
                Pattern = fieldModel.Validation.Pattern,
                Unique = fieldModel.Validation.Unique
            };

            return fieldValidationEntity;
        }

	    public FieldCombination NewFieldCombinationEntity(FieldBindingModel fieldModel)
        {
            var fieldCombinationEntity = new FieldCombination
            {
                Field1 = fieldModel.Combination.Field1,
                Field2 = fieldModel.Combination.Field2,
                CombinationCharacter = fieldModel.Combination.CombinationCharacter
            };

            return fieldCombinationEntity;
        }

	    public Relation NewRelationEntity(RelationBindingModel relationModel)
        {
            var relationEntity = new Relation
            {
                RelatedModule = relationModel.RelatedModule,
                RelationField = relationModel.RelationField,
                RelationType = relationModel.RelationType,
                DisplayFieldsArray = relationModel.DisplayFields,
                LabelEnSingular = relationModel.LabelEnSingular,
                LabelEnPlural = relationModel.LabelEnPlural,
                LabelTrSingular = relationModel.LabelTrSingular,
                LabelTrPlural = relationModel.LabelTrPlural,
                Readonly = relationModel.Readonly,
                Order = relationModel.Order
            };

            return relationEntity;
        }

	    public Dependency NewDependencyEntity(DependencyBindingModel dependencyModel)
        {
            var dependencyEntity = new Dependency
            {
                DependencyType = dependencyModel.DependencyType,
                ParentField = dependencyModel.ParentField,
                ChildField = dependencyModel.ChildField,
                ChildSection = dependencyModel.ChildSection,
                ValuesArray = dependencyModel.Values,
                FieldMapParent = dependencyModel.FieldMapParent,
                FieldMapChild = dependencyModel.FieldMapChild,
                ValueMap = dependencyModel.ValueMap,
                Otherwise = dependencyModel.Otherwise,
                Clear = dependencyModel.Clear
            };

            return dependencyEntity;
        }

	    public Calculation NewCalculationEntity(CalculationBindingModel calculationModel)
        {
            var calculationEntity = new Calculation
            {
                ResultField = calculationModel.ResultField,
                Field1 = calculationModel.Field1,
                Field2 = calculationModel.Field2,
                CustomValue = calculationModel.CustomValue,
                Operator = calculationModel.Operator,
                Order = calculationModel.Order
            };

            return calculationEntity;
        }
    }
}