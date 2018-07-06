'use strict';

angular.module('primeapps')

    .controller('AdvDocumentSearchController', ['$rootScope', '$scope','config', '$location', 'guidEmpty', '$filter', 'ngToast', 'DocumentService', 'entityTypes', 'helper', '$localStorage','$window',
        function ($rootScope, $scope, config,$location, guidEmpty, $filter, ngToast, DocumentService, entityTypes, helper, $localStorage,$window) {


            $scope.loading = false;
            $scope.searchParameters = [];


            $scope.modulesList = [];

            angular.forEach($rootScope.modules, function (module) {

                var documentFields = $filter('filter')(module.fields, { data_type: 'document',document_search:true,deleted:false }, true);
                if(documentFields.length > 0){
                    $scope.modulesList.push(module);
                }

            })

            function addSearchParameters(text,selectedQuery,selectedOperator) {

                var searchParameter = {};

                searchParameter.searchText = text;
                searchParameter.selectedQueryOperator = selectedQuery;
                searchParameter.selectedOperator = selectedOperator;

                $scope.searchParameters.push(searchParameter);
            }

            $scope.resetSearchParameters = function () {
                $scope.searchParameters = [];
                addSearchParameters();
                $scope.searched = null;
            }

            addSearchParameters();

            $scope.documents = [];

            $scope.applyFilter = function (take,skip,toploader,bottomloader) {

                var documentFilters = [];

                angular.forEach($scope.searchParameters, function (parameter) {

                    var documentFilter = {};
                    documentFilter["searchText"] = parameter.searchText;
                    documentFilter["queryOperator"] = parameter.selectedQueryOperator;
                    documentFilter["operator"] = parameter.selectedOperator;

                    if(parameter.searchText && parameter.selectedQueryOperator){
                        documentFilters.push(documentFilter);
                    }
                });

                if(documentFilters.length > 0 && $scope.searchModule){

                    if(toploader){
                        $scope.loading = true;
                        $scope.documents = [];
                    }
                    if(bottomloader){
                        $scope.searched = true;
                        $scope.bottomloading = true;
                    }


                    var selectedModule = $scope.searchModule.name;

                    DocumentService.advancedSearch(selectedModule, documentFilters,take,skip)
                        .then(function (result) {
                            result = result.data;
                            if(result && result.data){

                                angular.forEach(result.data, function (dataset) {
                                    $scope.documents.push(dataset);
                                });

                                $scope.searchResult = $filter('translate')('Filter.FoundResult', { searchResultCount: result.totalCount });
                                $scope.errorAddingFilter = null;
                            }
                            else{
                                $scope.searchResult = $filter('translate')('Filter.FoundResult', { searchResultCount: 0 });
                                $scope.errorAddingFilter = $filter('translate')('Filter.NotIndexedModule');
                            }
                            $scope.loading = false;
                            $scope.searched = true;
                            $scope.bottomloading = false;

                        })
                        .catch(function () {

                            $scope.bottomloading = false;
                            $scope.loading = false;
                            $scope.searched = true;
                        });

                    $scope.searched = true;
                }
                else{
                    $scope.errorAddingFilter = $filter('translate')('Filter.NoSearchStarted');
                }
                $scope.searched = false;


            }
            
            $scope.searchParameterAdd= function (parameter) {

                var text = parameter.searchText;
                var selectedQuery = parameter.selectedQueryOperator;
                var selectedOperator = parameter.selectedOperator;

                $scope.errorAddingFilter = null;

                if ($scope.searchModule,parameter.searchText && parameter.selectedQueryOperator && parameter.selectedOperator) {
                    if ($scope.searchParameters.length <= 10) {

                        addSearchParameters(text,selectedQuery,selectedOperator);
                    }
                    else {
                        ngToast.create({ content: $filter('translate')('Filter.MaximumFilterWarning', { limit: 10 }), className: 'warning'});
                        $scope.errorAddingFilter = $filter('translate')('Filter.MaximumFilterWarning', { limit: 10 });
                    }

                    var lastHookParameter = $scope.searchParameters[$scope.searchParameters.length - 1];
                    lastHookParameter.searchText = null;
                    lastHookParameter.selectedQueryOperator = '';
                    lastHookParameter.selectedOperator = '';
                }
                else{
                    if(!$scope.searchModule){
                        $scope.errorAddingFilter = $filter('translate')('Filter.PleaseSelectModule');
                    }
                    else if(!parameter.searchText){
                        $scope.errorAddingFilter = $filter('translate')('Filter.PleaseSearchText');
                    }
                    else if(!parameter.selectedQueryOperator){
                        $scope.errorAddingFilter = $filter('translate')('Filter.PleaseQueryOperator');
                    }
                    else if(!parameter.selectedOperator){
                        $scope.errorAddingFilter = $filter('translate')('Filter.PleaseOperator');
                    }
                }

            }
            $scope.searchParameterRemove = function (itemname) {
                var index = $scope.searchParameters.indexOf(itemname);
                $scope.searchParameters.splice(index, 1);
            };


            $scope.downloadDocument = function (fullfilename,viewfilename,modulename,recordid) {

                var fieldName = fullfilename.split('.')[0];
                fieldName = fieldName.replace(recordid+"_","");
                var fileName=viewfilename.split('.')[0];
                var download =  config.apiUrl + 'Document/download_module_document?module=' + modulename+ '&fileNameExt=' + helper.getFileExtension(fullfilename)+ "&fileName=" + fileName  + "&fieldName=" + fieldName + "&recordId=" + recordid + '&access_token=' + $localStorage.read('access_token');
                $window.location = download;
            }
        }
    ]);