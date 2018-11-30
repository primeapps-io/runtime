'use strict';

angular.module('primeapps')

    .factory('FeedService', ['$http', 'config',
        function ($http, config) {
            return {

                getActivityFeed: function (instanceId, entityId, entityType) {
                    return $http.post(config.apiUrl + 'Activity/GetActivityFeed', {
                        DisplayCount: 5,
                        EntityID: entityId,
                        EntityType: entityType,
                        InstanceID: instanceId
                    });
                },

                getActivityFeedDelta: function (instanceId, pageIndex, entityId, entityType) {
                    return $http.post(config.apiUrl + 'Activity/GetDelta', {
                        DisplayCount: (pageIndex - 1) * 5,
                        EntityID: entityId,
                        EntityType: entityType,
                        InstanceID: instanceId
                    });
                },

                processFeed: function (feed) {
                    var feedList = [];

                    angular.forEach(feed, function (feedItem) {


                        feedItem.actionText = '';

                        feedList.push(feedItem);
                    }, feedList);

                    return feedList;
                },

                comment: function (instanceId, activityId, comment, compiledText) {
                    return $http.post(config.apiUrl + 'Activity/Comment', {
                        ActivityID: activityId,
                        Comment: comment,
                        CompiledText: compiledText,
                        InstanceID: instanceId
                    });
                },

                create: function (instanceId, entityId, entityType, text) {
                    return $http.post(config.apiUrl + 'Activity/Note', {
                        EntityID: entityId,
                        EntityType: entityType,
                        Text: text,
                        InstanceID: instanceId
                    });
                }

            };
        }]);