/*
 Navicat Premium Data Transfer

 Source Server         : Localhost
 Source Server Type    : PostgreSQL
 Source Server Version : 90611
 Source Host           : localhost:5432
 Source Catalog        : platform
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 90611
 File Encoding         : 65001

 Date: 02/04/2019 11:02:48
*/


-- ----------------------------
-- Sequence structure for app_templates_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."app_templates_id_seq";
CREATE SEQUENCE "public"."app_templates_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."app_templates_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for apps_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."apps_id_seq";
CREATE SEQUENCE "public"."apps_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."apps_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for exchange_rates_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."exchange_rates_id_seq";
CREATE SEQUENCE "public"."exchange_rates_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."exchange_rates_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for tenants_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."tenants_id_seq";
CREATE SEQUENCE "public"."tenants_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."tenants_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for users_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."users_id_seq";
CREATE SEQUENCE "public"."users_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."users_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for warehouses_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."warehouses_id_seq";
CREATE SEQUENCE "public"."warehouses_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."warehouses_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Table structure for _migration_history
-- ----------------------------
DROP TABLE IF EXISTS "public"."_migration_history";
CREATE TABLE "public"."_migration_history" (
  "migration_id" varchar(150) COLLATE "pg_catalog"."default" NOT NULL,
  "product_version" varchar(32) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."_migration_history" OWNER TO "postgres";

-- ----------------------------
-- Records of _migration_history
-- ----------------------------
BEGIN;
INSERT INTO "public"."_migration_history" VALUES ('20181001152119_Initial', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20181125161005_Task2865', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20181129123413_Task2985', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20181205062410_Task2863', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20181213124105_Task3053', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20170126230815_InitialDatabase', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20170312161610_Events', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20170507214430_ControlStructures', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20170519231452_PersistOutcome', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20170722200412_WfReference', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20171223020844_StepScope', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20190110102730_Task2814', '2.2.0-rtm-35687');
INSERT INTO "public"."_migration_history" VALUES ('20190216151947_Task3224', '2.2.0-rtm-35687');
COMMIT;

-- ----------------------------
-- Table structure for app_settings
-- ----------------------------
DROP TABLE IF EXISTS "public"."app_settings";
CREATE TABLE "public"."app_settings" (
  "app_id" int4 NOT NULL,
  "app_domain" text COLLATE "pg_catalog"."default",
  "auth_domain" text COLLATE "pg_catalog"."default",
  "currency" text COLLATE "pg_catalog"."default",
  "culture" text COLLATE "pg_catalog"."default",
  "time_zone" text COLLATE "pg_catalog"."default",
  "language" text COLLATE "pg_catalog"."default",
  "auth_theme" jsonb,
  "app_theme" jsonb,
  "mail_sender_name" text COLLATE "pg_catalog"."default",
  "mail_sender_email" text COLLATE "pg_catalog"."default",
  "google_analytics_code" text COLLATE "pg_catalog"."default",
  "tenant_operation_webhook" text COLLATE "pg_catalog"."default",
  "external_auth" jsonb,
  "registration_type" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "public"."app_settings" OWNER TO "postgres";

-- ----------------------------
-- Records of app_settings
-- ----------------------------
BEGIN;
INSERT INTO "public"."app_settings" VALUES (1, 'app.primeapps.io', 'auth.primeapps.io', NULL, NULL, NULL, NULL, '{"logo": "/images/logo.png", "color": "#555198", "title": "PrimeApps", "banner": [{"image": "/images/banner.jpg", "descriptions": {"en": "Welcome to PrimeApps", "tr": "PrimeApps''e Hoşgeldiniz"}}], "favicon": "/images/favicon.ico"}', '{"logo": "http://primeapps.io/logo.png", "color": "#555198", "title": "PrimeApps", "favicon": "http://primeapps.io/favicon.ico"}', 'PrimeApps', 'app@primeapps.io', NULL, NULL, NULL, 2);
INSERT INTO "public"."app_settings" VALUES (2, 'studio.primeapps.io', 'auth.primeapps.io', NULL, NULL, NULL, NULL, '{"logo": "", "color": "", "title": "PrimeApps Studio", "banner": [{"image": "", "descriptions": {"en": "", "tr": ""}}], "custom": "Studio", "favicon": ""}', '{"logo": "", "color": "", "title": "PrimeApps Studio", "favicon": ""}', 'PrimeApps', 'studio@primeapps.io', NULL, NULL, NULL, 1);
INSERT INTO "public"."app_settings" VALUES (3, 'preview.primeapps.io', 'auth-preview.primeapps.io', NULL, NULL, NULL, NULL, '{"logo": "/images/logo.png", "color": "#555198", "title": "PrimeApps", "banner": [{"image": "/images/banner.jpg", "descriptions": {"en": "Welcome to PrimeApps", "tr": "PrimeApps''e Hoşgeldiniz"}}], "favicon": "/images/favicon.ico"}', '{"logo": "http://primeapps.io/logo.png", "color": "#555198", "title": "PrimeApps", "favicon": "http://primeapps.io/favicon.ico"}', 'PrimeApps', 'preview@primeapps.io', NULL, NULL, NULL, 2);
COMMIT;

-- ----------------------------
-- Table structure for app_templates
-- ----------------------------
DROP TABLE IF EXISTS "public"."app_templates";
CREATE TABLE "public"."app_templates" (
  "id" int4 NOT NULL DEFAULT nextval('app_templates_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "app_id" int4 NOT NULL,
  "name" varchar(200) COLLATE "pg_catalog"."default",
  "subject" varchar(200) COLLATE "pg_catalog"."default",
  "content" text COLLATE "pg_catalog"."default",
  "language" text COLLATE "pg_catalog"."default",
  "type" int4 NOT NULL,
  "system_code" text COLLATE "pg_catalog"."default",
  "active" bool NOT NULL,
  "settings" jsonb
)
;
ALTER TABLE "public"."app_templates" OWNER TO "postgres";

-- ----------------------------
-- Records of app_templates
-- ----------------------------
BEGIN;
INSERT INTO "public"."app_templates" VALUES (1, 1, NULL, '2018-10-01 11:41:02.829818', NULL, 'f', 1, 'E-Posta Onaylama', 'E-Posta Onaylama', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml"
      xmlns:v="urn:schemas-microsoft-com:vml"
      xmlns:o="urn:schemas-microsoft-com:office:office">
<head>
    <title>Ofisim CRM''e Hoşgeldiniz!</title>
    <meta http-equiv="Content-Type"
          content="text/html; charset=utf-8" />
    <style type="text/css">
        body, .maintable {
            height: 100% !important;
            width: 100% !important;
            margin: 0;
            padding: 0;
        }

        img, a img {
            border: 0;
            outline: none;
            text-decoration: none;
        }

        .imagefix {
            display: block;
        }

        p {
            margin-top: 0;
            margin-right: 0;
            margin-left: 0;
            padding: 0;
        }

        .ReadMsgBody {
            width: 100%;
        }

        .ExternalClass {
            width: 100%;
        }

            .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {
                line-height: 100%;
            }

        img {
            -ms-interpolation-mode: bicubic;
        }

        body, table, td, p, a, li, blockquote {
            -ms-text-size-adjust: 100%;
            -webkit-text-size-adjust: 100%;
        }
    </style>
    <style type="text/css">
        @media only screen and (max-width: 600px) {
            .rtable {
                width: 100% !important;
                table-layout: fixed;
            }

                .rtable tr {
                    height: auto !important;
                    display: block;
                }

            .contenttd {
                max-width: 100% !important;
                display: block;
            }

                .contenttd:after {
                    content: "";
                    display: table;
                    clear: both;
                }

            .hiddentds {
                display: none;
            }

            .imgtable, .imgtable table {
                max-width: 100% !important;
                height: auto;
                float: none;
                margin: 0 auto;
            }

                .imgtable.btnset td {
                    display: inline-block;
                }

                .imgtable img {
                    width: 100%;
                    height: auto;
                    display: block;
                }

            table {
                float: none;
                table-layout: fixed;
            }
        }
    </style>
    <!--[if gte mso 9]> 
    <xml>
      <o:OfficeDocumentSettings>
        <o:AllowPNG/>
        <o:PixelsPerInch>96</o:PixelsPerInch>
      </o:OfficeDocumentSettings>
    </xml>
    <![endif]-->
</head>
<body style="overflow: auto; padding:0; margin:0; font-size: 12px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#f4f4f4">
    <table cellspacing="0" cellpadding="0" width="100%"
           bgcolor="#f4f4f4">
        <tr>
            <td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td>
        </tr>
        <tr>
            <td valign="top">
                <table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto"
                       cellspacing="0" cellpadding="0" width="600" align="center"
                       border="0">
                    <tr>
                        <td class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                                   align="left">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 10px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN:center !important; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent; text-aling:center;">
                                        <table class="imgtable" cellspacing="0" cellpadding="0"
                                               align=" center" border="0">
                                            <tr>
                                                <td style="PADDING-BOTTOM: 2px; PADDING-TOP: 2px; PADDING-LEFT: 2px; PADDING-RIGHT: 2px; text-aling:center;" align="center">
                                                    <table cellspacing="0" cellpadding="0" border="0">
                                                        <tr>
                                                            <td style="BORDER-TOP: medium none; BORDER-RIGHT: medium none;  BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BACKGROUND-COLOR: transparent; text-align:center;">
                                                                <a href="http://www.ofisim.com/crm/" target="_blank"><img style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block" alt="" src="https://ofisimcom.azureedge.net/web/crmLogo.png?v=1" width="200" hspace="0" vspace="0" border="0" /></a>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </th>
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: center; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class="contenttd" style="BORDER-TOP: #fff 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="center">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 10px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                                        <!--[if gte mso 12]>
                                            <table cellspacing="0" cellpadding="0" border="0" width="100%"><tr><td align="center">
                                        <![endif]-->
                                        <table class="imgtable" style="MARGIN: 0px auto" cellspacing="0"
                                               cellpadding="0" align="center" border="0">
                                            <tr>
                                                <td style="PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px"
                                                    align="center">
                                                </td>
                                            </tr>
                                        </table>
                                        <!--[if gte mso 12]>
                                            </td></tr></table>
                                        <![endif]-->
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <tr>

                        

                        <td class="contenttd" style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                                   align="left">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 20px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 20px; BORDER-LEFT: medium none; PADDING-RIGHT: 20px; BACKGROUND-COLOR: #fff;">
                                        <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            Merhaba {:FirstName} {:LastName},<br />

                                        </p>
                                        <p style="FONT-SIZE: 16px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 24px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"
                                           align="center">Aşşağıda ki linke tıklayarak e-posta adresinizi etkinleştirebilirsiniz.</p>

                                        <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            <a style="color:#1c62e; text-decoration:underline; font-size:20px;" href="{:Url}">E-Posta mı etkinleştir</a><br />
                                        </p>

                                        <p style="FONT-SIZE: 12px;MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 14px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            Ofisim.com
                                        </p>
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    <tr style="HEIGHT: 20px;">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
                            <div style="PADDING-BOTTOM: 10px; TEXT-ALIGN: center; PADDING-TOP: 10px; PADDING-LEFT: 10px; PADDING-RIGHT: 10px">
                                <table class="imgtable" style="DISPLAY: inline-block"
                                       cellspacing="0" cellpadding="0" border="0">
                                    <tr>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://www.facebook.com/ofisimcrm" target="_blank">
                                                <img title="Facebook"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Facebook" src="http://www.ofisim.com/mail/KobiMail_files/fb.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://twitter.com/ofisim_com" target="_blank">
                                                <img title="Twitter"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Twitter" src="http://www.ofisim.com/mail/KobiMail_files/tw.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://www.linkedin.com/company/ofisim.com"
                                               target="_blank">
                                                <img title="Linkedin"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Linkedin" src="http://www.ofisim.com/mail/KobiMail_files/in.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <p style="FONT-SIZE: 14px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 21px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"
                               align="left">&nbsp;</p>
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="contenttd"
                style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 10px; PADDING-TOP: 10px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                       align="left">
                    <tr class="hiddentds">
                        <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                    </tr>
                    <tr style="HEIGHT: 10px">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="contenttd"
                style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 1px; PADDING-TOP: 1px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                       align="left">
                    <tr class="hiddentds">
                        <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                    </tr>
                    <tr style="HEIGHT: 10px">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 1px; TEXT-ALIGN: left; PADDING-TOP: 1px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td>
        </tr>
    </table>
 
    <!-- Created with MailStyler 2.0.0.330 -->
</body>
</html>
', 'tr', 1, 'email_confirm', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (2, 1, NULL, '2018-10-03 14:55:24', NULL, 'f', 1, 'Şifre Sıfırlama Talebi', 'Şifre Sıfırlama Talebi', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml"
	  xmlns:v="urn:schemas-microsoft-com:vml"
	  xmlns:o="urn:schemas-microsoft-com:office:office">
<head>
	<title>Ofisim CRM''e Hoşgeldiniz!</title>
	<meta http-equiv="Content-Type"
		  content="text/html; charset=utf-8" />
	<style type="text/css">
		body, .maintable {
			height: 100% !important;
			width: 100% !important;
			margin: 0;
			padding: 0;
		}

		img, a img {
			border: 0;
			outline: none;
			text-decoration: none;
		}

		.imagefix {
			display: block;
		}

		p {
			margin-top: 0;
			margin-right: 0;
			margin-left: 0;
			padding: 0;
		}

		.ReadMsgBody {
			width: 100%;
		}

		.ExternalClass {
			width: 100%;
		}

			.ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {
				line-height: 100%;
			}

		img {
			-ms-interpolation-mode: bicubic;
		}

		body, table, td, p, a, li, blockquote {
			-ms-text-size-adjust: 100%;
			-webkit-text-size-adjust: 100%;
		}
	</style>
	<style type="text/css">
		@media only screen and (max-width: 600px) {
			.rtable {
				width: 100% !important;
				table-layout: fixed;
			}

				.rtable tr {
					height: auto !important;
					display: block;
				}

			.contenttd {
				max-width: 100% !important;
				display: block;
			}

				.contenttd:after {
					content: "";
					display: table;
					clear: both;
				}

			.hiddentds {
				display: none;
			}

			.imgtable, .imgtable table {
				max-width: 100% !important;
				height: auto;
				float: none;
				margin: 0 auto;
			}

				.imgtable.btnset td {
					display: inline-block;
				}

				.imgtable img {
					width: 100%;
					height: auto;
					display: block;
				}

			table {
				float: none;
				table-layout: fixed;
			}
		}
	</style>
	<!--[if gte mso 9]>
	<xml>
	  <o:OfficeDocumentSettings>
		<o:AllowPNG/>
		<o:PixelsPerInch>96</o:PixelsPerInch>
	  </o:OfficeDocumentSettings>
	</xml>
	<![endif]-->
</head>
<body style="overflow: auto; padding:0; margin:0; font-size: 12px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#f4f4f4">
	<table cellspacing="0" cellpadding="0" width="100%"
		   bgcolor="#f4f4f4">
		<tr>
			<td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td>
		</tr>
		<tr>
			<td valign="top">
				<table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto"
					   cellspacing="0" cellpadding="0" width="600" align="center"
					   border="0">
					<tr>
						<td class="contenttd"
							style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
								   align="left">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 10px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN:center !important; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent; text-aling:center;">
										<table class="imgtable" cellspacing="0" cellpadding="0"
											   align=" center" border="0">
											<tr>
												<td style="PADDING-BOTTOM: 2px; PADDING-TOP: 2px; PADDING-LEFT: 2px; PADDING-RIGHT: 2px; text-aling:center;" align="center">
													<table cellspacing="0" cellpadding="0" border="0">
														<tr>
															<td style="BORDER-TOP: medium none; BORDER-RIGHT: medium none;  BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BACKGROUND-COLOR: transparent; text-align:center;">
																<a href="{{APP_URL}}" target="_blank"><img style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block" alt="" src="{{URL}}" width="200" hspace="0" vspace="0" border="0" /></a>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</table>
									</th>
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: center; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
									</th>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="contenttd" style="BORDER-TOP: #fff 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="center">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 10px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
										<!--[if gte mso 12]>
							<table cellspacing="0" cellpadding="0" border="0" width="100%"><tr><td align="center">
						<![endif]-->
										<table class="imgtable" style="MARGIN: 0px auto" cellspacing="0"
											   cellpadding="0" align="center" border="0">
											<tr>
												<td style="PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px"
													align="center"></td>
											</tr>
										</table>
										<!--[if gte mso 12]>
							</td></tr></table>
						<![endif]-->
									</th>
								</tr>
							</table>
						</td>
					</tr>

					<tr>



						<td class="contenttd" style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
								   align="left">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 20px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 20px; BORDER-LEFT: medium none; PADDING-RIGHT: 20px; BACKGROUND-COLOR: #fff;">
										<p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
										   align="center">
											{Greetings}<br />

										</p>
										<p style="FONT-SIZE: 16px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 24px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"
										   align="center">{ResetRequest}</p>

										<p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
										   align="center">
											<a style="color:#2560f6; text-decoration:underline; font-size:20px;" href=''{:EmailResetUrl}''>{:EmailResetUrl}</a><br />
										</p>
									</th>
								</tr>
							</table>
						</td>
					</tr>
					{{F}}<tr style="HEIGHT: 20px;display:{{SOCIAL_ICONS}};">
						<table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
							<tr style="border-collapse:collapse;">
								<td align="center" style="padding:0;Margin:0;">
									<table class="es-content-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="600" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
										<tr style="border-collapse:collapse;">
											<td align="left" style="Margin:0;padding-top:10px;padding-left:10px;padding-right:10px;padding-bottom:30px;">
												<table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;display:{{SOCIAL_ICONS}};">
													<tr style="border-collapse:collapse;">
														<td width="580" valign="top" align="center" style="padding:0;Margin:0;">
															<table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
																<tr style="border-collapse:collapse;">
																	<td align="center" class="es-infoblock" style="padding:10px;Margin:0;line-height:120%;font-size:12px;color:#CCCCCC;"><p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:arial, ''helvetica neue'', helvetica, sans-serif;line-height:150%;color:#333333;">{{FOOTER}}</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:arial, ''helvetica neue'', helvetica, sans-serif;line-height:150%;color:#333333;">+90 212 963 0060 &amp; +90 850 532 2800 (Hafta içi: 09.00 - 18.00)</p></td>
																</tr>
																<tr style="border-collapse:collapse;">
																	<td align="center" class="es-m-txt-c" style="padding:0;Margin:0;">
																		<table cellpadding="0" cellspacing="0" class="es-table-not-adapt es-social" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
																			<tr style="border-collapse:collapse;">
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.linkedin.com/company/ofisim.com/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/linkedin.png" alt="Li" title="Linkedin" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://twitter.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/twitter.png" alt="Tw" title="Twitter" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.facebook.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/facebook.png" alt="Fb" title="Facebook" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.instagram.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/instagram.png" alt="in" title="instagram" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;"><a target="_blank" href="https://www.youtube.com/channel/UClGV9dPtU1XC2VTn2Kb414A" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/youtube.png" alt="Yt" title="Youtube" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																			</tr>
																		</table>
																	</td>
																</tr>
															</table>
														</td>
													</tr>
												</table>
											</td>
										</tr>
									</table>
								</td>
							</tr>
						</table>
					</tr>{{/F}}
				</table>
			</td>
		</tr>
		<tr>
			<td class="contenttd"
				style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 10px; PADDING-TOP: 10px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
				<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
					   align="left">
					<tr class="hiddentds">
						<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
					</tr>
					<tr style="HEIGHT: 10px">
						<th class="contenttd"
							style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
						</th>
					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<td class="contenttd"
				style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 1px; PADDING-TOP: 1px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
				<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
					   align="left">
					<tr class="hiddentds">
						<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
					</tr>
					<tr style="HEIGHT: 10px">
						<th class="contenttd"
							style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 1px; TEXT-ALIGN: left; PADDING-TOP: 1px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
						</th>
					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td>
		</tr>
	</table>

	<!-- Created with MailStyler 2.0.0.330 -->
</body>
</html>










', 'tr', 1, 'password_reset', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (3, 1, NULL, '2018-10-03 14:55:24', NULL, 'f', 1, 'Password Reset Request', 'Password Reset Request', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml"
	  xmlns:v="urn:schemas-microsoft-com:vml"
	  xmlns:o="urn:schemas-microsoft-com:office:office">
<head>
	<title>Ofisim CRM''e Hoşgeldiniz!</title>
	<meta http-equiv="Content-Type"
		  content="text/html; charset=utf-8" />
	<style type="text/css">
		body, .maintable {
			height: 100% !important;
			width: 100% !important;
			margin: 0;
			padding: 0;
		}

		img, a img {
			border: 0;
			outline: none;
			text-decoration: none;
		}

		.imagefix {
			display: block;
		}

		p {
			margin-top: 0;
			margin-right: 0;
			margin-left: 0;
			padding: 0;
		}

		.ReadMsgBody {
			width: 100%;
		}

		.ExternalClass {
			width: 100%;
		}

			.ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {
				line-height: 100%;
			}

		img {
			-ms-interpolation-mode: bicubic;
		}

		body, table, td, p, a, li, blockquote {
			-ms-text-size-adjust: 100%;
			-webkit-text-size-adjust: 100%;
		}
	</style>
	<style type="text/css">
		@media only screen and (max-width: 600px) {
			.rtable {
				width: 100% !important;
				table-layout: fixed;
			}

				.rtable tr {
					height: auto !important;
					display: block;
				}

			.contenttd {
				max-width: 100% !important;
				display: block;
			}

				.contenttd:after {
					content: "";
					display: table;
					clear: both;
				}

			.hiddentds {
				display: none;
			}

			.imgtable, .imgtable table {
				max-width: 100% !important;
				height: auto;
				float: none;
				margin: 0 auto;
			}

				.imgtable.btnset td {
					display: inline-block;
				}

				.imgtable img {
					width: 100%;
					height: auto;
					display: block;
				}

			table {
				float: none;
				table-layout: fixed;
			}
		}
	</style>
	<!--[if gte mso 9]>
	<xml>
	  <o:OfficeDocumentSettings>
		<o:AllowPNG/>
		<o:PixelsPerInch>96</o:PixelsPerInch>
	  </o:OfficeDocumentSettings>
	</xml>
	<![endif]-->
</head>
<body style="overflow: auto; padding:0; margin:0; font-size: 12px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#f4f4f4">
	<table cellspacing="0" cellpadding="0" width="100%"
		   bgcolor="#f4f4f4">
		<tr>
			<td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td>
		</tr>
		<tr>
			<td valign="top">
				<table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto"
					   cellspacing="0" cellpadding="0" width="600" align="center"
					   border="0">
					<tr>
						<td class="contenttd"
							style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
								   align="left">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 10px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN:center !important; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent; text-aling:center;">
										<table class="imgtable" cellspacing="0" cellpadding="0"
											   align=" center" border="0">
											<tr>
												<td style="PADDING-BOTTOM: 2px; PADDING-TOP: 2px; PADDING-LEFT: 2px; PADDING-RIGHT: 2px; text-aling:center;" align="center">
													<table cellspacing="0" cellpadding="0" border="0">
														<tr>
															<td style="BORDER-TOP: medium none; BORDER-RIGHT: medium none;  BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BACKGROUND-COLOR: transparent; text-align:center;">														
																<a href="{{APP_URL}}" target="_blank"><img style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block" alt="" src="{{URL}}" width="200" hspace="0" vspace="0" border="0" /></a>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</table>
									</th>
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: center; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
									</th>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="contenttd" style="BORDER-TOP: #fff 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="center">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 10px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
										<!--[if gte mso 12]>
							<table cellspacing="0" cellpadding="0" border="0" width="100%"><tr><td align="center">
						<![endif]-->
										<table class="imgtable" style="MARGIN: 0px auto" cellspacing="0"
											   cellpadding="0" align="center" border="0">
											<tr>
												<td style="PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px"
													align="center"></td>
											</tr>
										</table>
										<!--[if gte mso 12]>
							</td></tr></table>
						<![endif]-->
									</th>
								</tr>
							</table>
						</td>
					</tr>

					<tr>



						<td class="contenttd" style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
								   align="left">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 20px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 20px; BORDER-LEFT: medium none; PADDING-RIGHT: 20px; BACKGROUND-COLOR: #fff;">
										<!-- <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly" -->
										   <!-- align="center"> -->
											<!-- {Greetings}<br /> -->

										<!-- </p> -->
										 <p style="FONT-SIZE: 16px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 24px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly" 
										   align="center"> Merhaba {:FullName},<br /></p> 

										<p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
										   align="center">
										    <a style="color:#2560f6; text-decoration:underline; font-size:20px;" href=''{:PasswordResetUrl}''>Şifremi Sıfırla</a><br />
											<!-- <a style="color:#2560f6; text-decoration:underline; font-size:20px;" href=''{:EmailResetUrl}''>{:EmailResetUrl}</a><br /> -->
										</p>
									</th>
								</tr>
							</table>
						</td>
					</tr>
					{{F}}<tr style="HEIGHT: 20px;display:{{SOCIAL_ICONS}};">
						<table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
							<tr style="border-collapse:collapse;">
								<td align="center" style="padding:0;Margin:0;">
									<table class="es-content-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="600" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
										<tr style="border-collapse:collapse;">
											<td align="left" style="Margin:0;padding-top:10px;padding-left:10px;padding-right:10px;padding-bottom:30px;">
												<table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;display:{{SOCIAL_ICONS}};">
													<tr style="border-collapse:collapse;">
														<td width="580" valign="top" align="center" style="padding:0;Margin:0;">
															<table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
																<tr style="border-collapse:collapse;">
																	<!-- <td align="center" class="es-infoblock" style="padding:10px;Margin:0;line-height:120%;font-size:12px;color:#CCCCCC;"><p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:arial, ''helvetica neue'', helvetica, sans-serif;line-height:150%;color:#333333;">{{FOOTER}}</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:arial, ''helvetica neue'', helvetica, sans-serif;line-height:150%;color:#333333;">+90 212 963 0060 &amp; +90 850 532 2800 (Hafta içi: 09.00 - 18.00)</p></td> -->
																</tr>
																<tr style="border-collapse:collapse;">
																	<td align="center" class="es-m-txt-c" style="padding:0;Margin:0;">
																		<table cellpadding="0" cellspacing="0" class="es-table-not-adapt es-social" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
																			<tr style="border-collapse:collapse;">
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.linkedin.com/company/ofisim.com/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/linkedin.png" alt="Li" title="Linkedin" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://twitter.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/twitter.png" alt="Tw" title="Twitter" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.facebook.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/facebook.png" alt="Fb" title="Facebook" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.instagram.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/instagram.png" alt="in" title="instagram" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;"><a target="_blank" href="https://www.youtube.com/channel/UClGV9dPtU1XC2VTn2Kb414A" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/youtube.png" alt="Yt" title="Youtube" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																			</tr>
																		</table>
																	</td>
																</tr>
															</table>
														</td>
													</tr>
												</table>
											</td>
										</tr>
									</table>
								</td>
							</tr>
						</table>
					</tr>{{/F}}
				</table>
			</td>
		</tr>
		<tr>
			<td class="contenttd"
				style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 10px; PADDING-TOP: 10px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
				<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
					   align="left">
					<tr class="hiddentds">
						<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
					</tr>
					<tr style="HEIGHT: 10px">
						<th class="contenttd"
							style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
						</th>
					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<td class="contenttd"
				style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 1px; PADDING-TOP: 1px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
				<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
					   align="left">
					<tr class="hiddentds">
						<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
					</tr>
					<tr style="HEIGHT: 10px">
						<th class="contenttd"
							style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 1px; TEXT-ALIGN: left; PADDING-TOP: 1px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
						</th>
					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td>
		</tr>
	</table>

	<!-- Created with MailStyler 2.0.0.330 -->
</body>
</html>










', 'en', 1, 'password_reset', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (4, 1, NULL, '2018-10-01 11:41:02.829818', NULL, 'f', 1, 'Account Activation', 'Account Activation', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml"
      xmlns:v="urn:schemas-microsoft-com:vml"
      xmlns:o="urn:schemas-microsoft-com:office:office">
<head>
    <title>Ofisim CRM''e Hoşgeldiniz!</title>
    <meta http-equiv="Content-Type"
          content="text/html; charset=utf-8" />
    <style type="text/css">
        body, .maintable {
            height: 100% !important;
            width: 100% !important;
            margin: 0;
            padding: 0;
        }

        img, a img {
            border: 0;
            outline: none;
            text-decoration: none;
        }

        .imagefix {
            display: block;
        }

        p {
            margin-top: 0;
            margin-right: 0;
            margin-left: 0;
            padding: 0;
        }

        .ReadMsgBody {
            width: 100%;
        }

        .ExternalClass {
            width: 100%;
        }

            .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {
                line-height: 100%;
            }

        img {
            -ms-interpolation-mode: bicubic;
        }

        body, table, td, p, a, li, blockquote {
            -ms-text-size-adjust: 100%;
            -webkit-text-size-adjust: 100%;
        }
    </style>
    <style type="text/css">
        @media only screen and (max-width: 600px) {
            .rtable {
                width: 100% !important;
                table-layout: fixed;
            }

                .rtable tr {
                    height: auto !important;
                    display: block;
                }

            .contenttd {
                max-width: 100% !important;
                display: block;
            }

                .contenttd:after {
                    content: "";
                    display: table;
                    clear: both;
                }

            .hiddentds {
                display: none;
            }

            .imgtable, .imgtable table {
                max-width: 100% !important;
                height: auto;
                float: none;
                margin: 0 auto;
            }

                .imgtable.btnset td {
                    display: inline-block;
                }

                .imgtable img {
                    width: 100%;
                    height: auto;
                    display: block;
                }

            table {
                float: none;
                table-layout: fixed;
            }
        }
    </style>
    <!--[if gte mso 9]> 
    <xml>
      <o:OfficeDocumentSettings>
        <o:AllowPNG/>
        <o:PixelsPerInch>96</o:PixelsPerInch>
      </o:OfficeDocumentSettings>
    </xml>
    <![endif]-->
</head>
<body style="overflow: auto; padding:0; margin:0; font-size: 12px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#f4f4f4">
    <table cellspacing="0" cellpadding="0" width="100%"
           bgcolor="#f4f4f4">
        <tr>
            <td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td>
        </tr>
        <tr>
            <td valign="top">
                <table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto"
                       cellspacing="0" cellpadding="0" width="600" align="center"
                       border="0">
                    <tr>
                        <td class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                                   align="left">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 10px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN:center !important; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent; text-aling:center;">
                                        <table class="imgtable" cellspacing="0" cellpadding="0"
                                               align=" center" border="0">
                                            <tr>
                                                <td style="PADDING-BOTTOM: 2px; PADDING-TOP: 2px; PADDING-LEFT: 2px; PADDING-RIGHT: 2px; text-aling:center;" align="center">
                                                    <table cellspacing="0" cellpadding="0" border="0">
                                                        <tr>
                                                            <td style="BORDER-TOP: medium none; BORDER-RIGHT: medium none;  BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BACKGROUND-COLOR: transparent; text-align:center;">
                                                                <a href="http://www.ofisim.com/crm/" target="_blank"><img style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block" alt="" src="https://ofisimcom.azureedge.net/web/crmLogo.png?v=1" width="200" hspace="0" vspace="0" border="0" /></a>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </th>
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: center; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class="contenttd" style="BORDER-TOP: #fff 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="center">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 10px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                                        <!--[if gte mso 12]>
                                            <table cellspacing="0" cellpadding="0" border="0" width="100%"><tr><td align="center">
                                        <![endif]-->
                                        <table class="imgtable" style="MARGIN: 0px auto" cellspacing="0"
                                               cellpadding="0" align="center" border="0">
                                            <tr>
                                                <td style="PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px"
                                                    align="center">
                                                </td>
                                            </tr>
                                        </table>
                                        <!--[if gte mso 12]>
                                            </td></tr></table>
                                        <![endif]-->
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <tr>

                        

                        <td class="contenttd" style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                                   align="left">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 20px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 20px; BORDER-LEFT: medium none; PADDING-RIGHT: 20px; BACKGROUND-COLOR: #fff;">
                                        <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            Merhaba {:FirstName} {:LastName},<br />

                                        </p>
                                        <p style="FONT-SIZE: 16px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 24px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"
                                           align="center">Aşşağıda ki linke tıklayarak e-posta adresinizi etkinleştirebilirsiniz.</p>

                                        <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            <a style="color:#1c62e; text-decoration:underline; font-size:20px;" href="{:Url}">E-Posta mı etkinleştir</a><br />
                                        </p>

                                        <p style="FONT-SIZE: 12px;MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 14px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            Ofisim.com
                                        </p>
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    <tr style="HEIGHT: 20px;">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
                            <div style="PADDING-BOTTOM: 10px; TEXT-ALIGN: center; PADDING-TOP: 10px; PADDING-LEFT: 10px; PADDING-RIGHT: 10px">
                                <table class="imgtable" style="DISPLAY: inline-block"
                                       cellspacing="0" cellpadding="0" border="0">
                                    <tr>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://www.facebook.com/ofisimcrm" target="_blank">
                                                <img title="Facebook"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Facebook" src="http://www.ofisim.com/mail/KobiMail_files/fb.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://twitter.com/ofisim_com" target="_blank">
                                                <img title="Twitter"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Twitter" src="http://www.ofisim.com/mail/KobiMail_files/tw.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://www.linkedin.com/company/ofisim.com"
                                               target="_blank">
                                                <img title="Linkedin"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Linkedin" src="http://www.ofisim.com/mail/KobiMail_files/in.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <p style="FONT-SIZE: 14px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 21px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"
                               align="left">&nbsp;</p>
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="contenttd"
                style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 10px; PADDING-TOP: 10px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                       align="left">
                    <tr class="hiddentds">
                        <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                    </tr>
                    <tr style="HEIGHT: 10px">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="contenttd"
                style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 1px; PADDING-TOP: 1px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                       align="left">
                    <tr class="hiddentds">
                        <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                    </tr>
                    <tr style="HEIGHT: 10px">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 1px; TEXT-ALIGN: left; PADDING-TOP: 1px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td>
        </tr>
    </table>
 
    <!-- Created with MailStyler 2.0.0.330 -->
</body>
</html>
', 'en', 1, 'email_confirm', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (9, 1, NULL, '2019-03-12 13:58:03', NULL, 'f', 2, 'Kullanıcı Ekleme - App', 'Kullanıcı Bilgisi - App', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
 <head>
  <meta charset="UTF-8">
  <meta content="width=device-width, initial-scale=1" name="viewport">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta content="telephone=no" name="format-detection">
  <title>New email 2</title>
  <!--[if (mso 16)]>
    <style type="text/css">
    a {text-decoration: none;}
    </style>
    <![endif]-->
  <!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]-->
  <!--[if !mso]><!-- -->
  <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,400,400i,700,700i&amp;subset=latin-ext" rel="stylesheet">
  <!--<![endif]-->
  <style type="text/css">
@media only screen and (max-width:600px) {p, ul li, ol li, a { font-size:14px!important; line-height:150%!important } h1 { font-size:26px!important; text-align:center } h2 { font-size:24px!important; text-align:center } h3 { font-size:20px!important; text-align:center } h1 a { font-size:26px!important } h2 a { font-size:24px!important } h3 a { font-size:20px!important } .es-menu td a { font-size:13px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:16px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:12px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class="gmail-fix"] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:block!important } a.es-button { font-size:14px!important; display:block!important; border-left-width:0px!important; border-right-width:0px!important } .es-btn-fw { border-width:10px 0px!important; text-align:center!important } .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } .es-desk-menu-hidden { display:table-cell!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } }
#outlook a {
    padding:0;
}
.ExternalClass {
    width:100%;
}
.ExternalClass,
.ExternalClass p,
.ExternalClass span,
.ExternalClass font,
.ExternalClass td,
.ExternalClass div {
    line-height:100%;
}
.es-button {
    mso-style-priority:100!important;
    text-decoration:none!important;
}
a[x-apple-data-detectors] {
    color:inherit!important;
    text-decoration:none!important;
    font-size:inherit!important;
    font-family:inherit!important;
    font-weight:inherit!important;
    line-height:inherit!important;
}
.es-desk-hidden {
    display:none;
    float:left;
    overflow:hidden;
    width:0;
    max-height:0;
    line-height:0;
    mso-hide:all;
}
</style>
 </head>
 <body style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
  <div class="es-wrapper-color" style="background-color:#333333;">
   <!--[if gte mso 9]>
			<v:background xmlns:v="urn:schemas-microsoft-com:vml" fill="t">
				<v:fill type="tile" src="https://www.primeapps.io/mail/images/bg.jpg" color="#333333" origin="0.5, 0" position="0.5,0"></v:fill>
			</v:background>
		<![endif]-->
   <table class="es-wrapper" width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-image:url(https://www.primeapps.io/mail/images/bg.jpg);background-repeat:repeat;background-position:center top;" background="https://www.primeapps.io/mail/images/bg.jpg">
     <tr style="border-collapse:collapse;">
      <td valign="top" style="padding:0;Margin:0;">
       <table class="es-header" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td class="es-adaptive" align="center" style="padding:0;Margin:0;">
           <table class="es-header-body" width="650" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;">
             <tr style="border-collapse:collapse;">
              <td style="padding:0;Margin:0;padding-bottom:30px;padding-top:40px;background-color:transparent;" bgcolor="transparent" align="left">
               <table cellspacing="0" cellpadding="0" width="100%" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td class="es-m-p0r" width="650" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-p0l es-m-txt-c" align="center" style="padding:0;Margin:0;"> <a href="https://www.ofisim.com/ik/" target="_blank" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#333333;"><img src="https://www.primeapps.io/mail/images/logo_white.png" alt="Ofisim İK" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" title="Ofisim İK" height="50" width="250"></a> </td> 
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
         <tr style="border-collapse:collapse;">
          <td align="center" style="padding:0;Margin:0;">
           <table class="es-content-body" width="650" cellspacing="0" cellpadding="0" bgcolor="#333333" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="610" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="table-layout: fixed; mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="Margin:0;padding-top:10px;padding-bottom:10px;padding-left:10px;padding-right:10px;"> <h1 style="Margin:0;line-height:43px;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-weight:200;font-size:36px;font-style:normal;color:#333333;">Kullanıcı Bilgisi</h1> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Merhaba,<br /> {:DisplayName} adlı kullanıcının giriş bilgileri aşağıdaki gibidir.</p> </td>
                     </tr>
                       <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">E-mail : {:Email} <br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Şifre &nbsp;&nbsp;&nbsp;: {:Password}</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Saygılar,&nbsp;</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">PrimeApps Takımı</p> </td>
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-footer" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0;background-color:transparent;">
           <table class="es-footer-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="650" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:10px;padding-right:10px;padding-top:20px;padding-bottom:40px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="630" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-txt-c" align="center" style="padding:10px;Margin:0;">
                       <table class="es-table-not-adapt es-social" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                         <tr style="border-collapse:collapse;">
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.linkedin.com/company/primeapps/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/linkedin.png" alt="Li" title="Linkedin" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://twitter.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/twitter.png" alt="Tw" title="Twitter" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.facebook.com/PrimeApps.io/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/facebook.png" alt="Fb" title="Facebook" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;"> <a target="_blank" href="https://www.instagram.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/instagram.png" alt="in" title="instagram" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                         </tr>
                       </table> </td>
                     </tr>
                   
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table> </td>
     </tr>
   </table>
  </div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;"></div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;padding:0px;border:0px none;width:1px;"></div>
 </body>
</html>
', 'tr', 1, 'app_draft_user', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (10, 1, NULL, '2018-10-01 11:41:02.829818', NULL, 'f', 2, 'E-Posta Onaylama', 'E-Posta Onaylama', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
 <head>
  <meta charset="UTF-8">
  <meta content="width=device-width, initial-scale=1" name="viewport">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta content="telephone=no" name="format-detection">
  <title>New email 2</title>
  <!--[if (mso 16)]>
    <style type="text/css">
    a {text-decoration: none;}
    </style>
    <![endif]-->
  <!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]-->
  <!--[if !mso]><!-- -->
  <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,400,400i,700,700i&amp;subset=latin-ext" rel="stylesheet">
  <!--<![endif]-->
  <style type="text/css">
@media only screen and (max-width:600px) {p, ul li, ol li, a { font-size:14px!important; line-height:150%!important } h1 { font-size:26px!important; text-align:center } h2 { font-size:24px!important; text-align:center } h3 { font-size:20px!important; text-align:center } h1 a { font-size:26px!important } h2 a { font-size:24px!important } h3 a { font-size:20px!important } .es-menu td a { font-size:13px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:16px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:12px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class="gmail-fix"] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:block!important } a.es-button { font-size:14px!important; display:block!important; border-left-width:0px!important; border-right-width:0px!important } .es-btn-fw { border-width:10px 0px!important; text-align:center!important } .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } .es-desk-menu-hidden { display:table-cell!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } }
#outlook a {
    padding:0;
}
.ExternalClass {
    width:100%;
}
.ExternalClass,
.ExternalClass p,
.ExternalClass span,
.ExternalClass font,
.ExternalClass td,
.ExternalClass div {
    line-height:100%;
}
.es-button {
    mso-style-priority:100!important;
    text-decoration:none!important;
}
a[x-apple-data-detectors] {
    color:inherit!important;
    text-decoration:none!important;
    font-size:inherit!important;
    font-family:inherit!important;
    font-weight:inherit!important;
    line-height:inherit!important;
}
.es-desk-hidden {
    display:none;
    float:left;
    overflow:hidden;
    width:0;
    max-height:0;
    line-height:0;
    mso-hide:all;
}
</style>
 </head>
 <body style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
  <div class="es-wrapper-color" style="background-color:#333333;">
   <!--[if gte mso 9]>
			<v:background xmlns:v="urn:schemas-microsoft-com:vml" fill="t">
				<v:fill type="tile" src="https://www.primeapps.io/mail/images/bg.jpg" color="#333333" origin="0.5, 0" position="0.5,0"></v:fill>
			</v:background>
		<![endif]-->
   <table class="es-wrapper" width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-image:url(https://www.primeapps.io/mail/images/bg.jpg);background-repeat:repeat;background-position:center top;" background="https://www.primeapps.io/mail/images/bg.jpg">
     <tr style="border-collapse:collapse;">
      <td valign="top" style="padding:0;Margin:0;">
       <table class="es-header" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td class="es-adaptive" align="center" style="padding:0;Margin:0;">
           <table class="es-header-body" width="650" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;">
             <tr style="border-collapse:collapse;">
              <td style="padding:0;Margin:0;padding-bottom:30px;padding-top:40px;background-color:transparent;" bgcolor="transparent" align="left">
               <table cellspacing="0" cellpadding="0" width="100%" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td class="es-m-p0r" width="650" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-p0l es-m-txt-c" align="center" style="padding:0;Margin:0;"> <a href="https://www.ofisim.com/ik/" target="_blank" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#333333;"><img src="https://www.primeapps.io/mail/images/logo_white.png" alt="Ofisim İK" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" title="Ofisim İK" height="50" width="250"></a> </td> 
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
         <tr style="border-collapse:collapse;">
          <td align="center" style="padding:0;Margin:0;">
           <table class="es-content-body" width="650" cellspacing="0" cellpadding="0" bgcolor="#333333" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="610" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="table-layout: fixed; mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="Margin:0;padding-top:10px;padding-bottom:10px;padding-left:10px;padding-right:10px;"> <h1 style="Margin:0;line-height:43px;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-weight:200;font-size:36px;font-style:normal;color:#333333;">Merhaba {:FirstName}, PrimeApps''e üye olduğun için teşekkür ederiz!</h1> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Aşağıdaki butona tıklayarak hesabınızı aktifleştirebilirsiniz.</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="padding:10px;Margin:0;"> <span class="es-button-border" style="border-style:solid;border-color:#808080;background:#0D6FAA;border-width:0px;display:inline-block;border-radius:30px;width:auto;"> <a href="{:Url}" class="es-button" target="_blank" style="mso-style-priority:100 !important;text-decoration:none;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-size:14px;color:#FFFFFF;border-style:solid;border-color:#0D6FAA;border-width:20px 40px;display:inline-block;background:#0D6FAA;border-radius:30px;font-weight:normal;font-style:normal;line-height:17px;width:auto;text-align:center;">Hesabımı Etkinleştir</a> </span> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Saygılar,&nbsp;</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">PrimeApps Takımı</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style=" word-wrap: break-word; padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:18px;color:#333333;">Yukarıdaki buton çalışmazsa lütfen aşağıda yer alan linki kopyalayıp tarayıcınıza yapıştırın :</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:18px;color:#333333;">{:Url}</p> </td>
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-footer" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0;background-color:transparent;">
           <table class="es-footer-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="650" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:10px;padding-right:10px;padding-top:20px;padding-bottom:40px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="630" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-txt-c" align="center" style="padding:10px;Margin:0;">
                       <table class="es-table-not-adapt es-social" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                         <tr style="border-collapse:collapse;">
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.linkedin.com/company/primeapps/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/linkedin.png" alt="Li" title="Linkedin" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://twitter.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/twitter.png" alt="Tw" title="Twitter" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.facebook.com/PrimeApps.io/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/facebook.png" alt="Fb" title="Facebook" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;"> <a target="_blank" href="https://www.instagram.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/instagram.png" alt="in" title="instagram" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                         </tr>
                       </table> </td>
                     </tr>
                     
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table> </td>
     </tr>
   </table>
  </div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;"></div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;padding:0px;border:0px none;width:1px;"></div>
 </body>
</html>
', 'tr', 1, 'email_confirm', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (11, 1, NULL, '2018-10-01 11:41:02.829818', NULL, 'f', 2, 'Account Activation', 'Account Activation', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
 <head>
  <meta charset="UTF-8">
  <meta content="width=device-width, initial-scale=1" name="viewport">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta content="telephone=no" name="format-detection">
  <title>New email 2</title>
  <!--[if (mso 16)]>
    <style type="text/css">
    a {text-decoration: none;}
    </style>
    <![endif]-->
  <!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]-->
  <!--[if !mso]><!-- -->
  <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,400,400i,700,700i&amp;subset=latin-ext" rel="stylesheet">
  <!--<![endif]-->
  <style type="text/css">
@media only screen and (max-width:600px) {p, ul li, ol li, a { font-size:14px!important; line-height:150%!important } h1 { font-size:26px!important; text-align:center } h2 { font-size:24px!important; text-align:center } h3 { font-size:20px!important; text-align:center } h1 a { font-size:26px!important } h2 a { font-size:24px!important } h3 a { font-size:20px!important } .es-menu td a { font-size:13px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:16px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:12px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class="gmail-fix"] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:block!important } a.es-button { font-size:14px!important; display:block!important; border-left-width:0px!important; border-right-width:0px!important } .es-btn-fw { border-width:10px 0px!important; text-align:center!important } .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } .es-desk-menu-hidden { display:table-cell!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } }
#outlook a {
    padding:0;
}
.ExternalClass {
    width:100%;
}
.ExternalClass,
.ExternalClass p,
.ExternalClass span,
.ExternalClass font,
.ExternalClass td,
.ExternalClass div {
    line-height:100%;
}
.es-button {
    mso-style-priority:100!important;
    text-decoration:none!important;
}
a[x-apple-data-detectors] {
    color:inherit!important;
    text-decoration:none!important;
    font-size:inherit!important;
    font-family:inherit!important;
    font-weight:inherit!important;
    line-height:inherit!important;
}
.es-desk-hidden {
    display:none;
    float:left;
    overflow:hidden;
    width:0;
    max-height:0;
    line-height:0;
    mso-hide:all;
}
</style>
 </head>
 <body style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
  <div class="es-wrapper-color" style="background-color:#333333;">
   <!--[if gte mso 9]>
			<v:background xmlns:v="urn:schemas-microsoft-com:vml" fill="t">
				<v:fill type="tile" src="https://www.primeapps.io/mail/images/bg.jpg" color="#333333" origin="0.5, 0" position="0.5,0"></v:fill>
			</v:background>
		<![endif]-->
   <table class="es-wrapper" width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-image:url(https://www.primeapps.io/mail/images/bg.jpg);background-repeat:repeat;background-position:center top;" background="https://www.primeapps.io/mail/images/bg.jpg">
     <tr style="border-collapse:collapse;">
      <td valign="top" style="padding:0;Margin:0;">
       <table class="es-header" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td class="es-adaptive" align="center" style="padding:0;Margin:0;">
           <table class="es-header-body" width="650" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;">
             <tr style="border-collapse:collapse;">
              <td style="padding:0;Margin:0;padding-bottom:30px;padding-top:40px;background-color:transparent;" bgcolor="transparent" align="left">
               <table cellspacing="0" cellpadding="0" width="100%" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td class="es-m-p0r" width="650" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-p0l es-m-txt-c" align="center" style="padding:0;Margin:0;"> <a href="https://www.ofisim.com/ik/" target="_blank" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#333333;"><img src="https://www.primeapps.io/mail/images/logo_white.png" alt="Ofisim İK" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" title="Ofisim İK" height="50" width="250"></a> </td> 
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
         <tr style="border-collapse:collapse;">
          <td align="center" style="padding:0;Margin:0;">
           <table class="es-content-body" width="650" cellspacing="0" cellpadding="0" bgcolor="#333333" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="610" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="table-layout: fixed; mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="Margin:0;padding-top:10px;padding-bottom:10px;padding-left:10px;padding-right:10px;"> <h1 style="Margin:0;line-height:43px;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-weight:200;font-size:36px;font-style:normal;color:#333333;">Thanks for signing up for PrimeApps {:FirstName},</h1> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Please click the button below to confirm your email address and activate your account. Thank you for using our plaform!</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="padding:10px;Margin:0;"> <span class="es-button-border" style="border-style:solid;border-color:#808080;background:#0D6FAA;border-width:0px;display:inline-block;border-radius:30px;width:auto;"> <a href="{:Url}" class="es-button" target="_blank" style="mso-style-priority:100 !important;text-decoration:none;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-size:14px;color:#FFFFFF;border-style:solid;border-color:#0D6FAA;border-width:20px 40px;display:inline-block;background:#0D6FAA;border-radius:30px;font-weight:normal;font-style:normal;line-height:17px;width:auto;text-align:center;">Activate Your Account</a> </span> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Regards,&nbsp;</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">PrimeApps Team</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="word-wrap: break-word; padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:18px;color:#333333;">If the button doesn''t work, please copy and paste the following link into your browser:</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:18px;color:#333333;">{:Url}</p> </td>
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-footer" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0;background-color:transparent;">
           <table class="es-footer-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="650" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:10px;padding-right:10px;padding-top:20px;padding-bottom:40px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="630" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-txt-c" align="center" style="padding:10px;Margin:0;">
                       <table class="es-table-not-adapt es-social" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                         <tr style="border-collapse:collapse;">
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.linkedin.com/company/primeapps/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/linkedin.png" alt="Li" title="Linkedin" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://twitter.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/twitter.png" alt="Tw" title="Twitter" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.facebook.com/PrimeApps.io/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/facebook.png" alt="Fb" title="Facebook" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;"> <a target="_blank" href="https://www.instagram.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/instagram.png" alt="in" title="instagram" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                         </tr>
                       </table> </td>
                     </tr>
                   
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table> </td>
     </tr>
   </table>
  </div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;"></div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;padding:0px;border:0px none;width:1px;"></div>
 </body>
</html>
', 'en', 1, 'email_confirm', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (12, 1, NULL, '2018-10-03 14:55:24', NULL, 'f', 2, 'Password Reset Request', 'Password Reset Request', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
 <head>
  <meta charset="UTF-8">
  <meta content="width=device-width, initial-scale=1" name="viewport">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta content="telephone=no" name="format-detection">
  <title>New email 2</title>
  <!--[if (mso 16)]>
    <style type="text/css">
    a {text-decoration: none;}
    </style>
    <![endif]-->
  <!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]-->
  <!--[if !mso]><!-- -->
  <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,400,400i,700,700i&amp;subset=latin-ext" rel="stylesheet">
  <!--<![endif]-->
  <style type="text/css">
@media only screen and (max-width:600px) {p, ul li, ol li, a { font-size:14px!important; line-height:150%!important } h1 { font-size:26px!important; text-align:center } h2 { font-size:24px!important; text-align:center } h3 { font-size:20px!important; text-align:center } h1 a { font-size:26px!important } h2 a { font-size:24px!important } h3 a { font-size:20px!important } .es-menu td a { font-size:13px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:16px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:12px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class="gmail-fix"] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:block!important } a.es-button { font-size:14px!important; display:block!important; border-left-width:0px!important; border-right-width:0px!important } .es-btn-fw { border-width:10px 0px!important; text-align:center!important } .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } .es-desk-menu-hidden { display:table-cell!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } }
#outlook a {
    padding:0;
}
.ExternalClass {
    width:100%;
}
.ExternalClass,
.ExternalClass p,
.ExternalClass span,
.ExternalClass font,
.ExternalClass td,
.ExternalClass div {
    line-height:100%;
}
.es-button {
    mso-style-priority:100!important;
    text-decoration:none!important;
}
a[x-apple-data-detectors] {
    color:inherit!important;
    text-decoration:none!important;
    font-size:inherit!important;
    font-family:inherit!important;
    font-weight:inherit!important;
    line-height:inherit!important;
}
.es-desk-hidden {
    display:none;
    float:left;
    overflow:hidden;
    width:0;
    max-height:0;
    line-height:0;
    mso-hide:all;
}
</style>
 </head>
 <body style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
  <div class="es-wrapper-color" style="background-color:#333333;">
   <!--[if gte mso 9]>
			<v:background xmlns:v="urn:schemas-microsoft-com:vml" fill="t">
				<v:fill type="tile" src="https://www.primeapps.io/mail/images/bg.jpg" color="#333333" origin="0.5, 0" position="0.5,0"></v:fill>
			</v:background>
		<![endif]-->
   <table class="es-wrapper" width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-image:url(https://www.primeapps.io/mail/images/bg.jpg);background-repeat:repeat;background-position:center top;" background="https://www.primeapps.io/mail/images/bg.jpg">
     <tr style="border-collapse:collapse;">
      <td valign="top" style="padding:0;Margin:0;">
       <table class="es-header" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td class="es-adaptive" align="center" style="padding:0;Margin:0;">
           <table class="es-header-body" width="650" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;">
             <tr style="border-collapse:collapse;">
              <td style="padding:0;Margin:0;padding-bottom:30px;padding-top:40px;background-color:transparent;" bgcolor="transparent" align="left">
               <table cellspacing="0" cellpadding="0" width="100%" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td class="es-m-p0r" width="650" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-p0l es-m-txt-c" align="center" style="padding:0;Margin:0;"> <a href="https://www.ofisim.com/ik/" target="_blank" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#333333;"><img src="https://www.primeapps.io/mail/images/logo_white.png" alt="Ofisim İK" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" title="Ofisim İK" height="50" width="250"></a> </td> 
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
         <tr style="border-collapse:collapse;">
          <td align="center" style="padding:0;Margin:0;">
           <table class="es-content-body" width="650" cellspacing="0" cellpadding="0" bgcolor="#333333" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="610" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="table-layout: fixed; mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="Margin:0;padding-top:10px;padding-bottom:10px;padding-left:10px;padding-right:10px;"> <h1 style="Margin:0;line-height:43px;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-weight:200;font-size:36px;font-style:normal;color:#333333;">Reset Password Requested</h1> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Reset the password on your PrimeApps Studio account by clicking the button below:</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="padding:10px;Margin:0;"> <span class="es-button-border" style="border-style:solid;border-color:#808080;background:#0D6FAA;border-width:0px;display:inline-block;border-radius:30px;width:auto;"> <a href="{:PasswordResetUrl}" class="es-button" target="_blank" style="mso-style-priority:100 !important;text-decoration:none;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-size:14px;color:#FFFFFF;border-style:solid;border-color:#0D6FAA;border-width:20px 40px;display:inline-block;background:#0D6FAA;border-radius:30px;font-weight:normal;font-style:normal;line-height:17px;width:auto;text-align:center;">Reset Your Password</a> </span> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Regards,&nbsp;</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">PrimeApps Team</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="word-wrap: break-word; padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:18px;color:#333333;">If the button doesn''t work, please copy and paste the following link into your browser:</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:18px;color:#333333;">{:PasswordResetUrl}</p> </td>
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-footer" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0;background-color:transparent;">
           <table class="es-footer-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="650" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:10px;padding-right:10px;padding-top:20px;padding-bottom:40px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="630" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-txt-c" align="center" style="padding:10px;Margin:0;">
                       <table class="es-table-not-adapt es-social" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                         <tr style="border-collapse:collapse;">
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.linkedin.com/company/primeapps/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/linkedin.png" alt="Li" title="Linkedin" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://twitter.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/twitter.png" alt="Tw" title="Twitter" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.facebook.com/PrimeApps.io/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/facebook.png" alt="Fb" title="Facebook" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;"> <a target="_blank" href="https://www.instagram.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/instagram.png" alt="in" title="instagram" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                         </tr>
                       </table> </td>
                     </tr>
                     
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table> </td>
     </tr>
   </table>
  </div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;"></div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;padding:0px;border:0px none;width:1px;"></div>
 </body>
</html>
', 'en', 1, 'password_reset', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (13, 1, NULL, '2018-10-03 14:55:24', NULL, 'f', 2, 'Şifre Sıfırlama Talebi', 'Şifre Sıfırlama Talebi', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
 <head>
  <meta charset="UTF-8">
  <meta content="width=device-width, initial-scale=1" name="viewport">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta content="telephone=no" name="format-detection">
  <title>New email 2</title>
  <!--[if (mso 16)]>
    <style type="text/css">
    a {text-decoration: none;}
    </style>
    <![endif]-->
  <!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]-->
  <!--[if !mso]><!-- -->
  <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,400,400i,700,700i&amp;subset=latin-ext" rel="stylesheet">
  <!--<![endif]-->
  <style type="text/css">
@media only screen and (max-width:600px) {p, ul li, ol li, a { font-size:14px!important; line-height:150%!important } h1 { font-size:26px!important; text-align:center } h2 { font-size:24px!important; text-align:center } h3 { font-size:20px!important; text-align:center } h1 a { font-size:26px!important } h2 a { font-size:24px!important } h3 a { font-size:20px!important } .es-menu td a { font-size:13px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:16px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:12px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class="gmail-fix"] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:block!important } a.es-button { font-size:14px!important; display:block!important; border-left-width:0px!important; border-right-width:0px!important } .es-btn-fw { border-width:10px 0px!important; text-align:center!important } .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } .es-desk-menu-hidden { display:table-cell!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } }
#outlook a {
    padding:0;
}
.ExternalClass {
    width:100%;
}
.ExternalClass,
.ExternalClass p,
.ExternalClass span,
.ExternalClass font,
.ExternalClass td,
.ExternalClass div {
    line-height:100%;
}
.es-button {
    mso-style-priority:100!important;
    text-decoration:none!important;
}
a[x-apple-data-detectors] {
    color:inherit!important;
    text-decoration:none!important;
    font-size:inherit!important;
    font-family:inherit!important;
    font-weight:inherit!important;
    line-height:inherit!important;
}
.es-desk-hidden {
    display:none;
    float:left;
    overflow:hidden;
    width:0;
    max-height:0;
    line-height:0;
    mso-hide:all;
}
</style>
 </head>
 <body style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
  <div class="es-wrapper-color" style="background-color:#333333;">
   <!--[if gte mso 9]>
			<v:background xmlns:v="urn:schemas-microsoft-com:vml" fill="t">
				<v:fill type="tile" src="https://www.primeapps.io/mail/images/bg.jpg" color="#333333" origin="0.5, 0" position="0.5,0"></v:fill>
			</v:background>
		<![endif]-->
   <table class="es-wrapper" width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-image:url(https://www.primeapps.io/mail/images/bg.jpg);background-repeat:repeat;background-position:center top;" background="https://www.primeapps.io/mail/images/bg.jpg">
     <tr style="border-collapse:collapse;">
      <td valign="top" style="padding:0;Margin:0;">
       <table class="es-header" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td class="es-adaptive" align="center" style="padding:0;Margin:0;">
           <table class="es-header-body" width="650" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;">
             <tr style="border-collapse:collapse;">
              <td style="padding:0;Margin:0;padding-bottom:30px;padding-top:40px;background-color:transparent;" bgcolor="transparent" align="left">
               <table cellspacing="0" cellpadding="0" width="100%" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td class="es-m-p0r" width="650" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-p0l es-m-txt-c" align="center" style="padding:0;Margin:0;"> <a href="https://www.ofisim.com/ik/" target="_blank" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#333333;"><img src="https://www.primeapps.io/mail/images/logo_white.png" alt="Ofisim İK" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" title="Ofisim İK" height="50" width="250"></a> </td> 
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
         <tr style="border-collapse:collapse;">
          <td align="center" style="padding:0;Margin:0;">
           <table class="es-content-body" width="650" cellspacing="0" cellpadding="0" bgcolor="#333333" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="610" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="table-layout: fixed; mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="Margin:0;padding-top:10px;padding-bottom:10px;padding-left:10px;padding-right:10px;"> <h1 style="Margin:0;line-height:43px;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-weight:200;font-size:36px;font-style:normal;color:#333333;">Şifrenizi Yenileme İsteğiniz</h1> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Aşağıdaki butona tıklayarak şifrenizi sıfırlayabilirsiniz:</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="padding:10px;Margin:0;"> <span class="es-button-border" style="border-style:solid;border-color:#808080;background:#0D6FAA;border-width:0px;display:inline-block;border-radius:30px;width:auto;"> <a href="{:PasswordResetUrl}" class="es-button" target="_blank" style="mso-style-priority:100 !important;text-decoration:none;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-size:14px;color:#FFFFFF;border-style:solid;border-color:#0D6FAA;border-width:20px 40px;display:inline-block;background:#0D6FAA;border-radius:30px;font-weight:normal;font-style:normal;line-height:17px;width:auto;text-align:center;">Şifre Sıfırla</a> </span> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Saygılar,&nbsp;</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">PrimeApps Takımı</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="word-wrap: break-word; padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:18px;color:#333333;">Yukarıdaki buton çalışmazsa lütfen aşağıda yer alan linki kopyalayıp tarayıcınıza yapıştırın :</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:18px;color:#333333;">{:PasswordResetUrl}</p> </td>
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-footer" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0;background-color:transparent;">
           <table class="es-footer-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="650" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:10px;padding-right:10px;padding-top:20px;padding-bottom:40px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="630" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-txt-c" align="center" style="padding:10px;Margin:0;">
                       <table class="es-table-not-adapt es-social" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                         <tr style="border-collapse:collapse;">
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.linkedin.com/company/primeapps/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/linkedin.png" alt="Li" title="Linkedin" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://twitter.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/twitter.png" alt="Tw" title="Twitter" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.facebook.com/PrimeApps.io/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/facebook.png" alt="Fb" title="Facebook" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;"> <a target="_blank" href="https://www.instagram.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/instagram.png" alt="in" title="instagram" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                         </tr>
                       </table> </td>
                     </tr>
                    
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table> </td>
     </tr>
   </table>
  </div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;"></div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;padding:0px;border:0px none;width:1px;"></div>
 </body>
</html>
', 'tr', 1, 'password_reset', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (14, 1, NULL, '2019-03-12 14:11:10', NULL, 'f', 2, 'Add User - App', 'User Info - App', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
 <head>
  <meta charset="UTF-8">
  <meta content="width=device-width, initial-scale=1" name="viewport">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta content="telephone=no" name="format-detection">
  <title>New email 2</title>
  <!--[if (mso 16)]>
    <style type="text/css">
    a {text-decoration: none;}
    </style>
    <![endif]-->
  <!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]-->
  <!--[if !mso]><!-- -->
  <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,400,400i,700,700i&amp;subset=latin-ext" rel="stylesheet">
  <!--<![endif]-->
  <style type="text/css">
@media only screen and (max-width:600px) {p, ul li, ol li, a { font-size:14px!important; line-height:150%!important } h1 { font-size:26px!important; text-align:center } h2 { font-size:24px!important; text-align:center } h3 { font-size:20px!important; text-align:center } h1 a { font-size:26px!important } h2 a { font-size:24px!important } h3 a { font-size:20px!important } .es-menu td a { font-size:13px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:16px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:12px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class="gmail-fix"] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:block!important } a.es-button { font-size:14px!important; display:block!important; border-left-width:0px!important; border-right-width:0px!important } .es-btn-fw { border-width:10px 0px!important; text-align:center!important } .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } .es-desk-menu-hidden { display:table-cell!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } }
#outlook a {
    padding:0;
}
.ExternalClass {
    width:100%;
}
.ExternalClass,
.ExternalClass p,
.ExternalClass span,
.ExternalClass font,
.ExternalClass td,
.ExternalClass div {
    line-height:100%;
}
.es-button {
    mso-style-priority:100!important;
    text-decoration:none!important;
}
a[x-apple-data-detectors] {
    color:inherit!important;
    text-decoration:none!important;
    font-size:inherit!important;
    font-family:inherit!important;
    font-weight:inherit!important;
    line-height:inherit!important;
}
.es-desk-hidden {
    display:none;
    float:left;
    overflow:hidden;
    width:0;
    max-height:0;
    line-height:0;
    mso-hide:all;
}
</style>
 </head>
 <body style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
  <div class="es-wrapper-color" style="background-color:#333333;">
   <!--[if gte mso 9]>
			<v:background xmlns:v="urn:schemas-microsoft-com:vml" fill="t">
				<v:fill type="tile" src="https://www.primeapps.io/mail/images/bg.jpg" color="#333333" origin="0.5, 0" position="0.5,0"></v:fill>
			</v:background>
		<![endif]-->
   <table class="es-wrapper" width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-image:url(https://www.primeapps.io/mail/images/bg.jpg);background-repeat:repeat;background-position:center top;" background="https://www.primeapps.io/mail/images/bg.jpg">
     <tr style="border-collapse:collapse;">
      <td valign="top" style="padding:0;Margin:0;">
       <table class="es-header" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td class="es-adaptive" align="center" style="padding:0;Margin:0;">
           <table class="es-header-body" width="650" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;">
             <tr style="border-collapse:collapse;">
              <td style="padding:0;Margin:0;padding-bottom:30px;padding-top:40px;background-color:transparent;" bgcolor="transparent" align="left">
               <table cellspacing="0" cellpadding="0" width="100%" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td class="es-m-p0r" width="650" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-p0l es-m-txt-c" align="center" style="padding:0;Margin:0;"> <a href="https://www.ofisim.com/ik/" target="_blank" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#333333;"><img src="https://www.primeapps.io/mail/images/logo_white.png" alt="Ofisim İK" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" title="Ofisim İK" height="50" width="250"></a> </td> 
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
         <tr style="border-collapse:collapse;">
          <td align="center" style="padding:0;Margin:0;">
           <table class="es-content-body" width="650" cellspacing="0" cellpadding="0" bgcolor="#333333" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="610" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="table-layout: fixed; mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="Margin:0;padding-top:10px;padding-bottom:10px;padding-left:10px;padding-right:10px;"> <h1 style="Margin:0;line-height:43px;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-weight:200;font-size:36px;font-style:normal;color:#333333;">User Info</h1> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Hello,<br /> {:DisplayName}''s login information.</p> </td>
                     </tr>
                       <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">E-mail : {:Email} <br />Password : {:Password}</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Regards,&nbsp;</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">PrimeApps Team</p> </td>
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-footer" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0;background-color:transparent;">
           <table class="es-footer-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="650" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:10px;padding-right:10px;padding-top:20px;padding-bottom:40px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="630" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-txt-c" align="center" style="padding:10px;Margin:0;">
                       <table class="es-table-not-adapt es-social" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                         <tr style="border-collapse:collapse;">
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.linkedin.com/company/primeapps/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/linkedin.png" alt="Li" title="Linkedin" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://twitter.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/twitter.png" alt="Tw" title="Twitter" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.facebook.com/PrimeApps.io/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/facebook.png" alt="Fb" title="Facebook" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;"> <a target="_blank" href="https://www.instagram.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/instagram.png" alt="in" title="instagram" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                         </tr>
                       </table> </td>
                     </tr>
                    
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table> </td>
     </tr>
   </table>
  </div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;"></div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;padding:0px;border:0px none;width:1px;"></div>
 </body>
</html>
', 'en', 1, 'app_draft_user', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (15, 1, NULL, '2019-03-12 14:39:25', NULL, 'f', 2, 'Kullanıcı Ekleme - Org', 'Kullanıcı Bilgisi - Org', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
 <head>
  <meta charset="UTF-8">
  <meta content="width=device-width, initial-scale=1" name="viewport">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta content="telephone=no" name="format-detection">
  <title>New email 2</title>
  <!--[if (mso 16)]>
    <style type="text/css">
    a {text-decoration: none;}
    </style>
    <![endif]-->
  <!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]-->
  <!--[if !mso]><!-- -->
  <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,400,400i,700,700i&amp;subset=latin-ext" rel="stylesheet">
  <!--<![endif]-->
  <style type="text/css">
@media only screen and (max-width:600px) {p, ul li, ol li, a { font-size:14px!important; line-height:150%!important } h1 { font-size:26px!important; text-align:center } h2 { font-size:24px!important; text-align:center } h3 { font-size:20px!important; text-align:center } h1 a { font-size:26px!important } h2 a { font-size:24px!important } h3 a { font-size:20px!important } .es-menu td a { font-size:13px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:16px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:12px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class="gmail-fix"] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:block!important } a.es-button { font-size:14px!important; display:block!important; border-left-width:0px!important; border-right-width:0px!important } .es-btn-fw { border-width:10px 0px!important; text-align:center!important } .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } .es-desk-menu-hidden { display:table-cell!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } }
#outlook a {
    padding:0;
}
.ExternalClass {
    width:100%;
}
.ExternalClass,
.ExternalClass p,
.ExternalClass span,
.ExternalClass font,
.ExternalClass td,
.ExternalClass div {
    line-height:100%;
}
.es-button {
    mso-style-priority:100!important;
    text-decoration:none!important;
}
a[x-apple-data-detectors] {
    color:inherit!important;
    text-decoration:none!important;
    font-size:inherit!important;
    font-family:inherit!important;
    font-weight:inherit!important;
    line-height:inherit!important;
}
.es-desk-hidden {
    display:none;
    float:left;
    overflow:hidden;
    width:0;
    max-height:0;
    line-height:0;
    mso-hide:all;
}
</style>
 </head>
 <body style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
  <div class="es-wrapper-color" style="background-color:#333333;">
   <!--[if gte mso 9]>
			<v:background xmlns:v="urn:schemas-microsoft-com:vml" fill="t">
				<v:fill type="tile" src="https://www.primeapps.io/mail/images/bg.jpg" color="#333333" origin="0.5, 0" position="0.5,0"></v:fill>
			</v:background>
		<![endif]-->
   <table class="es-wrapper" width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-image:url(https://www.primeapps.io/mail/images/bg.jpg);background-repeat:repeat;background-position:center top;" background="https://www.primeapps.io/mail/images/bg.jpg">
     <tr style="border-collapse:collapse;">
      <td valign="top" style="padding:0;Margin:0;">
       <table class="es-header" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td class="es-adaptive" align="center" style="padding:0;Margin:0;">
           <table class="es-header-body" width="650" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;">
             <tr style="border-collapse:collapse;">
              <td style="padding:0;Margin:0;padding-bottom:30px;padding-top:40px;background-color:transparent;" bgcolor="transparent" align="left">
               <table cellspacing="0" cellpadding="0" width="100%" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td class="es-m-p0r" width="650" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-p0l es-m-txt-c" align="center" style="padding:0;Margin:0;"> <a href="https://www.ofisim.com/ik/" target="_blank" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#333333;"><img src="https://www.primeapps.io/mail/images/logo_white.png" alt="Ofisim İK" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" title="Ofisim İK" height="50" width="250"></a> </td> 
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
         <tr style="border-collapse:collapse;">
          <td align="center" style="padding:0;Margin:0;">
           <table class="es-content-body" width="650" cellspacing="0" cellpadding="0" bgcolor="#333333" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="610" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="table-layout: fixed; mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="Margin:0;padding-top:10px;padding-bottom:10px;padding-left:10px;padding-right:10px;"> <h1 style="Margin:0;line-height:43px;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-weight:200;font-size:36px;font-style:normal;color:#333333;">İş Ortağı Bilgisi</h1> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Merhaba,<br /> {:FirstName} adlı iş ortağının giriş bilgileri aşağıdaki gibidir.</p> </td>
                     </tr>
                       <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">E-mail : {:Email} <br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Şifre &nbsp;&nbsp;&nbsp;: {:Password}</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Saygılar,&nbsp;</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">PrimeApps Takımı</p> </td>
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-footer" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0;background-color:transparent;">
           <table class="es-footer-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="650" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:10px;padding-right:10px;padding-top:20px;padding-bottom:40px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="630" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-txt-c" align="center" style="padding:10px;Margin:0;">
                       <table class="es-table-not-adapt es-social" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                         <tr style="border-collapse:collapse;">
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.linkedin.com/company/primeapps/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/linkedin.png" alt="Li" title="Linkedin" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://twitter.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/twitter.png" alt="Tw" title="Twitter" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.facebook.com/PrimeApps.io/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/facebook.png" alt="Fb" title="Facebook" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;"> <a target="_blank" href="https://www.instagram.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/instagram.png" alt="in" title="instagram" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                         </tr>
                       </table> </td>
                     </tr>
                   
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table> </td>
     </tr>
   </table>
  </div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;"></div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;padding:0px;border:0px none;width:1px;"></div>
 </body>
</html>
', 'tr', 1, 'add_collaborator', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (16, 1, NULL, '2019-03-12 14:44:03', NULL, 'f', 2, 'Add Collaborator - Org', 'Collaborator Info - Org', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
 <head>
  <meta charset="UTF-8">
  <meta content="width=device-width, initial-scale=1" name="viewport">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta content="telephone=no" name="format-detection">
  <title>New email 2</title>
  <!--[if (mso 16)]>
    <style type="text/css">
    a {text-decoration: none;}
    </style>
    <![endif]-->
  <!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]-->
  <!--[if !mso]><!-- -->
  <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,400,400i,700,700i&amp;subset=latin-ext" rel="stylesheet">
  <!--<![endif]-->
  <style type="text/css">
@media only screen and (max-width:600px) {p, ul li, ol li, a { font-size:14px!important; line-height:150%!important } h1 { font-size:26px!important; text-align:center } h2 { font-size:24px!important; text-align:center } h3 { font-size:20px!important; text-align:center } h1 a { font-size:26px!important } h2 a { font-size:24px!important } h3 a { font-size:20px!important } .es-menu td a { font-size:13px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:16px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:12px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class="gmail-fix"] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:block!important } a.es-button { font-size:14px!important; display:block!important; border-left-width:0px!important; border-right-width:0px!important } .es-btn-fw { border-width:10px 0px!important; text-align:center!important } .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } .es-desk-menu-hidden { display:table-cell!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } }
#outlook a {
    padding:0;
}
.ExternalClass {
    width:100%;
}
.ExternalClass,
.ExternalClass p,
.ExternalClass span,
.ExternalClass font,
.ExternalClass td,
.ExternalClass div {
    line-height:100%;
}
.es-button {
    mso-style-priority:100!important;
    text-decoration:none!important;
}
a[x-apple-data-detectors] {
    color:inherit!important;
    text-decoration:none!important;
    font-size:inherit!important;
    font-family:inherit!important;
    font-weight:inherit!important;
    line-height:inherit!important;
}
.es-desk-hidden {
    display:none;
    float:left;
    overflow:hidden;
    width:0;
    max-height:0;
    line-height:0;
    mso-hide:all;
}
</style>
 </head>
 <body style="width:100%;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0;">
  <div class="es-wrapper-color" style="background-color:#333333;">
   <!--[if gte mso 9]>
			<v:background xmlns:v="urn:schemas-microsoft-com:vml" fill="t">
				<v:fill type="tile" src="https://www.primeapps.io/mail/images/bg.jpg" color="#333333" origin="0.5, 0" position="0.5,0"></v:fill>
			</v:background>
		<![endif]-->
   <table class="es-wrapper" width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-image:url(https://www.primeapps.io/mail/images/bg.jpg);background-repeat:repeat;background-position:center top;" background="https://www.primeapps.io/mail/images/bg.jpg">
     <tr style="border-collapse:collapse;">
      <td valign="top" style="padding:0;Margin:0;">
       <table class="es-header" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td class="es-adaptive" align="center" style="padding:0;Margin:0;">
           <table class="es-header-body" width="650" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;">
             <tr style="border-collapse:collapse;">
              <td style="padding:0;Margin:0;padding-bottom:30px;padding-top:40px;background-color:transparent;" bgcolor="transparent" align="left">
               <table cellspacing="0" cellpadding="0" width="100%" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td class="es-m-p0r" width="650" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-p0l es-m-txt-c" align="center" style="padding:0;Margin:0;"> <a href="https://www.ofisim.com/ik/" target="_blank" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#333333;"><img src="https://www.primeapps.io/mail/images/logo_white.png" alt="Ofisim İK" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" title="Ofisim İK" height="50" width="250"></a> </td> 
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
         <tr style="border-collapse:collapse;">
          <td align="center" style="padding:0;Margin:0;">
           <table class="es-content-body" width="650" cellspacing="0" cellpadding="0" bgcolor="#333333" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="610" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="table-layout: fixed; mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td align="center" style="Margin:0;padding-top:10px;padding-bottom:10px;padding-left:10px;padding-right:10px;"> <h1 style="Margin:0;line-height:43px;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, sans-serif;font-weight:200;font-size:36px;font-style:normal;color:#333333;">Collaborator Info</h1> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Hello,<br /> {:FirstName}''s login information.</p> </td>
                     </tr>
                       <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">E-mail : {:Email} <br />Password : {:Password}</p> </td>
                     </tr>
                     <tr style="border-collapse:collapse;">
                      <td align="center" class="es-m-txt-c" style="padding:0;Margin:0;padding-top:10px;padding-bottom:10px;"> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">Regards,&nbsp;</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:16px;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;line-height:24px;color:#333333;">PrimeApps Team</p> </td>
                     </tr>
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table>
       <table class="es-footer" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top;">
         <tr style="border-collapse:collapse;">
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0;background-color:transparent;">
           <table class="es-footer-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="650" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
             <tr style="border-collapse:collapse;">
              <td align="left" style="Margin:0;padding-left:10px;padding-right:10px;padding-top:20px;padding-bottom:40px;">
               <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                 <tr style="border-collapse:collapse;">
                  <td width="630" valign="top" align="center" style="padding:0;Margin:0;">
                   <table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                     <tr style="border-collapse:collapse;">
                      <td class="es-m-txt-c" align="center" style="padding:10px;Margin:0;">
                       <table class="es-table-not-adapt es-social" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
                         <tr style="border-collapse:collapse;">
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.linkedin.com/company/primeapps/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/linkedin.png" alt="Li" title="Linkedin" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://twitter.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/twitter.png" alt="Tw" title="Twitter" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"> <a target="_blank" href="https://www.facebook.com/PrimeApps.io/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/facebook.png" alt="Fb" title="Facebook" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                          <td align="center" valign="top" style="padding:0;Margin:0;"> <a target="_blank" href="https://www.instagram.com/PrimeAppsStudio/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Nunito, ''helvetica neue'', helvetica, arial, sans-serif;font-size:14px;text-decoration:underline;color:#CCCCCC;"> <img src="https://www.primeapps.io/mail/images/social/instagram.png" alt="in" title="instagram" width="32" height="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;"> </a> </td>
                         </tr>
                       </table> </td>
                     </tr>
                    
                   </table> </td>
                 </tr>
               </table> </td>
             </tr>
           </table> </td>
         </tr>
       </table> </td>
     </tr>
   </table>
  </div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;"></div>
  <div style="position:absolute;left:-9999px;top:-9999px;margin:0px;padding:0px;border:0px none;width:1px;"></div>
 </body>
</html>
', 'en', 1, 'add_collaborator', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (17, 1, NULL, '2018-10-01 11:41:02.829818', NULL, 'f', 3, 'E-Posta Onaylama', 'E-Posta Onaylama', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml"
      xmlns:v="urn:schemas-microsoft-com:vml"
      xmlns:o="urn:schemas-microsoft-com:office:office">
<head>
    <title>Ofisim CRM''e Hoşgeldiniz!</title>
    <meta http-equiv="Content-Type"
          content="text/html; charset=utf-8" />
    <style type="text/css">
        body, .maintable {
            height: 100% !important;
            width: 100% !important;
            margin: 0;
            padding: 0;
        }

        img, a img {
            border: 0;
            outline: none;
            text-decoration: none;
        }

        .imagefix {
            display: block;
        }

        p {
            margin-top: 0;
            margin-right: 0;
            margin-left: 0;
            padding: 0;
        }

        .ReadMsgBody {
            width: 100%;
        }

        .ExternalClass {
            width: 100%;
        }

            .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {
                line-height: 100%;
            }

        img {
            -ms-interpolation-mode: bicubic;
        }

        body, table, td, p, a, li, blockquote {
            -ms-text-size-adjust: 100%;
            -webkit-text-size-adjust: 100%;
        }
    </style>
    <style type="text/css">
        @media only screen and (max-width: 600px) {
            .rtable {
                width: 100% !important;
                table-layout: fixed;
            }

                .rtable tr {
                    height: auto !important;
                    display: block;
                }

            .contenttd {
                max-width: 100% !important;
                display: block;
            }

                .contenttd:after {
                    content: "";
                    display: table;
                    clear: both;
                }

            .hiddentds {
                display: none;
            }

            .imgtable, .imgtable table {
                max-width: 100% !important;
                height: auto;
                float: none;
                margin: 0 auto;
            }

                .imgtable.btnset td {
                    display: inline-block;
                }

                .imgtable img {
                    width: 100%;
                    height: auto;
                    display: block;
                }

            table {
                float: none;
                table-layout: fixed;
            }
        }
    </style>
    <!--[if gte mso 9]> 
    <xml>
      <o:OfficeDocumentSettings>
        <o:AllowPNG/>
        <o:PixelsPerInch>96</o:PixelsPerInch>
      </o:OfficeDocumentSettings>
    </xml>
    <![endif]-->
</head>
<body style="overflow: auto; padding:0; margin:0; font-size: 12px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#f4f4f4">
    <table cellspacing="0" cellpadding="0" width="100%"
           bgcolor="#f4f4f4">
        <tr>
            <td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td>
        </tr>
        <tr>
            <td valign="top">
                <table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto"
                       cellspacing="0" cellpadding="0" width="600" align="center"
                       border="0">
                    <tr>
                        <td class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                                   align="left">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 10px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN:center !important; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent; text-aling:center;">
                                        <table class="imgtable" cellspacing="0" cellpadding="0"
                                               align=" center" border="0">
                                            <tr>
                                                <td style="PADDING-BOTTOM: 2px; PADDING-TOP: 2px; PADDING-LEFT: 2px; PADDING-RIGHT: 2px; text-aling:center;" align="center">
                                                    <table cellspacing="0" cellpadding="0" border="0">
                                                        <tr>
                                                            <td style="BORDER-TOP: medium none; BORDER-RIGHT: medium none;  BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BACKGROUND-COLOR: transparent; text-align:center;">
                                                                <a href="http://www.ofisim.com/crm/" target="_blank"><img style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block" alt="" src="https://ofisimcom.azureedge.net/web/crmLogo.png?v=1" width="200" hspace="0" vspace="0" border="0" /></a>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </th>
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: center; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class="contenttd" style="BORDER-TOP: #fff 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="center">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 10px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                                        <!--[if gte mso 12]>
                                            <table cellspacing="0" cellpadding="0" border="0" width="100%"><tr><td align="center">
                                        <![endif]-->
                                        <table class="imgtable" style="MARGIN: 0px auto" cellspacing="0"
                                               cellpadding="0" align="center" border="0">
                                            <tr>
                                                <td style="PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px"
                                                    align="center">
                                                </td>
                                            </tr>
                                        </table>
                                        <!--[if gte mso 12]>
                                            </td></tr></table>
                                        <![endif]-->
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <tr>

                        

                        <td class="contenttd" style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                                   align="left">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 20px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 20px; BORDER-LEFT: medium none; PADDING-RIGHT: 20px; BACKGROUND-COLOR: #fff;">
                                        <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            Merhaba {:FirstName} {:LastName},<br />

                                        </p>
                                        <p style="FONT-SIZE: 16px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 24px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"
                                           align="center">Aşşağıda ki linke tıklayarak e-posta adresinizi etkinleştirebilirsiniz.</p>

                                        <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            <a style="color:#1c62e; text-decoration:underline; font-size:20px;" href="{:Url}">E-Posta mı etkinleştir</a><br />
                                        </p>

                                        <p style="FONT-SIZE: 12px;MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 14px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            Ofisim.com
                                        </p>
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    <tr style="HEIGHT: 20px;">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
                            <div style="PADDING-BOTTOM: 10px; TEXT-ALIGN: center; PADDING-TOP: 10px; PADDING-LEFT: 10px; PADDING-RIGHT: 10px">
                                <table class="imgtable" style="DISPLAY: inline-block"
                                       cellspacing="0" cellpadding="0" border="0">
                                    <tr>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://www.facebook.com/ofisimcrm" target="_blank">
                                                <img title="Facebook"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Facebook" src="http://www.ofisim.com/mail/KobiMail_files/fb.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://twitter.com/ofisim_com" target="_blank">
                                                <img title="Twitter"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Twitter" src="http://www.ofisim.com/mail/KobiMail_files/tw.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://www.linkedin.com/company/ofisim.com"
                                               target="_blank">
                                                <img title="Linkedin"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Linkedin" src="http://www.ofisim.com/mail/KobiMail_files/in.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <p style="FONT-SIZE: 14px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 21px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"
                               align="left">&nbsp;</p>
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="contenttd"
                style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 10px; PADDING-TOP: 10px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                       align="left">
                    <tr class="hiddentds">
                        <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                    </tr>
                    <tr style="HEIGHT: 10px">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="contenttd"
                style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 1px; PADDING-TOP: 1px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                       align="left">
                    <tr class="hiddentds">
                        <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                    </tr>
                    <tr style="HEIGHT: 10px">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 1px; TEXT-ALIGN: left; PADDING-TOP: 1px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td>
        </tr>
    </table>
 
    <!-- Created with MailStyler 2.0.0.330 -->
</body>
</html>
', 'tr', 1, 'email_confirm', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (18, 1, NULL, '2018-10-03 14:55:24', NULL, 'f', 3, 'Şifre Sıfırlama Talebi', 'Şifre Sıfırlama Talebi', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml"
	  xmlns:v="urn:schemas-microsoft-com:vml"
	  xmlns:o="urn:schemas-microsoft-com:office:office">
<head>
	<title>Ofisim CRM''e Hoşgeldiniz!</title>
	<meta http-equiv="Content-Type"
		  content="text/html; charset=utf-8" />
	<style type="text/css">
		body, .maintable {
			height: 100% !important;
			width: 100% !important;
			margin: 0;
			padding: 0;
		}

		img, a img {
			border: 0;
			outline: none;
			text-decoration: none;
		}

		.imagefix {
			display: block;
		}

		p {
			margin-top: 0;
			margin-right: 0;
			margin-left: 0;
			padding: 0;
		}

		.ReadMsgBody {
			width: 100%;
		}

		.ExternalClass {
			width: 100%;
		}

			.ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {
				line-height: 100%;
			}

		img {
			-ms-interpolation-mode: bicubic;
		}

		body, table, td, p, a, li, blockquote {
			-ms-text-size-adjust: 100%;
			-webkit-text-size-adjust: 100%;
		}
	</style>
	<style type="text/css">
		@media only screen and (max-width: 600px) {
			.rtable {
				width: 100% !important;
				table-layout: fixed;
			}

				.rtable tr {
					height: auto !important;
					display: block;
				}

			.contenttd {
				max-width: 100% !important;
				display: block;
			}

				.contenttd:after {
					content: "";
					display: table;
					clear: both;
				}

			.hiddentds {
				display: none;
			}

			.imgtable, .imgtable table {
				max-width: 100% !important;
				height: auto;
				float: none;
				margin: 0 auto;
			}

				.imgtable.btnset td {
					display: inline-block;
				}

				.imgtable img {
					width: 100%;
					height: auto;
					display: block;
				}

			table {
				float: none;
				table-layout: fixed;
			}
		}
	</style>
	<!--[if gte mso 9]>
	<xml>
	  <o:OfficeDocumentSettings>
		<o:AllowPNG/>
		<o:PixelsPerInch>96</o:PixelsPerInch>
	  </o:OfficeDocumentSettings>
	</xml>
	<![endif]-->
</head>
<body style="overflow: auto; padding:0; margin:0; font-size: 12px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#f4f4f4">
	<table cellspacing="0" cellpadding="0" width="100%"
		   bgcolor="#f4f4f4">
		<tr>
			<td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td>
		</tr>
		<tr>
			<td valign="top">
				<table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto"
					   cellspacing="0" cellpadding="0" width="600" align="center"
					   border="0">
					<tr>
						<td class="contenttd"
							style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
								   align="left">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 10px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN:center !important; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent; text-aling:center;">
										<table class="imgtable" cellspacing="0" cellpadding="0"
											   align=" center" border="0">
											<tr>
												<td style="PADDING-BOTTOM: 2px; PADDING-TOP: 2px; PADDING-LEFT: 2px; PADDING-RIGHT: 2px; text-aling:center;" align="center">
													<table cellspacing="0" cellpadding="0" border="0">
														<tr>
															<td style="BORDER-TOP: medium none; BORDER-RIGHT: medium none;  BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BACKGROUND-COLOR: transparent; text-align:center;">
																<a href="{{APP_URL}}" target="_blank"><img style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block" alt="" src="{{URL}}" width="200" hspace="0" vspace="0" border="0" /></a>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</table>
									</th>
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: center; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
									</th>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="contenttd" style="BORDER-TOP: #fff 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="center">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 10px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
										<!--[if gte mso 12]>
							<table cellspacing="0" cellpadding="0" border="0" width="100%"><tr><td align="center">
						<![endif]-->
										<table class="imgtable" style="MARGIN: 0px auto" cellspacing="0"
											   cellpadding="0" align="center" border="0">
											<tr>
												<td style="PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px"
													align="center"></td>
											</tr>
										</table>
										<!--[if gte mso 12]>
							</td></tr></table>
						<![endif]-->
									</th>
								</tr>
							</table>
						</td>
					</tr>

					<tr>



						<td class="contenttd" style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
								   align="left">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 20px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 20px; BORDER-LEFT: medium none; PADDING-RIGHT: 20px; BACKGROUND-COLOR: #fff;">
										<p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
										   align="center">
											{Greetings}<br />

										</p>
										<p style="FONT-SIZE: 16px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 24px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"
										   align="center">{ResetRequest}</p>

										<p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
										   align="center">
											<a style="color:#2560f6; text-decoration:underline; font-size:20px;" href=''{:EmailResetUrl}''>{:EmailResetUrl}</a><br />
										</p>
									</th>
								</tr>
							</table>
						</td>
					</tr>
					{{F}}<tr style="HEIGHT: 20px;display:{{SOCIAL_ICONS}};">
						<table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
							<tr style="border-collapse:collapse;">
								<td align="center" style="padding:0;Margin:0;">
									<table class="es-content-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="600" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
										<tr style="border-collapse:collapse;">
											<td align="left" style="Margin:0;padding-top:10px;padding-left:10px;padding-right:10px;padding-bottom:30px;">
												<table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;display:{{SOCIAL_ICONS}};">
													<tr style="border-collapse:collapse;">
														<td width="580" valign="top" align="center" style="padding:0;Margin:0;">
															<table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
																<tr style="border-collapse:collapse;">
																	<td align="center" class="es-infoblock" style="padding:10px;Margin:0;line-height:120%;font-size:12px;color:#CCCCCC;"><p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:arial, ''helvetica neue'', helvetica, sans-serif;line-height:150%;color:#333333;">{{FOOTER}}</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:arial, ''helvetica neue'', helvetica, sans-serif;line-height:150%;color:#333333;">+90 212 963 0060 &amp; +90 850 532 2800 (Hafta içi: 09.00 - 18.00)</p></td>
																</tr>
																<tr style="border-collapse:collapse;">
																	<td align="center" class="es-m-txt-c" style="padding:0;Margin:0;">
																		<table cellpadding="0" cellspacing="0" class="es-table-not-adapt es-social" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
																			<tr style="border-collapse:collapse;">
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.linkedin.com/company/ofisim.com/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/linkedin.png" alt="Li" title="Linkedin" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://twitter.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/twitter.png" alt="Tw" title="Twitter" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.facebook.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/facebook.png" alt="Fb" title="Facebook" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.instagram.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/instagram.png" alt="in" title="instagram" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;"><a target="_blank" href="https://www.youtube.com/channel/UClGV9dPtU1XC2VTn2Kb414A" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/youtube.png" alt="Yt" title="Youtube" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																			</tr>
																		</table>
																	</td>
																</tr>
															</table>
														</td>
													</tr>
												</table>
											</td>
										</tr>
									</table>
								</td>
							</tr>
						</table>
					</tr>{{/F}}
				</table>
			</td>
		</tr>
		<tr>
			<td class="contenttd"
				style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 10px; PADDING-TOP: 10px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
				<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
					   align="left">
					<tr class="hiddentds">
						<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
					</tr>
					<tr style="HEIGHT: 10px">
						<th class="contenttd"
							style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
						</th>
					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<td class="contenttd"
				style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 1px; PADDING-TOP: 1px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
				<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
					   align="left">
					<tr class="hiddentds">
						<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
					</tr>
					<tr style="HEIGHT: 10px">
						<th class="contenttd"
							style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 1px; TEXT-ALIGN: left; PADDING-TOP: 1px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
						</th>
					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td>
		</tr>
	</table>

	<!-- Created with MailStyler 2.0.0.330 -->
</body>
</html>










', 'tr', 1, 'password_reset', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (19, 1, NULL, '2018-10-03 14:55:24', NULL, 'f', 3, 'Password Reset Request', 'Password Reset Request', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml"
	  xmlns:v="urn:schemas-microsoft-com:vml"
	  xmlns:o="urn:schemas-microsoft-com:office:office">
<head>
	<title>Ofisim CRM''e Hoşgeldiniz!</title>
	<meta http-equiv="Content-Type"
		  content="text/html; charset=utf-8" />
	<style type="text/css">
		body, .maintable {
			height: 100% !important;
			width: 100% !important;
			margin: 0;
			padding: 0;
		}

		img, a img {
			border: 0;
			outline: none;
			text-decoration: none;
		}

		.imagefix {
			display: block;
		}

		p {
			margin-top: 0;
			margin-right: 0;
			margin-left: 0;
			padding: 0;
		}

		.ReadMsgBody {
			width: 100%;
		}

		.ExternalClass {
			width: 100%;
		}

			.ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {
				line-height: 100%;
			}

		img {
			-ms-interpolation-mode: bicubic;
		}

		body, table, td, p, a, li, blockquote {
			-ms-text-size-adjust: 100%;
			-webkit-text-size-adjust: 100%;
		}
	</style>
	<style type="text/css">
		@media only screen and (max-width: 600px) {
			.rtable {
				width: 100% !important;
				table-layout: fixed;
			}

				.rtable tr {
					height: auto !important;
					display: block;
				}

			.contenttd {
				max-width: 100% !important;
				display: block;
			}

				.contenttd:after {
					content: "";
					display: table;
					clear: both;
				}

			.hiddentds {
				display: none;
			}

			.imgtable, .imgtable table {
				max-width: 100% !important;
				height: auto;
				float: none;
				margin: 0 auto;
			}

				.imgtable.btnset td {
					display: inline-block;
				}

				.imgtable img {
					width: 100%;
					height: auto;
					display: block;
				}

			table {
				float: none;
				table-layout: fixed;
			}
		}
	</style>
	<!--[if gte mso 9]>
	<xml>
	  <o:OfficeDocumentSettings>
		<o:AllowPNG/>
		<o:PixelsPerInch>96</o:PixelsPerInch>
	  </o:OfficeDocumentSettings>
	</xml>
	<![endif]-->
</head>
<body style="overflow: auto; padding:0; margin:0; font-size: 12px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#f4f4f4">
	<table cellspacing="0" cellpadding="0" width="100%"
		   bgcolor="#f4f4f4">
		<tr>
			<td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td>
		</tr>
		<tr>
			<td valign="top">
				<table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto"
					   cellspacing="0" cellpadding="0" width="600" align="center"
					   border="0">
					<tr>
						<td class="contenttd"
							style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
								   align="left">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 10px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN:center !important; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent; text-aling:center;">
										<table class="imgtable" cellspacing="0" cellpadding="0"
											   align=" center" border="0">
											<tr>
												<td style="PADDING-BOTTOM: 2px; PADDING-TOP: 2px; PADDING-LEFT: 2px; PADDING-RIGHT: 2px; text-aling:center;" align="center">
													<table cellspacing="0" cellpadding="0" border="0">
														<tr>
															<td style="BORDER-TOP: medium none; BORDER-RIGHT: medium none;  BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BACKGROUND-COLOR: transparent; text-align:center;">														
																<a href="{{APP_URL}}" target="_blank"><img style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block" alt="" src="{{URL}}" width="200" hspace="0" vspace="0" border="0" /></a>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</table>
									</th>
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: center; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
									</th>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="contenttd" style="BORDER-TOP: #fff 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="center">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 10px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
										<!--[if gte mso 12]>
							<table cellspacing="0" cellpadding="0" border="0" width="100%"><tr><td align="center">
						<![endif]-->
										<table class="imgtable" style="MARGIN: 0px auto" cellspacing="0"
											   cellpadding="0" align="center" border="0">
											<tr>
												<td style="PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px"
													align="center"></td>
											</tr>
										</table>
										<!--[if gte mso 12]>
							</td></tr></table>
						<![endif]-->
									</th>
								</tr>
							</table>
						</td>
					</tr>

					<tr>



						<td class="contenttd" style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
							<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
								   align="left">
								<tr class="hiddentds">
									<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
								</tr>
								<tr style="HEIGHT: 20px">
									<th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 20px; BORDER-LEFT: medium none; PADDING-RIGHT: 20px; BACKGROUND-COLOR: #fff;">
										<!-- <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly" -->
										   <!-- align="center"> -->
											<!-- {Greetings}<br /> -->

										<!-- </p> -->
										 <p style="FONT-SIZE: 16px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 24px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly" 
										   align="center"> Merhaba {:FullName},<br /></p> 

										<p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
										   align="center">
										    <a style="color:#2560f6; text-decoration:underline; font-size:20px;" href=''{:PasswordResetUrl}''>Şifremi Sıfırla</a><br />
											<!-- <a style="color:#2560f6; text-decoration:underline; font-size:20px;" href=''{:EmailResetUrl}''>{:EmailResetUrl}</a><br /> -->
										</p>
									</th>
								</tr>
							</table>
						</td>
					</tr>
					{{F}}<tr style="HEIGHT: 20px;display:{{SOCIAL_ICONS}};">
						<table class="es-content" cellspacing="0" cellpadding="0" align="center" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;">
							<tr style="border-collapse:collapse;">
								<td align="center" style="padding:0;Margin:0;">
									<table class="es-content-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;" width="600" cellspacing="0" cellpadding="0" align="center" bgcolor="rgba(0, 0, 0, 0)">
										<tr style="border-collapse:collapse;">
											<td align="left" style="Margin:0;padding-top:10px;padding-left:10px;padding-right:10px;padding-bottom:30px;">
												<table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;display:{{SOCIAL_ICONS}};">
													<tr style="border-collapse:collapse;">
														<td width="580" valign="top" align="center" style="padding:0;Margin:0;">
															<table width="100%" cellspacing="0" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
																<tr style="border-collapse:collapse;">
																	<!-- <td align="center" class="es-infoblock" style="padding:10px;Margin:0;line-height:120%;font-size:12px;color:#CCCCCC;"><p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:arial, ''helvetica neue'', helvetica, sans-serif;line-height:150%;color:#333333;">{{FOOTER}}</p> <p style="Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-size:12px;font-family:arial, ''helvetica neue'', helvetica, sans-serif;line-height:150%;color:#333333;">+90 212 963 0060 &amp; +90 850 532 2800 (Hafta içi: 09.00 - 18.00)</p></td> -->
																</tr>
																<tr style="border-collapse:collapse;">
																	<td align="center" class="es-m-txt-c" style="padding:0;Margin:0;">
																		<table cellpadding="0" cellspacing="0" class="es-table-not-adapt es-social" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;">
																			<tr style="border-collapse:collapse;">
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.linkedin.com/company/ofisim.com/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/linkedin.png" alt="Li" title="Linkedin" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://twitter.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/twitter.png" alt="Tw" title="Twitter" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.facebook.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/facebook.png" alt="Fb" title="Facebook" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;padding-right:5px;"><a target="_blank" href="https://www.instagram.com/ofisimcom/" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/instagram.png" alt="in" title="instagram" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																				<td align="center" valign="top" style="padding:0;Margin:0;"><a target="_blank" href="https://www.youtube.com/channel/UClGV9dPtU1XC2VTn2Kb414A" style="-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, ''helvetica neue'', helvetica, sans-serif;font-size:14px;text-decoration:underline;color:#2CB543;"> <img src="https://ofisim.com/mail/signature/youtube.png" alt="Yt" title="Youtube" width="32" style="display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;" /> </a></td>
																			</tr>
																		</table>
																	</td>
																</tr>
															</table>
														</td>
													</tr>
												</table>
											</td>
										</tr>
									</table>
								</td>
							</tr>
						</table>
					</tr>{{/F}}
				</table>
			</td>
		</tr>
		<tr>
			<td class="contenttd"
				style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 10px; PADDING-TOP: 10px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
				<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
					   align="left">
					<tr class="hiddentds">
						<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
					</tr>
					<tr style="HEIGHT: 10px">
						<th class="contenttd"
							style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
						</th>
					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<td class="contenttd"
				style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 1px; PADDING-TOP: 1px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
				<table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
					   align="left">
					<tr class="hiddentds">
						<td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
					</tr>
					<tr style="HEIGHT: 10px">
						<th class="contenttd"
							style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 1px; TEXT-ALIGN: left; PADDING-TOP: 1px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
						</th>
					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td>
		</tr>
	</table>

	<!-- Created with MailStyler 2.0.0.330 -->
</body>
</html>










', 'en', 1, 'password_reset', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
INSERT INTO "public"."app_templates" VALUES (20, 1, NULL, '2018-10-01 11:41:02.829818', NULL, 'f', 3, 'Account Activation', 'Account Activation', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml"
      xmlns:v="urn:schemas-microsoft-com:vml"
      xmlns:o="urn:schemas-microsoft-com:office:office">
<head>
    <title>Ofisim CRM''e Hoşgeldiniz!</title>
    <meta http-equiv="Content-Type"
          content="text/html; charset=utf-8" />
    <style type="text/css">
        body, .maintable {
            height: 100% !important;
            width: 100% !important;
            margin: 0;
            padding: 0;
        }

        img, a img {
            border: 0;
            outline: none;
            text-decoration: none;
        }

        .imagefix {
            display: block;
        }

        p {
            margin-top: 0;
            margin-right: 0;
            margin-left: 0;
            padding: 0;
        }

        .ReadMsgBody {
            width: 100%;
        }

        .ExternalClass {
            width: 100%;
        }

            .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {
                line-height: 100%;
            }

        img {
            -ms-interpolation-mode: bicubic;
        }

        body, table, td, p, a, li, blockquote {
            -ms-text-size-adjust: 100%;
            -webkit-text-size-adjust: 100%;
        }
    </style>
    <style type="text/css">
        @media only screen and (max-width: 600px) {
            .rtable {
                width: 100% !important;
                table-layout: fixed;
            }

                .rtable tr {
                    height: auto !important;
                    display: block;
                }

            .contenttd {
                max-width: 100% !important;
                display: block;
            }

                .contenttd:after {
                    content: "";
                    display: table;
                    clear: both;
                }

            .hiddentds {
                display: none;
            }

            .imgtable, .imgtable table {
                max-width: 100% !important;
                height: auto;
                float: none;
                margin: 0 auto;
            }

                .imgtable.btnset td {
                    display: inline-block;
                }

                .imgtable img {
                    width: 100%;
                    height: auto;
                    display: block;
                }

            table {
                float: none;
                table-layout: fixed;
            }
        }
    </style>
    <!--[if gte mso 9]> 
    <xml>
      <o:OfficeDocumentSettings>
        <o:AllowPNG/>
        <o:PixelsPerInch>96</o:PixelsPerInch>
      </o:OfficeDocumentSettings>
    </xml>
    <![endif]-->
</head>
<body style="overflow: auto; padding:0; margin:0; font-size: 12px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#f4f4f4">
    <table cellspacing="0" cellpadding="0" width="100%"
           bgcolor="#f4f4f4">
        <tr>
            <td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td>
        </tr>
        <tr>
            <td valign="top">
                <table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto"
                       cellspacing="0" cellpadding="0" width="600" align="center"
                       border="0">
                    <tr>
                        <td class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                                   align="left">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 10px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN:center !important; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent; text-aling:center;">
                                        <table class="imgtable" cellspacing="0" cellpadding="0"
                                               align=" center" border="0">
                                            <tr>
                                                <td style="PADDING-BOTTOM: 2px; PADDING-TOP: 2px; PADDING-LEFT: 2px; PADDING-RIGHT: 2px; text-aling:center;" align="center">
                                                    <table cellspacing="0" cellpadding="0" border="0">
                                                        <tr>
                                                            <td style="BORDER-TOP: medium none; BORDER-RIGHT: medium none;  BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BACKGROUND-COLOR: transparent; text-align:center;">
                                                                <a href="http://www.ofisim.com/crm/" target="_blank"><img style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block" alt="" src="https://ofisimcom.azureedge.net/web/crmLogo.png?v=1" width="200" hspace="0" vspace="0" border="0" /></a>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </th>
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: center; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class="contenttd" style="BORDER-TOP: #fff 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="center">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 10px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                                        <!--[if gte mso 12]>
                                            <table cellspacing="0" cellpadding="0" border="0" width="100%"><tr><td align="center">
                                        <![endif]-->
                                        <table class="imgtable" style="MARGIN: 0px auto" cellspacing="0"
                                               cellpadding="0" align="center" border="0">
                                            <tr>
                                                <td style="PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px"
                                                    align="center">
                                                </td>
                                            </tr>
                                        </table>
                                        <!--[if gte mso 12]>
                                            </td></tr></table>
                                        <![endif]-->
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <tr>

                        

                        <td class="contenttd" style="BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #fff;">
                            <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                                   align="left">
                                <tr class="hiddentds">
                                    <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                                </tr>
                                <tr style="HEIGHT: 20px">
                                    <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 20px; BORDER-LEFT: medium none; PADDING-RIGHT: 20px; BACKGROUND-COLOR: #fff;">
                                        <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            Merhaba {:FirstName} {:LastName},<br />

                                        </p>
                                        <p style="FONT-SIZE: 16px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 24px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"
                                           align="center">Aşşağıda ki linke tıklayarak e-posta adresinizi etkinleştirebilirsiniz.</p>

                                        <p style="FONT-SIZE: 22px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 33px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            <a style="color:#1c62e; text-decoration:underline; font-size:20px;" href="{:Url}">E-Posta mı etkinleştir</a><br />
                                        </p>

                                        <p style="FONT-SIZE: 12px;MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #333330; LINE-HEIGHT: 14px; BACKGROUND-COLOR: #fff; mso-line-height-rule: exactly"
                                           align="center">
                                            Ofisim.com
                                        </p>
                                    </th>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    <tr style="HEIGHT: 20px;">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
                            <div style="PADDING-BOTTOM: 10px; TEXT-ALIGN: center; PADDING-TOP: 10px; PADDING-LEFT: 10px; PADDING-RIGHT: 10px">
                                <table class="imgtable" style="DISPLAY: inline-block"
                                       cellspacing="0" cellpadding="0" border="0">
                                    <tr>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://www.facebook.com/ofisimcrm" target="_blank">
                                                <img title="Facebook"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Facebook" src="http://www.ofisim.com/mail/KobiMail_files/fb.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://twitter.com/ofisim_com" target="_blank">
                                                <img title="Twitter"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Twitter" src="http://www.ofisim.com/mail/KobiMail_files/tw.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                        <td style="PADDING-RIGHT: 5px">
                                            <a href="https://www.linkedin.com/company/ofisim.com"
                                               target="_blank">
                                                <img title="Linkedin"
                                                     style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; DISPLAY: block"
                                                     alt="Linkedin" src="http://www.ofisim.com/mail/KobiMail_files/in.png" width="34"
                                                     height="34" />
                                            </a>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <p style="FONT-SIZE: 14px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #575757; LINE-HEIGHT: 21px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly"
                               align="left">&nbsp;</p>
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="contenttd"
                style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 10px; PADDING-TOP: 10px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                       align="left">
                    <tr class="hiddentds">
                        <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                    </tr>
                    <tr style="HEIGHT: 10px">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="contenttd"
                style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 1px; PADDING-TOP: 1px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent">
                <table style="WIDTH: 100%" cellspacing="0" cellpadding="0"
                       align="left">
                    <tr class="hiddentds">
                        <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td>
                    </tr>
                    <tr style="HEIGHT: 10px">
                        <th class="contenttd"
                            style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 1px; TEXT-ALIGN: left; PADDING-TOP: 1px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent">
                        </th>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td>
        </tr>
    </table>
 
    <!-- Created with MailStyler 2.0.0.330 -->
</body>
</html>
', 'en', 1, 'email_confirm', 't', '{"MailSenderName": "PrimeApps", "MailSenderEmail": "app@primeapps.io"}');
COMMIT;

-- ----------------------------
-- Table structure for apps
-- ----------------------------
DROP TABLE IF EXISTS "public"."apps";
CREATE TABLE "public"."apps" (
  "id" int4 NOT NULL DEFAULT nextval('apps_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "name" varchar(50) COLLATE "pg_catalog"."default",
  "label" varchar(400) COLLATE "pg_catalog"."default",
  "description" varchar(4000) COLLATE "pg_catalog"."default",
  "logo" text COLLATE "pg_catalog"."default",
  "use_tenant_settings" bool NOT NULL,
  "app_draft_id" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "public"."apps" OWNER TO "postgres";

-- ----------------------------
-- Records of apps
-- ----------------------------
BEGIN;
INSERT INTO "public"."apps" VALUES (2, 1, NULL, '2019-02-18 08:34:44', NULL, 'f', 'primeapps_studio', 'PrimeApps Studio', 'PrimeApps Studio', NULL, 'f', 0);
INSERT INTO "public"."apps" VALUES (1, 1, NULL, '2019-01-07 17:27:11.541459', NULL, 'f', 'primeapps_app', 'PrimeApps App', 'PrimeApps App', NULL, 'f', 0);
INSERT INTO "public"."apps" VALUES (3, 1, NULL, '2019-02-24 15:47:31', NULL, 'f', 'primeapps_preview', 'PrimeApps Preview', 'PrimeApps Preview', NULL, 'f', 0);
COMMIT;

-- ----------------------------
-- Table structure for exchange_rates
-- ----------------------------
DROP TABLE IF EXISTS "public"."exchange_rates";
CREATE TABLE "public"."exchange_rates" (
  "id" int4 NOT NULL DEFAULT nextval('exchange_rates_id_seq'::regclass),
  "usd" numeric NOT NULL,
  "eur" numeric NOT NULL,
  "date" timestamp(6) NOT NULL,
  "year" int4 NOT NULL,
  "month" int4 NOT NULL,
  "day" int4 NOT NULL
)
;
ALTER TABLE "public"."exchange_rates" OWNER TO "postgres";

-- ----------------------------
-- Table structure for tenant_licenses
-- ----------------------------
DROP TABLE IF EXISTS "public"."tenant_licenses";
CREATE TABLE "public"."tenant_licenses" (
  "tenant_id" int4 NOT NULL,
  "user_license_count" int4 NOT NULL,
  "module_license_count" int4 NOT NULL,
  "analytics_license_count" int4 NOT NULL,
  "sip_license_count" int4 NOT NULL,
  "is_paid_customer" bool NOT NULL,
  "is_deactivated" bool NOT NULL,
  "is_suspended" bool NOT NULL,
  "deactivated_at" timestamp(6),
  "suspended_at" timestamp(6)
)
;
ALTER TABLE "public"."tenant_licenses" OWNER TO "postgres";

-- ----------------------------
-- Records of tenant_licenses
-- ----------------------------
BEGIN;
INSERT INTO "public"."tenant_licenses" VALUES (1, 9999, 9999, 9999, 9999, 't', 'f', 'f', NULL, NULL);
COMMIT;

-- ----------------------------
-- Table structure for tenant_settings
-- ----------------------------
DROP TABLE IF EXISTS "public"."tenant_settings";
CREATE TABLE "public"."tenant_settings" (
  "tenant_id" int4 NOT NULL,
  "currency" text COLLATE "pg_catalog"."default",
  "culture" text COLLATE "pg_catalog"."default",
  "time_zone" text COLLATE "pg_catalog"."default",
  "language" text COLLATE "pg_catalog"."default",
  "logo" text COLLATE "pg_catalog"."default",
  "mail_sender_name" text COLLATE "pg_catalog"."default",
  "mail_sender_email" text COLLATE "pg_catalog"."default",
  "custom_domain" text COLLATE "pg_catalog"."default",
  "custom_title" text COLLATE "pg_catalog"."default",
  "custom_description" text COLLATE "pg_catalog"."default",
  "custom_favicon" text COLLATE "pg_catalog"."default",
  "custom_color" text COLLATE "pg_catalog"."default",
  "custom_image" text COLLATE "pg_catalog"."default",
  "has_sample_data" bool NOT NULL
)
;
ALTER TABLE "public"."tenant_settings" OWNER TO "postgres";

-- ----------------------------
-- Records of tenant_settings
-- ----------------------------
BEGIN;
INSERT INTO "public"."tenant_settings" VALUES (1, NULL, 'tr-TR', NULL, 'tr', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'f');
COMMIT;

-- ----------------------------
-- Table structure for tenants
-- ----------------------------
DROP TABLE IF EXISTS "public"."tenants";
CREATE TABLE "public"."tenants" (
  "id" int4 NOT NULL DEFAULT nextval('tenants_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "app_id" int4 NOT NULL,
  "guid_id" uuid NOT NULL,
  "title" text COLLATE "pg_catalog"."default",
  "owner_id" int4 NOT NULL,
  "use_user_settings" bool NOT NULL
)
;
ALTER TABLE "public"."tenants" OWNER TO "postgres";

-- ----------------------------
-- Records of tenants
-- ----------------------------
BEGIN;
INSERT INTO "public"."tenants" VALUES (1, 1, NULL, '2019-01-07 18:45:44.071752', NULL, 'f', 1, '00000000-0000-0000-0000-000000000000', 'Master Tenant', 1, 't');
COMMIT;

-- ----------------------------
-- Table structure for user_settings
-- ----------------------------
DROP TABLE IF EXISTS "public"."user_settings";
CREATE TABLE "public"."user_settings" (
  "user_id" int4 NOT NULL,
  "phone" text COLLATE "pg_catalog"."default",
  "culture" text COLLATE "pg_catalog"."default",
  "currency" text COLLATE "pg_catalog"."default",
  "time_zone" text COLLATE "pg_catalog"."default",
  "language" text COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "public"."user_settings" OWNER TO "postgres";

-- ----------------------------
-- Records of user_settings
-- ----------------------------
BEGIN;
INSERT INTO "public"."user_settings" VALUES (1, NULL, 'en-US', 'en', NULL, 'en');
COMMIT;

-- ----------------------------
-- Table structure for user_tenants
-- ----------------------------
DROP TABLE IF EXISTS "public"."user_tenants";
CREATE TABLE "public"."user_tenants" (
  "user_id" int4 NOT NULL,
  "tenant_id" int4 NOT NULL
)
;
ALTER TABLE "public"."user_tenants" OWNER TO "postgres";

-- ----------------------------
-- Records of user_tenants
-- ----------------------------
BEGIN;
INSERT INTO "public"."user_tenants" VALUES (1, 1);
COMMIT;

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS "public"."users";
CREATE TABLE "public"."users" (
  "id" int4 NOT NULL DEFAULT nextval('users_id_seq'::regclass),
  "email" text COLLATE "pg_catalog"."default" NOT NULL,
  "first_name" text COLLATE "pg_catalog"."default" NOT NULL,
  "last_name" text COLLATE "pg_catalog"."default" NOT NULL,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "is_integration_user" bool NOT NULL DEFAULT false,
  "profile_picture" text COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "public"."users" OWNER TO "postgres";

-- ----------------------------
-- Records of users
-- ----------------------------
BEGIN;
INSERT INTO "public"."users" VALUES (1, 'app@primeapps.io', 'Master', 'User', '2019-01-07 17:25:26.688474', NULL, 'f', NULL);
COMMIT;

-- ----------------------------
-- Table structure for warehouses
-- ----------------------------
DROP TABLE IF EXISTS "public"."warehouses";
CREATE TABLE "public"."warehouses" (
  "id" int4 NOT NULL DEFAULT nextval('warehouses_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "tenant_id" int4 NOT NULL,
  "database_name" text COLLATE "pg_catalog"."default",
  "database_user" text COLLATE "pg_catalog"."default",
  "powerbi_workspace_id" text COLLATE "pg_catalog"."default",
  "completed" bool NOT NULL
)
;
ALTER TABLE "public"."warehouses" OWNER TO "postgres";

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
ALTER SEQUENCE "public"."app_templates_id_seq"
OWNED BY "public"."app_templates"."id";
SELECT setval('"public"."app_templates_id_seq"', 21, true);
ALTER SEQUENCE "public"."apps_id_seq"
OWNED BY "public"."apps"."id";
SELECT setval('"public"."apps_id_seq"', 6, true);
ALTER SEQUENCE "public"."exchange_rates_id_seq"
OWNED BY "public"."exchange_rates"."id";
SELECT setval('"public"."exchange_rates_id_seq"', 6, false);
ALTER SEQUENCE "public"."tenants_id_seq"
OWNED BY "public"."tenants"."id";
SELECT setval('"public"."tenants_id_seq"', 6, false);
ALTER SEQUENCE "public"."users_id_seq"
OWNED BY "public"."users"."id";
SELECT setval('"public"."users_id_seq"', 6, false);
ALTER SEQUENCE "public"."warehouses_id_seq"
OWNED BY "public"."warehouses"."id";
SELECT setval('"public"."warehouses_id_seq"', 6, false);

-- ----------------------------
-- Primary Key structure for table _migration_history
-- ----------------------------
ALTER TABLE "public"."_migration_history" ADD CONSTRAINT "PK__migration_history" PRIMARY KEY ("migration_id");

-- ----------------------------
-- Primary Key structure for table app_settings
-- ----------------------------
ALTER TABLE "public"."app_settings" ADD CONSTRAINT "PK_app_settings" PRIMARY KEY ("app_id");

-- ----------------------------
-- Indexes structure for table app_templates
-- ----------------------------
CREATE INDEX "IX_app_templates_active" ON "public"."app_templates" USING btree (
  "active" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_templates_app_id" ON "public"."app_templates" USING btree (
  "app_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_templates_created_by" ON "public"."app_templates" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_templates_language" ON "public"."app_templates" USING btree (
  "language" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_templates_name" ON "public"."app_templates" USING btree (
  "name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_templates_system_code" ON "public"."app_templates" USING btree (
  "system_code" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_templates_type" ON "public"."app_templates" USING btree (
  "type" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_templates_updated_by" ON "public"."app_templates" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table app_templates
-- ----------------------------
ALTER TABLE "public"."app_templates" ADD CONSTRAINT "PK_app_templates" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table apps
-- ----------------------------
CREATE INDEX "IX_apps_app_draft_id" ON "public"."apps" USING btree (
  "app_draft_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_created_at" ON "public"."apps" USING btree (
  "created_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_created_by" ON "public"."apps" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_deleted" ON "public"."apps" USING btree (
  "deleted" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_name" ON "public"."apps" USING btree (
  "name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_updated_at" ON "public"."apps" USING btree (
  "updated_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_updated_by" ON "public"."apps" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table apps
-- ----------------------------
ALTER TABLE "public"."apps" ADD CONSTRAINT "PK_apps" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table exchange_rates
-- ----------------------------
CREATE INDEX "IX_exchange_rates_date" ON "public"."exchange_rates" USING btree (
  "date" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_exchange_rates_day" ON "public"."exchange_rates" USING btree (
  "day" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_exchange_rates_month" ON "public"."exchange_rates" USING btree (
  "month" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_exchange_rates_year" ON "public"."exchange_rates" USING btree (
  "year" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table exchange_rates
-- ----------------------------
ALTER TABLE "public"."exchange_rates" ADD CONSTRAINT "PK_exchange_rates" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table tenant_licenses
-- ----------------------------
CREATE INDEX "IX_tenant_licenses_deactivated_at" ON "public"."tenant_licenses" USING btree (
  "deactivated_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenant_licenses_is_deactivated" ON "public"."tenant_licenses" USING btree (
  "is_deactivated" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenant_licenses_is_paid_customer" ON "public"."tenant_licenses" USING btree (
  "is_paid_customer" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenant_licenses_is_suspended" ON "public"."tenant_licenses" USING btree (
  "is_suspended" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenant_licenses_suspended_at" ON "public"."tenant_licenses" USING btree (
  "suspended_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table tenant_licenses
-- ----------------------------
ALTER TABLE "public"."tenant_licenses" ADD CONSTRAINT "PK_tenant_licenses" PRIMARY KEY ("tenant_id");

-- ----------------------------
-- Indexes structure for table tenant_settings
-- ----------------------------
CREATE INDEX "IX_tenant_settings_custom_domain" ON "public"."tenant_settings" USING btree (
  "custom_domain" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table tenant_settings
-- ----------------------------
ALTER TABLE "public"."tenant_settings" ADD CONSTRAINT "PK_tenant_settings" PRIMARY KEY ("tenant_id");

-- ----------------------------
-- Indexes structure for table tenants
-- ----------------------------
CREATE INDEX "IX_tenants_app_id" ON "public"."tenants" USING btree (
  "app_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenants_created_at" ON "public"."tenants" USING btree (
  "created_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenants_created_by" ON "public"."tenants" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenants_deleted" ON "public"."tenants" USING btree (
  "deleted" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenants_guid_id" ON "public"."tenants" USING btree (
  "guid_id" "pg_catalog"."uuid_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenants_owner_id" ON "public"."tenants" USING btree (
  "owner_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenants_updated_at" ON "public"."tenants" USING btree (
  "updated_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_tenants_updated_by" ON "public"."tenants" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table tenants
-- ----------------------------
ALTER TABLE "public"."tenants" ADD CONSTRAINT "PK_tenants" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table user_settings
-- ----------------------------
CREATE INDEX "IX_user_settings_culture" ON "public"."user_settings" USING btree (
  "culture" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_user_settings_currency" ON "public"."user_settings" USING btree (
  "currency" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_user_settings_language" ON "public"."user_settings" USING btree (
  "language" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_user_settings_phone" ON "public"."user_settings" USING btree (
  "phone" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_user_settings_time_zone" ON "public"."user_settings" USING btree (
  "time_zone" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table user_settings
-- ----------------------------
ALTER TABLE "public"."user_settings" ADD CONSTRAINT "PK_user_settings" PRIMARY KEY ("user_id");

-- ----------------------------
-- Indexes structure for table user_tenants
-- ----------------------------
CREATE INDEX "IX_user_tenants_tenant_id" ON "public"."user_tenants" USING btree (
  "tenant_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_user_tenants_user_id" ON "public"."user_tenants" USING btree (
  "user_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table user_tenants
-- ----------------------------
ALTER TABLE "public"."user_tenants" ADD CONSTRAINT "PK_user_tenants" PRIMARY KEY ("user_id", "tenant_id");

-- ----------------------------
-- Indexes structure for table users
-- ----------------------------
CREATE INDEX "IX_users_created_at" ON "public"."users" USING btree (
  "created_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_users_email" ON "public"."users" USING btree (
  "email" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_users_first_name" ON "public"."users" USING btree (
  "first_name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_users_id" ON "public"."users" USING btree (
  "id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_users_last_name" ON "public"."users" USING btree (
  "last_name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_users_updated_at" ON "public"."users" USING btree (
  "updated_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table users
-- ----------------------------
ALTER TABLE "public"."users" ADD CONSTRAINT "PK_users" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table warehouses
-- ----------------------------
CREATE INDEX "IX_warehouses_completed" ON "public"."warehouses" USING btree (
  "completed" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_warehouses_created_by" ON "public"."warehouses" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_warehouses_database_name" ON "public"."warehouses" USING btree (
  "database_name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_warehouses_tenant_id" ON "public"."warehouses" USING btree (
  "tenant_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_warehouses_updated_by" ON "public"."warehouses" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table warehouses
-- ----------------------------
ALTER TABLE "public"."warehouses" ADD CONSTRAINT "PK_warehouses" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table app_settings
-- ----------------------------
ALTER TABLE "public"."app_settings" ADD CONSTRAINT "FK_app_settings_apps_app_id" FOREIGN KEY ("app_id") REFERENCES "public"."apps" ("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- ----------------------------
-- Foreign Keys structure for table app_templates
-- ----------------------------
ALTER TABLE "public"."app_templates" ADD CONSTRAINT "FK_app_templates_apps_app_id" FOREIGN KEY ("app_id") REFERENCES "public"."apps" ("id") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "public"."app_templates" ADD CONSTRAINT "FK_app_templates_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."app_templates" ADD CONSTRAINT "FK_app_templates_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table apps
-- ----------------------------
ALTER TABLE "public"."apps" ADD CONSTRAINT "FK_apps_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."apps" ADD CONSTRAINT "FK_apps_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table tenant_licenses
-- ----------------------------
ALTER TABLE "public"."tenant_licenses" ADD CONSTRAINT "FK_tenant_licenses_tenants_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES "public"."tenants" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table tenant_settings
-- ----------------------------
ALTER TABLE "public"."tenant_settings" ADD CONSTRAINT "FK_tenant_settings_tenants_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES "public"."tenants" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table tenants
-- ----------------------------
ALTER TABLE "public"."tenants" ADD CONSTRAINT "FK_tenants_apps_app_id" FOREIGN KEY ("app_id") REFERENCES "public"."apps" ("id") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "public"."tenants" ADD CONSTRAINT "FK_tenants_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."tenants" ADD CONSTRAINT "FK_tenants_users_owner_id" FOREIGN KEY ("owner_id") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."tenants" ADD CONSTRAINT "FK_tenants_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table user_settings
-- ----------------------------
ALTER TABLE "public"."user_settings" ADD CONSTRAINT "FK_user_settings_users_user_id" FOREIGN KEY ("user_id") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table user_tenants
-- ----------------------------
ALTER TABLE "public"."user_tenants" ADD CONSTRAINT "FK_user_tenants_tenants_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES "public"."tenants" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."user_tenants" ADD CONSTRAINT "FK_user_tenants_users_user_id" FOREIGN KEY ("user_id") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table warehouses
-- ----------------------------
ALTER TABLE "public"."warehouses" ADD CONSTRAINT "FK_warehouses_tenants_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES "public"."tenants" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."warehouses" ADD CONSTRAINT "FK_warehouses_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."warehouses" ADD CONSTRAINT "FK_warehouses_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;
