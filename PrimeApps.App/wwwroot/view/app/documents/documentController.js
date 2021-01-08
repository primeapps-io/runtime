'use strict';

angular.module('primeapps')

    .controller('DocumentController', ['$rootScope', '$scope', '$location', 'guidEmpty', '$filter', 'DocumentService', 'entityTypes', 'helper', 'operations', '$stateParams', 'ModuleService',
        function ($rootScope, $scope, $location, guidEmpty, $filter, DocumentService, entityTypes, helper, operations, $stateParams, ModuleService) {
            $scope.documents = [];
            $scope.title = $filter('translate')('Documents.Label');
            $scope.guidEmpty = guidEmpty;
            $scope.entityId = guidEmpty;
            $scope.entityType = guidEmpty;
            $scope.type = $stateParams.type;

            var recordId = $location.search().id;
            var type = $stateParams.type;
            var documentsData = [];

            $scope.filter = {};
            $scope.filter.created_by = guidEmpty;
            $scope.sortPredicate = 'timestamp';
            $scope.sortReverse = true;

            $scope.hasPermission = helper.hasPermission;
            $scope.entityTypes = entityTypes;
            $scope.operations = operations;

            $scope.module = $filter('filter')($rootScope.modules, { name: $stateParams.type }, true)[0];


            if (recordId && type) {
                if (recordId === null && type === null) {
                    $scope.title = $filter('translate')('Dashboard.AllDocuments');
                    $scope.filter.created_by = $rootScope.user.id;
                } else {
                    ModuleService.getRecord($scope.module.name, recordId).then(function (response) {

                        if (response.data.name == null) {
                            $scope.title = $filter('translate')('Documents.Label');
                        } else {
                            $scope.title = response.data.name;//TODO:Primary field get required for consistency. name field is temporary.
                        }

                    });

                    $scope.entityId = recordId;
                    $scope.entityType = type;
                }
            }

            function getDocuments(date) {
                $scope.loading = !date;

                DocumentService.getDocuments($rootScope.workgroup.tenant_id, $scope.entityId, $scope.module.id)
                    .then(function (response) {
                        $rootScope.processLanguages(response.data.documents);
                        var processResults = DocumentService.processDocuments(response.data, $rootScope.users, $scope.filter, $scope.sortPredicate, $scope.sortReverse);
                        $scope.documents = processResults.documentList;
                        documentsData = angular.copy(processResults.documentList);
                        $scope.loading = false;
                    });

            }

            getDocuments();

            $scope.filterDocuments = function (createdBy) {
                $scope.filter.created_by = createdBy;

                DocumentService.getDocuments($rootScope.workgroup.tenant_id, $scope.entityId, $scope.module.id)
                    .then(function (response) {
                        $rootScope.processLanguages(response.data.documents);
                        var processResults = DocumentService.processDocuments(response.data, $rootScope.users, $scope.filter, $scope.sortPredicate, $scope.sortReverse);
                        $scope.documents = processResults.documentList;
                        documentsData = angular.copy(processResults.documentList);
                    });

            };

            $scope.searchDocuments = function (searchKey) {
                var searchedDocuments = [];

                for (var i = 0; i < documentsData.length; i++) {
                    var document = documentsData[i];
                    var name = $rootScope.getLanguageValue(document.languages, 'name');
                    var description = $rootScope.getLanguageValue(document.languages, 'description');
                    if (!name)
                        return;

                    if (name.toLowerCase().indexOf(searchKey.toLowerCase()) > -1 || description.toLowerCase().indexOf(searchKey.toLowerCase()) > -1)
                        searchedDocuments.push(document);
                }

                var documentsResults = {
                    documents: searchedDocuments,
                    totalDocumentCount: searchedDocuments.length,
                    filteredDocumentCount: searchedDocuments.length
                };

                var processResults = DocumentService.processDocuments(documentsResults, $rootScope.users, $scope.filter, $scope.sortPredicate, $scope.sortReverse);
                $scope.documents = processResults.documentList;

            };

            $scope.sortDocuments = function (predicate, reverse) {
                $scope.sortPredicate = predicate;
                $scope.sortReverse = reverse;

                var documentsResults = {
                    documents: angular.copy(documentsData),
                    totalDocumentCount: angular.copy(documentsData).length,
                    filteredDocumentCount: angular.copy(documentsData).length//change if any need on filter
                };

                var processResults = DocumentService.processDocuments(documentsResults, $rootScope.users, $scope.filter, $scope.sortPredicate, $scope.sortReverse);
                $scope.documents = processResults.documentList;

            };

        }
    ]);