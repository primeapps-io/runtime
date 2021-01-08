'use strict';
angular.module('primeapps').controller('ComponentdesignController', ['$rootScope', '$scope', '$mdDialog', '$mdSidenav', '$mdToast', '$window', '$localStorage', '$cookies',
    function ($rootScope, $scope, $mdDialog, $mdSidenav, $mdToast, $window, $localStorage, $cookies) {

        var accessToken = $localStorage.read('access_token');
        var postArgs = {
            "module": "egitim_siniflari",
            //"group_by": "string",
            "logic_type": "and, or",
            "two_way": "bool",
            "many_to_many": "string",
            //"offset": "int",
            //"limit": "int",
            //"sort_direction": "asc, desc",
            //"sort_field": "string",
            "filter_logic": "string",
            //"filters": [{
            //    "field": "string",
            //   "operator": "is, is_not, equals, not_equal, contains, starts_with, ends_with, empty, not_empty, greater, greater_equal, less, less_equal, not_in",
            //   "value": "obj",
            //    "no": "int"
            //}],
            //"convert": true,
            "fields": ["sinif_adi", "egitim_adi.egitim_katalogu.egitim_adi.primary", "egitim_turu", "egitim_firmasi", "baslangic_tarihi", "bitis_tarihi"]
        };

        $scope.mainGridOptions2 = {
            dataSource: {
                page: 1,
                pageSize: 5,
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true,
                transport: {
                    read: function (options) {
                        $.ajax({
                            url: '/api/record/find_custom',
                            contentType: 'application/json',
                            dataType: 'json',
                            type: 'POST',
                            data: JSON.stringify(Object.assign(postArgs, options.data)),
                            success: function (result) {
                                options.success(result);
                            },
                            beforeSend: function (req) {
                                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                req.setRequestHeader('X-Tenant-Id', $rootScope.user.tenant_id);
                            }
                        })

                    }
                },
                schema: {
                    data: "items",
                    total: "count"
                }
            },
            filterable: {
                mode: "row"
            },
            sortable: true,
            noRecords: true,
            groupable: true,
            pageable: true,
            columns: [/*{
                template: "<div class='customer-photo'" +
                    "style='background-image: url(https://demos.telerik.com/kendo-ui/content/web/Customers/#:data.CustomerID#.jpg);'></div>" +
                    "<div class='customer-name'>#: ContactName #</div>",
                field: "ContactName",
                title: "Contact Name",
                width: 240,
                filterable: {
                    cell: {
                        operator: "contains",
                        suggestionOperator: "contains"
                    }
                }
            }, */{
                field: "egitim_firmasi",
                title: "Eğitim Firması"
            }, {
                field: "egitim_turu",
                title: "Eğitim Türü"
            }]
        };
        /*
                var accessToken = $localStorage.read('access_token');
                var dataSource = new kendo.data.DataSource({
                    transport: {
                        read: function (options) {
                            $.ajax({
                                url: '/api/record/find_custom',
                                contentType: 'application/json',
                                dataType: 'json',
                                type: 'POST',
                                data: JSON.stringify(Object.assign(postArgs, options.data)),
                                success: function (result) {
                                    options.success(result);
                                },
                                beforeSend: function (req) {
                                    req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                    req.setRequestHeader('X-Tenant-Id', $rootScope.user.tenant_id);
                                }
                            })

                        }
                    },
                    group: {
                        field: "email",
                        dir: "asc"
                    },
                    schema: {
                        data: "items",
                        total: "count",
                        fields: [
                            {field: 'email', type: 'string'}
                        ]
                    },
                    sort: { field: "culture", dir: "desc" },
                    filter: { logic: "and", filters: [ { field: "email", operator: "startswith", value: "yusuf" } ] },
                    //filter: { filters: [{field: 'email', operator: 'eq', value: 'yusuf'}] },
                    page: 1,
                    pageSize: 5,
                    serverPaging: true,
                    serverFiltering: true,
                    serverSorting: true,
                    serverGrouping: true,
                    filterable: {
                        mode: "row"
                    },
                    sortable: true,noRecords: true,
                    groupable: true,
                    pageable: true,
                    columns: [/*{
                        template: "<div class='customer-photo'" +
                            "style='background-image: url(https://demos.telerik.com/kendo-ui/content/web/Customers/#:data.CustomerID#.jpg);'></div>" +
                            "<div class='customer-name'>#: ContactName #</div>",
                        field: "ContactName",
                        title: "Contact Name",
                        width: 240,
                        filterable: {
                            cell: {
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    }, {
                        field: "egitim_firmasi",
                        title: "Eğitim Firması"
                    }, {
                        field: "egitim_turu",
                        title: "Eğitim Türü"
                    }]
                });

                $("#grid").kendoGrid({
                    columns: [
                        {field: "name"},
                        {field: "age"}
                    ],
                    dataSource: dataSource
                });
        */
        $scope.sideModalLeft = function () {
            $rootScope.buildToggler('sideModal', 'view/app/componentDesign/dialog2-sidenav.html');
        };

        $scope.filterModalOpen = function () {
            $rootScope.buildToggler('sideModal', 'view/app/componentDesign/add-view.html');
        }

        //Global yapılacak bu function
        $(".ripple-effect").click(function (e) {
            var rippler = $(this);
            if (rippler.find(".ink").length == 0) {
                rippler.append("<span class='ink'></span>");
            }
            var ink = rippler.find(".ink");
            ink.removeClass("animate");
            if (!ink.height() && !ink.width()) {
                var d = Math.max(rippler.outerWidth(), rippler.outerHeight());
                ink.css({height: d, width: d});
            }
            var x = e.pageX - rippler.offset().left - ink.width() / 2;
            var y = e.pageY - rippler.offset().top - ink.height() / 2;
            ink.css({
                top: y + 'px',
                left: x + 'px'
            }).addClass("animate");
        });

        $scope.showAlert = function (ev) {
            $mdDialog.show(
                $mdDialog.alert()
                    .parent(angular.element(document.body))
                    .clickOutsideToClose(true)
                    .title('This is an alert title')
                    .textContent('You can specify some description text in here.')
                    .ariaLabel('Alert Dialog Demo')
                    .ok('Got it!')
                    .targetEvent(ev)
            );
        };

        $scope.showAdvanced = function (ev) {
            $mdDialog.show({
                controller: DialogController,
                templateUrl: 'view/app/componentDesign/dialog1.html',
                parent: angular.element(document.body),
                targetEvent: ev,
                clickOutsideToClose: true,
                fullscreen: false // Only for -xs, -sm breakpoints.
            })
        };
        $scope.showAdvanced2 = function (ev) {
            $mdDialog.show({
                controller: DialogController,
                templateUrl: 'view/app/componentDesign/modal-with-step.html',
                parent: angular.element(document.body),
                targetEvent: ev,
                clickOutsideToClose: true,
                fullscreen: false // Only for -xs, -sm breakpoints.
            })
        };

        $scope.kendoToastOptions = {
            animation: {
                open: {
                    effects: "slideIn:left"
                },
                close: {
                    effects: "slideIn:left",
                    reverse: true
                }
            }
        };
        $scope.kendoToastinfo = function () {
            $scope.kendoToast.show("Are you the 6 fingered man?", "info");
        }
        $scope.kendoToastwarning = function () {
            $scope.kendoToast.show("My name is Inigo Montoya. You killed my father, prepare to die!", "warning");
        }
        $scope.kendoToastsuccess = function () {
            $scope.kendoToast.show("Have fun storming the castle!", "success");
        }
        $scope.kendoToasterror = function () {
            $scope.kendoToast.show("I do not think that word means what you think it means.", "error");
        }

        $scope.dialogOptions = {
            appendTo: 'section#contentAll',
            modal: true,
            animation: {
                open: {
                    effects: "fade:in"
                },
                close: {
                    effects: "fade:out"
                }
            },

            actions: [
                {text: 'Skip this version'},
                {text: 'Remind me later'},
                {text: 'Install update', primary: true},
            ]
        }

        $("#module-views").kendoToolBar({
            items: [
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn active"}},
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn"}},
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn"}},
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn"}},
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn"}},
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn"}},
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn"}},
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn"}},
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn"}},
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn"}},
                {type: "button", text: "<span>Another Action</span>", overflow: "auto", attributes: {"class": "btn"}},
                {type: "button", text: "<span><i class='fas fa-plus'></i></span>", overflow: "never", attributes: {"class": "btn"}}
            ]
        });

        $("#percentage").kendoNumericTextBox({
            format: "p0",
            min: 0,
            max: 0.1,
            step: 0.01
        });
        $("#datetimepicker").kendoDateTimePicker({
            value: new Date(),
            dateInput: true
        });
        $("#optionallist").kendoListBox({
            connectWith: "selectedlist",
            toolbar: {
                tools: ["transferTo", "transferFrom", "transferAllTo", "transferAllFrom"]
            }
        });
        $("#selectedlist").kendoListBox();

        $("#optionallist2").kendoListBox({
            draggable: true,
            connectWith: "selectedlist2",
            dropSources: ["selectedlist2"],
        });
        $("#selectedlist2").kendoListBox({
            draggable: true,
            connectWith: "optionallist2",
            dropSources: ["optionallist2"],
        });

        $scope.selectOptions1 = {
            draggable: true,
            dataTextField: "name",
            dataValueField: "id",
            dropSources: ["sag"],
            connectWith: "sag",
            dataSource: [{name: "Galip ÇEVRİK", id: 1}, {name: "Galip ÇEVRİK", id: 2}, {name: "Galip ÇEVRİK", id: 3}, {name: "Galip ÇEVRİK", id: 4}]
        };
        $scope.selectOptions2 = {
            draggable: true,
            dataTextField: "name",
            dataValueField: "id",
            dropSources: ["sol"],
            connectWith: "sol",
        };

        $(".sortable-list").kendoSortable({
            hint: function (element) {
                return element.clone().addClass("sortable-list-hint");
            },
            placeholder: function (element) {
                return element.clone().addClass("sortable-list-placeholder").text("Drop Here");
            },
            cursorOffset: {
                top: -10,
                left: 20
            }
        });

        $("#multipleselect").kendoMultiSelect({
            autoClose: false
        }).data("kendoMultiSelect");

        $("#datepicker").kendoDatePicker();
        $("#dropdowntree").kendoDropDownTree();
        $("#phone_number").kendoMaskedTextBox({mask: "(999) 000-0000"});
        $("#timepicker").kendoTimePicker({dateInput: true});
        $("#daterangepicker").kendoDateRangePicker();
        $("#primaryTextButton").kendoButton();
        $("#textButton").kendoButton();
        $("#primaryDisabledButton").kendoButton({enable: false});
        $("#disabledButton").kendoButton({enable: false});
        $("#iconTextButton").kendoButton({icon: "filter"});
        $("#kendoIconTextButton").kendoButton({icon: "filter-clear"});
        $("#iconButton").kendoButton({icon: "refresh"});
        $("#select-period").kendoButtonGroup();
        $("#tabstrip").kendoTabStrip({
            animation: {
                open: {
                    effects: "fadeIn"
                }
            }
        });
        $("#toolbar").kendoToolBar({
            items: [
                {type: "button", text: "Button"},
                {type: "button", text: "Toggle Button", togglable: true},
                {
                    type: "splitButton",
                    text: "Insert",
                    menuButtons: [
                        {text: "Insert above", icon: "insert-up"},
                        {text: "Insert between", icon: "insert-middle"},
                        {text: "Insert below", icon: "insert-down"}
                    ]
                },
                {type: "separator"},
                {template: "<label for='dropdown'>Format:</label>"},
                {
                    template: "<input id='dropdown' style='width: 150px;' />",
                    overflow: "never"
                },
                {type: "separator"},
                {
                    type: "buttonGroup",
                    buttons: [
                        {icon: "align-left", text: "Left", togglable: true, group: "text-align"},
                        {icon: "align-center", text: "Center", togglable: true, group: "text-align"},
                        {icon: "align-right", text: "Right", togglable: true, group: "text-align"}
                    ]
                },
                {
                    type: "buttonGroup",
                    buttons: [
                        {icon: "bold", text: "Bold", togglable: true},
                        {icon: "italic", text: "Italic", togglable: true},
                        {icon: "underline", text: "Underline", togglable: true}
                    ]
                },
                {
                    type: "button",
                    text: "Action",
                    overflow: "always"
                },
                {
                    type: "button",
                    text: "Another Action",
                    overflow: "always"
                },
                {
                    type: "button",
                    text: "Something else here",
                    overflow: "always"
                }
            ]
        });
        $("#calendar").kendoCalendar();

        $scope.mainGridOptions = {
            dataSource: {
                type: "odata-v4",
                page: 1,
                pageSize: 5,
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true,
                transport: {
                    read: {
                        url: "/api/user/find",
                        type: 'GET',
                        dataType: "json",
                        beforeSend:$rootScope.beforeSend()
                    }
                },
                schema: {
                    data: "items",
                    total: "count"
                }
            },
            filterable: {
                mode: "row"
            },
            sortable: true,
            noRecords: true,
            groupable: true,
            pageable: true,
            columns: [/*{
                template: "<div class='customer-photo'" +
                    "style='background-image: url(https://demos.telerik.com/kendo-ui/content/web/Customers/#:data.CustomerID#.jpg);'></div>" +
                    "<div class='customer-name'>#: ContactName #</div>",
                field: "ContactName",
                title: "Contact Name",
                width: 240,
                filterable: {
                    cell: {
                        operator: "contains",
                        suggestionOperator: "contains"
                    }
                }
            }, */{
                field: "email",
                title: "Email"
            }, {
                field: "culture",
                title: "Culture"
            }]
        };

        $("#dateinput").kendoDateInput();
        $("#slider").kendoSlider({
            increaseButtonTitle: "Right",
            decreaseButtonTitle: "Left",
            min: -10,
            max: 10,
            smallStep: 2,
            largeStep: 1
        });
        $("#notifications-switch").kendoSwitch();

        $("#mail-switch").kendoSwitch({
            messages: {
                checked: "YES",
                unchecked: "NO"
            }
        });

        $("#visible-switch").kendoSwitch({
            checked: true
        });

        $("#name-switch").kendoSwitch();

        var countries = [
            "Albania",
            "Andorra",
            "Armenia",
            "Austria",
            "Azerbaijan",
            "Belarus",
            "Belgium",
            "Bosnia & Herzegovina",
            "Bulgaria",
            "Croatia",
            "Cyprus",
            "Czech Republic",
            "Denmark",
            "Estonia",
            "Finland",
            "France",
            "Georgia",
            "Germany",
            "Greece",
            "Hungary",
            "Iceland",
            "Ireland",
            "Italy",
            "Kosovo",
            "Latvia",
            "Liechtenstein",
            "Lithuania",
            "Luxembourg",
            "Macedonia",
            "Malta",
            "Moldova",
            "Monaco",
            "Montenegro",
            "Netherlands",
            "Norway",
            "Poland",
            "Portugal",
            "Romania",
            "Russia",
            "San Marino",
            "Serbia",
            "Slovakia",
            "Slovenia",
            "Spain",
            "Sweden",
            "Switzerland",
            "Turkey",
            "Ukraine",
            "United Kingdom",
            "Vatican City"
        ];

        $("#countries").kendoAutoComplete({
            dataSource: countries,
            filter: "startswith",
            separator: ", "
        });

        $("#ticketsForm").kendoValidator().data("kendoValidator");

        $("#scheduler").kendoScheduler({
            date: new Date("2013/6/13"),
            startTime: new Date("2013/6/13 07:00 AM"),
            height: 400,
            views: [
                "day",
                {type: "workWeek", selected: true},
                "week",
                "month",
                "agenda",
                {type: "timeline", eventHeight: 50}
            ],
            timezone: "Etc/UTC"
        });

        function DialogController($scope, $mdDialog) {
            $scope.cancel = function () {
                $mdDialog.cancel();
            };
        }

        setTimeout(function () {
            $scope.toolbarOptions = {
                items: [
                    {
                        template: "<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",
                        overflowTemplate: "<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",
                        overflow: "auto"
                    },
                    {
                        template: "<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",
                        overflowTemplate: "<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",
                        overflow: "auto"
                    },
                    {
                        template: "<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",
                        overflowTemplate: "<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",
                        overflow: "auto"
                    },
                    {
                        template: "<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",
                        overflowTemplate: "<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",
                        overflow: "auto"
                    },
                    {
                        template: "<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",
                        overflowTemplate: "<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",
                        overflow: "auto"
                    },
                    {
                        template: "<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",
                        overflowTemplate: "<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",
                        overflow: "auto"
                    },
                    {
                        template: "<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",
                        overflowTemplate: "<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",
                        overflow: "auto"
                    },
                    {
                        template: "<md-button class='btn btn-secondary' aria-label='Send E-mail' > <i class='fas fa-envelope'></i> <span>Send E-mail</span></md-button>",
                        overflowTemplate: "<md-button class='action-dropdown-item'><i class='fas fa-home'></i><span>Test</span></md-button>",
                        overflow: "auto"
                    }
                ]
            };
        }, 100);
        angular.element($window).bind('resize', function () {
            //$("#action-btn").data("kendoToolBar").resize(true);
        });

        $scope.schedulerOptions = {
            date: new Date("2013/6/13"),
            startTime: new Date("2013/6/13 07:00 AM"),
            height: 600,
            views: [
                "day",
                { type: "workWeek", selected: true },
                "week",
                "month",
            ],
            timezone: "Etc/UTC",
            dataSource: {
                batch: true,
                transport: {
                    read: {
                        url: "https://demos.telerik.com/kendo-ui/service/tasks",
                        dataType: "jsonp"
                    },
                    update: {
                        url: "https://demos.telerik.com/kendo-ui/service/tasks/update",
                        dataType: "jsonp"
                    },
                    create: {
                        url: "https://demos.telerik.com/kendo-ui/service/tasks/create",
                        dataType: "jsonp"
                    },
                    destroy: {
                        url: "https://demos.telerik.com/kendo-ui/service/tasks/destroy",
                        dataType: "jsonp"
                    },
                    parameterMap: function(options, operation) {
                        if (operation !== "read" && options.models) {
                            return {models: kendo.stringify(options.models)};
                        }
                    }
                },
                schema: {
                    model: {
                        id: "taskId",
                        fields: {
                            taskId: { from: "TaskID", type: "number" },
                            title: { from: "Title", defaultValue: "No title", validation: { required: true } },
                            start: { type: "date", from: "Start" },
                            end: { type: "date", from: "End" },
                            startTimezone: { from: "StartTimezone" },
                            endTimezone: { from: "EndTimezone" },
                            description: { from: "Description" },
                            recurrenceId: { from: "RecurrenceID" },
                            recurrenceRule: { from: "RecurrenceRule" },
                            recurrenceException: { from: "RecurrenceException" },
                            ownerId: { from: "OwnerID", defaultValue: 1 },
                            isAllDay: { type: "boolean", from: "IsAllDay" }
                        }
                    }
                },
                filter: {
                    logic: "or",
                    filters: [
                        { field: "ownerId", operator: "eq", value: 1 },
                        { field: "ownerId", operator: "eq", value: 2 }
                    ]
                }
            },
            resources: [
                {
                    field: "ownerId",
                    title: "Owner",
                    dataSource: [
                        { text: "Alex", value: 1, color: "#f8a398" },
                        { text: "Bob", value: 2, color: "#51a0ed" },
                        { text: "Charlie", value: 3, color: "#56ca85" }
                    ]
                }
            ]
        };

        $scope.materialToast = function (value) {
            $mdToast.show(
                $mdToast.simple()
                    .textContent('Marked as read')
                    .action('UNDO')
                    .position('bottom right')
                    .actionKey('z')
                    .theme(value)
                    .hideDelay(0)
            );
        };

    }]);
