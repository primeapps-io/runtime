'use strict';

angular.module('ofisim')
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