'use strict';

angular.module('primeapps')

    .controller('UserController', ['$rootScope', '$scope', '$filter', '$state', 'ngToast', 'guidEmpty', '$popover', 'helper', 'UserService', 'WorkgroupService', 'AppService', 'ProfileService', 'RoleService', 'LicenseService', '$q', 'officeHelper',
        function ($rootScope, $scope, $filter, $state, ngToast, guidEmpty, $popover, helper, UserService, WorkgroupService, AppService, ProfileService, RoleService, LicenseService, $q, officeHelper) {
            $scope.loading = true;
            $scope.isOfficeConnected = false;
            //user add button popover controller
            $scope.officeUserReady = false;

            $scope.officeUsers = null;
            $scope.selectedOfficeUser = {};
            $scope.addUserModel = {};
            $scope.addUserForm = true;
            $scope.submitting = false;
            $scope.hideSendEmailToUser = false;

            $scope.officeUserChanged = function (selectedOfficeUser) {
                $scope.addUserModel.email = selectedOfficeUser.email;
                $scope.addUserModel.phone = selectedOfficeUser.phone;
                $scope.addUserModel.fullName = selectedOfficeUser.fullName;
                $scope.addUserModel.firstName = selectedOfficeUser.name;
                $scope.addUserModel.lastName = selectedOfficeUser.surname;
            };

            $scope.sendOfficeUserPassword = function () {
                var app = 'crm';
                switch ($rootScope.user.appId) {
                    case 2:
                        app = 'kobi';
                        break;
                    case 3:
                        app = 'asistan';
                        break;
                    case 4:
                        app = 'ik';
                        break;

                    case 5:
                        app = 'cagri';
                        break;
                    default:
                        app = 'crm';
                }


                $scope.submitting = true;
                var emailBody = $filter('translate')('Setup.Office.EmailNotification.Hello') + " " + $scope.addedUser.fullName + "<br />" + $filter('translate')('Setup.Office.EmailNotification.Created') + "<br />" + $filter('translate')('Setup.Office.EmailNotification.Email') + $scope.addedUser.email + "<br />" + $filter('translate')('Setup.Office.EmailNotification.Password') + $scope.userPassword;
                var requestMail = {};
                requestMail.Subject = $filter('translate')('Setup.Office.EmailNotification.Subject');
                requestMail.TemplateWithBody = '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"> <html xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office"> <head><title>Ofisim</title><meta http-equiv="Content-Type" content="text/html; charset=utf-8" /><style type="text/css">body, .maintable {height: 100% !important; width: 100% !important; margin: 0; padding: 0;}img, a img {border: 0;outline: none;             text-decoration: none;         }          .imagefix {             display: block;         }          p {             margin-top: 0;             margin-right: 0;             margin-left: 0;             padding: 0;         }          .ReadMsgBody {             width: 100%;         }          .ExternalClass {             width: 100%;         }              .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {                 line-height: 100%;             }          img {             -ms-interpolation-mode: bicubic;         }          body, table, td, p, a, li, blockquote {             -ms-text-size-adjust: 100%;             -webkit-text-size-adjust: 100%;         }     </style>     <style type="text/css">         @media only screen and (max-width: 600px) {             .rtable {                 width: 100% !important;                 table-layout: fixed;             }                  .rtable tr {                     height: auto !important;                     display: block;                 }              .contenttd {                 max-width: 100% !important;                 display: block;             }                  .contenttd:after {                     content: "";                     display: table;                     clear: both;                 }              .hiddentds {                 display: none;             }              .imgtable, .imgtable table {                 max-width: 100% !important;                 height: auto;                 float: none;                 margin: 0 auto;             }                  .imgtable.btnset td {                     display: inline-block;                 }                  .imgtable img {                     width: 100%;                     height: auto;                     display: block;                 }              table {                 float: none;                 table-layout: fixed;             }         }     </style>     <!--[if gte mso 9]>     <xml>       <o:OfficeDocumentSettings>         <o:AllowPNG/>         <o:PixelsPerInch>96</o:PixelsPerInch>       </o:OfficeDocumentSettings>     </xml>     <![endif]--> </head> <body style="overflow: auto; padding:0; margin:0; font-size: 12px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#f4f4f4">     <table cellspacing="0" cellpadding="0" width="100%"            bgcolor="#f4f4f4">         <tr>             <td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td>         </tr>         <tr>             <td valign="top">                 <table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto"                        cellspacing="0" cellpadding="0" width="600" align="center"                        border="0">                     <tr>                         <td class="contenttd"                             style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">                             <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"                                    align="left">                                 <tr class="hiddentds">                                     <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>                                     <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>                                 </tr>                                 <tr style="HEIGHT: 10px">                                     <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN:center !important; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent; text-aling:center;">                                         <table class="imgtable" cellspacing="0" cellpadding="0"                                                align=" center" border="0">                                             <tr>                                                 <td style="PADDING-BOTTOM: 2px; PADDING-TOP: 2px; PADDING-LEFT: 2px; PADDING-RIGHT: 2px; text-aling:center;" align="center">                                                     <table cellspacing="0" cellpadding="0" border="0">                                                         <tr>                                                             <td style="BORDER-TOP: medium none; BORDER-RIGHT: medium none;  BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BACKGROUND-COLOR: transparent; text-align:center;">                                                                 <a href="http://www.ofisim.com/' + app + '/" target="_blank"><img style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block" alt="" src="http://www.ofisim.com/mail/' + app + '/logo.png" width="200" height="50" hspace="0" vspace="0" border="0" /></a>                                                             </td>                                                         </tr>                                                     </table>                                                 </td>                                             </tr>                                         </table>                                     </th>                                     <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: center; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">                                     </th>                                 </tr>                             </table>                         </td>                     </tr>                     <tr>                         <td class="contenttd" style="BORDER-TOP: #fff 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">                             <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="center">                                 <tr class="hiddentds">                                     <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>                                 </tr>                                 <tr style="HEIGHT: 10px">                                     <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">                                         <!--[if gte mso 12]>                                             <table cellspacing="0" cellpadding="0" border="0" width="100%"><tr><td align="center">                                         <![endif]-->                                         <table class="imgtable" style="MARGIN: 0px auto" cellspacing="0"                                                cellpadding="0" align="center" border="0">                                             <tr>                                                 <td style="PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px"                                                     align="center"></td>                                             </tr>                                         </table>                                         <!--[if gte mso 12]>                                             </td></tr></table>                                         <![endif]-->                                     </th>                                 </tr>                             </table>                         </td>                     </tr>                      <tr>                            <td class="contenttd" style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">                             <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"                                    align="left">                                 <tr class="hiddentds">                                     <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>                                 </tr>                                 <tr style="HEIGHT: 20px">                                     <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 20px; BORDER-LEFT: medium none; PADDING-RIGHT: 20px; BACKGROUND-COLOR: #fff;">                                         <p style="FONT-SIZE: 18px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"                                            align="center">                                             ' + emailBody + '<br />                                                                                     </p>                                         <p style="FONT-SIZE: 16px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 24px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"                                            align="center"></p>                                          <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"                                            align="center">                                             <br />                                         </p>                                          <p style="FONT-SIZE: 11px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 14px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"                                            align="center">' + $filter('translate')('Setup.Office.EmailNotification.Footer') + '</p>                                     </th>                                 </tr>                             </table>                         </td>                     </tr>                     <tr>                     <tr style="HEIGHT: 20px">                         <th class="contenttd"                             style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">                             <div style="PADDING-BOTTOM: 10px; TEXT-ALIGN: center; PADDING-TOP: 10px; PADDING-LEFT: 10px; PADDING-RIGHT: 10px">                                 <table class="imgtable" style="DISPLAY: inline-block"                                        cellspacing="0" cellpadding="0" border="0">                                     <tr>                                         <td style="PADDING-RIGHT: 5px">                                             <a href="https://www.facebook.com/ofisimcrm" target="_blank">                                                 <img title="Facebook"                                                      style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"                                                      alt="Facebook" src="http://www.ofisim.com/mail/KobiMail_files/fb.png" width="34"                                                      height="34" />                                             </a>                                         </td>                                         <td style="PADDING-RIGHT: 5px">                                             <a href="https://twitter.com/ofisim_com" target="_blank">                                                 <img title="Twitter"                                                      style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"                                                      alt="Twitter" src="http://www.ofisim.com/mail/KobiMail_files/tw.png" width="34"                                                      height="34" />                                             </a>                                         </td>                                         <td style="PADDING-RIGHT: 5px">                                             <a href="https://www.linkedin.com/company/ofisim.com"                                                target="_blank">                                                 <img title="Linkedin"                                                      style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"                                                      alt="Linkedin" src="http://www.ofisim.com/mail/KobiMail_files/in.png" width="34"                                                      height="34" />                                             </a>                                         </td>                                     </tr>                                 </table>                             </div>                             <p style="FONT-SIZE: 14px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 21px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"                                align="left">&nbsp;</p>                         </th>                     </tr>                 </table>             </td>         </tr>         <tr>             <td class="contenttd"                 style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 10px; PADDING-TOP: 10px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">                 <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"                        align="left">                     <tr class="hiddentds">                         <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>                     </tr>                     <tr style="HEIGHT: 10px">                         <th class="contenttd"                             style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">                         </th>                     </tr>                 </table>             </td>         </tr>         <tr>             <td class="contenttd"                 style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 1px; PADDING-TOP: 1px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">                 <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"                        align="left">                     <tr class="hiddentds">                         <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>                     </tr>                     <tr style="HEIGHT: 10px">                         <th class="contenttd"                             style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 1px; TEXT-ALIGN: left; PADDING-TOP: 1px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">                         </th>                     </tr>                 </table>             </td>         </tr>         <tr>             <td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td>         </tr>     </table>      <!-- Created with MailStyler 2.0.0.330 --> </body> </html>';
                requestMail.ToAddresses = [$scope.addedUser.email];

                UserService.sendPasswordToOfficeUser(requestMail)
                    .then(function (response) {
                        $scope.closeUserInfoPopover();
                        ngToast.create({
                            content: $filter('translate')('Setup.Office.SendEmailSuccess'),
                            className: 'success',
                            timeout: 5000
                        });
                    }).catch(function (response) {
                    $scope.closeUserInfoPopover();
                    ngToast.create({
                        content: $filter('translate')('Setup.Office.SendEmailError'),
                        className: 'success',
                        timeout: 5000
                    });
                });

            };

            $scope.closeUserInfoPopover = function closePasswordPopup() {
                $scope.submitting = false;
                $scope.addUserForm = true;
                $scope.userPassword = null;
                $scope.hideSendEmailToUser = false;
                if ($scope.createOfficePopover) {
                    $scope.createOfficePopover.hide();
                } else if ($scope.createPopover) {
                    $scope.createPopover.hide();
                }

                $scope.addedUser = {};
            };

            //TODO Removed
            /*getOfficeUsers();

            function getOfficeUsers() {
                officeHelper.officeTenantInfo()
                    .then(function (adInfo) {
                        if (adInfo.data) {
                            $scope.isOfficeConnected = true;
                            UserService.getOfficeUsers()
                                .then(function (response) {
                                    if (response.data) {
                                        $scope.officeUsers = response.data;
                                        $scope.officeUserReady = true;
                                    }
                                });
                            $rootScope.user.azureDirectory = adInfo.data;
                        }
                    });
            }*/

            function getUsers() {
                var promises = [];

                promises.push(UserService.getAllUser());
                promises.push(ProfileService.getAll());
                promises.push(RoleService.getAll());
                promises.push(LicenseService.getUserLicenseStatus());


                $q.all(promises).then(function (data) {
                    var users = data[0].data,
                        responseProfiles = data[1].data,
                        responseRoles = data[2].data,
                        license = data[3].data;

                    //var workgroup = $filter('filter')($rootScope.workgroups, { tenant_id: $rootScope.user.tenant_id }, true)[0];
                    $rootScope.workgroup.users = users;

                    $scope.profiles = ProfileService.getProfiles(responseProfiles, $rootScope.workgroup.tenant_id, true);
                    $scope.roles = responseRoles;
                    $scope.users = UserService.getUsers(users, $scope.profiles, $scope.roles);
                    $scope.licensesBought = license.total || 0;
                    $scope.licensesUsed = license.used || 0;
                    $scope.licenseAvailable = $scope.licensesBought - $scope.licensesUsed;
                    $scope.loading = false;
                });

            }

            getUsers();

            $scope.showCreateForm = function () {
                $scope.addedUser = {};
                $scope.addUserForm = true;
                $scope.createPopover = $scope.createPopover || $popover(angular.element(document.getElementById('createButton')), {
                        templateUrl: 'view/setup/users/userCreate.html',
                        placement: 'bottom-right',
                        scope: $scope,
                        autoClose: true,
                        show: true
                    });
            };

            $scope.showOfficeUserCreateForm = function () {
                $scope.addedUser = {};
                $scope.addUserForm = true;
                $scope.createOfficePopover = $scope.createOfficePopover || $popover(angular.element(document.getElementById('officeCreateButton')), {
                        templateUrl: 'view/setup/users/officeUserCreate.html',
                        placement: 'bottom-right',
                        scope: $scope,
                        autoClose: true,
                        show: true
                    });
            };

            $scope.addUser = function (inviteModel) {
                if (!inviteModel || !inviteModel.email || !inviteModel.profile || !inviteModel.role || !inviteModel.firstName || !inviteModel.lastName)
                    return;

                if (!inviteModel.fullName) {
                    inviteModel.fullName = inviteModel.firstName + " " + inviteModel.lastName;
                }

                $scope.userInviting = true;
                inviteModel.profileId = inviteModel.profile;
                inviteModel.roleId = inviteModel.role;

                $scope.addedUser = angular.copy(inviteModel);
                inviteModel = helper.SnakeToCamel(inviteModel);

                UserService.addUser(inviteModel)
                    .then(function (response) {
                        if (response.data) {
                            //TODO Removed
                            /*getOfficeUsers();*/
                            getUsers();

                            $scope.userInviting = false;
                            ngToast.create({
                                content: $filter('translate')('Setup.Users.NewUserSuccess'),
                                className: 'success'
                            });

                            $scope.userPassword = response.data.password;
                            $scope.hideSendEmailToUser = response.data.password.contains("***");
                            $scope.addUserForm = false;
                            $scope.addUserModel = {};
                        }
                    })
                    .catch(function (response) {
                        //$scope.inviteOfficeModel = {};
                        $scope.userInviting = false;
                        if (response.status === 409) {
                            ngToast.create({
                                content: $filter('translate')('Setup.Users.NewUserError'),
                                className: 'warning'
                            });
                        }
                    });
                /*$scope.userInviting = true;

                 var success = function (resultInvite, inviteModel) {
                 if (resultInvite.HasAccount) {
                 getUsers();

                 $scope.userInviting = false;
                 ngToast.create({
                 content: $filter('translate')('Setup.Users.NewUserSuccess'),
                 className: 'success'
                 });
                 $scope.createPopover.hide();
                 inviteModel = null;
                 return;
                 }

                 getUsers();

                 $scope.userInviting = false;
                 ngToast.create({
                 content: $filter('translate')('Setup.Users.NewUserSuccess'),
                 className: 'success'
                 });
                 $scope.createPopover.hide();
                 inviteModel = null;
                 };

                 UserService.isAvailableToInvite(inviteModel.email, $rootScope.workgroup.instanceID)
                 .then(function onSuccess(data) {
                 if (!data.data) {
                 $scope.inviteModel = null;
                 $scope.userInviting = false;
                 ngToast.create({
                 content: $filter('translate')('Setup.Users.NewUserError'),
                 className: 'warning'
                 });
                 return;
                 }

                 var culture = helper.getCulture();

                 UserService.invite(inviteModel.email, $rootScope.workgroup.instanceID, inviteModel.profile, inviteModel.role, culture, $rootScope.user.email)
                 .then(function onSuccess(resultInvite) {
                 if (!resultInvite.data) {
                 $filter('translate')('Common.Error');
                 return;
                 }

                 if (!resultInvite.data.Result) {
                 $scope.userInviting = false;
                 ngToast.create({
                 content: $filter('translate')('Setup.Users.LicenceLimitError'),
                 className: 'warning',
                 timeout: 6000
                 });
                 $scope.createPopover.hide();
                 return;
                 }

                 success(resultInvite.data, inviteModel);
                 })
                 .catch(function onError() {
                 $scope.userInviting = false;
                 });
                 });*/
            };

            $scope.showEditForm = function (user) {
                $scope.selectedUser = user;
                $scope.editModel = {};
                $scope.editModel.profile = user.profile.id;
                $scope.editModel.role = user.role.id;
                $scope.editModel.activeDirectoryEmail = user.activeDirectoryEmail;
                $scope.userHaveActiveDirectoryEmail = user.activeDirectoryEmail !== null && user.activeDirectoryEmail !== "null" && user.activeDirectoryEmail !== '';
                $scope.editModelState = angular.copy($scope.editModel);
                $scope['editPopover' + user.id] = $scope['editPopover' + user.id] || $popover(angular.element(document.getElementById('editButton' + user.id)), {
                    templateUrl: 'view/setup/users/userEdit.html',
                    placement: 'left',
                    scope: $scope,
                    autoClose: true,
                    show: true
                });

            };

            $scope.edit = function () {
                $scope.userEditing = true;

                if ($scope.editModel.profile === $scope.editModelState.profile &&
                    $scope.editModel.role === $scope.editModelState.role &&
                    $scope.editModel.activeDirectoryEmail === $scope.editModelState.activeDirectoryEmail) {
                    $scope.userEditing = false;

                    $scope.popover.hide();
                    //if need on alert for "no changed user" uncomment line
                    //ngToast.create({ content: $filter('translate')('Setup.Users.EditSuccess'), className: 'success' });
                    return;
                }

                var success = function () {
                    getUsers();
                    $scope.userEditing = false;
                    $scope.popover.hide();
                    ngToast.create({
                        content: $filter('translate')('Setup.Users.EditSuccess'),
                        className: 'success'
                    });
                };

                var updateActiveDirectoryEmail = function () {
                    UserService.updateActiveDirectoryEmail($scope.selectedUser.id, $scope.editModel.activeDirectoryEmail)
                        .then(function () {
                            //TODO Removed
                            /*getOfficeUsers();*/
                            success();
                        })
                        .catch(function (response) {
                            $scope.userEditing = false;
                            if (response.status === 409) {
                                ngToast.create({
                                    content: $filter('translate')('Setup.Users.NewUserError'),
                                    className: 'warning'
                                });
                            }
                        });
                };

                ProfileService.changeUserProfile($scope.selectedUser.id, $rootScope.workgroup.tenant_id, $scope.editModel.profile)
                    .then(function onSuccess() {
                        RoleService.updateUserRole($scope.selectedUser.id, $scope.editModel.role)
                            .then(function onSuccess() {
                                if (($scope.editModel.activeDirectoryEmail !== null || $scope.editModel.activeDirectoryEmail !== "" ) &&
                                    $scope.editModel.activeDirectoryEmail !== $scope.editModelState.activeDirectoryEmail) {
                                    updateActiveDirectoryEmail();
                                } else {
                                    success();
                                }
                            })
                            .catch(function onError() {
                                $scope.userEditing = false;
                            });
                    })
                    .catch(function onError() {
                        $scope.userEditing = false;
                    });
            };

            $scope.dismiss = function (user, index, close) {
                $scope.userDeleting = true;

                UserService.dismiss(user, $rootScope.workgroup.tenant_id)
                    .then(function onSuccess() {
                        $scope.users.splice(index, 1);
                        $scope.userDeleting = false;
                        ngToast.create({
                            content: $filter('translate')('Setup.Users.DismissSuccess'),
                            className: 'success'
                        });

                        AppService.getMyAccount(true);

                        LicenseService.getUserLicenseStatus().then(function onSuccess(license) {
                            $scope.licensesBought = license.data.total || 0;
                            $scope.licensesUsed = license.data.used || 0;
                            $scope.licenseAvailable = $scope.licensesBought - $scope.licensesUsed;
                        });
                        close();

                    })
                    .catch(function onError() {
                        $scope.userDeleting = false;
                        close();
                    });
            };

            $scope.gotoLicencePage = function () {
                var menuItem = $filter('filter')($scope.$parent.menuItems, { link: '#/app/setup/license' })[0];
                $scope.$parent.selectMenuItem(menuItem);

                $state.go('app.setup.license');
            };

        }
    ]);