'use strict';

angular.module('primeapps')

    .controller('NewsfeedController', ['$rootScope', '$scope', 'NoteService', 'ngToast', '$filter', '$window', '$modal',
        function ($rootScope, $scope, NoteService, ngToast, $filter, $window, $modal) {

            $window.scrollTo(0, 0);
            $scope.module = {
                id:null
            };

            var request = {
                module_id: null,
                record_id: null,
                limit: 10,
                offset: 0
            };

            $scope.refresh = function () {
                NoteService.count(request).then(function (totalCount) {
                    NoteService.find(request)
                        .then(function (notes) {
                            $scope.notes = notes.data;
                            $scope.loadingNotes = false;
                            $scope.allNotesLoaded = false;
                            $scope.currentPage = 1;
                            $scope.limit = 10;
                            ngToast.create({ content: $filter('translate')('Note.NoteRefresh'), className: 'success' });
                            $scope.$parent.notesCount = totalCount.data;

                            if (totalCount.data <= $scope.limit)
                                $scope.hidePaging = true;
                        });
                });
            };

            $scope.addActivity = function (difference, type) {
                if (type === 'date') {
                    $scope.calendarDate = moment(difference);
                }
                else if (type === 'month') {
                    $scope.calendarDate = moment(difference);
                }
                else {
                    var today = new Date();
                    var eventStartDateAllDay = new Date(today.getFullYear(), today.getMonth(), today.getDate() + difference, 0, 0, 0);
                    $scope.calendarDate = moment(eventStartDateAllDay);
                }

                $scope.isNewsfeed = true;
                $scope.currentLookupField = { lookup_type: 'activities' };

                $scope.formModal = $scope.formModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/module/moduleFormModal.html',
                        animation: '',
                        backdrop: 'static',
                        show: false,
                        tag: 'createModal'
                    });

                $scope.formModal.$promise.then($scope.formModal.show);
            };
        }
    ]);