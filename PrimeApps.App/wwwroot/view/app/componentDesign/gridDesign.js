'use strict';
angular.module('primeapps').controller('GridDesignController', ['$rootScope', '$scope', '$mdDialog', '$mdSidenav', '$mdToast', '$window', '$localStorage', '$cookies',
    function ($rootScope, $scope, $mdDialog, $mdSidenav, $mdToast, $window, $localStorage, $cookies) {
        $scope.title = 'Grid Design';
        var imagePath = 'https://material.angularjs.org/latest/img/list/60.jpeg';
        $scope.todos = [];
        for (var i = 0; i < 15; i++) {
            $scope.todos.push({
                face: imagePath,
                what: "Brunch this weekend?",
                who: "Min Li Chan",
                notes: "I'll be in your neighborhood doing errands."
            });
        }
        $scope.filterModalOpen = function(){
            $rootScope.buildToggler('sideModal','view/app/componentDesign/add-view.html');
        }
        

        $scope.selectOptions1 = {
            draggable: true,
            dataTextField: "name",
            dataValueField: "id"  ,
            dropSources: ["sag"],
            connectWith: "sag",
            dataSource:  [{name:"Galip ÇEVRİK",id:1},{name:"Galip ÇEVRİK",id:2},{name:"Galip ÇEVRİK",id:3},{name:"Galip ÇEVRİK",id:4}]
        };
        $scope.selectOptions2 = {
            draggable: true,
            dataTextField: "name",
            dataValueField: "id"  ,
            dropSources: ["sol"],
            connectWith: "sol",
        };
        
        
        setTimeout(function() {
            $scope.toolbarOptions = {
                resizable:true,
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
        },100);
   
        
        //Global yapılacak bu function
        $(".ripple-effect").click(function(e){
            var rippler = $(this);
            if(rippler.find(".ink").length == 0) {
                rippler.append("<span class='ink'></span>");
            }
            var ink = rippler.find(".ink");
            ink.removeClass("animate");
            if(!ink.height() && !ink.width())
            {
                var d = Math.max(rippler.outerWidth(), rippler.outerHeight());
                ink.css({height: d, width: d});
            }
            var x = e.pageX - rippler.offset().left - ink.width()/2;
            var y = e.pageY - rippler.offset().top - ink.height()/2;
            ink.css({
                top: y+'px',
                left:x+'px'
            }).addClass("animate");
        });

        var accessToken = $localStorage.read('access_token');

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
                    total: "count",
                    model: {
                        fields: {
                            email: { type: "string" },
                            culture: { type: "string" }
                        }
                    }
                }
            },
            pageable: {
                refresh: false,
                pageSizes: true,
                buttonCount: 5,
                info:false
            },
            
            scrollable: false,
            persistSelection: true,
            sortable: true,
            noRecords: true, 
            columns: [
                { field: "email", title: "Email" },
                { field: "culture", title: "Culture"}]
        };
        
 
        
        ///Refresh button on top Angular a uyarlanacak///
        $scope.refreshGrid = function(){
            $scope.grid.dataSource.read();
        }
        
        ///Grid General Search Angular a uyarlanacak///
        $('#grid-search').on('input', function (e) {
            var grid = $('#kendo-grid').data('kendoGrid');
            var columns = grid.columns;

            var filter = { logic: 'or', filters: [] };
            columns.forEach(function (x) {
                if (x.field) {
                    var type = grid.dataSource.options.schema.model.fields[x.field].type;
                    if (type == 'string') {
                        filter.filters.push({
                            field: x.field,
                            operator: 'contains',
                            value: e.target.value
                        })
                    }
                    else if (type == 'number') {
                        if (isNumeric(e.target.value)) {
                            filter.filters.push({
                                field: x.field,
                                operator: 'eq',
                                value: e.target.value
                            });
                        }

                    } else if (type == 'date') {
                        var data = grid.dataSource.data();
                        for (var i=0;i<data.length ; i++){
                            var dateStr = kendo.format(x.format, data[i][x.field]);
                            // change to includes() if you wish to filter that way https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/includes
                            if(dateStr.startsWith(e.target.value)){
                                filter.filters.push({
                                    field: x.field,
                                    operator:'eq',
                                    value: data[i][x.field]
                                })
                            }
                        }
                    } else if (type == 'boolean' && getBoolean(e.target.value) !== null) {
                        var bool = getBoolean(e.target.value);
                        filter.filters.push({
                            field: x.field,
                            operator: 'eq',
                            value: bool
                        });
                    }
                }
            });
            grid.dataSource.filter(filter);
        });
        
    }
]);