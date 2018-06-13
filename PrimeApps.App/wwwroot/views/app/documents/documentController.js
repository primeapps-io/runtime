'use strict';

angular.module('primeapps')

    .controller('DocumentController', ['$rootScope', '$scope', '$location', 'guidEmpty', '$filter', '$popover', 'DocumentService', 'entityTypes', 'helper', 'operations','$stateParams','ModuleService',
        function ($rootScope, $scope, $location, guidEmpty, $filter, $popover, DocumentService, entityTypes, helper, operations,$stateParams,ModuleService) {
            $scope.documents = [];
            $scope.title = $filter('translate')('Documents.Documents');
            $scope.guidEmpty = guidEmpty;
            $scope.entityId = guidEmpty;
            $scope.entityType = guidEmpty;
            $scope.type = $stateParams.type;

            var recordId = $location.search().id;
            var type = $stateParams.type;
            var documentsData = [];

            $scope.filter = {};
            $scope.filter.CreatedBy = guidEmpty;
            $scope.sortPredicate = 'Timestamp';
            $scope.sortReverse = true;

            $scope.hasPermission = helper.hasPermission;
            $scope.entityTypes = entityTypes;
            $scope.operations = operations;

            $scope.module = $filter('filter')($rootScope.modules, { name: $stateParams.type }, true)[0];


            if (recordId && type) {
                if (recordId === null && type === null) {
                    $scope.title = $filter('translate')('Dashboard.AllDocuments');
                    $scope.filter.CreatedBy = $rootScope.user.ID;
                }
                else {
                    ModuleService.getRecord($scope.module.name, recordId).then(function (response) {

                        if (response.data.name == null) {
                            $scope.title = $filter('translate')('Documents.Documents');
                        }
                        else{
                            $scope.title = response.data.name;//TODO:Primary field get required for consistency. name field is temporary.
                        }

                    });

                    $scope.entityId = recordId;
                    $scope.entityType = type;
                }
            }

            function getDocuments(date) {
                $scope.loading = !date;

                DocumentService.getDocuments($rootScope.workgroup.instanceID, $scope.entityId, $scope.module.id)
                    .then(function (response) {
                        var processResults = DocumentService.processDocuments(response.data, $rootScope.users, $scope.filter, $scope.sortPredicate, $scope.sortReverse);
                        $scope.documents = processResults.documentList;
                        documentsData = angular.copy(processResults.documentList);
                        $scope.loading = false;
                    });

            }

            getDocuments();

            $scope.filterDocuments = function (createdBy) {
                $scope.filter.CreatedBy = createdBy;

                DocumentService.getDocuments($rootScope.workgroup.instanceID, $scope.entityId, $scope.module.id)
                    .then(function (response) {
                        var processResults = DocumentService.processDocuments(response.data, $rootScope.users, $scope.filter, $scope.sortPredicate, $scope.sortReverse);
                        $scope.documents = processResults.documentList;
                        documentsData = angular.copy(processResults.documentList);
                    });

            };

            $scope.searchDocuments = function (searchKey) {
                var searchedDocuments = [];

                angular.forEach(angular.copy(documentsData), function (document) {
                    if (!document.Name)
                        return;

                    if (document.Name.toLowerCase().indexOf(searchKey.toLowerCase()) > -1 || document.Description.toLowerCase().indexOf(searchKey.toLowerCase()) > -1)
                        searchedDocuments.push(document);

                });

                var documentsResults = {
                    Documents: searchedDocuments,
                    TotalDocumentsCount:searchedDocuments.length,
                    FilteredDocumentsCount:searchedDocuments.length
                }


                var processResults = DocumentService.processDocuments(documentsResults, $rootScope.users, $scope.filter, $scope.sortPredicate, $scope.sortReverse);
                $scope.documents = processResults.documentList;

            };

            $scope.sortDocuments = function (predicate, reverse) {
                $scope.sortPredicate = predicate;
                $scope.sortReverse = reverse;

                var documentsResults = {
                    Documents: angular.copy(documentsData),
                    TotalDocumentsCount:angular.copy(documentsData).length,
                    FilteredDocumentsCount:angular.copy(documentsData).length//change if any need on filter
                }

                var processResults = DocumentService.processDocuments(documentsResults, $rootScope.users, $scope.filter, $scope.sortPredicate, $scope.sortReverse);
                $scope.documents = processResults.documentList;

            };

        }
    ]);