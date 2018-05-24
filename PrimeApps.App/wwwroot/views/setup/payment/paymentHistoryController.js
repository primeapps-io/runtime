'use strict';

angular.module('ofisim')

    .controller('PaymentHistoryController', ['$scope', 'PaymentService',
        function ($scope, PaymentService) {
            $scope.loading = true;
            PaymentService.getPaymentHistory()
                .then(function (response) {
                    $scope.invoiceList = PaymentService.processPaymentHistory(response.data);
                    $scope.loading = false;
                });
        }
    ]);