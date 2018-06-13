'use strict';

angular.module('primeapps')

    .factory('UserService', ['$http', 'config',
        function ($http, config) {
            return {
                getUsers: function (workgroupUsers, profiles, roles) {
                    var users = [];

                    angular.forEach(workgroupUsers, function (workgroupUser) {
                        var user = workgroupUser;

                        angular.forEach(profiles, function (profile) {
                            if (profile.UserIDs.indexOf(workgroupUser.Id) > -1) {
                                user.profile = profile;
                            }
                        });

                        angular.forEach(roles, function (role) {
                            if (role.users.indexOf(workgroupUser.Id) > -1) {
                                user.role = role;
                            }
                        });

                        users.push(user);
                    });

                    return users;
                },
                sendPasswordToOfficeUser: function (requestMail) {
                    return $http.post(config.apiUrl + 'messaging/send_external_email', requestMail);
                },
                addUser: function(user){
                  return $http.post(config.apiUrl + 'User/add_user',user);
                },
                getOfficeUsers: function(){
                    return $http.get(config.apiUrl + 'User/GetOfficeUsers');
                },
                isAvailableToInvite: function (email, instanceId) {
                    return $http.post(config.apiUrl + 'User/IsAvailableToInvite', {
                        EMail: email,
                        InstanceID: instanceId
                    });
                },
                updateActiveDirectoryEmail: function (id, email){
                    return $http.get(config.apiUrl + 'User/UpdateActiveDirectoryEmail?UserId=' + id + '&Email=' + email);
                },
                invite: function (email, instanceId, profileId, roleId, culture, createdBy) {
                    return $http.post(config.apiUrl + 'Instance/Invite', {
                        EMail: email,
                        InstanceID: instanceId,
                        ProfileID: profileId,
                        RoleID: roleId,
                        Culture: culture,
                        CreatedBy: createdBy
                    });
                },

                dismiss: function (user, instanceId) {
                    return $http.post(config.apiUrl + 'Instance/Dismiss', {
                        UserID: user.ID,
                        EMail: user.email,
                        HasAccount: user.hasAccount,
                        InstanceID: instanceId
                    });
                }
            };
        }]);