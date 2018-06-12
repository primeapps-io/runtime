'use strict';

angular.module('ofisim')
    .factory('SalesInvoiceProductsService', ['$rootScope', '$http', '$filter', 'config',
        function ($rootScope, $http, $filter, config) {
            return {};
        }]);

angular.module('ofisim')
    .constant('pdfLabels', {
        PdfUrlTr: "Teklifi buraya tıklayarak indirebilirsiniz",
        PdfUrlEn: "Click here to download quote",
        VatPercentTr: "KDV ({{percent}})",
        VatPercentEn: "VAT ({{percent}})",
        QuoteEn: "Quote",
        QuoteTr: "Teklif"
    });
