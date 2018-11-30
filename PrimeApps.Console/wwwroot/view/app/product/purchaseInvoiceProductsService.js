'use strict';

angular.module('primeapps')
    .factory('PurchaseInvoiceProductsService', ['$rootScope', '$http', '$filter', 'config',
        function ($rootScope, $http, $filter, config) {
            return {};
        }]);

angular.module('primeapps')
    .constant('pdfLabels', {
        PdfUrlTr: "Teklifi buraya tıklayarak indirebilirsiniz",
        PdfUrlEn: "Click here to download quote",
        VatPercentTr: "KDV ({{percent}})",
        VatPercentEn: "VAT ({{percent}})",
        QuoteEn: "Quote",
        QuoteTr: "Teklif"
    });
