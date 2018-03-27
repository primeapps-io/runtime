'use strict';

angular.module('ofisim')

    .controller('ModuleLayoutController', ['$timeout', '$scope', '$filter', '$element', 'dragularService',
        function ($timeout, $scope, $filter, $element, dragularService) {
            var drakeRows;
            var drakeCells;

            var setDraggableLayout = function () {
                if (drakeRows)
                    drakeRows.destroy();

                if (drakeCells)
                    drakeCells.destroy();

                var moduleLayout = $scope.$parent.moduleLayout;
                var container = $element.children().eq(0);
                var rowContainers = [];
                var cellContainers = [];

                for (var i = 0; i < container.children().length; i++) {
                    var rowContainer = container.children().eq(i);

                    if (rowContainer[0].className.indexOf('subpanel') > -1)
                        rowContainers.push(rowContainer);
                }

                for (var j = 0; j < rowContainers.length; j++) {
                    var columnContainer = rowContainers[j].children().children().children();

                    for (var k = 0; k < columnContainer.length; k++) {
                        if (columnContainer[k].className.indexOf('cell-container') > -1)
                            cellContainers.push(columnContainer[k]);
                    }
                }

                drakeRows = dragularService(container, {
                    scope: $scope,
                    nameSpace: 'rows',
                    containersModel: moduleLayout.rows,
                    classes: {
                        mirror: 'gu-mirror-module',
                        transit: 'gu-transit-module'
                    },
                    moves: function (el, container, handle) {
                        return handle.classList.contains('row-handle');
                    }
                });

                drakeCells = dragularService(cellContainers, {
                    scope: $scope,
                    nameSpace: 'cells',
                    containersModel: (function () {
                        var containersModel = [];

                        angular.forEach(moduleLayout.rows, function (row) {
                            angular.forEach(row.columns, function (column) {
                                containersModel.push(column.cells);
                            })
                        });

                        return containersModel;
                    })(),
                    classes: {
                        mirror: 'gu-mirror-field',
                        transit: 'gu-transit-field'
                    },
                    moves: function (el, container, handle) {
                        return handle.classList.contains('cell-handle');
                    }
                });
            };

            $timeout(function () {
                setDraggableLayout();
            }, 0);

            $scope.$on('dragulardrop', function (e, el) {
                e.stopPropagation();
                $timeout(function () {
                    $scope.$parent.refreshModule();
                }, 0);
            });

            $scope.$parent.$watch('moduleChange', function (value) {
                if (!value)
                    return;

                $timeout(function () {
                    setDraggableLayout();
                }, 0);
            });
        }]);