'use strict';

angular.module('primeapps')
    .controller('PaymentController', ['$scope', 'PaymentService',
        function ($scope, PaymentService) {
            PaymentService.getPayment()
                .then(function (data) {
                    data = data.data;
                    if (data.HasAdjustment)
                        data.CampaignCode = null;

                    data.CardNumberMask = data.LastFourDigitsOfCard ? '************' + data.LastFourDigitsOfCard : '';
                    $scope.payment = data;
                });
        }
    ]);