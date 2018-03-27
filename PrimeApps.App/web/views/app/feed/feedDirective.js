'use strict';

angular.module('ofisim')

    .directive('feedList', ['convert', 'entityTypes', 'guidEmpty', '$localStorage', 'FeedService',
        function (convert, entityTypes, guidEmpty, $localStorage, FeedService) {
            return {
                restrict: 'EA',
                scope: {
                    feed: '=',
                    entityId: '=',
                    entityType: '='
                },
                templateUrl: cdnUrl + 'web/views/app/feed/feedList.html',
                controller: ['$rootScope', '$scope', function ($rootScope, $scope) {
                    $scope.pageIndex = 2;
                    $scope.pagingIcon = 'fa-chevron-right';
                    $scope.entityTypes = entityTypes;
                    $scope.guidEmpty = guidEmpty;
                    $scope.config = $rootScope.config;

                    $scope.loadMore = function () {
                        $scope.pagingIcon = 'fa-spinner fa-spin';

                        FeedService.getActivityFeedDelta($rootScope.workgroup.instanceID, $scope.pageIndex, $scope.entityId, $scope.entityType)
                            .then(function (response) {
                                $scope.feed = $scope.feed.concat(response.data);
                                $scope.count = response.data.length;
                                $scope.pageIndex += 1;
                                $scope.pagingIcon = 'fa-chevron-right';
                            });
                    };

                    $scope.comment = function (activity) {
                        if (!activity.comment)
                            return;

                        activity.commentSending = true;

                        FeedService.comment($rootScope.workgroup.instanceID, activity.ID, activity.comment, '')
                            .then(function () {
                                var comment = {};
                                comment.userID = $scope.$root.user.ID;
                                comment.userName = $scope.$root.user.firstName + ' ' + $scope.$root.user.lastName;
                                comment.timeStamp = convert.toMsDate(new Date());
                                comment.entityName = activity.comment;

                                activity.activities.push(comment);

                                activity.commentSending = false;
                                activity.comment = null;
                                activity.formOpened = false;
                            });
                    };

                    $scope.cancelComment = function (activity) {
                        activity.comment = null;
                        activity.formOpened = false;
                    };

                    $scope.enterToSend = true;

                    $scope.changeEnterToSend = function (val) {
                        $localStorage.write('EnterToSend', val);
                    }
                }]
            };
        }])

    .directive('feedForm', ['$filter', 'convert', 'entityTypes', 'FeedService',
        function ($filter, convert, entityTypes, FeedService) {
            return {
                restrict: 'EA',
                scope: {
                    entityId: '=',
                    entityType: '=',
                    show: '='
                },
                templateUrl: cdnUrl + 'web/views/app/feed/feedForm.html',
                controller: ['$scope',
                    function ($scope) {
                        $scope.feedCreating = false;

                        $scope.create = function (feed) {
                            if (!feed || !feed.text.trim())
                                return;

                            $scope.feedCreating = true;
                            var instanceId = $scope.$root.workgroup.instanceID;
                            var text = feed.text;

                            FeedService.create(instanceId, $scope.entityId, $scope.entityType, text)
                                .then(function (activityId) {
                                    var newFeed = {};
                                    newFeed.ID = activityId.data;
                                    newFeed.userID = $scope.$root.user.ID;
                                    newFeed.userName = $scope.$root.user.firstName + ' ' + $scope.$root.user.lastName;
                                    newFeed.timeStamp = convert.toMsDate(new Date());
                                    newFeed.entityName = text;
                                    newFeed.entityType = entityTypes.note;
                                    newFeed.activities = [];

                                    $scope.$parent.feed.unshift(newFeed);

                                    $scope.feedCreating = false;
                                    $scope.show = false;
                                    feed.text = null;
                                })
                                .catch(function () {
                                    $scope.feedCreating = false;
                                });
                        };

                        $scope.cancelCreate = function (feed) {
                            if (feed)
                                feed.text = null;

                            $scope.show = false;
                        }
                    }]
            };
        }])

    .directive('customSubmit', function () {
        return {
            restrict: 'A',
            scope: {
                action: '&'
            },
            link: function (scope, element, attrs) {
                element.bind("keydown keypress", function (event) {
                    if (!attrs.customSubmit || !angular.fromJson(attrs.customSubmit))
                        return;

                    if (event.which === 13) {
                        scope.$apply(function () {
                            scope.action();
                        });

                        event.preventDefault();
                    }
                });
            }
        }
    })

    .directive('onEnter', function () {
        return function (scope, element, attrs) {
            element.bind("keydown keypress", function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.onEnter, {'event': event});
                    });

                    event.preventDefault();
                }
            });
        };
    });