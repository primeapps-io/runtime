"use strict";angular.module("primeapps").controller("ComponentdesignController",["$rootScope","$scope","$mdDialog","$mdSidenav","$mdToast","$window","$localStorage","$cookies",function(e,t,o,a,n,i,s){function l(e,t){e.cancel=function(){t.cancel()}}var r=s.read("access_token"),d={module:"egitim_siniflari",logic_type:"and, or",two_way:"bool",many_to_many:"string",filter_logic:"string",fields:["sinif_adi","egitim_adi.egitim_katalogu.egitim_adi.primary","egitim_turu","egitim_firmasi","baslangic_tarihi","bitis_tarihi"]};t.mainGridOptions2={dataSource:{page:1,pageSize:5,serverPaging:!0,serverFiltering:!0,serverSorting:!0,transport:{read:function(t){$.ajax({url:"/api/record/find_custom",contentType:"application/json",dataType:"json",type:"POST",data:JSON.stringify(Object.assign(d,t.data)),success:function(e){t.success(e)},beforeSend:function(t){t.setRequestHeader("Authorization","Bearer "+r),t.setRequestHeader("X-Tenant-Id",e.user.tenant_id)}})}},schema:{data:"items",total:"count"}},filterable:{mode:"row"},sortable:!0,noRecords:!0,groupable:!0,pageable:!0,columns:[{field:"egitim_firmasi",title:"Eğitim Firması"},{field:"egitim_turu",title:"Eğitim Türü"}]},t.sideModalLeft=function(){e.buildToggler("sideModal","view/app/componentDesign/dialog2-sidenav.html")},t.filterModalOpen=function(){e.buildToggler("sideModal","view/app/componentDesign/add-view.html")},$(".ripple-effect").click(function(e){var t=$(this);0==t.find(".ink").length&&t.append("<span class='ink'></span>");var o=t.find(".ink");if(o.removeClass("animate"),!o.height()&&!o.width()){var a=Math.max(t.outerWidth(),t.outerHeight());o.css({height:a,width:a})}var n=e.pageX-t.offset().left-o.width()/2,i=e.pageY-t.offset().top-o.height()/2;o.css({top:i+"px",left:n+"px"}).addClass("animate")}),t.showAlert=function(e){o.show(o.alert().parent(angular.element(document.body)).clickOutsideToClose(!0).title("This is an alert title").textContent("You can specify some description text in here.").ariaLabel("Alert Dialog Demo").ok("Got it!").targetEvent(e))},t.showAdvanced=function(e){o.show({controller:l,templateUrl:"view/app/componentDesign/dialog1.html",parent:angular.element(document.body),targetEvent:e,clickOutsideToClose:!0,fullscreen:!1})},t.showAdvanced2=function(e){o.show({controller:l,templateUrl:"view/app/componentDesign/modal-with-step.html",parent:angular.element(document.body),targetEvent:e,clickOutsideToClose:!0,fullscreen:!1})},t.kendoToastOptions={animation:{open:{effects:"slideIn:left"},close:{effects:"slideIn:left",reverse:!0}}},t.kendoToastinfo=function(){t.kendoToast.show("Are you the 6 fingered man?","info")},t.kendoToastwarning=function(){t.kendoToast.show("My name is Inigo Montoya. You killed my father, prepare to die!","warning")},t.kendoToastsuccess=function(){t.kendoToast.show("Have fun storming the castle!","success")},t.kendoToasterror=function(){t.kendoToast.show("I do not think that word means what you think it means.","error")},t.dialogOptions={appendTo:"section#contentAll",modal:!0,animation:{open:{effects:"fade:in"},close:{effects:"fade:out"}},actions:[{text:"Skip this version"},{text:"Remind me later"},{text:"Install update",primary:!0}]},$("#module-views").kendoToolBar({items:[{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn active"}},{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn"}},{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn"}},{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn"}},{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn"}},{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn"}},{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn"}},{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn"}},{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn"}},{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn"}},{type:"button",text:"<span>Another Action</span>",overflow:"auto",attributes:{"class":"btn"}},{type:"button",text:"<span><i class='fas fa-plus'></i></span>",overflow:"never",attributes:{"class":"btn"}}]}),$("#percentage").kendoNumericTextBox({format:"p0",min:0,max:.1,step:.01}),$("#datetimepicker").kendoDateTimePicker({value:new Date,dateInput:!0}),$("#optionallist").kendoListBox({connectWith:"selectedlist",toolbar:{tools:["transferTo","transferFrom","transferAllTo","transferAllFrom"]}}),$("#selectedlist").kendoListBox(),$("#optionallist2").kendoListBox({draggable:!0,connectWith:"selectedlist2",dropSources:["selectedlist2"]}),$("#selectedlist2").kendoListBox({draggable:!0,connectWith:"optionallist2",dropSources:["optionallist2"]}),t.selectOptions1={draggable:!0,dataTextField:"name",dataValueField:"id",dropSources:["sag"],connectWith:"sag",dataSource:[{name:"Galip ÇEVRİK",id:1},{name:"Galip ÇEVRİK",id:2},{name:"Galip ÇEVRİK",id:3},{name:"Galip ÇEVRİK",id:4}]},t.selectOptions2={draggable:!0,dataTextField:"name",dataValueField:"id",dropSources:["sol"],connectWith:"sol"},$(".sortable-list").kendoSortable({hint:function(e){return e.clone().addClass("sortable-list-hint")},placeholder:function(e){return e.clone().addClass("sortable-list-placeholder").text("Drop Here")},cursorOffset:{top:-10,left:20}}),$("#multipleselect").kendoMultiSelect({autoClose:!1}).data("kendoMultiSelect"),$("#datepicker").kendoDatePicker(),$("#dropdowntree").kendoDropDownTree(),$("#phone_number").kendoMaskedTextBox({mask:"(999) 000-0000"}),$("#timepicker").kendoTimePicker({dateInput:!0}),$("#daterangepicker").kendoDateRangePicker(),$("#primaryTextButton").kendoButton(),$("#textButton").kendoButton(),$("#primaryDisabledButton").kendoButton({enable:!1}),$("#disabledButton").kendoButton({enable:!1}),$("#iconTextButton").kendoButton({icon:"filter"}),$("#kendoIconTextButton").kendoButton({icon:"filter-clear"}),$("#iconButton").kendoButton({icon:"refresh"}),$("#select-period").kendoButtonGroup(),$("#tabstrip").kendoTabStrip({animation:{open:{effects:"fadeIn"}}}),$("#toolbar").kendoToolBar({items:[{type:"button",text:"Button"},{type:"button",text:"Toggle Button",togglable:!0},{type:"splitButton",text:"Insert",menuButtons:[{text:"Insert above",icon:"insert-up"},{text:"Insert between",icon:"insert-middle"},{text:"Insert below",icon:"insert-down"}]},{type:"separator"},{template:"<label for='dropdown'>Format:</label>"},{template:"<input id='dropdown' style='width: 150px;' />",overflow:"never"},{type:"separator"},{type:"buttonGroup",buttons:[{icon:"align-left",text:"Left",togglable:!0,group:"text-align"},{icon:"align-center",text:"Center",togglable:!0,group:"text-align"},{icon:"align-right",text:"Right",togglable:!0,group:"text-align"}]},{type:"buttonGroup",buttons:[{icon:"bold",text:"Bold",togglable:!0},{icon:"italic",text:"Italic",togglable:!0},{icon:"underline",text:"Underline",togglable:!0}]},{type:"button",text:"Action",overflow:"always"},{type:"button",text:"Another Action",overflow:"always"},{type:"button",text:"Something else here",overflow:"always"}]}),$("#calendar").kendoCalendar(),t.mainGridOptions={dataSource:{type:"odata-v4",page:1,pageSize:5,serverPaging:!0,serverFiltering:!0,serverSorting:!0,transport:{read:{url:"/api/user/find",type:"GET",dataType:"json",beforeSend:e.beforeSend()}},schema:{data:"items",total:"count"}},filterable:{mode:"row"},sortable:!0,noRecords:!0,groupable:!0,pageable:!0,columns:[{field:"email",title:"Email"},{field:"culture",title:"Culture"}]},$("#dateinput").kendoDateInput(),$("#slider").kendoSlider({increaseButtonTitle:"Right",decreaseButtonTitle:"Left",min:-10,max:10,smallStep:2,largeStep:1}),$("#notifications-switch").kendoSwitch(),$("#mail-switch").kendoSwitch({messages:{checked:"YES",unchecked:"NO"}}),$("#visible-switch").kendoSwitch({checked:!0}),$("#name-switch").kendoSwitch();var c=["Albania","Andorra","Armenia","Austria","Azerbaijan","Belarus","Belgium","Bosnia & Herzegovina","Bulgaria","Croatia","Cyprus","Czech Republic","Denmark","Estonia","Finland","France","Georgia","Germany","Greece","Hungary","Iceland","Ireland","Italy","Kosovo","Latvia","Liechtenstein","Lithuania","Luxembourg","Macedonia","Malta","Moldova","Monaco","Montenegro","Netherlands","Norway","Poland","Portugal","Romania","Russia","San Marino","Serbia","Slovakia","Slovenia","Spain","Sweden","Switzerland","Turkey","Ukraine","United Kingdom","Vatican City"];$("#countries").kendoAutoComplete({dataSource:c,filter:"startswith",separator:", "}),$("#ticketsForm").kendoValidator().data("kendoValidator"),$("#scheduler").kendoScheduler({date:new Date("2013/6/13"),startTime:new Date("2013/6/13 07:00 AM"),height:400,views:["day",{type:"workWeek",selected:!0},"week","month","agenda",{type:"timeline",eventHeight:50}],timezone:"Etc/UTC"}),setTimeout(function(){t.toolbarOptions={items:[{template:"<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",overflowTemplate:"<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",overflow:"auto"},{template:"<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",overflowTemplate:"<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",overflow:"auto"},{template:"<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",overflowTemplate:"<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",overflow:"auto"},{template:"<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",overflowTemplate:"<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",overflow:"auto"},{template:"<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",overflowTemplate:"<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",overflow:"auto"},{template:"<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",overflowTemplate:"<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",overflow:"auto"},{template:"<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",overflowTemplate:"<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",overflow:"auto"},{template:"<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",overflowTemplate:"<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",overflow:"auto"}]}},100),angular.element(i).bind("resize",function(){}),t.schedulerOptions={date:new Date("2013/6/13"),startTime:new Date("2013/6/13 07:00 AM"),height:600,views:["day",{type:"workWeek",selected:!0},"week","month"],timezone:"Etc/UTC",dataSource:{batch:!0,transport:{read:{url:"https://demos.telerik.com/kendo-ui/service/tasks",dataType:"jsonp"},update:{url:"https://demos.telerik.com/kendo-ui/service/tasks/update",dataType:"jsonp"},create:{url:"https://demos.telerik.com/kendo-ui/service/tasks/create",dataType:"jsonp"},destroy:{url:"https://demos.telerik.com/kendo-ui/service/tasks/destroy",dataType:"jsonp"},parameterMap:function(e,t){return"read"!==t&&e.models?{models:kendo.stringify(e.models)}:void 0}},schema:{model:{id:"taskId",fields:{taskId:{from:"TaskID",type:"number"},title:{from:"Title",defaultValue:"No title",validation:{required:!0}},start:{type:"date",from:"Start"},end:{type:"date",from:"End"},startTimezone:{from:"StartTimezone"},endTimezone:{from:"EndTimezone"},description:{from:"Description"},recurrenceId:{from:"RecurrenceID"},recurrenceRule:{from:"RecurrenceRule"},recurrenceException:{from:"RecurrenceException"},ownerId:{from:"OwnerID",defaultValue:1},isAllDay:{type:"boolean",from:"IsAllDay"}}}},filter:{logic:"or",filters:[{field:"ownerId",operator:"eq",value:1},{field:"ownerId",operator:"eq",value:2}]}},resources:[{field:"ownerId",title:"Owner",dataSource:[{text:"Alex",value:1,color:"#f8a398"},{text:"Bob",value:2,color:"#51a0ed"},{text:"Charlie",value:3,color:"#56ca85"}]}]},t.materialToast=function(e){n.show(n.simple().textContent("Marked as read").action("UNDO").position("bottom right").actionKey("z").theme(e).hideDelay(0))}}]);