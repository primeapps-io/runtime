'use strict';

angular.module('ofisim')

    .controller('PaymentFormController', ['$rootScope', '$scope', '$state', 'helper', '$http', 'config', 'ngToast', '$filter', 'AuthService', '$window',
        function ($rootScope, $scope, $state, helper, $http, config, ngToast, $filter, AuthService, $window) {
            $scope.promotion = {};
            helper.hideLoader();

            $scope.showPromotionModal = function () {
                $scope.promotionModal = $scope.promotionModal || $modal({
                        scope: $scope,
                        templateUrl: '/web/views/app/trial/promotionFormModal.html',
                        size: 'modal-sm',
                        backdrop: 'static',
                        show: false
                    });
                $scope.promotionModal.$promise.then($scope.promotionModal.show);
            };

            $scope.sendEmail = function (promotionForm) {
                if (promotionForm.$valid) {
                    $scope.submitting = true;
                    var requestMail = {};
                    requestMail.Subject = "Demo Uzatma Talebi";
                    requestMail.TemplateWithBody = '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"><html xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office"><head> <title></title> <meta http-equiv="Content-Type" content="text/html; charset=utf-8" /> <style type="text/css"> body, .maintable { height: 100% !important; width: 100% !important; margin: 0; padding: 0; } img, a img { border: 0; outline: none; text-decoration: none; } .imagefix { display: block; } p { margin-top: 0; margin-right: 0; margin-left: 0; padding: 0; } .ReadMsgBody { width: 100%; } .ExternalClass { width: 100%; } .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; } img { -ms-interpolation-mode: bicubic; } body, table, td, p, a, li, blockquote { -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; } </style> <style type="text/css"> @media only screen and (max-width: 600px) { .rtable { width: 100% !important; table-layout: fixed; } .rtable tr { height: auto !important; display: block; } .contenttd { max-width: 100% !important; display: block; } .contenttd:after { content: ""; display: table; clear: both; } .hiddentds { display: none; } .imgtable, .imgtable table { max-width: 100% !important; height: auto; float: none; margin: 0 auto; } .imgtable.btnset td { display: inline-block; } .imgtable img { width: 100%; height: auto; display: block; } table { float: none; table-layout: fixed; } } </style> <!--[if gte mso 9]><xml> <o:OfficeDocumentSettings> <o:AllowPNG/> <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings></xml><![endif]--></head><body style="overflow: auto; padding:0; margin:0; font-size: 14px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#444545"> <table cellspacing="0" cellpadding="0" width="100%" bgcolor="#444545"> <tr> <td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td> </tr> <tr> <td valign="top"> <table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto" cellspacing="0" cellpadding="0" width="600" align="center" border="0"> <tr> <td class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent"> <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="left"> <tr class="hiddentds"> <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td> <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td> </tr> <tr style="HEIGHT: 10px"> <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent"></th> <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent"></th> </tr> </table> </td> </tr> <tr> <td class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #feffff"> <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="left"> <tr class="hiddentds"> <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td> </tr> <tr style="HEIGHT: 20px"> <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: bottom; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #1296f7"> <p style="FONT-SIZE: 36px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #fffeff; LINE-HEIGHT: 36px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly" align="center"><br /> ' + requestMail.Subject + '</p> </th> </tr> </table> </td> </tr> <tr> <td class="contenttd" style="BORDER-TOP: #e73d11 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #feffff"> <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="left"> <tr class="hiddentds"> <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly" colspan="2"></td> </tr> <tr style="HEIGHT: 71px"> <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 5px; TEXT-ALIGN: left; PADDING-TOP: 5px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent" colspan="2"> <div> <p style="FONT-SIZE: 18px; MARGIN-BOTTOM: 1em; FONT-FAMILY: geneve, arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #2d2d2d; TEXT-ALIGN: justify; PADDING-LEFT: 110px; LINE-HEIGHT: 29px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly" align="justify"><strong><br />&#304;sim Soyisim</strong>:' + $scope.promotion.fullName + '<br /> <strong>Telefon Numaras&#305;</strong>:' + $scope.promotion.phoneNumber + '<br /> <strong>M&uuml;&#351;teri Epostas&#305;</strong>:' + $scope.promotion.email + '</p> </div> </th> </tr> </table> </td> </tr> <tr> <td class="contenttd" style="BORDER-TOP: #e73d11 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #feffff"> <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="left"> <tr class="hiddentds"> <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td> </tr> <tr style="HEIGHT: 20px"> <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent"></th> </tr> </table> </td> </tr> </table> </td> </tr> <tr> <td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td> </tr> </table> <!-- Created with MailStyler 2.0.1.300 --></body></html>';
                    requestMail.ToAddresses = ["info@ofisim.com"];

                    $http.post(config.apiUrl + 'messaging/send_external_email', requestMail).then(function (response) {
                        $scope.submitting = false;
                        ngToast.create({
                            content: $filter('translate')('Trial.RequestMessage'),
                            className: 'success',
                            timeout: 5000
                        });
                    })
                }
            };

            $scope.logout = function () {
                AuthService.logout()
                    .then(function () {
                        AuthService.logoutComplete();
                        $window.location.href = '/Auth/SignOut';
                    });
            };

            $scope.sector = [
                {
                    label: "Ağaç İşleri, Kağıt ve Kağıt Ürünleri",
                    value: "Ağaç İşleri, Kağıt ve Kağıt Ürünleri"
                },
                {
                    label: "Banka, Finans",
                    value: "Banka, Finans"
                },
                {
                    label: "Bilişim Teknolojileri",
                    value: "Bilişim Teknolojileri"
                },
                {
                    label: "Çevre",
                    value: "Çevre"
                },
                {
                    label: "Diğer",
                    value: "Diğer"
                },
                {
                    label: "Eğitim",
                    value: "Eğitim"
                },
                {
                    label: "Elektrik, Elektronik",
                    value: "Elektrik, Elektronik"
                },
                {
                    label: "Enerji",
                    value: "Enerji"
                },
                {
                    label: "Gıda",
                    value: "Gıda"
                },
                {
                    label: "Hukuk Firmaları",
                    value: "Hukuk Firmaları"
                },
                {
                    label: "İnşaat",
                    value: "İnşaat"
                },
                {
                    label: "Kamu Kurumları",
                    value: "Kamu Kurumları"
                },
                {
                    label: "Kar Amacı Gütmeyen Kurumlar",
                    value: "Kar Amacı Gütmeyen Kurumlar"
                },
                {
                    label: "Kimya, Petrol, Lastik ve Plastik",
                    value: "Enerji"
                },
                {
                    label: "Kültür, Sanat",
                    value: "Kültür, Sanat"
                },
                {
                    label: "Madencilik",
                    value: "Madencilik"
                },
                {
                    label: "Medya, İletişim",
                    value: "Medya, İletişim"
                },
                {
                    label: "Otomotiv",
                    value: "Otomotiv"
                },
                {
                    label: "Perakende",
                    value: "Perakende"
                },
                {
                    label: "Sağlık ve Sosyal Hizmetler",
                    value: "Sağlık ve Sosyal Hizmetler"
                },
                {
                    label: "Tarım, Avcılık, Balıkçılık",
                    value: "Tarım, Avcılık, Balıkçılık"
                },
                {
                    label: "Tekstil, Hazır Giyim, Deri",
                    value: "Tekstil, Hazır Giyim, Deri"
                },
                {
                    label: "Telekomünikasyon",
                    value: "Telekomünikasyon"
                },
                {
                    label: "Ticaret (Satış ve Pazarlama)",
                    value: "Ticaret (Satış ve Pazarlama)"
                },
                {
                    label: "Turizm, Konaklama",
                    value: "Turizm, Konaklama"
                },
                {
                    label: "Ulaştırma, Lojistik ve Haberleşme",
                    value: "Ulaştırma, Lojistik ve Haberleşme"
                },
                {
                    label: "Üretim",
                    value: "Üretim"
                }
            ];
        }
    ]);