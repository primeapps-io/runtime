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
                return index < 10; // Disallow dropping in the third row.
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


            $scope.$watch('model', function (model) {
                $scope.modelAsJson = angular.toJson(module, true);
            }, true);


            $scope.templatesFields = [
                {
                    "type": "item",
                    "id": 2,
                    "name": "Text",
                    "order": 0,
                    "label_tr": "Cep Telefonu",
                    "label": "Cep Telefonu"
                },
                {
                    "type": "item",
                    "id": 2,
                    "name": "Text",
                    "order": 0,
                    "label_tr": "Yazı Tek Satır",
                    "label": "Yazı Tek Satır"
                },
                {
                    "type": "item",
                    "id": 2,
                    "name": "Text",
                    "order": 0,
                    "label_tr": "Yazı Çok Satır",
                    "label": "Yazı Çok Satır"
                },

                {
                    "type": "item",
                    "id": 2,
                    "name": "Text",
                    "order": 0,
                    "label_tr": "Sayı",
                    "label": "Sayı"
                },
                {
                    "type": "item",
                    "id": 2,
                    "name": "Text",
                    "order": 0,
                    "label_tr": "Sayı Otomatik",
                    "label": "Sayı Otomatik"
                },
                {
                    "type": "item",
                    "id": 2,
                    "name": "Text",
                    "order": 0,
                    "label_tr": "Sayı Otomatik",
                    "label": "Sayı Otomatik"
                },
                {
                    "type": "item",
                    "id": 2,
                    "name": "Text",
                    "order": 0,
                    "label_tr": "Sayı Ondalık",
                    "label": "Sayı Ondalık"
                }
            ];


            $scope.module = [
                {
                    "fields": [
                        {
                            "label": "all 12",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 10",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 15",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 13",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 14",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 16",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 11",
                            "effectAllowed": "all"
                        }
                    ],
                    "effectAllowed": "all",
                    "name": "Section 1 "
                },
                {
                    "fields": [
                        {
                            "label": "all 12",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 10",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 15",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 13",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 14",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 16",
                            "effectAllowed": "all"
                        },
                        {
                            "label": "all 11",
                            "effectAllowed": "all"
                        }
                    ],
                    "effectAllowed": "all",
                    "name": "Section 2 "
                }
            ]

        }


    ]);