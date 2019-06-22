"use strict";angular.module("primeapps").controller("TemplateGuideController",["$rootScope","$scope","$filter","ngToast","ModuleService","$window",function(e,l,a,o,t,u){l.templateModules=[],l.$parent.collapsed=!0,angular.forEach(e.modules,function(e){0!=e.order&&l.templateModules.push(angular.copy(e))});var s=function(l){if(l){for(var a=[],o=0;o<l.fields.length;o++)if("lookup"==l.fields[o].data_type){if("quote_products"===l.name&&"quotes"===l.fields[o].lookup_type)continue;if("order_products"===l.name&&"sales_order"===l.fields[o].lookup_type)continue;if("purchase_order_products"===l.name&&"purchase_order"===l.fields[o].lookup_type)continue;if("sales_invoices_products"===l.name&&"sales_invoices"===l.fields[o].lookup_type)continue;if("purchase_invoices_products"===l.name&&"purchase_invoices"===l.fields[o].lookup_type)continue;for(var t=0;t<e.modules.length;t++)if(l.fields[o].lookup_type==e.modules[t].name){var u=angular.copy(e.modules[t]);u.parent_field=l.fields[o],a.push(u);break}}a.length&&(l.lookupModules=a)}},r=function(e){var l={};l.type="custom",l.name="notes",l.label_tr_singular="Not",l.label_tr_plural="Notlar",l.label_en_singular="Note",l.label_en_plural="Notes",l.order=9999,l.fields=[],l.fields.push({id:1,name:"text",label_tr:"Not",label_en:"Note"}),l.fields.push({id:2,name:"first_name",label_tr:"Oluşturan - Adı",label_en:"First Name"}),l.fields.push({id:3,name:"last_name",label_tr:"Oluşturan - Soyadı",label_en:"Last Name"}),l.fields.push({id:4,name:"full_name",label_tr:"Oluşturan - Adı Soyadı",label_en:"Full Name"}),l.fields.push({id:5,name:"email",label_tr:"Oluşturan - Eposta",label_en:"Email"}),l.fields.push({id:6,name:"created_at",label_tr:"Oluşturulma Tarihi",label_en:"Created at"}),e.relatedModules.push(l)};l.getDownloadUrlExcel=function(){var t=l.selectedModuleExcel.name;u.open("/attach/export_excel?module="+t+"&locale="+e.locale,"_blank"),o.create({content:a("translate")("Module.ExcelDesktop"),className:"success"})},l.moduleChanged=function(){l.lookupModules=s(l.selectedModule),l.getModuleRelations(l.selectedModule),l.selectedSubModule=null},l.getModuleRelations=function(o){if(o){if(o.relatedModules=[],"quotes"===o.name){var t=a("filter")(e.modules,{name:"quote_products"},!0)[0];s(t),o.relatedModules.push(t)}if("sales_orders"===o.name){var u=a("filter")(e.modules,{name:"order_products"},!0)[0];s(u),o.relatedModules.push(u)}if("purchase_orders"===o.name){var n=a("filter")(e.modules,{name:"purchase_order_products"},!0)[0];s(n),o.relatedModules.push(n)}if("sales_invoices"===o.name){var d=a("filter")(e.modules,{name:"sales_invoices_products"},!0)[0];s(d),o.relatedModules.push(d)}if("purchase_invoices"===o.name){var i=a("filter")(e.modules,{name:"purchase_invoices_products"},!0)[0];s(i),o.relatedModules.push(i)}angular.forEach(l.selectedModule.relations,function(l){var t=a("filter")(e.modules,{name:l.related_module},!0)[0];!l.deleted&&t&&0!==t.order&&(t=angular.copy(t),"many_to_many"===l.relation_type?angular.forEach(t.fields,function(e){e.name=l.related_module+"_id."+e.name}):s(t),o.relatedModules.push(t))}),r(o)}},l.filterUsers=function(e){return"users"!=e.data_type},l.getRelatedFieldName=function(e,l){return l.parent_field.name+"."+(e.multiline_type_use_html?"html__":"image"==e.data_type?"img__":"")+e.name}}]);