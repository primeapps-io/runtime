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


            $scope.templatesFields = [
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Text (Single Line)",
                    "label": "Text (Single Line)",
                    "icon": "k-i-foreground-color"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Text (Multi Line)",
                    "label": "Text (Multi Line)",
                    "icon": "k-i-table-align-top-left"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Number",
                    "label": "Number",
                    "icon": "k-i-custom-format"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Number (Auto) ",
                    "label": "Number (Auto) ",
                    "icon": "k-i-list-numbered"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Number (Decimal) ",
                    "label": "Number (Decimal) ",
                    "icon": "k-i-decimal-decrease"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Currency ",
                    "label": "Currency ",
                    "icon": "k-i-dollar"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Date ",
                    "label": "Date ",
                    "icon": "k-i-calendar"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Date / Time ",
                    "label": "Date / Time ",
                    "icon": "k-i-calendar-date"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Time ",
                    "label": "Time ",
                    "icon": "k-i-clock"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "E-mail ",
                    "label": "E-mail ",
                    "icon": "k-i-email"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Pick List ",
                    "label": "Pick List ",
                    "icon": "k-i-list-unordered"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Multi Select ",
                    "label": "Multi Select ",
                    "icon": "k-i-select-box"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Lookup ",
                    "label": "Lookup ",
                    "icon": "k-i-search"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Check Box",
                    "label": "Check Box",
                    "icon": "k-i-checkbox-checked"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Document",
                    "label": "Document",
                    "icon": "k-i-file"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Url",
                    "label": "Url",
                    "icon": "k-i-link-horizontal"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Location",
                    "label": "Location",
                    "icon": "k-i-marker-pin-target"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Image",
                    "label": "Image",
                    "icon": "k-i-image"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Rating",
                    "label": "Rating",
                    "icon": "k-i-star-outline"
                },
                {
                    "type": "item",
                    "order": 0,
                    "label_tr": "Combination",
                    "label": "Combination",
                    "icon": "k-i-hyperlink-open-sm"
                }

            ];


            $scope.module = [
                {
                    "fields": [
                        {
                            "label": "all 12",
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
                        }
                    ],
                    "effectAllowed": "all",
                    "name": "Section 2 "
                }
            ]

        }


    ]);