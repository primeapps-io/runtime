'use strict';

angular.module('primeapps')

    .controller('moduleDesignerController', ['$rootScope', '$scope', '$filter', '$location', '$state', 'ngToast', '$q', '$popover', '$modal', 'helper', '$timeout', 'dragularService', 'defaultLabels', '$interval', '$cache', 'systemRequiredFields', 'systemReadonlyFields', 'ModuleService', 'LayoutService',
        function ($rootScope, $scope, $filter, $location, $state, ngToast, $q, $popover, $modal, helper, $timeout, dragularService, defaultLabels, $interval, $cache, systemRequiredFields, systemReadonlyFields, ModuleService, LayoutService) {
            $scope.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'ss';
            $scope.tabTitle = 'ss';


            $scope.dragoverCallback = function (index, external, type, callback) {
                $scope.logListEvent('dragged over', index, external, type);
                // Invoke callback to origin for container types.
                if (type == 'container' && !external) {
                    console.log('Container being dragged contains ' + callback() + ' items');
                }

            };

            $scope.dropCallback = function (index, item, external, type) {
                $scope.logListEvent('dropped at', index, external, type);
                // Return false here to cancel drop. Return true if you insert the item yourself.
                return item;
            };

            $scope.logEvent = function (message) {
                console.log(message);
            };

            $scope.logListEvent = function (action, index, external, type) {
                var message = external ? 'External ' : '';
                message += type + ' element was ' + action + ' position ' + index;
                console.log(message);
            };

            $scope.templatesFields = [
                {
                    "type": "item",
                    "id": 2,
                    "name": "Text",
                    "order": 0,
                    "label_tr": "Cep Telefonu",
                    "label": "Cep Telefonu"
                }
            ];


            // Initialize model


            $scope.$watch('model', function (model) {
                $scope.modelAsJson = angular.toJson(model, true);

            }, true);


            $scope.model = {
                sections: [
                    {
                        column: 1,
                        name: 'Section 1',
                        fields: [
                            {
                                "type": "item",
                                "id": 2,
                                "name": "Text",
                                "order": 0,
                                "label_tr": "Cep Telefonu",
                                "label": "Cep Telefonu"
                            }
                        ]

                    },
                    {
                        column: 1,
                        name: 'Section 2',
                        fields: [
                            {
                                "type": "item",
                                "id": 2,
                                "name": "Text",
                                "order": 0,
                                "label_tr": "Cep Telefonu",
                                "label": "Cep Telefonu"
                            }
                        ]

                    }
                ]
            }


        }
    ]);