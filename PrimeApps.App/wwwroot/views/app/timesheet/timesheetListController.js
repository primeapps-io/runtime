'use strict';

var app = angular.module('ofisim', []);

app.controller('TimesheetListController', ['$rootScope', '$scope', '$filter', 'ngToast', '$popover', 'helper', 'ModuleService',
        function ($rootScope, $scope, $filter, ngToast, $popover, helper, ModuleService) {
            $scope.loading = true;
            $scope.user = $rootScope.user.ID;

            $scope.filterList = [
                {name : 'Waiting Approval From Me', status : 'draft', id:1},
                {name : 'Approved by Me', status : 'approved', id:2},
                {name : 'Rejected by Me', status : 'rejected', id:3}
            ];

            $scope.getTimesheets = function (status) {
                status = status ? status : 'draft';
                var findRequest = {};
                findRequest.fields = ['related_timesheet.timesheet.term', 'related_timesheet', 'selected_project', 'status', 'owner', 'approval_type'];
                if(status === 'draft'){
                    findRequest.filters = [
                        { field: '1_approver', operator: 'equals', value: $scope.user, no: 1 },
                        { field: 'status', operator: 'is', value: 'Waiting for Approval (First Level)', no: 2 },
                        { field: '2_approver', operator: 'equals', value: $scope.user, no: 3 },
                        { field: 'status', operator: 'is', value: 'Waiting for Approval (Second Level)', no: 4 }
                    ];
                    findRequest.filter_logic = "(1 and 2) or (3 and 4)";
                }else if(status === 'approved'){
                    findRequest.filters = [
                        { field: '1_approver', operator: 'equals', value: $scope.user, no: 1 },
                        { field: 'status', operator: 'is', value: 'Approved', no: 2 },
                        { field: '1_approver', operator: 'equals', value: $scope.user, no: 3 },
                        { field: 'status', operator: 'is', value: 'Waiting for Approval (Second Level)', no: 4 },
                        { field: '2_approver', operator: 'equals', value: $scope.user, no: 5 },
                        { field: 'status', operator: 'is', value: 'Approved', no: 6 },
                        { field: '1_approver', operator: 'equals', value: $scope.user, no: 7 },
                        { field: 'status', operator: 'is', value: 'Rejected', no: 8 }
                    ];
                    findRequest.filter_logic = "(1 and 2) or (3 and 4) or (5 and 6) or (7 and 8)";
                }else if(status === 'rejected'){
                    findRequest.filters = [
                        { field: '1_approver', operator: 'equals', value: $scope.user, no: 1 },
                        { field: 'status', operator: 'is', value: 'Rejected (First Level)', no: 2 },
                        { field: '2_approver', operator: 'equals', value: $scope.user, no: 3 },
                        { field: 'status', operator: 'is', value: 'Rejected', no: 4 }
                    ];
                    findRequest.filter_logic = "(1 and 2) or (3 and 4)";
                }
                findRequest.limit = 9999;

                ModuleService.findRecords('timesheet_item', findRequest)
                    .then(function (response) {
                        $scope.loading = false;
                        $scope.timesheetList = response.data;
                    });
            };

            $scope.getTimesheets();

        }
    ]);
