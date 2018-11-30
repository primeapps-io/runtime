'use strict';

angular.module('primeapps')
    .factory('PaymentService', ['$http', '$filter', 'config',
        function ($http, $filter, config) {
            return {

                getPayment: function () {
                    return $http.post(config.apiUrl + 'Payment/Get', {});
                },

                getPaymentHistory: function (payment) {
                    return $http.post(config.apiUrl + 'License/GetLicenseStatus', angular.toJson(payment));
                },

                checkCampaignCode: function (code) {
                    return $http.get(config.apiUrl + 'Payment/CheckCampaign?code=' + code);
                },

                update: function (payment) {
                    return $http.post(config.apiUrl + 'Payment/SaveOrUpdate', payment);
                },

                updateCampaign: function (paymentId, campaignCode) {
                    return $http.post(config.apiUrl + 'Payment/UpdateCampaignCode', {
                        PaymentId: paymentId,
                        CampaignCode: campaignCode
                    });
                },

                changeCurrency: function (currency) {
                    return $http.post(config.apiUrl + 'User/ChangeCurrency', angular.toJson(currency));
                },

                processPaymentHistory: function (license) {
                    var newLicense = {};
                    angular.copy(license, newLicense);
                    var licenseHistories = [];
                    newLicense.LicenseHistory = null;

                    angular.forEach(license.LicenseHistory, function (licenseHistory) {
                        var invoiceDetails = [];
                        var newLicenseHistory = {};
                        angular.copy(licenseHistory, newLicenseHistory);
                        newLicenseHistory.InvoiceDetails = null;

                        angular.forEach(licenseHistory.InvoiceDetails, function (invoiceDetail) {
                            var detail = $filter('filter')(invoiceDetails, {Title: invoiceDetail.Title}, true)[0];

                            if (!detail) {
                                if (invoiceDetail.Title && invoiceDetail.Title.split(' (').length > 0) {
                                    var item = invoiceDetail.Title.split(' (');

                                    invoiceDetail.Code = item[0] == 'User' || item[0] == 'Storage' ? item[0] : 'Usage';
                                    invoiceDetail.Period = 'Monthly';
                                    newLicenseHistory.MountCount = 1;

                                    var period = item[1] && item[1].replace(/[{()} ]/g, '');

                                    if (period && parseInt(period) > 1) {
                                        newLicenseHistory.MountCount = parseInt(period);
                                        invoiceDetail.Period = 'Annual';
                                    }
                                }

                                invoiceDetail.Count = 1;

                                this.push(invoiceDetail);
                            }
                            else {
                                detail.Amount += invoiceDetail.Amount;
                                detail.Count++;
                            }
                        }, invoiceDetails);

                        newLicenseHistory.InvoiceDetails = invoiceDetails;

                        this.push(newLicenseHistory);
                    }, licenseHistories);

                    newLicense.LicenseHistory = licenseHistories;

                    return newLicense;
                }
            };
        }]);

