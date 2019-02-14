'use strict';

angular.module('primeapps')

    .controller('pickListsController', ['$rootScope', '$scope', '$state', '$stateParams', 'PickListsService', '$modal',
        function ($rootScope, $scope, $state, $stateParams, $location, PickListsService, $modal) {


            $scope.$parent.activeMenuItem = 'picklists';

            $rootScope.breadcrumblist[2].title = 'Picklists';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);

            $scope.loading = true;
            $scope.wizardStep = 0;
            $scope.requestModel = {
                limit: '10',
                offset: 0
            };

            // ViewsService.count($scope.id).then(function (response) {
            //     $scope.pageTotal = response.data;
            // });
            //
            // ViewsService.find($scope.id, $scope.requestModel).then(function (response) {
            //     var customViews = angular.copy(response.data);
            //     for (var i = customViews.length - 1; i >= 0; i--) {
            //         var parentModule = $filter('filter')($rootScope.appModules, {id: customViews[i].module_id}, true)[0];
            //         if (parentModule) {
            //             customViews[i].parent_module = $filter('filter')($rootScope.appModules, {id: customViews[i].module_id}, true)[0];
            //         } else {
            //             customViews.splice(i, 1);
            //         }
            //     }
            //     $scope.customViews = customViews;
            //     $scope.customViewsState = customViews;
            //     $scope.loading = false;
            // });
            //
            // $scope.changePage = function (page) {
            //     $scope.loading = true;
            //     var requestModel = angular.copy($scope.requestModel);
            //     requestModel.offset = page - 1;
            //
            //     ViewsService.find($scope.id, requestModel).then(function (response) {
            //         var customViews = angular.copy(response.data);
            //         for (var i = customViews.length - 1; i >= 0; i--) {
            //             var parentModule = $filter('filter')($rootScope.appModules, {id: customViews[i].module_id}, true)[0];
            //             if (parentModule) {
            //                 customViews[i].parent_module = $filter('filter')($rootScope.appModules, {id: customViews[i].module_id}, true)[0];
            //             } else {
            //                 customViews.splice(i, 1);
            //             }
            //         }
            //         $scope.customViews = customViews;
            //         $scope.loading = false;
            //     });
            // };
            //
            // $scope.changeOffset = function () {
            //     $scope.changePage(1)
            // };
            //
            // $scope.deleteView = function (id) {
            //     var willDelete =
            //         swal({
            //             title: "Are you sure?",
            //             text: " ",
            //             icon: "warning",
            //             buttons: ['Cancel', 'Yes'],
            //             dangerMode: true
            //         }).then(function (value) {
            //             if (value) {
            //                 if (id) {
            //                     ViewsService.deleteView(id)
            //                         .then(function () {
            //                             $scope.changePage(1);
            //                             $scope.pageTotal = $scope.pageTotal - 1;
            //                             toastr.success("Filter is deleted successfully.", "Deleted!");
            //                         }).catch(function () {
            //                         $scope.customViews = $scope.customViewsState;
            //
            //                         if ($scope.addNewFiltersModal) {
            //                             $scope.addNewFiltersModal.hide();
            //                             $scope.saving = false;
            //                         }
            //                     });
            //                 }
            //                 else {
            //                     toastr.warning($filter('translate')('Setup.Modules.OneView'));
            //                     return;
            //                 }
            //             }
            //         });
            // };
            //
            // $scope.showFormModal = function (view) {
            //     if (view) {
            //         ViewsService.getView(view.id).then(function (view) {
            //             $scope.view = angular.copy(view);
            //             $scope.module = $filter('filter')($rootScope.appModules, {id: view.module_id}, true)[0];
            //             $scope.view.label = $scope.view['label_' + $scope.language];
            //             $scope.view.edit = true;
            //
            //             // $scope.isOwner = $scope.view.created_by === $rootScope.user.ID;
            //
            //             // if (!$scope.view) {
            //             //     TODO
            //             //     $state.go('app.crm.moduleList', { type: module.name });
            //             //     return;
            //             // }
            //
            //             if ($scope.view.filter_logic && $scope.language === 'tr')
            //                 $scope.view.filter_logic = $scope.view.filter_logic.replace('or', 'veya').replace('and', 've');
            //
            //             moduleChanged($scope.module, false);
            //         });
            //     }
            //     else {
            //         $scope.view = {};
            //         $scope.module = undefined;
            //         //moduleChanged($scope.module, true);
            //     }
            //     $scope.addNewFiltersModal = $scope.addNewFiltersModal || $modal({
            //         scope: $scope,
            //         templateUrl: 'view/app/model/filters/filtersForm.html',
            //         animation: 'am-fade-and-slide-right',
            //         backdrop: 'static',
            //         show: false,
            //         controller: function ($scope) {
            //             $scope.$on('dragulardrop', function (e, el) {
            //                 $scope.viewForm.$setValidity('field', true);
            //             });
            //         }
            //     });
            //
            //     $scope.addNewFiltersModal.$promise.then(function () {
            //         $scope.addNewFiltersModal.show();
            //     });
            // };

        }
    ]);