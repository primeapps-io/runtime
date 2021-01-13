'use strict';


angular.module('primeapps')

    .directive('focus', ['$timeout',
        function ($timeout) {
            return function (scope, elem, attrs) {
                scope.$watch(attrs.focus, function (newval) {
                    if (newval) {
                        $timeout(function () {
                            elem[0].focus();
                        }, 0, false);
                    }
                });
            };
        }])

    .directive('ngPriorityNav', ['$timeout', '$window', 'PriorityNavService', '$interpolate', function ($timeout, $window, PriorityNavService, $interpolate) {
        return {
            restrict: 'A',
            priority: -999,
            link: function (scope, horizontalNav, attrs) {

                var
                    verticalNav =
                        angular.element(
                            '<div class="vertical-nav">' +
                            '<a href data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" class="more-link btn btn-secondary btn-sm"><span class="bubble"></span></a>' +
                            '<ul class="vertical-nav-dropdown dropdown-menu dropdowlist shadow"></ul>' +
                            '</div>'
                        ),
                    verticalNavMoreLink = angular.element(verticalNav[0].querySelector('.more-link')),
                    verticalNavDropDown = angular.element(verticalNav[0].querySelector('.vertical-nav-dropdown')),
                    verticalNavMoreLinkBubble = angular.element(verticalNav[0].querySelector('.bubble'));

                horizontalNav.addClass('priority-nav');
                horizontalNav.addClass(attrs.ngPriorityNavClass);
                verticalNavMoreLink.addClass(attrs.ngPriorityNavMoreLinkClass);
                verticalNavDropDown.addClass(attrs.ngPriorityNavDropDownClass);
                verticalNavMoreLinkBubble.addClass(attrs.ngPriorityNavBubbleClass);

                var initDebounced = PriorityNavService.debounce(function () {
                    init(horizontalNav, verticalNav, verticalNavDropDown, verticalNavMoreLinkBubble, false);
                }, 500); // Maximum run of 1 per 500 milliseconds


                function init(horizontalNav, verticalNav, verticalNavDropDown, verticalNavMoreLinkBubble, hardStart) {
                    horizontalNav.append(verticalNav).addClass('go-away'); //append it hidden so that we can get width
                    if (hardStart) { // remove all items (before render)
                        verticalNavDropDown.children().remove();
                        horizontalNav.children().remove();
                    }
                    $timeout(function () {
                        $timeout(function () { //weird/annoying - but need this to ensure is rendered
                            if (hardStart) {
                                verticalNavMoreLink[0].style.cssText = 'line-height:' + '1.5';
                                PriorityNavService.addIds(horizontalNav.children());
                            }
                            PriorityNavService.calculatebreakPoint(horizontalNav, verticalNav, verticalNavDropDown, verticalNavMoreLinkBubble);
                            $timeout(function () {
                                horizontalNav.removeClass('go-away');
                            }, 200);// default 200 milliseconds
                        });
                    });
                }

                ////for dynamic nav items, you can add the binded {{object/model}} into the priorityNav attribute...
                //// then if your object/model changes, then we re-run the directive
                if (!attrs.ngPriorityNav) { //normal init
                    init(horizontalNav, verticalNav, verticalNavDropDown, verticalNavMoreLinkBubble, true);
                } else { //init with listener
                    attrs.$observe('ngPriorityNav', function (val) {//this is probably not best way, but couldnt find another way
                        if (val) {
                            init(horizontalNav, verticalNav, verticalNavDropDown, verticalNavMoreLinkBubble, true);
                        }
                    }, true)
                }

                //re-init on
                angular.element($window)
                    .on('resize', initDebounced)
                    .on('orientationchange', initDebounced);
            }
        };
    }])

    .directive('blur', function () {
        return function (scope, elem, attrs) {
            elem.bind('blur', function () {
                scope.$apply(attrs.blur);
            });
        };
    })

    .directive('languageClass', function () {
        return {
            restrict: 'A',
            controller: ['$rootScope', '$scope', '$element',
                function ($rootScope, $scope, $element) {
                    var language = function () {
                        return $rootScope.language;
                    };

                    var addClass = function (newLanguage, oldLanguage) {
                        if (oldLanguage)
                            $element.removeClass(oldLanguage);

                        return $element.addClass(newLanguage);
                    };

                    addClass($rootScope.language);

                    return $scope.$watch(language, function (newVal, oldVal) {
                        if (newVal === oldVal) {
                            return;
                        }

                        return addClass(newVal, oldVal);
                    });
                }
            ]
        };
    })

    .directive('customOnChange', function () {
        return {
            require: 'ngModel',
            link: function (scope, element, attrs, ngModel) {
                element.bind('change', function () {
                    scope.$apply(function () {
                        if (element.length > 0) {
                            ngModel.$setViewValue(element[0].files[0].name);
                            ngModel.$render();
                        }

                    });
                });
            }
        };
    })

    .directive('autoGrow', function () {
        return function (scope, element, attr) {
            var update = function () {
                element.css('height', 'auto');

                var offsetHeight = element[0].offsetHeight;
                var scrollHeight = element[0].scrollHeight;

                if (scrollHeight > offsetHeight) {
                    element.css('height', scrollHeight + 'px');
                }
            };

            scope.$watch(attr.ngModel, function () {
                update();
            });

            element.bind('focus', function () {
                update();
            });

            attr.$set('ngTrim', 'false');
        };
    })

    .directive('resetField', ['$compile', '$timeout',
        function ($compile, $timeout) {
            return {
                require: 'ngModel',
                scope: {},
                link: function (scope, element, attrs, ctrl) {
                    var inputTypes = /text|search|tel|url|email|password/i;

                    if (element[0].nodeName !== 'INPUT')
                        throw new Error('resetField is limited to input elements');

                    if (!inputTypes.test(attrs.type))
                        throw new Error('Invalid input type for resetField: ' + attrs.type);

                    var template = $compile('<i ng-show="enabled" ng-mousedown="reset()" class="fa fa-times-circle"></i>')(scope);
                    element.after(template);

                    scope.reset = function () {
                        ctrl.$setViewValue(null);
                        ctrl.$render();
                        $timeout(function () {
                            element[0].focus();
                        }, 0, false);
                    };

                    element.bind('input', function () {
                        scope.enabled = !ctrl.$isEmpty(element.val());
                    })
                        .bind('focus', function () {
                            scope.enabled = !ctrl.$isEmpty(element.val());
                            scope.$apply();
                        })
                        .bind('blur', function () {
                            scope.enabled = false;
                            scope.$apply();
                        });
                }
            };
        }])

    .directive('compareTo', function () {
        return {
            require: 'ngModel',
            scope: {
                otherModelValue: '=compareTo'
            },
            link: function (scope, element, attributes, ngModel) {

                ngModel.$validators.compareTo = function (modelValue) {
                    return modelValue == scope.otherModelValue;
                };

                scope.$watch('otherModelValue', function () {
                    ngModel.$validate();
                });
            }
        };
    })

    .directive('ngThumb', ['$window',
        function ($window) {
            var helper = {
                support: !!($window.FileReader && $window.CanvasRenderingContext2D),
                isFile: function (item) {
                    return angular.isObject(item) && item instanceof $window.File;
                },
                isImage: function (file) {
                    var type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
                    return '|jpg|png|jpeg|bmp|gif|'.indexOf(type) > -1;
                }
            };

            return {
                restrict: 'A',
                template: '<canvas/>',
                link: function (scope, element, attributes) {
                    if (!helper.support) return;

                    var params = scope.$eval(attributes.ngThumb);

                    if (!helper.isFile(params.file)) return;
                    if (!helper.isImage(params.file)) return;

                    var canvas = element.find('canvas');
                    var reader = new FileReader();

                    reader.onload = onLoadFile;
                    reader.readAsDataURL(params.file);

                    function onLoadFile(event) {
                        var img = new Image();
                        img.onload = onLoadImage;
                        img.src = event.target.result;
                    }

                    function onLoadImage() {
                        var width = params.width || this.width / this.height * params.height;
                        var height = params.height || this.height / this.width * params.width;
                        canvas.attr({ width: width, height: height });
                        canvas[0].getContext('2d').drawImage(this, 0, 0, width, height);
                    }
                }
            };
        }])

    .directive('numeric', function () {
        return {
            require: 'ngModel',
            scope: {
                min: '=minValue',
                max: '=maxValue',
                ngRequired: '=ngRequired'
            },
            link: function (scope, element, attrs, modelCtrl) {
                modelCtrl.$parsers.push(function (inputValue) {
                    if (inputValue === undefined || inputValue.indexOf(' ').length > -1)
                        return '';

                    var transformedInput = inputValue.replace(/[^0-9]/g, '');
                    if (transformedInput != inputValue) {
                        modelCtrl.$setViewValue(transformedInput);
                        modelCtrl.$render();
                    }

                    modelCtrl.$validators.min = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.min !== 'undefined') {
                            return cVal >= parseInt(scope.min);
                        }
                        return true;
                    };

                    scope.$watch('min', function (val) {
                        modelCtrl.$validate();
                    });

                    modelCtrl.$validators.max = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.max !== 'undefined') {
                            return cVal <= parseInt(scope.max);
                        }
                        return true;
                    };

                    scope.$watch('max', function (val) {
                        modelCtrl.$validate();
                    });

                    return transformedInput;
                });
            }
        };
    })

    .directive('restrict', ['$parse',
        function ($parse) {
            return {
                restrict: 'A',
                require: 'ngModel',
                link: function (scope, element, attrs) {
                    scope.$watch(attrs.ngModel, function (value) {
                        if (!value) {
                            return;
                        }

                        $parse(attrs.ngModel).assign(scope, value.replace(new RegExp(attrs.restrict, 'g'), ''));
                    });
                }
            }
        }])

    .directive('placeholder', ['$timeout',
        function ($timeout) {
            var inp = document.createElement('input');

            if ('placeholder' in inp) {
                return {}
            }

            return {
                link: function (scope, elm, attrs) {
                    $timeout(function () {
                        elm.val(attrs.placeholder);

                        elm.bind('focus', function () {
                            if (elm.val() == attrs.placeholder) {
                                elm.val('');
                            }
                        }).bind('blur', function () {
                            if (elm.val() == '') {
                                elm.val(attrs.placeholder);
                            }
                        });
                    });
                }
            }
        }])

    .directive('ngEnter', function () {
        return function (scope, element, attrs) {
            element.bind('keydown keypress', function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.ngEnter);
                    });

                    event.preventDefault();
                }
            });
        };
    })

    .directive('subTable', ['$rootScope', 'ngTableParams', 'blockUI', '$filter', '$cache', 'helper', 'exportFile', 'operations', 'ModuleService', 'components', 'mdToast',
        function ($rootScope, ngTableParams, blockUI, $filter, $cache, helper, exportFile, operations, ModuleService, components, mdToast) {
            return {
                restrict: 'EA',
                scope: {
                    relatedModule: '=',
                    parentModule: '=',
                    reload: '=',
                    showFilter: '=',
                    isSelectable: '@',
                    disableSelectAll: '@',
                    disableLinks: '@'
                },
                templateUrl: 'view/common/subtable.html?v=' + version,
                controller: ['$scope',
                    function ($scope) {
                        $scope.loading = true;
                        $scope.relatedModule.loading = true;
                        $scope.module = $filter('filter')($rootScope.modules, { name: $scope.relatedModule.related_module }, true)[0];
                        $scope.type = $scope.relatedModule.related_module;
                        $scope.readonly = $scope.relatedModule.readonly || false;
                        $scope.parentType = $scope.relatedModule.relation_field;
                        $scope.parentId = $scope.$parent.id;
                        $scope.language = $rootScope.language;
                        $scope.operations = operations;
                        $scope.hasPermission = helper.hasPermission;
                        $scope.lookupUser = helper.lookupUser;
                        $scope.relatedModuleInModal = ($scope.$parent.selectedRelatedModule && $scope.$parent.selectedRelatedModule.relatedModuleInModal) ? true : false;
                        $scope.previousParentType = $scope.$parent.previousParentType;
                        $scope.previousParentId = $scope.$parent.previousParentId;
                        $scope.previousReturnTab = $scope.$parent.previousReturnTab;
                        $scope.isAdmin = $rootScope.user.profile.has_admin_rights;
                        $scope.hideDeleteAll = $filter('filter')($rootScope.deleteAllHiddenModules, $scope.parentModule + '|' + $scope.type, true)[0];

                        var counts = [10, 25, 50, 100];
                        var displayFields = $scope.relatedModule.display_fields;

                        var parent = $scope.parentType + (!$scope.isSelectable ? $scope.parentId : '');
                        $scope.cacheKey = parent + '_' + $scope.module.name;
                        $scope.$parent['selectedRows' + $scope.type] = [];
                        var isManyToMany = $scope.relatedModule.relation_type === 'many_to_many';
                        var filters = [];
                        if ($scope.isSelectable === undefined || $scope.isSelectable === null)
                            $scope.isSelectable = false;

                        if (!$scope.isSelectable) {
                            var fieldName = $scope.parentType;

                            if (isManyToMany)
                                fieldName = $scope.parentModule != $scope.module.name ? $scope.parentModule + '_id' : $scope.parentModule + '1_id';

                            filters.push({
                                field: fieldName,
                                operator: 'equals',
                                value: $scope.parentId,
                                no: 1
                            });

                            if ($scope.parentType === 'related_to') {
                                var parentModule = $filter('filter')($rootScope.modules, { name: $scope.parentModule }, true)[0];
                                $scope.parentType = $scope.parentModule;

                                filters.push({
                                    field: 'related_module',
                                    operator: 'is',
                                    value: parentModule['label_' + $rootScope.user.tenant_language + '_singular'],
                                    no: 1
                                });
                            }
                        }

                        //should be true for tables and subtables to delete selecteds
                        $scope.isSelectable = true;

                        if (isManyToMany)
                            $scope.cacheKey = parent + '_' + $scope.relatedModule.relation_field + '_' + $scope.relatedModule.related_module;

                        var tableBlockUI = blockUI.instances.get('tableBlockUISubTable' + $scope.cacheKey);

                        ModuleService.setTable($scope, tableBlockUI, counts, 10, angular.copy(filters), parent, $scope.type, $scope.isSelectable, (!$scope.disableLinks ? $scope.parentId : null), (!$scope.disableLinks ? $scope.parentType : null), displayFields, $scope.relatedModule, $scope.parentModule, $scope.relatedModule.id, $scope.previousParentType, $scope.previousParentId, $scope.previousReturnTab, $scope.$parent);

                        if ($scope.tableParams)
                            $scope.tableParams.disableSelectAll = $scope.disableSelectAll;

                        $scope.isManyToManyModal = true;

                        $scope.refresh = function (clearFilter) {
                            $cache.remove($scope.cacheKey);

                            if (clearFilter) {
                                $scope.tableParams.filterList = filters;
                                $scope.tableParams.refreshing = true;
                            }

                            $scope.tableParams.reloading = true;
                            $scope.tableParams.reload();
                        };

                        $scope.$watch('reload', function (value) {
                            if (!value)
                                return;

                            $scope.refresh(false);
                        });

                        $scope.$watch('showFilter', function (value) {
                            if (!value)
                                return;

                            $scope.tableParams.showFilter = !$scope.tableParams.showFilter
                        });

                        $scope.delete = function (id) {
                            ModuleService.getRecord($scope.module.name, id)
                                .then(function (recordResponse) {
                                    var record = ModuleService.processRecordSingle(recordResponse.data, $scope.$parent.$parent.module, $scope.$parent.$parent.picklistsModule);
                                    $scope.executeCode = false;
                                    components.run('BeforeDelete', 'Script', $scope, record);

                                    if ($scope.executeCode)
                                        return;

                                    ModuleService.deleteRecord($scope.module.name, id)
                                        .then(function (deletedRecordCount) {
                                            setTimeout(function () {
                                                ModuleService.getRecord($scope.parentModule, $scope.parentId)
                                                    .then(function (response) {
                                                        var record = ModuleService.processRecordSingle(response.data, $scope.$parent.$parent.module, $scope.$parent.$parent.picklistsModule);
                                                        ModuleService.formatRecordFieldValues(record, $scope.$parent.$parent.module, $scope.$parent.$parent.picklistsModule);
                                                        components.run('AfterDelete', 'Script', $scope, record);
                                                        $scope.$parent.$parent.$parent.record = record;
                                                        var parentCacheKey = $scope.parentModule + '_' + $scope.parentModule;
                                                        $cache.remove(parentCacheKey);
                                                        $scope.tableParams.reload();
                                                    });
                                            }, 1000);

                                        });
                                });
                        };

                        $scope.deleteRelation = function (id) {
                            var relation = {};
                            relation[$scope.parentModule + '_id'] = parseInt($scope.parentId);
                            relation[$scope.relatedModule.related_module + '_id'] = id;

                            ModuleService.deleteRelation($scope.parentModule, $scope.relatedModule.related_module, relation)
                                .then(function (deletedRecordCount) {
                                    $cache.remove($scope.cacheKey);
                                    $scope.tableParams.reload();
                                });
                        };

                        $scope.multiselect = function (searchTerm, field) {
                            var picklistItems = [];

                            angular.forEach($scope.tableParams.picklists[field.picklist_id], function (picklistItem) {
                                if (picklistItem.inactive)
                                    return;

                                if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                                    picklistItems.push(picklistItem);
                            });

                            return picklistItems;
                        };

                        $scope.selectAllModal = function (event, data) {
                            $scope.$parent['selectedRows' + $scope.type] = [];
                            if ($scope.isAllSelectedModal) {
                                $scope.isAllSelectedModal = false;
                            } else {
                                $scope.isAllSelectedModal = true;
                                angular.forEach(data, function (record) {
                                    record.fields.forEach(function (field) {
                                        /*find primary field and get its value*/
                                        if (field.primary == true) {
                                            /*add selected record*/
                                            $scope.$parent['selectedRows' + $scope.type].push({
                                                id: record.id,
                                                displayName: field.valueFormatted
                                            });
                                        }
                                    });
                                });
                            }
                        };

                        $scope.selectRow = function ($event, record) {
                            /*selects or unselects records*/
                            if ($event.target.checked) {
                                record.fields.forEach(function (field) {
                                    /*find primary field and get its value*/
                                    if (field.primary == true && !field.isJoin) {
                                        /*add selected record*/
                                        $scope.$parent['selectedRows' + $scope.type].push({
                                            id: record.id,
                                            displayName: field.valueFormatted
                                        });
                                    }
                                });
                            } else {
                                $scope.$parent['selectedRows' + $scope.type] = $scope.$parent['selectedRows' + $scope.type].filter(function (selectedItem) {
                                    return selectedItem.id != record.id;
                                });

                            }

                            $scope.isAllSelectedModal = false;
                        };

                        $scope.isRowSelected = function (id) {
                            return $scope.$parent['selectedRows' + $scope.type].filter(function (selectedItem) {
                                return selectedItem.id == id;
                            }).length > 0;
                        };

                        $scope.$parent.$parent.$parent.isManyToMany = isManyToMany;

                        //global delete selecteds function
                        $scope.deleteSelectedsSubTable = function () {
                            if (!$scope.relatedModuleInModal) {
                                if (!$scope.$parent['selectedRows' + $scope.type] || !$scope.$parent['selectedRows' + $scope.type].length)
                                    return;

                                var records = [];
                                $scope.$parent['selectedRows' + $scope.type].filter(function (itm) {
                                    records.push(itm.id)
                                });

                                ModuleService.deleteRecordBulk($scope.module.name, records)
                                    .then(function (response) {
                                        $cache.remove($scope.cacheKey);
                                        $scope.tableParams.reloading = true;
                                        $scope.tableParams.reload();
                                        $scope.$parent['selectedRows' + $scope.type] = [];
                                        $scope.isAllSelectedModal = false;
                                        var deletedCount = {};
                                        deletedCount.data = records.length;
                                    });
                            }
                        };

                        //global export function
                        $scope.export = function () {
                            if (!$scope.relatedModuleInModal) {
                                if ($scope.tableParams.total() < 1)
                                    return;

                                var isFileSaverSupported = false;

                                try {
                                    isFileSaverSupported = !!new Blob;
                                } catch (e) {
                                }

                                if (!isFileSaverSupported) {
                                    mdToast.warning({
                                        content: $filter('translate')('Module.ExportUnsupported'),
                                        timeout: 8000
                                    });
                                    return;
                                }

                                if ($scope.tableParams.total() > 3000) {
                                    mdToast.warning({
                                        content: $filter('translate')('Module.ExportWarning'),
                                        timeout: 8000
                                    });
                                    return;
                                }

                                var fileName = $scope.module['label_' + $rootScope.language + '_plural'] + '-' + $filter('date')(new Date(), 'dd-MM-yyyy') + '.xls';
                                $scope.exporting = true;

                                ModuleService.getCSVData($scope, $scope.type)
                                    .then(function (csvData) {
                                        mdToast.success({
                                            content: $filter('translate')('Module.ExcelExportSuccess'),
                                            timeout: 5000
                                        });
                                        exportFile.excel(csvData, fileName);
                                        $scope.exporting = false;
                                    });
                            }

                        };
                    }]
            };
        }])

    .directive('numberCurrency', ['$rootScope', '$filter', '$locale', 'helper',
        function ($rootScope, $filter, $locale, helper) {
            return {
                restrict: 'A',
                require: 'ngModel',
                scope: {
                    minValue: '=',
                    maxValue: '=',
                    currencySymbol: '=',
                    ngRequired: '=',
                    places: '=',
                    rounding: '='
                },
                link: function (scope, element, attrs, ngModel) {
                    if (attrs.numberCurrency === 'false') return;

                    var places = (typeof scope.places !== 'undefined' && scope.places != null) ? scope.places : 2;
                    var rounding = (typeof scope.rounding !== 'undefined' && scope.rounding != null) ? scope.rounding : 'none';

                    function decimalRex(dChar) {
                        return RegExp("\\d|\\-|\\" + dChar, 'g');
                    }

                    function clearRex(dChar) {
                        return RegExp("\\-{0,1}((\\" + dChar + ")|([0-9]{1,}\\" + dChar + "?))&?[0-9]{0," + 100 + "}", 'g');
                    }

                    function parseValue(value) {
                        value = String(value);
                        var dSeparator = $locale.NUMBER_FORMATS.DECIMAL_SEP;
                        var valueFloat = null;

                        // Replace negative pattern to minus sign (-)
                        var neg_dummy = $filter('currency')("-1", getCurrencySymbol(), scope.places);
                        var neg_idx = neg_dummy.indexOf("1");
                        var neg_str = neg_dummy.substring(0, neg_idx);
                        value = value.replace(neg_str, "-");

                        if (RegExp("^-[\\s]*$", 'g').test(value)) {
                            value = "-0";
                        }

                        if (decimalRex(dSeparator).test(value)) {
                            var cleared = value.match(decimalRex(dSeparator)).join("").match(clearRex(dSeparator));

                            if (cleared)
                                cleared = cleared[0].replace(dSeparator, ".");

                            if (!cleared)
                                return null;

                            valueFloat = parseFloat(cleared);

                            switch (rounding) {
                                case 'off':
                                    valueFloat = helper.roundBy(Math.round, valueFloat, places);
                                    break;
                                case 'down':
                                    valueFloat = helper.roundBy(Math.floor, valueFloat, places);
                                    break;
                                case 'up':
                                    valueFloat = helper.roundBy(Math.ceil, valueFloat, places);
                                    break;
                            }
                        }

                        return valueFloat;
                    }

                    function clearValue(value) {
                        value = String(value);
                        var dSeparator = $locale.NUMBER_FORMATS.DECIMAL_SEP;
                        var cleared = null;

                        // Replace negative pattern to minus sign (-)
                        var neg_dummy = $filter('currency')("-1", getCurrencySymbol(), scope.places);
                        var neg_idx = neg_dummy.indexOf("1");
                        var neg_str = neg_dummy.substring(0, neg_idx);
                        value = value.replace(neg_str, "-");

                        if (RegExp("^-[\\s]*$", 'g').test(value)) {
                            value = "-0";
                        }

                        if (decimalRex(dSeparator).test(value)) {
                            cleared = value.match(decimalRex(dSeparator))
                                .join("").match(clearRex(dSeparator));
                            cleared = cleared ? cleared[0].replace(dSeparator, ".") : null;
                        }

                        return cleared;
                    }

                    function getCurrencySymbol() {
                        if (scope.currencySymbol) {
                            if (scope.currencySymbol === 'false')
                                scope.currencySymbol = '';

                            return scope.currencySymbol;
                        } else {
                            if (!$rootScope.currencySymbol) {
                                if ($rootScope.language === 'tr') {
                                    $locale.NUMBER_FORMATS.CURRENCY_SYM = '₺'
                                } else if ($rootScope.language === 'en') {
                                    $locale.NUMBER_FORMATS.CURRENCY_SYM = '$'
                                }
                                return $locale.NUMBER_FORMATS.CURRENCY_SYM;
                            }
                            else
                                return $rootScope.currencySymbol;
                        }
                    }

                    function reformatViewValue() {
                        var formatters = ngModel.$formatters,
                            idx = formatters.length;

                        var viewValue = ngModel.$$rawModelValue;

                        while (idx--) {
                            viewValue = formatters[idx](viewValue);
                        }

                        ngModel.$setViewValue(viewValue);
                        ngModel.$render();
                    }

                    ngModel.$parsers.push(function (viewValue) {
                        var cVal = parseValue(viewValue);
                        //return parseFloat(cVal);
                        // Check for fast digitation (-. or .)
                        if (cVal == "." || cVal == "-.") {
                            cVal = ".0";
                        }
                        return parseFloat(cVal);
                    });

                    element.on('blur', function () {
                        ngModel.$commitViewValue();
                        reformatViewValue();
                    });

                    ngModel.$formatters.unshift(function (value) {
                        return $filter('currency')(value, getCurrencySymbol(), scope.places);
                    });

                    ngModel.$validators.min = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.minValue !== 'undefined') {
                            return cVal >= parseFloat(scope.minValue);
                        }
                        return true;
                    };

                    ngModel.$validators.max = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.maxValue !== 'undefined') {
                            return cVal <= parseFloat(scope.maxValue);
                        }
                        return true;
                    };

                    scope.$watch('maxValue', function (val) {
                        ngModel.$validate();
                    });

                    scope.$watch('minValue', function (val) {
                        ngModel.$validate();
                    });

                    scope.$watch('currencySymbol', function (val) {
                        ngModel.$commitViewValue();
                        reformatViewValue();
                    });

                    ngModel.$validators.places = function (cVal) {
                        if (!!cVal && isNaN(cVal)) {
                            return false;
                        }

                        return true;
                    };
                }
            }
        }])

    .directive('numberDecimal', ['$rootScope', '$filter', '$locale', 'helper',
        function ($rootScope, $filter, $locale, helper) {
            return {
                restrict: 'A',
                require: 'ngModel',
                scope: {
                    min: '=minValue',
                    max: '=maxValue',
                    ngRequired: '=',
                    places: '=',
                    rounding: '='
                },
                link: function (scope, element, attrs, ngModel) {
                    if (attrs.numberDecimal === 'false') return;

                    var places = (typeof scope.places !== 'undefined' && scope.places != null) ? scope.places : 2;
                    var rounding = (typeof scope.rounding !== 'undefined' && scope.rounding != null) ? scope.rounding : 'none';

                    function decimalRex(dChar) {
                        return RegExp("\\d|\\-|\\" + dChar, 'g');
                    }

                    function clearRex(dChar) {
                        return RegExp("\\-{0,1}((\\" + dChar + ")|([0-9]{1,}\\" + dChar + "?))&?[0-9]{0," + 100 + "}", 'g');
                    }

                    function parseValue(value) {
                        value = String(value);
                        var dSeparator = $locale.NUMBER_FORMATS.DECIMAL_SEP;
                        var valueFloat = null;

                        // Replace negative pattern to minus sign (-)
                        var neg_dummy = $filter('number')("-1", scope.places);
                        var neg_idx = neg_dummy.indexOf("1");
                        var neg_str = neg_dummy.substring(0, neg_idx);
                        value = value.replace(neg_str, "-");

                        if (RegExp("^-[\\s]*$", 'g').test(value)) {
                            value = "-0";
                        }

                        if (decimalRex(dSeparator).test(value)) {
                            var cleared = value.match(decimalRex(dSeparator)).join("").match(clearRex(dSeparator));

                            if (cleared)
                                cleared = cleared[0].replace(dSeparator, ".");

                            if (!cleared)
                                return null;

                            valueFloat = parseFloat(cleared);

                            switch (rounding) {
                                case 'off':
                                    valueFloat = helper.roundBy(Math.round, valueFloat, places);
                                    break;
                                case 'down':
                                    valueFloat = helper.roundBy(Math.floor, valueFloat, places);
                                    break;
                                case 'up':
                                    valueFloat = helper.roundBy(Math.ceil, valueFloat, places);
                                    break;
                            }
                        }

                        return valueFloat;
                    }

                    function reformatViewValue() {
                        var formatters = ngModel.$formatters,
                            idx = formatters.length;

                        var viewValue = ngModel.$$rawModelValue;

                        while (idx--) {
                            viewValue = formatters[idx](viewValue);
                        }

                        ngModel.$setViewValue(viewValue);
                        ngModel.$render();
                    }

                    ngModel.$parsers.push(function (viewValue) {
                        var cVal = parseValue(viewValue);
                        //return parseFloat(cVal);
                        // Check for fast digitation (-. or .)
                        if (cVal == "." || cVal == "-.") {
                            cVal = ".0";
                        }
                        return parseFloat(cVal);
                    });

                    element.on('blur', function () {
                        ngModel.$commitViewValue();
                        reformatViewValue();
                    });

                    ngModel.$formatters.unshift(function (value) {
                        return $filter('number')(value, scope.places);
                    });

                    ngModel.$validators.min = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.min !== 'undefined') {
                            return cVal >= parseFloat(scope.min);
                        }
                        return true;
                    };

                    scope.$watch('min', function (val) {
                        ngModel.$validate();
                    });

                    ngModel.$validators.max = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.max !== 'undefined') {
                            return cVal <= parseFloat(scope.max);
                        }
                        return true;
                    };

                    scope.$watch('max', function (val) {
                        ngModel.$validate();
                    });

                    ngModel.$validators.places = function (cVal) {
                        if (!!cVal && isNaN(cVal)) {
                            return false;
                        }

                        return true;
                    };
                }
            }
        }])

    .directive('tooltip', function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                $(element).hover(function () {
                    // on mouseenter
                    $(element).tooltip('show');
                }, function () {
                    // on mouseleave
                    $(element).tooltip('hide');
                });
            }
        };
    })

    .directive('webHook', ['$http', '$filter', 'mdToast',
        function ($http, $filter, mdToast) {
            return {
                restrict: 'A',
                link: function (scope, element, attrs) {
                    element.bind('click', function () {
                        var actionDetails = angular.fromJson(attrs['webHook']);
                        if (actionDetails.template && actionDetails.url) {
                            scope.loading = true;
                            var templateData = actionDetails.template.split(',');
                            var data = {};
                            var recordData = scope.$parent.$parent.record;

                            angular.forEach(templateData, function (value, key) {
                                var inputvalue = recordData[value];
                                if (inputvalue) {
                                    if (inputvalue.length > 0) {//object check
                                        data[value] = inputvalue;
                                    } else {
                                        data[value] = inputvalue['labelStr'];
                                    }
                                }
                            });

                            $http.post(actionDetails.url, data)
                                .then(function (data) {

                                    mdToast.success($filter('translate')('Common.ProcessTriggerSuccess'));
                                })
                                .error(function () {
                                    mdToast.error($filter('translate')('Common.Error'));
                                    scope.loading = false;
                                });
                        }

                    });
                }
            };
        }])

    .directive('customModalFrame', [function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                element.bind('click', function () {
                    var actionDetails = angular.fromJson(attrs['customModalFrame']);
                    //Future usage
                });
            }
        };
    }])
    .directive('uiTinymceMulti', ['$rootScope', 'uiTinymceConfig', function ($rootScope, uiTinymceConfig) {
        uiTinymceConfig = uiTinymceConfig || {};
        var generatedIds = 0;
        return {
            require: 'ngModel',
            link: function (scope, elm, attrs, ngModel) {
                var expression, options, tinyInstance;

                // generate an ID if not present
                if (!attrs.id) {
                    attrs.$set('id', 'uiTinymce' + generatedIds++);
                }

                options = {
                    // Update model when calling setContent (such as from the source editor popup)
                    setup: function (ed) {
                        ed.on('init', function (args) {
                            ngModel.$render();
                        });
                        // Update model on button click
                        ed.on('ExecCommand', function (e) {
                            ed.save();
                            ngModel.$setViewValue(elm.val());
                            if (!scope.$$phase) {
                                scope.$apply();
                            }
                        });
                        // Update model on keypress
                        ed.on('KeyUp', function (e) {
                            ed.save();
                            ngModel.$setViewValue(elm.val());
                            if (!scope.$$phase) {
                                scope.$apply();
                            }
                        });
                    },
                    style_formats: [
                        {
                            title: ($rootScope.language === 'tr' ? 'Yazı Boyutu' : 'Font Size'), items: [
                                {
                                    title: ($rootScope.language === 'tr' ? 'Çok Büyük' : 'Very Big'),
                                    block: 'h2',
                                    styles: { fontWeight: 'normal' }
                                },
                                {
                                    title: ($rootScope.language === 'tr' ? 'Büyük' : 'Big'),
                                    block: 'h3',
                                    styles: { fontWeight: 'normal' }
                                },
                                {
                                    title: ($rootScope.language === 'tr' ? 'Normal' : 'Normal'),
                                    block: 'h4',
                                    styles: { fontWeight: 'normal' }
                                },
                                {
                                    title: ($rootScope.language === 'tr' ? 'Küçük' : 'Small'),
                                    block: 'h5',
                                    styles: { fontWeight: 'normal' }
                                },
                                {
                                    title: ($rootScope.language === 'tr' ? 'Çok Küçük' : 'Very Small'),
                                    block: 'h6',
                                    styles: { fontWeight: 'normal' }
                                }
                            ]
                        }
                    ],
                    mode: 'exact',
                    elements: attrs.id,
                    language: $rootScope.language,
                    menubar: false,
                    statusbar: false,
                    plugins: 'fullscreen paste',
                    paste_as_text: true,
                    toolbar: 'bold italic bullist numlist | styleselect | fullscreen',
                    skin: 'lightgray',
                    theme: 'modern',
                    height: '200'
                };

                if (attrs.uiTinymce) {
                    expression = scope.$eval(attrs.uiTinymce);
                } else {
                    expression = {};
                }

                angular.extend(options, uiTinymceConfig, expression);

                setTimeout(function () {
                    tinymce.init(options);
                });

                ngModel.$render = function () {
                    if (!tinyInstance) {
                        tinyInstance = tinymce.get(attrs.id);
                    }
                    if (tinyInstance) {
                        tinyInstance.setContent(ngModel.$viewValue || '');
                    }
                };
            }
        };
    }])

    .directive('location', ['$rootScope', 'config', '$filter', '$timeout',
        function ($rootScope, config, $filter, $timeout) {
            return {
                restrict: 'E',
                require: '^?ngModel',

                link: function (scope, element, attrs, ngModel) {

                    var conf = {
                        latitude: 39.93948807471046,
                        longitude: 32.85907745361328,
                        zoom: 5
                    };

                    function success(pos) {
                        conf.latitude = pos.coords.latitude;
                        conf.longitude = pos.coords.longitude;
                        conf.zoom = 10;
                        getlocation();
                    };

                    function error() {
                        getlocation();
                    }

                    navigator.geolocation.getCurrentPosition(success, error);

                    function getlocation() {
                        var mapCanvas = element[0];

                        if (scope.addres && !scope.location) {
                            var geocoder = new google.maps.Geocoder();
                            geocoder.geocode({ 'address': scope.addres }, function (results, status) {
                                $timeout(function () {
                                    if (status === 'OK') {
                                        conf.latitude = results[0].geometry.location.lat();
                                        conf.longitude = results[0].geometry.location.lng();
                                        map(mapCanvas);

                                    } else {
                                        map(mapCanvas);
                                    }
                                });

                            });
                        } else {
                            map(mapCanvas);
                        }

                    }

                    function map(mapCanvas) {
                        if (scope.location) {
                            var location = scope.location.split(",");
                            conf.latitude = location[0];
                            conf.longitude = location[1];
                            conf.zoom = 17;
                        }
                        var defaultCoord = new google.maps.LatLng(conf.latitude, conf.longitude);
                        var mapOptions = { center: defaultCoord, zoom: conf.zoom };
                        var map = new google.maps.Map(mapCanvas, mapOptions);

                        var marker = new google.maps.Marker({
                            draggable: true,
                            animation: google.maps.Animation.DROP,
                            position: defaultCoord,
                        });
                        ngModel.$setViewValue(conf.latitude + "," + conf.longitude);
                        var coordInfoWindow = new google.maps.InfoWindow();
                        map.addListener('mouseup', function (e) {
                            var latlng = e.latLng;

                            ngModel.$setViewValue(latlng.lat() + "," + latlng.lng());
                            coordInfoWindow.setContent(latlng.lat() + "," + latlng.lng());
                            coordInfoWindow.open(map, marker);

                        });
                        map.addListener('click', function (e) {
                            var latlng = e.latLng;
                            var latlng = e.latLng;
                            scope.ngModel = latlng.lat() + "," + latlng.lng();
                            coordInfoWindow.setContent(latlng.lat() + "," + latlng.lng());
                            marker.setPosition(latlng);
                            coordInfoWindow.open(map, marker);
                            ngModel.$setViewValue(latlng.lat() + "," + latlng.lng());
                        });
                        marker.setMap(map);
                    }
                }

            };
        }])

    .directive('helpPage', ['$rootScope', '$http', 'config', '$filter', 'HelpService', '$sce', '$cache', '$localStorage',
        function ($rootScope, $http, config, $filter, HelpService, $sce, $cache, $localStorage) {
            return {
                restrict: 'EA',
                scope: {
                    moduleId: '=',
                    route: '=',
                },
                controller: ['$scope',
                    function ($scope) {
                        if ($rootScope.isMobile()) {
                            return false;
                        }

                        if ($rootScope.firtScreenShow && !$scope.moduleId) {
                            if (!$rootScope.helpPageFirstScreen) {
                                $rootScope.helpTemplatesModal = undefined;
                                $rootScope.show = false;
                                return;
                            }

                            $rootScope.helpTemplatesModal = $rootScope.helpPageFirstScreen;
                            if (!angular.isObject($rootScope.helpTemplatesModal.template))
                                $rootScope.helpTemplatesModal.template = $sce.trustAsHtml($rootScope.helpTemplatesModal.template);
                            $rootScope.show = true;
                            $rootScope.firtScreenShow = false;
                            return;
                        }

                        $rootScope.selectedClose = true;
                        $rootScope.show = false;
                        $scope.selectedCloseModalForRoute = 2;
                        $scope.selectedCloseModalForModule = true;
                        $rootScope.helpTemplatesModal = undefined;
                        var routeShowControl = undefined;
                        var startPageFilter = undefined;
                        if ($localStorage.read("startPage")) {
                            $scope.startPage = JSON.parse($localStorage.read("startPage"));
                            if ($localStorage.read("routeShow")) {
                                $scope.selectedCloseStartPage = JSON.parse($localStorage.read("routeShow"));
                                routeShowControl = $filter('filter')($scope.selectedCloseStartPage, { name: $scope.route })[0];
                            }
                            startPageFilter = $filter('filter')($scope.startPage, { name: $scope.route })[0];
                            if (routeShowControl)
                                $scope.selectedCloseModalForRoute = routeShowControl.value;
                            else if (startPageFilter) {
                                $scope.selectedCloseModalForRoute = startPageFilter.value;
                            }
                        }

                        if ($localStorage.read("moduleShow")) {
                            $scope.selectedCloseModal = JSON.parse($localStorage.read("moduleShow"));
                            var showControl = $filter('filter')($scope.selectedCloseModal, { name: $scope.moduleId })[0];
                            if (showControl) {
                                $scope.selectedCloseModalForModule = showControl.value;
                                $rootScope.show = $scope.selectedCloseModalForModule;
                            }
                        }

                        if ($localStorage.read("routeShow")) {
                            $scope.selectedCloseRoute = JSON.parse($localStorage.read("routeShow"));
                            routeShowControl = $filter('filter')($scope.selectedCloseRoute, { name: $scope.route })[0];
                            if (routeShowControl) {
                                $scope.selectedCloseModalForRoute = routeShowControl.value;
                                $rootScope.show = false;
                            } else {
                                $scope.selectedCloseModalForRoute = 2;
                            }

                        }

                        $scope.openModal = function () {
                            if ($rootScope.helpTemplatesModal && $rootScope.helpTemplatesModal.show_type === "publish") {

                                if (!angular.isObject($rootScope.helpTemplatesModal.template))
                                    $rootScope.helpTemplatesModal.template = $sce.trustAsHtml($rootScope.helpTemplatesModal.template);

                                if ($localStorage.read('ModalShow')) {

                                    $rootScope.selectedClose = JSON.parse($localStorage.read('ModalShow'));
                                    $rootScope.show = false;
                                }

                                if ($rootScope.selectedClose === true) {
                                    if ($scope.moduleId && $scope.selectedCloseModalForModule) {
                                        $rootScope.show = true;
                                    }

                                    if ($scope.route && $scope.selectedCloseModalForRoute === 1) {
                                        if ($localStorage.read("startPage")) {
                                            $scope.startPage = JSON.parse($localStorage.read("startPage"));
                                            var startPageFilter = $filter('filter')($scope.startPage, { name: $scope.route })[0];
                                            if (startPageFilter && startPageFilter.value === 1) {
                                                var routes = [];
                                                var routeShow = {
                                                    name: $rootScope.currentPath,
                                                    value: 2
                                                };
                                                routes.push(routeShow);
                                                $localStorage.write("startPage", JSON.stringify(routes));
                                            }
                                        }
                                        $rootScope.show = true;
                                    }

                                    if ($scope.route && $scope.selectedCloseModalForRoute === 2) {

                                        $rootScope.show = true;
                                    }
                                }
                            }
                        };

                        var cacheKey = 'help-';

                        if ($scope.moduleId)
                            cacheKey += '-' + $scope.moduleId;

                        if ($scope.route) {
                            $scope.route.replace('/', '--');
                            cacheKey += $scope.route;

                        }

                        if ($cache.get(cacheKey)) {
                            $rootScope.helpTemplatesModal = $cache.get(cacheKey);
                            if ($rootScope.selectedClose)
                                $scope.openModal();

                        }

                        if (!$rootScope.helpTemplatesModal && !$rootScope.dashboardHelpTemplate) {
                            //We couldn't get custom helps with modules help. We have to get with service dashboard help datas.
                            if (!$scope.moduleId) {
                                HelpService.getByType('modal', null, $scope.route)
                                    .then(function (response) {
                                        if (!response.data)
                                            return;

                                        response.data.template = $sce.trustAsHtml(response.data.template);
                                        $rootScope.helpTemplatesModal = response.data;

                                        $cache.put(cacheKey, response.data);

                                        if ($rootScope.selectedClose)
                                            $scope.openModal();
                                    });
                            } else {
                                var module = $filter('filter')($rootScope.modules, { id: $scope.moduleId }, true)[0];
                                var help = $filter('filter')(module.helps, {
                                    modal_type: 'modal',
                                    module_type: 'module_list'
                                }, true)[0];
                                if (!help)
                                    return;

                                $rootScope.helpTemplatesModal = help;
                                $cache.put(cacheKey, help);
                            }

                            if ($rootScope.selectedClose)
                                $scope.openModal();

                        }

                        $scope.showModal = function () {

                            if ($scope.moduleId || $scope.route) {

                                $localStorage.write('ModalShow', false);
                            }

                        };

                    }]
            };
        }])
    .directive('queryBuilder', ['$compile', '$rootScope', '$filter', function ($compile, $rootScope, $filter) {
        return {
            restrict: 'E',
            scope: {
                group: '=',
                fieldskey: '=',
                module: '=',
                viewfilter: '='
            },
            templateUrl: 'view/app/module/filters.html',
            compile: function (element, attrs) {
                var content, directive;
                content = element.contents().remove();
                return function (scope, element, attrs) {
                    scope.language = $rootScope.language;
                    scope.globalization = $rootScope.globalization;
                    scope.addCondition = function () {
                        scope.group.filters.unshift({
                            key: "key".generateRandomKey(20),
                            operator: "",
                            field: "selectFirstFiled.name",
                            value: ''
                        });
                    };

                    scope.changeOpertor = function (filter) {
                        if ((filter.operator === "not_empty" || filter.operator === "empty") && filter.field != null) {
                            filter.value = "-";
                            scope.viewfilter();
                        } else
                            filter.value = "";
                    };

                    scope.viewFiltere = function (filter) {
                        if (filter.value != null && filter.operator !== "") {
                            scope.viewfilter();
                        }
                    };

                    scope.dateFormat = [
                        {
                            label: $filter('translate')('View.Second'),
                            value: "s"
                        },
                        {
                            label: $filter('translate')('View.Minute'),
                            value: "m"
                        },
                        {
                            label: $filter('translate')('View.Hour'),
                            value: "h"
                        },
                        {
                            label: $filter('translate')('View.Day'),
                            value: "D"
                        },
                        {
                            label: $filter('translate')('View.Week'),
                            value: "W"
                        },
                        {
                            label: $filter('translate')('View.Month'),
                            value: "M"
                        },
                        {
                            label: $filter('translate')('View.Year'),
                            value: "Y"
                        }
                    ];

                    scope.costumeDateFilter = [
                        {
                            name: "thisNow",
                            label: $filter('translate')('View.Now'),
                            value: "now()"
                        },
                        {
                            name: "thisToday",
                            label: $filter('translate')('View.StartOfTheDay'),
                            value: "today()"
                        },
                        {
                            name: "thisWeek",
                            label: $filter('translate')('View.StartOfThisWeek'),
                            value: "this_week()"
                        },
                        {
                            name: "thisMonth",
                            label: $filter('translate')('View.StartOfThisMonth'),
                            value: "this_month()"
                        },
                        {
                            name: "thisYear",
                            label: $filter('translate')('View.StartOfThisYear'),
                            value: "this_year()"
                        },
                        {
                            name: "year",
                            label: $filter('translate')('View.NowYear'),
                            value: "year()"
                        },
                        {
                            name: "month",
                            label: $filter('translate')('View.NowMonth'),
                            value: "month()"
                        },
                        {
                            name: "day",
                            label: $filter('translate')('View.NowDay'),
                            value: "day()"
                        },
                        {
                            name: "costume",
                            label: $filter('translate')('View.CustomDate'),
                            value: "costume"
                        },
                        {
                            name: "todayNextPrev",
                            label: $filter('translate')('View.FromTheBeginningOfTheDay'),
                            value: "costumeN",
                            nextprevdatetype: "D"
                        },
                        {
                            name: "weekNextPrev",
                            label: $filter('translate')('View.FromTheBeginningOfTheWeek'),
                            value: "costumeW",
                            nextprevdatetype: "M"
                        },
                        {
                            name: "monthNextPrev",
                            label: $filter('translate')('View.FromTheBeginningOfTheMonth'),
                            value: "costumeM",
                            nextprevdatetype: "M"
                        },
                        {
                            name: "yearNextPrev",
                            label: $filter('translate')('View.FromTheBeginningOfTheYear'),
                            value: "costumeY",
                            nextprevdatetype: "Y"
                        }
                    ];

                    scope.removeCondition = function (index) {
                        scope.group.filters.splice(index, 1);
                        scope.viewfilter();
                    };

                    scope.addGroup = function () {
                        scope.group.filters.push({
                            group: {
                                level: scope.group.level + 1,
                                logic: 'and',
                                filters: []
                            }
                        });
                    };

                    scope.removeGroup = function () {
                        "group" in scope.$parent && scope.$parent.group.filters.splice(scope.$parent.$index, 1);
                        scope.viewfilter();
                    };

                    directive || (directive = $compile(content));

                    element.append(directive(scope, function ($compile) {
                        return $compile;
                    }));
                }
            }
        }
    }])
    .directive('inputStars', [function () {
        function isFloat(n) {
            return Number(n) === n && n % 1 !== 0;
        }

        var directive = {
            restrict: 'EA',
            replace: true,
            template: '<ul ng-class="listClass">' +
                '<li ng-touch="paintStars($index)" ng-mouseenter="paintStars($index, true, $event)" ng-mouseleave="unpaintStars($index, false)" ng-repeat="item in items track by $index">' +
                '<i  ng-class="getClass($index)" ng-click="setValue($index, $event)"></i>' +
                '</li>' +
                '</ul>',
            require: 'ngModel',
            scope: {
                bindModel: '=ngModel'
            },
            link: link
        };

        return directive;

        function link(scope, element, attrs, ngModelCtrl) {
            var computed = {
                get allowHalf() {
                    return typeof attrs.allowHalf == 'string' && attrs.allowHalf != 'false'
                },
                get readonly() {
                    return attrs.readonly != 'false' && (attrs.readonly || attrs.readonly === '');
                },
                get fullIcon() {
                    return attrs.iconFull || 'fa-star';
                },
                get halfIcon() {
                    return attrs.iconHalf || 'fa-star-half-o';
                },
                get emptyIcon() {
                    return attrs.iconEmpty || 'fa-star-o';
                },
                get iconBase() {
                    return attrs.iconBase || 'fa fa-fw';
                },
                get iconHover() {
                    return attrs.iconHover || 'angular-input-stars-hover';
                }
            };

            scope.items = new Array(+attrs.max);
            scope.listClass = attrs.listClass || 'angular-input-stars';

            ngModelCtrl.$render = function () {
                if (isFloat(ngModelCtrl.$viewValue)) {
                    scope.lastValue = (Math.round(parseFloat(ngModelCtrl.$viewValue) * 2) / 2)
                } else {
                    scope.lastValue = parseFloat(ngModelCtrl.$viewValue) || 0;
                }
            };

            scope.getClass = function (index) {
                var icon;

                if (index >= scope.lastValue) {
                    icon = computed.iconBase + ' ' + computed.emptyIcon;
                } else {
                    var isHalf = index + 0.5;
                    if (computed.allowHalf && isHalf === scope.lastValue) {
                        icon = computed.iconBase + ' ' + computed.halfIcon + ' active ';
                    } else {
                        icon = computed.iconBase + ' ' + computed.fullIcon + ' active ';
                    }
                }
                return computed.readonly ? icon + ' readonly' : icon;
            };

            scope.unpaintStars = function ($index, hover) {
                scope.paintStars(scope.lastValue - 1, hover);
            };

            scope.paintStars = function ($index, hover, $event) {
                // ignore painting if readonly
                if (computed.readonly) {
                    return;
                }

                var items = element.find('li').find('i');

                for (var index = 0; index < items.length; index++) {
                    var $star = angular.element(items[index]);
                    var classesToRemove;
                    var classesToAdd;

                    if ($index >= index) {
                        classesToRemove = [computed.emptyIcon, computed.halfIcon]
                        classesToAdd = [computed.iconHover, computed.fullIcon, 'active']
                    } else {
                        classesToRemove = [computed.fullIcon, computed.iconHover, computed.halfIcon, 'active']

                        // isHalf
                        if (computed.allowHalf && $index + 0.5 === index) {
                            classesToAdd = [computed.halfIcon, 'active']
                        } else {
                            classesToAdd = [computed.emptyIcon]
                        }
                    }

                    $star.removeClass(classesToRemove.join(' '));
                    $star.addClass(classesToAdd.join(' '));
                }

                if (!hover) {
                    items.removeClass(computed.iconHover);
                }
            };

            /**
             * Returns whether the user is hovering the first half of the star or not.
             *
             * @param {MouseEvent} e The mouse event.
             * @param {HTMLLIElement} starDOMNode The scope "star" dom node.
             * @returns {boolean}
             */
            function isHoveringFirstHalf(e, starDOMNode) {
                return e.pageX < starDOMNode.getBoundingClientRect().left + starDOMNode.offsetWidth / 2
            }

            scope.setValue = function (index, e) {
                // ignore setting value if readonly
                if (computed.readonly) {
                    return;
                }

                var star = e.target,
                    newValue;

                if (computed.allowHalf && isHoveringFirstHalf(e, star)) {
                    newValue = index + 0.5;
                } else {
                    newValue = index + 1;
                }

                // sets to 0 if the user clicks twice on the first "star"
                // the user should be allowed to give a 0 score
                if (newValue === scope.lastValue) {
                    newValue = 0;
                }

                scope.lastValue = newValue;

                ngModelCtrl.$setViewValue(newValue);
                ngModelCtrl.$render();

                //Execute custom trigger function if there is one
                if (attrs.onStarClick) {
                    try {
                        scope.$parent.$eval(attrs.onStarClick, { $event: e });
                    } catch (e) {
                        void 0;
                    }
                }

            };
        }

    }]);


