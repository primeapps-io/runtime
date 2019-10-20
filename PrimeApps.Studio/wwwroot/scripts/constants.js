'use strict';

angular.module('primeapps')

    .constant('taskDate', {
        today: new Date().setHours(23, 59, 59, 0),
        tomorrow: new Date().setHours(47, 59, 59, 0),
        nextWeek: new Date().setHours(191, 59, 59, 0),
        future: new Date('2100-12-12').getTime()
    })

    .constant('appMenus', {
        overview: [],
        model: ['modules', 'relations', 'dependencies', 'picklists'],
        automation: ['simpleworkflows', 'approvalprocesses', 'advancedWorkflows'],
        visualization: ['views', 'reports', 'menus', 'buttons'],
        templates: ['templatesEmail', 'templatesWord', 'templatesExcel'],
        customcode: ['scripts', 'components', 'functions'],
        accessControl: ['profiles', 'roles'],
        help: [],
        manage: ['appDetails', 'appCollaborators', 'users', 'security', 'identity', 'extensions',
            'appEmailTemplates', 'tenants', 'notifications', 'singleSingOn', 'authentication', 'networkAccess',
            'cors', 'auditTrail', 'passwordPolicies', 'certificates', 'diagnostics', 'passwordPolicies', 'componentsDeployment']
    })

    .constant('entityTypes', {
        user: '11111111-1111-1111-1111-111111111111',
        task: '22222222-2222-2222-2222-222222222222',
        document: '33333333-3333-3333-3333-333333333333',
        note: '44444444-4444-4444-4444-444444444444',
        comment: '55555555-5555-5555-5555-555555555555'
    })

    .constant('component', {
        avatars: 1,
        tasks: 2,
        documents: 4,
        entities: 8,
        users: 16,
        licenses: 32
    })
    .constant('componentTypeEnums', {
        script: 1,
        component: 2
    })
    .constant('componentTypes', [
        { name: 'Script', value: 1 },
        { name: 'Component', value: 2 }
    ])
    .constant('componentPlaceEnums', {
        field_change: 1,
        before_create: 2,
        after_create: 3,
        before_update: 4,
        after_update: 5,
        before_delete: 6,
        after_delete: 7,
        after_record_loaded: 8,
        before_lookup: 9,
        picklist_filter: 10,
        before_approve_process: 11,
        before_reject_process: 12,
        after_approve_process: 13,
        after_reject_process: 14,
        before_send_to_process_approval: 15,
        after_send_to_process_approval: 16,
        before_list_request: 17,
        before_form_loaded: 18,
        after_form_loaded: 19,
        before_form_picklist_loaded: 20,
        after_form_picklist_loaded: 21,
        before_form_record_loaded: 22,
        after_form_record_loaded: 23,
        before_detail_loaded: 24,
        after_detail_loaded: 25,
        before_form_submit: 26,
        before_form_submit_result: 27,
        sub_list_loaded: 28,
        before_import: 29,
        empty_list: 30,
        after_email: 31
    })
    .constant('componentPlaces', [
        { name: 'Field Change', value: 1 },
        { name: 'Before Create', value: 2 },
        { name: 'After Create', value: 3 },
        { name: 'Before Update', value: 4 },
        { name: 'After Update', value: 5 },
        { name: 'Before Delete', value: 6 },
        { name: 'After Delete', value: 7 },
        { name: 'After Record Loaded', value: 8 },
        { name: 'Before Lookup', value: 9 },
        { name: 'Picklist Filter', value: 10 },
        { name: 'Before Approve Process', value: 11 },
        { name: 'Before Reject Process', value: 12 },
        { name: 'After Approve Process', value: 13 },
        { name: 'After Reject Process', value: 14 },
        { name: 'Before Send To Process Approval', value: 15 },
        { name: 'After Send To Process Approval', value: 16 },
        { name: 'Before List Request', value: 17 },
        { name: 'Before Form Loaded', value: 18 },
        { name: 'After Form Loaded', value: 19 },
        { name: 'Before Form Picklist Loaded', value: 20 },
        { name: 'After Form Picklist Loaded', value: 21 },
        { name: 'Before Form Record Loaded', value: 22 },
        { name: 'After Form Record Loaded', value: 23 },
        { name: 'Before Detail Loaded', value: 24 },
        { name: 'After Detail Loaded', value: 25 },
        { name: 'Before Form Submit', value: 26 },
        { name: 'Before Form Submit Result', value: 27 },
        { name: 'Sublist Loaded', value: 28 },
        { name: 'Before Import', value: 29 },
        { name: 'Empty List', value: 30 }



    ])
    .constant('operations', {
        read: 'read',
        modify: 'modify',
        write: 'write',
        remove: 'remove'
    })

    .constant('operations', {
        read: 'read',
        modify: 'modify',
        write: 'write',
        remove: 'remove'
    })

    .constant('dataTypes', {
        text_single: {
            name: 'text_single',
            label: {
                en: 'Text (Single Line)',
                tr: 'Yazı (Tek Satır)'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 1
        },
        text_multi: {
            name: 'text_multi',
            label: {
                en: 'Text (Multi Line)',
                tr: 'Yazı (Çok Satır)'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 2
        },
        number: {
            name: 'number',
            label: {
                en: 'Number',
                tr: 'Sayı'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 3
        },
        number_auto: {
            name: 'number_auto',
            label: {
                en: 'Number (Auto)',
                tr: 'Sayı (Otomatik)'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 4
        },
        number_decimal: {
            name: 'number_decimal',
            label: {
                en: 'Number (Decimal)',
                tr: 'Sayı (Ondalık)'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 5
        },
        currency: {
            name: 'currency',
            label: {
                en: 'Money',
                tr: 'Para'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 6
        },
        date: {
            name: 'date',
            label: {
                en: 'Date',
                tr: 'Tarih'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 7
        },
        date_time: {
            name: 'date_time',
            label: {
                en: 'Date / Time',
                tr: 'Tarih / Saat'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 8
        },
        time: {
            name: 'time',
            label: {
                en: 'Time',
                tr: 'Saat'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 9
        },
        email: {
            name: 'email',
            label: {
                en: 'E-Mail',
                tr: 'E-Posta'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 10
        },
        picklist: {
            name: 'picklist',
            label: {
                en: 'Pick List',
                tr: 'Seçim Listesi'
            },
            operators: [
                'is',
                'is_not',
                'empty',
                'not_empty'
            ],
            order: 11
        },
        multiselect: {
            name: 'multiselect',
            label: {
                en: 'Multi Select',
                tr: 'Çoklu Seçim'
            },
            operators: [
                'contains',
                'not_contain',
                'is',
                'is_not',
                'empty',
                'not_empty'
            ],
            order: 12
        },
        lookup: {
            name: 'lookup',
            label: {
                en: 'Lookup',
                tr: 'Arama'
            },
            operators: [],
            order: 13
        },
        checkbox: {
            name: 'checkbox',
            label: {
                en: 'Check Box',
                tr: 'Onay Kutusu'
            },
            operators: [
                'equals',
                'not_equal'
            ],
            order: 14
        },
        document: {
            name: 'document',
            label: {
                en: 'Document',
                tr: 'Doküman'
            },
            operators: [
                'starts_with',
                'is'
            ],
            order: 15
        },
        url: {
            name: 'url',
            label: {
                en: 'Url',
                tr: 'Url'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 16
        },
        location: {
            name: 'location',
            label: {
                en: 'Location',
                tr: 'Konum'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 17
        },
        image: {
            name: 'image',
            label: {
                en: 'Image',
                tr: 'Resim'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 18
        },
        rating: {
            name: 'rating',
            label: {
                en: 'Rating',
                tr: 'Derecelendirme'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 19
        },
        tag: {
            name: 'tag',
            label: {
                en: 'Tag',
                tr: 'Etiket'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 20
        },
        combination: {
            name: 'combination',
            label: {
                en: 'Combination',
                tr: 'Birleşim Alanı'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 21
        },

        // json: {
        //     name: 'json',
        //     label: {
        //         en: 'Json',
        //         tr: 'Json'
        //     },
        //     operators: [
        //         'contains',
        //         'not_contain',
        //         'is',
        //         'is_not',
        //         'empty',
        //         'not_empty'
        //     ],
        //     order: 21
        // }
    })

    .constant('operators', {
        contains: {
            name: 'contains',
            label: {
                en: 'contains',
                tr: 'içerir'
            },
            order: 1
        },
        not_contain: {
            name: 'not_contain',
            label: {
                en: 'doesn\'t contain',
                tr: 'içermez'
            },
            order: 2
        },
        is: {
            name: 'is',
            label: {
                en: 'is',
                tr: 'eşit'
            },
            order: 3
        },
        is_not: {
            name: 'is_not',
            label: {
                en: 'isn\'t',
                tr: 'eşit değil'
            },
            order: 4
        },
        equals: {
            name: 'equals',
            label: {
                en: 'equals',
                tr: 'eşit'
            },
            order: 5
        },
        not_equal: {
            name: 'not_equal',
            label: {
                en: 'doesn\'t equal',
                tr: 'eşit değil'
            },
            order: 6
        },
        starts_with: {
            name: 'starts_with',
            label: {
                en: 'starts with',
                tr: 'ile başlar'
            },
            order: 7
        },
        ends_with: {
            name: 'ends_with',
            label: {
                en: 'ends with',
                tr: 'ile biter'
            },
            order: 8
        },
        empty: {
            name: 'empty',
            label: {
                en: 'empty',
                tr: 'boş'
            },
            order: 9
        },
        not_empty: {
            name: 'not_empty',
            label: {
                en: 'not empty',
                tr: 'boş değil'
            },
            order: 10
        },
        greater: {
            name: 'greater',
            label: {
                en: 'greater',
                tr: 'büyük'
            },
            order: 11
        },
        greater_equal: {
            name: 'greater_equal',
            label: {
                en: 'greater or equal',
                tr: 'büyük eşit'
            },
            order: 12
        },
        less: {
            name: 'less',
            label: {
                en: 'less',
                tr: 'küçük'
            },
            order: 13
        },
        less_equal: {
            name: 'less_equal',
            label: {
                en: 'less or equal',
                tr: 'küçük eşit'
            },
            order: 14
        }
    })

    .constant('activityTypes', [
        {
            type: 1,
            id: 1,
            label: {
                en: 'Task',
                tr: 'Görev'
            },
            system_code: 'task',
            value: 'task',
            order: 1,
            hidden: false
        },
        {
            type: 1,
            id: 2,
            label: {
                en: 'Event',
                tr: 'Etkinlik'
            },
            system_code: 'event',
            value: 'event',
            order: 2,
            hidden: false
        },
        {
            type: 1,
            id: 3,
            label: {
                en: 'Call',
                tr: 'Arama'
            },
            system_code: 'call',
            value: 'call',
            order: 3,
            hidden: false
        }
    ])

    .constant('transactionTypes', [
        {
            type: 1,
            id: 350,
            label: {
                en: 'Collection',
                tr: 'Tahsilat'
            },
            system_code: 'collection',
            value: 'collection',
            order: 1,
            show: true
        },
        {
            type: 2,
            id: 351,
            label: {
                en: 'Payment',
                tr: 'Ödeme'
            },
            system_code: 'payment',
            value: 'payment',
            order: 2,
            show: true
        }
        ,
        {
            type: 1,
            id: 352,
            label: {
                en: 'Sales Invoice',
                tr: 'Satış Faturası'
            },
            system_code: 'sales_invoice',
            value: 'sales_invoice',
            order: 3,
            show: true
        },
        {
            type: 2,
            id: 353,
            label: {
                en: 'Purchase Invoice',
                tr: 'Alış Faturası'
            },
            system_code: 'purchase_invoice',
            value: 'purchase_invoice',
            order: 4,
            show: true
        }
    ])

    .constant('yesNo', [
        {
            type: 2,
            id: 4,
            label: {
                en: 'Yes',
                tr: 'Evet'
            },
            system_code: 'true',
            order: 1
        },
        {
            type: 2,
            id: 5,
            label: {
                en: 'No',
                tr: 'Hayır'
            },
            system_code: 'false',
            order: 2
        }
    ])

    .constant('systemFields', [
        'id',
        'deleted',
        'shared_users',
        'shared_user_groups',
        'shared_users_edit',
        'shared_user_groups_edit',
        'is_sample',
        'is_converted',
        'master_id',
        'migration_id',
        'import_id',
        'created_by',
        'updated_by',
        'created_at',
        'updated_at',
        'currency'
    ])

    .constant('systemRequiredFields', {
        all: [
            'owner',
            'created_by',
            'created_at',
            'updated_by',
            'updated_at'
        ],
        activities: [
            'activity_type',
            'related_module',
            'related_to',
            'subject',
            'task_due_date',
            'task_status',
            'task_notification',
            'task_reminder',
            'reminder_recurrence',
            'event_start_date',
            'event_end_date',
            'event_reminder',
            'call_time'
        ],
        opportunities: [
            'amount',
            'closing_date',
            'stage',
            'probability',
            'expected_revenue'
        ],
        products: [
            'name',
            'unit_price',
            'usage_unit',
            'purchase_price',
            'vat_percent',
            'using_stock',
            'stock_quantity',
            'critical_stock_limit'
        ],
        quotes: [
            'account',
            'quote_stage',
            'total',
            'vat_total',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'vat_list',
            'email'
        ],
        sales_orders: [
            'account',
            'order_stage',
            'total',
            'vat_total',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'vat_list',
            'email',
            'currency'
        ],
        purchase_orders: [
            'supplier',
            'order_stage',
            'total',
            'vat_total',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'vat_list',
            'email',
            'currency'
        ],
        calisanlar: [
            'sabit_devreden_izin',
            'devreden_izin',
            'kalan_izin_hakki',
            'hakedilen_izin'
        ],
        human_resources: [
            'sabit_devreden_izin',
            'devreden_izin',
            'kalan_izin_hakki',
            'hakedilen_izin'
        ],
        izinler: [
            'bitis_tarihi',
            'baslangic_tarihi',
            'from_entry_type',
            'to_entry_type',
            'mevcut_kullanilabilir_izin',
            'hesaplanan_alinacak_toplam_izin',
            'custom_approver',
            'talep_edilen_izin',
            'calisan',
            'izin_turu'
        ],
        izin_turleri: [
            'adi',
            'yillik_izin',
            'yillik_hakedilen_limit_gun',
            'tek_seferde_alinabilecek_en_fazla_izin_gun',
            'izin_hakkindan_dusulsun',
            'yillik_izin_hakki_gun',
            'cuma_gunu_alinan_izinlere_cumartesiyi_de_ekle',
            'izin_hakkindan_takvim_gunu_olarak_dusulsun',
            'resmi_tatiller_ile_birlestirilebilir',
            'izin_borclanma_yapilabilir',
            'saatlik_kullanim_yapilir',
            'saatlik_kullanimi_yukari_yuvarla',
            'dogum_gunu_izni',
            'sonraki_doneme_devredilen_izin_gun',
            'ilk_izin_kullanimi_hakedis_zamani_ay',
            'yasa_gore_asgari_izin_gun',
            'yillik_izine_ek_izin_suresi_ekle',
            'yillik_izine_ek_izin_suresi_gun',
            'ek_izin_sonraki_yillara_devreder',
            'oncelikle_yillik_izin_kullanimi_dusulur',
            'toplam_calisma_saati',
            'dogum_gunu_izni_kullanimi',
            'sadece_tam_gun_olarak_kullanilir'
        ],
        current_accounts: [
            'transaction_type',
            'date',
            'customer',
            'supplier'
        ],
        stock_transactions: [
            'transaction_date',
            'stock_transaction_type',
            'quantity',
            'product',
            'sales_order',
            'purchase_order',
            'supplier',
            'customer'
        ],
        holidays: [
            'date',
            'country'
        ]
    })

    .constant('systemReadonlyFields', {
        all: [
            'owner',
            'created_by',
            'created_at',
            'updated_by',
            'updated_at'
        ],
        activities: [
            'activity_type',
            'related_module',
            'related_to',
            'task_status'
        ],
        opportunities: [
            'expected_revenue'
        ],
        products: [
            'unit_price',
            'vat_percent',
            'purchase_price',
            'stock_quantity'
        ],
        quotes: [
            'account',
            'quote_stage',
            'total',
            'vat_total',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'email'
        ],
        orders: [
            'account',
            'order_stage',
            'total',
            'vat_total',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'email'
        ],
        current_accounts: [
            'transaction_type',
            'date',
            'amount',
            'customer',
            'supplier'
        ],
        holidays: [
            'date',
            'country'
        ],
        timetrackers: [
            'week',
            'month',
            'year',
            'timetracker_id',
            'date_range',
            'toplam_saat'
        ],
        timetracker_items: [
            'saat',
            'tarih',
            'izindir',
            'related_timetracker'
        ]
    })
    .constant('icons', {
        icons: [
            {
                "value": "fa fa-adjust",
                "label": "<i class=\"fa fa-adjust\"> Adjust"
            },
            {
                "value": "fa fa-anchor",
                "label": "<i class=\"fa fa-anchor\"> Anchor"
            },
            {
                "value": "fa fa-archive",
                "label": "<i class=\"fa fa-archive\"> Archive"
            },
            {
                "value": "fa fa-area-chart",
                "label": "<i class=\"fa fa-area-chart\"> Area-chart",
                "chart": true
            },
            {
                "value": "fa fa-arrows",
                "label": "<i class=\"fa fa-arrows\"> Arrows"
            },
            {
                "value": "fa fa-arrows-h",
                "label": "<i class=\"fa fa-arrows-h\"> Arrows-h"
            },
            {
                "value": "fa fa-arrows-v",
                "label": "<i class=\"fa fa-arrows-v\"> Arrows-v"
            },
            {
                "value": "fa fa-asterisk",
                "label": "<i class=\"fa fa-asterisk\"> Asterisk"
            },
            {
                "value": "fa fa-at",
                "label": "<i class=\"fa fa-at\"> At"
            },
            {
                "value": "fa fa-automobile",
                "label": "<i class=\"fa fa-automobile\"> Automobile"
            },
            {
                "value": "fa fa-ban",
                "label": "<i class=\"fa fa-ban\"> Ban"
            },
            {
                "value": "fa fa-bank",
                "label": "<i class=\"fa fa-bank\"> Bank"
            },
            {
                "value": "fa fa-bar-chart",
                "label": "<i class=\"fa fa-bar-chart\"> Bar-chart",
                "chart": true
            },
            {
                "value": "fa fa-bar-chart-o",
                "label": "<i class=\"fa fa-bar-chart-o\"> Bar-chart-o"
            },
            {
                "value": "fa fa-barcode",
                "label": "<i class=\"fa fa-barcode\"> Barcode"
            },
            {
                "value": "fa fa-bars",
                "label": "<i class=\"fa fa-bars\"> Bars"
            },
            {
                "value": "fa fa-bed",
                "label": "<i class=\"fa fa-bed\"> Bed"
            },
            {
                "value": "fa fa-beer",
                "label": "<i class=\"fa fa-beer\"> Beer"
            },
            {
                "value": "fa fa-bell",
                "label": "<i class=\"fa fa-bell\"> Bell"
            },
            {
                "value": "fa fa-bell-o",
                "label": "<i class=\"fa fa-bell-o\"> Bell-o"
            },
            {
                "value": "fa fa-bell-slash",
                "label": "<i class=\"fa fa-bell-slash\"> Bell-slash"
            },
            {
                "value": "fa fa-bell-slash-o",
                "label": "<i class=\"fa fa-bell-slash-o\"> Bell-slash-o"
            },
            {
                "value": "fa fa-bicycle",
                "label": "<i class=\"fa fa-bicycle\"> Bicycle"
            },
            {
                "value": "fa fa-binoculars",
                "label": "<i class=\"fa fa-binoculars\"> Binoculars"
            },
            {
                "value": "fa fa-birthday-cake",
                "label": "<i class=\"fa fa-birthday-cake\"> Birthday-cake"
            },
            {
                "value": "fa fa-bolt",
                "label": "<i class=\"fa fa-bolt\"> Bolt"
            },
            {
                "value": "fa fa-bomb",
                "label": "<i class=\"fa fa-bomb\"> Bomb"
            },
            {
                "value": "fa fa-book",
                "label": "<i class=\"fa fa-book\"> Book"
            },
            {
                "value": "fa fa-bookmark",
                "label": "<i class=\"fa fa-bookmark\"> Bookmark"
            },
            {
                "value": "fa fa-bookmark-o",
                "label": "<i class=\"fa fa-bookmark-o\"> Bookmark-o"
            },
            {
                "value": "fa fa-briefcase",
                "label": "<i class=\"fa fa-briefcase\"> Briefcase"
            },
            {
                "value": "fa fa-bug",
                "label": "<i class=\"fa fa-bug\"> Bug"
            },
            {
                "value": "fa fa-building",
                "label": "<i class=\"fa fa-building\"> Building"
            },
            {
                "value": "fa fa-building-o",
                "label": "<i class=\"fa fa-building-o\"> Building-o"
            },
            {
                "value": "fa fa-bullhorn",
                "label": "<i class=\"fa fa-bullhorn\"> Bullhorn"
            },
            {
                "value": "fa fa-bullseye",
                "label": "<i class=\"fa fa-bullseye\"> Bullseye"
            },
            {
                "value": "fa fa-bus",
                "label": "<i class=\"fa fa-bus\"> Bus"
            },
            {
                "value": "fa fa-cab",
                "label": "<i class=\"fa fa-cab\"> Cab"
            },
            {
                "value": "fa fa-calculator",
                "label": "<i class=\"fa fa-calculator\"> Calculator"
            },
            {
                "value": "fa fa-calendar",
                "label": "<i class=\"fa fa-calendar\"> Calendar"
            },
            {
                "value": "fa fa-calendar-o",
                "label": "<i class=\"fa fa-calendar-o\"> Calendar-o"
            },
            {
                "value": "fa fa-camera",
                "label": "<i class=\"fa fa-camera\"> Camera"
            },
            {
                "value": "fa fa-camera-retro",
                "label": "<i class=\"fa fa-camera-retro\"> Camera-retro"
            },
            {
                "value": "fa fa-car",
                "label": "<i class=\"fa fa-car\"> Car"
            },
            {
                "value": "fa fa-caret-square-o-down",
                "label": "<i class=\"fa fa-caret-square-o-down\"> Caret-square-o-down"
            },
            {
                "value": "fa fa-caret-square-o-left",
                "label": "<i class=\"fa fa-caret-square-o-left\"> Caret-square-o-left"
            },
            {
                "value": "fa fa-caret-square-o-right",
                "label": "<i class=\"fa fa-caret-square-o-right\"> Caret-square-o-right"
            },
            {
                "value": "fa fa-caret-square-o-up",
                "label": "<i class=\"fa fa-caret-square-o-up\"> Caret-square-o-up"
            },
            {
                "value": "fa fa-cart-arrow-down",
                "label": "<i class=\"fa fa-cart-arrow-down\"> Cart-arrow-down"
            },
            {
                "value": "fa fa-cart-plus",
                "label": "<i class=\"fa fa-cart-plus\"> Cart-plus"
            },
            {
                "value": "fa fa-cc",
                "label": "<i class=\"fa fa-cc\"> Cc"
            },
            {
                "value": "fa fa-certificate",
                "label": "<i class=\"fa fa-certificate\"> Certificate"
            },
            {
                "value": "fa fa-check",
                "label": "<i class=\"fa fa-check\"> Check"
            },
            {
                "value": "fa fa-check-circle",
                "label": "<i class=\"fa fa-check-circle\"> Check-circle"
            },
            {
                "value": "fa fa-check-circle-o",
                "label": "<i class=\"fa fa-check-circle-o\"> Check-circle-o"
            },
            {
                "value": "fa fa-check-square",
                "label": "<i class=\"fa fa-check-square\"> Check-square"
            },
            {
                "value": "fa fa-check-square-o",
                "label": "<i class=\"fa fa-check-square-o\"> Check-square-o"
            },
            {
                "value": "fa fa-child",
                "label": "<i class=\"fa fa-child\"> Child"
            },
            {
                "value": "fa fa-circle",
                "label": "<i class=\"fa fa-circle\"> Circle"
            },
            {
                "value": "fa fa-circle-o",
                "label": "<i class=\"fa fa-circle-o\"> Circle-o"
            },
            {
                "value": "fa fa-circle-o-notch",
                "label": "<i class=\"fa fa-circle-o-notch\"> Circle-o-notch"
            },
            {
                "value": "fa fa-circle-thin",
                "label": "<i class=\"fa fa-circle-thin\"> Circle-thin"
            },
            {
                "value": "fa fa-clock-o",
                "label": "<i class=\"fa fa-clock-o\"> Clock-o"
            },
            {
                "value": "fa fa-close",
                "label": "<i class=\"fa fa-close\"> Close"
            },
            {
                "value": "fa fa-cloud",
                "label": "<i class=\"fa fa-cloud\"> Cloud"
            },
            {
                "value": "fa fa-cloud-download",
                "label": "<i class=\"fa fa-cloud-download\"> Cloud-download"
            },
            {
                "value": "fa fa-cloud-upload",
                "label": "<i class=\"fa fa-cloud-upload\"> Cloud-upload"
            },
            {
                "value": "fa fa-code",
                "label": "<i class=\"fa fa-code\"> Code"
            },
            {
                "value": "fa fa-code-fork",
                "label": "<i class=\"fa fa-code-fork\"> Code-fork"
            },
            {
                "value": "fa fa-coffee",
                "label": "<i class=\"fa fa-coffee\"> Coffee"
            },
            {
                "value": "fa fa-cog",
                "label": "<i class=\"fa fa-cog\"> Cog"
            },
            {
                "value": "fa fa-cogs",
                "label": "<i class=\"fa fa-cogs\"> Cogs"
            },
            {
                "value": "fa fa-comment",
                "label": "<i class=\"fa fa-comment\"> Comment"
            },
            {
                "value": "fa fa-comment-o",
                "label": "<i class=\"fa fa-comment-o\"> Comment-o"
            },
            {
                "value": "fa fa-comments",
                "label": "<i class=\"fa fa-comments\"> Comments"
            },
            {
                "value": "fa fa-comments-o",
                "label": "<i class=\"fa fa-comments-o\"> Comments-o"
            },
            {
                "value": "fa fa-compass",
                "label": "<i class=\"fa fa-compass\"> Compass"
            },
            {
                "value": "fa fa-copyright",
                "label": "<i class=\"fa fa-copyright\"> Copyright"
            },
            {
                "value": "fa fa-credit-card",
                "label": "<i class=\"fa fa-credit-card\"> Credit-card"
            },
            {
                "value": "fa fa-crop",
                "label": "<i class=\"fa fa-crop\"> Crop"
            },
            {
                "value": "fa fa-crosshairs",
                "label": "<i class=\"fa fa-crosshairs\"> Crosshairs"
            },
            {
                "value": "fa fa-cube",
                "label": "<i class=\"fa fa-cube\"> Cube"
            },
            {
                "value": "fa fa-cubes",
                "label": "<i class=\"fa fa-cubes\"> Cubes"
            },
            {
                "value": "fa fa-cutlery",
                "label": "<i class=\"fa fa-cutlery\"> Cutlery"
            },
            {
                "value": "fa fa-dashboard",
                "label": "<i class=\"fa fa-dashboard\"> Dashboard"
            },
            {
                "value": "fa fa-database",
                "label": "<i class=\"fa fa-database\"> Database"
            },
            {
                "value": "fa fa-desktop",
                "label": "<i class=\"fa fa-desktop\"> Desktop"
            },
            {
                "value": "fa fa-diamond",
                "label": "<i class=\"fa fa-diamond\"> Diamond"
            },
            {
                "value": "fa fa-dot-circle-o",
                "label": "<i class=\"fa fa-dot-circle-o\"> Dot-circle-o"
            },
            {
                "value": "fa fa-download",
                "label": "<i class=\"fa fa-download\"> Download"
            },
            {
                "value": "fa fa-edit",
                "label": "<i class=\"fa fa-edit\"> Edit"
            },
            {
                "value": "fa fa-ellipsis-h",
                "label": "<i class=\"fa fa-ellipsis-h\"> Ellipsis-h"
            },
            {
                "value": "fa fa-ellipsis-v",
                "label": "<i class=\"fa fa-ellipsis-v\"> Ellipsis-v"
            },
            {
                "value": "fa fa-envelope",
                "label": "<i class=\"fa fa-envelope\"> Envelope"
            },
            {
                "value": "fa fa-envelope-o",
                "label": "<i class=\"fa fa-envelope-o\"> Envelope-o"
            },
            {
                "value": "fa fa-envelope-square",
                "label": "<i class=\"fa fa-envelope-square\"> Envelope-square"
            },
            {
                "value": "fa fa-eraser",
                "label": "<i class=\"fa fa-eraser\"> Eraser"
            },
            {
                "value": "fa fa-exchange",
                "label": "<i class=\"fa fa-exchange\"> Exchange"
            },
            {
                "value": "fa fa-exclamation",
                "label": "<i class=\"fa fa-exclamation\"> Exclamation"
            },
            {
                "value": "fa fa-exclamation-circle",
                "label": "<i class=\"fa fa-exclamation-circle\"> Exclamation-circle"
            },
            {
                "value": "fa fa-exclamation-triangle",
                "label": "<i class=\"fa fa-exclamation-triangle\"> Exclamation-triangle"
            },
            {
                "value": "fa fa-external-link",
                "label": "<i class=\"fa fa-external-link\"> External-link"
            },
            {
                "value": "fa fa-external-link-square",
                "label": "<i class=\"fa fa-external-link-square\"> External-link-square"
            },
            {
                "value": "fa fa-eye",
                "label": "<i class=\"fa fa-eye\"> Eye"
            },
            {
                "value": "fa fa-eye-slash",
                "label": "<i class=\"fa fa-eye-slash\"> Eye-slash"
            },
            {
                "value": "fa fa-eyedropper",
                "label": "<i class=\"fa fa-eyedropper\"> Eyedropper"
            },
            {
                "value": "fa fa-fax",
                "label": "<i class=\"fa fa-fax\"> Fax"
            },
            {
                "value": "fa fa-female",
                "label": "<i class=\"fa fa-female\"> Female"
            },
            {
                "value": "fa fa-fighter-jet",
                "label": "<i class=\"fa fa-fighter-jet\"> Fighter-jet"
            },
            {
                "value": "fa fa-file-archive-o",
                "label": "<i class=\"fa fa-file-archive-o\"> File-archive-o"
            },
            {
                "value": "fa fa-file-audio-o",
                "label": "<i class=\"fa fa-file-audio-o\"> File-audio-o"
            },
            {
                "value": "fa fa-file-code-o",
                "label": "<i class=\"fa fa-file-code-o\"> File-code-o"
            },
            {
                "value": "fa fa-file-excel-o",
                "label": "<i class=\"fa fa-file-excel-o\"> File-excel-o"
            },
            {
                "value": "fa fa-file-image-o",
                "label": "<i class=\"fa fa-file-image-o\"> File-image-o"
            },
            {
                "value": "fa fa-file-movie-o",
                "label": "<i class=\"fa fa-file-movie-o\"> File-movie-o"
            },
            {
                "value": "fa fa-file-pdf-o",
                "label": "<i class=\"fa fa-file-pdf-o\"> File-pdf-o"
            },
            {
                "value": "fa fa-file-photo-o",
                "label": "<i class=\"fa fa-file-photo-o\"> File-photo-o"
            },
            {
                "value": "fa fa-file-picture-o",
                "label": "<i class=\"fa fa-file-picture-o\"> File-picture-o"
            },
            {
                "value": "fa fa-file-powerpoint-o",
                "label": "<i class=\"fa fa-file-powerpoint-o\"> File-powerpoint-o"
            },
            {
                "value": "fa fa-file-sound-o",
                "label": "<i class=\"fa fa-file-sound-o\"> File-sound-o"
            },
            {
                "value": "fa fa-file-video-o",
                "label": "<i class=\"fa fa-file-video-o\"> File-video-o"
            },
            {
                "value": "fa fa-file-word-o",
                "label": "<i class=\"fa fa-file-word-o\"> File-word-o"
            },
            {
                "value": "fa fa-file-zip-o",
                "label": "<i class=\"fa fa-file-zip-o\"> File-zip-o"
            },
            {
                "value": "fa fa-film",
                "label": "<i class=\"fa fa-film\"> Film"
            },
            {
                "value": "fa fa-filter",
                "label": "<i class=\"fa fa-filter\"> Filter"
            },
            {
                "value": "fa fa-fire",
                "label": "<i class=\"fa fa-fire\"> Fire"
            },
            {
                "value": "fa fa-fire-extinguisher",
                "label": "<i class=\"fa fa-fire-extinguisher\"> Fire-extinguisher"
            },
            {
                "value": "fa fa-flag",
                "label": "<i class=\"fa fa-flag\"> Flag"
            },
            {
                "value": "fa fa-flag-checkered",
                "label": "<i class=\"fa fa-flag-checkered\"> Flag-checkered"
            },
            {
                "value": "fa fa-flag-o",
                "label": "<i class=\"fa fa-flag-o\"> Flag-o"
            },
            {
                "value": "fa fa-flash",
                "label": "<i class=\"fa fa-flash\"> Flash"
            },
            {
                "value": "fa fa-flask",
                "label": "<i class=\"fa fa-flask\"> Flask"
            },
            {
                "value": "fa fa-folder",
                "label": "<i class=\"fa fa-folder\"> Folder"
            },
            {
                "value": "fa fa-folder-o",
                "label": "<i class=\"fa fa-folder-o\"> Folder-o"
            },
            {
                "value": "fa fa-folder-open",
                "label": "<i class=\"fa fa-folder-open\"> Folder-open"
            },
            {
                "value": "fa fa-folder-open-o",
                "label": "<i class=\"fa fa-folder-open-o\"> Folder-open-o"
            },
            {
                "value": "fa fa-frown-o",
                "label": "<i class=\"fa fa-frown-o\"> Frown-o"
            },
            {
                "value": "fa fa-futbol-o",
                "label": "<i class=\"fa fa-futbol-o\"> Futbol-o"
            },
            {
                "value": "fa fa-gamepad",
                "label": "<i class=\"fa fa-gamepad\"> Gamepad"
            },
            {
                "value": "fa fa-gavel",
                "label": "<i class=\"fa fa-gavel\"> Gavel"
            },
            {
                "value": "fa fa-gear",
                "label": "<i class=\"fa fa-gear\"> Gear"
            },
            {
                "value": "fa fa-gears",
                "label": "<i class=\"fa fa-gears\"> Gears"
            },
            {
                "value": "fa fa-gift",
                "label": "<i class=\"fa fa-gift\"> Gift"
            },
            {
                "value": "fa fa-glass",
                "label": "<i class=\"fa fa-glass\"> Glass"
            },
            {
                "value": "fa fa-globe",
                "label": "<i class=\"fa fa-globe\"> Globe"
            },
            {
                "value": "fa fa-graduation-cap",
                "label": "<i class=\"fa fa-graduation-cap\"> Graduation-cap"
            },
            {
                "value": "fa fa-group",
                "label": "<i class=\"fa fa-group\"> Group"
            },
            {
                "value": "fa fa-hdd-o",
                "label": "<i class=\"fa fa-hdd-o\"> Hdd-o"
            },
            {
                "value": "fa fa-headphones",
                "label": "<i class=\"fa fa-headphones\"> Headphones"
            },
            {
                "value": "fa fa-heart",
                "label": "<i class=\"fa fa-heart\"> Heart"
            },
            {
                "value": "fa fa-heart-o",
                "label": "<i class=\"fa fa-heart-o\"> Heart-o"
            },
            {
                "value": "fa fa-heartbeat",
                "label": "<i class=\"fa fa-heartbeat\"> Heartbeat"
            },
            {
                "value": "fa fa-history",
                "label": "<i class=\"fa fa-history\"> History"
            },
            {
                "value": "fa fa-home",
                "label": "<i class=\"fa fa-home\"> Home"
            },
            {
                "value": "fa fa-hotel",
                "label": "<i class=\"fa fa-hotel\"> Hotel"
            },
            {
                "value": "fa fa-image",
                "label": "<i class=\"fa fa-image\"> Image"
            },
            {
                "value": "fa fa-inbox",
                "label": "<i class=\"fa fa-inbox\"> Inbox"
            },
            {
                "value": "fa fa-info",
                "label": "<i class=\"fa fa-info\"> Info"
            },
            {
                "value": "fa fa-info-circle",
                "label": "<i class=\"fa fa-info-circle\"> Info-circle"
            },
            {
                "value": "fa fa-institution",
                "label": "<i class=\"fa fa-institution\"> Institution"
            },
            {
                "value": "fa fa-key",
                "label": "<i class=\"fa fa-key\"> Key"
            },
            {
                "value": "fa fa-keyboard-o",
                "label": "<i class=\"fa fa-keyboard-o\"> Keyboard-o"
            },
            {
                "value": "fa fa-language",
                "label": "<i class=\"fa fa-language\"> Language"
            },
            {
                "value": "fa fa-laptop",
                "label": "<i class=\"fa fa-laptop\"> Laptop"
            },
            {
                "value": "fa fa-leaf",
                "label": "<i class=\"fa fa-leaf\"> Leaf"
            },
            {
                "value": "fa fa-legal",
                "label": "<i class=\"fa fa-legal\"> Legal"
            },
            {
                "value": "fa fa-lemon-o",
                "label": "<i class=\"fa fa-lemon-o\"> Lemon-o"
            },
            {
                "value": "fa fa-level-down",
                "label": "<i class=\"fa fa-level-down\"> Level-down"
            },
            {
                "value": "fa fa-level-up",
                "label": "<i class=\"fa fa-level-up\"> Level-up"
            },
            {
                "value": "fa fa-life-bouy",
                "label": "<i class=\"fa fa-life-bouy\"> Life-bouy"
            },
            {
                "value": "fa fa-life-buoy",
                "label": "<i class=\"fa fa-life-buoy\"> Life-buoy"
            },
            {
                "value": "fa fa-life-ring",
                "label": "<i class=\"fa fa-life-ring\"> Life-ring"
            },
            {
                "value": "fa fa-life-saver",
                "label": "<i class=\"fa fa-life-saver\"> Life-saver"
            },
            {
                "value": "fa fa-lightbulb-o",
                "label": "<i class=\"fa fa-lightbulb-o\"> Lightbulb-o"
            },
            {
                "value": "fa fa-line-chart",
                "label": "<i class=\"fa fa-line-chart\"> Line-chart",
                "chart": true
            },
            {
                "value": "fa fa-location-arrow",
                "label": "<i class=\"fa fa-location-arrow\"> Location-arrow"
            },
            {
                "value": "fa fa-lock",
                "label": "<i class=\"fa fa-lock\"> Lock"
            },
            {
                "value": "fa fa-magic",
                "label": "<i class=\"fa fa-magic\"> Magic"
            },
            {
                "value": "fa fa-magnet",
                "label": "<i class=\"fa fa-magnet\"> Magnet"
            },
            {
                "value": "fa fa-mail-forward",
                "label": "<i class=\"fa fa-mail-forward\"> Mail-forward"
            },
            {
                "value": "fa fa-mail-reply",
                "label": "<i class=\"fa fa-mail-reply\"> Mail-reply"
            },
            {
                "value": "fa fa-mail-reply-all",
                "label": "<i class=\"fa fa-mail-reply-all\"> Mail-reply-all"
            },
            {
                "value": "fa fa-male",
                "label": "<i class=\"fa fa-male\"> Male"
            },
            {
                "value": "fa fa-meh-o",
                "label": "<i class=\"fa fa-meh-o\"> Meh-o"
            },
            {
                "value": "fa fa-microphone",
                "label": "<i class=\"fa fa-microphone\"> Microphone"
            },
            {
                "value": "fa fa-microphone-slash",
                "label": "<i class=\"fa fa-microphone-slash\"> Microphone-slash"
            },
            {
                "value": "fa fa-minus",
                "label": "<i class=\"fa fa-minus\"> Minus"
            },
            {
                "value": "fa fa-minus-circle",
                "label": "<i class=\"fa fa-minus-circle\"> Minus-circle"
            },
            {
                "value": "fa fa-minus-square",
                "label": "<i class=\"fa fa-minus-square\"> Minus-square"
            },
            {
                "value": "fa fa-minus-square-o",
                "label": "<i class=\"fa fa-minus-square-o\"> Minus-square-o"
            },
            {
                "value": "fa fa-mobile",
                "label": "<i class=\"fa fa-mobile\"> Mobile"
            },
            {
                "value": "fa fa-mobile-phone",
                "label": "<i class=\"fa fa-mobile-phone\"> Mobile-phone"
            },
            {
                "value": "fa fa-money",
                "label": "<i class=\"fa fa-money\"> Money"
            },
            {
                "value": "fa fa-moon-o",
                "label": "<i class=\"fa fa-moon-o\"> Moon-o"
            },
            {
                "value": "fa fa-mortar-board",
                "label": "<i class=\"fa fa-mortar-board\"> Mortar-board"
            },
            {
                "value": "fa fa-motorcycle",
                "label": "<i class=\"fa fa-motorcycle\"> Motorcycle"
            },
            {
                "value": "fa fa-music",
                "label": "<i class=\"fa fa-music\"> Music"
            },
            {
                "value": "fa fa-navicon",
                "label": "<i class=\"fa fa-navicon\"> Navicon"
            },
            {
                "value": "fa fa-newspaper-o",
                "label": "<i class=\"fa fa-newspaper-o\"> Newspaper-o"
            },
            {
                "value": "fa fa-paint-brush",
                "label": "<i class=\"fa fa-paint-brush\"> Paint-brush"
            },
            {
                "value": "fa fa-paper-plane",
                "label": "<i class=\"fa fa-paper-plane\"> Paper-plane"
            },
            {
                "value": "fa fa-paper-plane-o",
                "label": "<i class=\"fa fa-paper-plane-o\"> Paper-plane-o"
            },
            {
                "value": "fa fa-paw",
                "label": "<i class=\"fa fa-paw\"> Paw"
            },
            {
                "value": "fa fa-pencil",
                "label": "<i class=\"fa fa-pencil\"> Pencil"
            },
            {
                "value": "fa fa-pencil-square",
                "label": "<i class=\"fa fa-pencil-square\"> Pencil-square"
            },
            {
                "value": "fa fa-pencil-square-o",
                "label": "<i class=\"fa fa-pencil-square-o\"> Pencil-square-o"
            },
            {
                "value": "fa fa-phone",
                "label": "<i class=\"fa fa-phone\"> Phone"
            },
            {
                "value": "fa fa-phone-square",
                "label": "<i class=\"fa fa-phone-square\"> Phone-square"
            },
            {
                "value": "fa fa-photo",
                "label": "<i class=\"fa fa-photo\"> Photo"
            },
            {
                "value": "fa fa-picture-o",
                "label": "<i class=\"fa fa-picture-o\"> Picture-o"
            },
            {
                "value": "fa fa-pie-chart",
                "label": "<i class=\"fa fa-pie-chart\"> Pie-chart",
                "chart": true
            },
            {
                "value": "fa fa-plane",
                "label": "<i class=\"fa fa-plane\"> Plane"
            },
            {
                "value": "fa fa-plug",
                "label": "<i class=\"fa fa-plug\"> Plug"
            },
            {
                "value": "fa fa-plus",
                "label": "<i class=\"fa fa-plus\"> Plus"
            },
            {
                "value": "fa fa-plus-circle",
                "label": "<i class=\"fa fa-plus-circle\"> Plus-circle"
            },
            {
                "value": "fa fa-plus-square",
                "label": "<i class=\"fa fa-plus-square\"> Plus-square"
            },
            {
                "value": "fa fa-plus-square-o",
                "label": "<i class=\"fa fa-plus-square-o\"> Plus-square-o"
            },
            {
                "value": "fa fa-power-off",
                "label": "<i class=\"fa fa-power-off\"> Power-off"
            },
            {
                "value": "fa fa-print",
                "label": "<i class=\"fa fa-print\"> Print"
            },
            {
                "value": "fa fa-puzzle-piece",
                "label": "<i class=\"fa fa-puzzle-piece\"> Puzzle-piece"
            },
            {
                "value": "fa fa-qrcode",
                "label": "<i class=\"fa fa-qrcode\"> Qrcode"
            },
            {
                "value": "fa fa-question",
                "label": "<i class=\"fa fa-question\"> Question"
            },
            {
                "value": "fa fa-question-circle",
                "label": "<i class=\"fa fa-question-circle\"> Question-circle"
            },
            {
                "value": "fa fa-quote-left",
                "label": "<i class=\"fa fa-quote-left\"> Quote-left"
            },
            {
                "value": "fa fa-quote-right",
                "label": "<i class=\"fa fa-quote-right\"> Quote-right"
            },
            {
                "value": "fa fa-random",
                "label": "<i class=\"fa fa-random\"> Random"
            },
            {
                "value": "fa fa-recycle",
                "label": "<i class=\"fa fa-recycle\"> Recycle"
            },
            {
                "value": "fa fa-refresh",
                "label": "<i class=\"fa fa-refresh\"> Refresh"
            },
            {
                "value": "fa fa-remove",
                "label": "<i class=\"fa fa-remove\"> Remove"
            },
            {
                "value": "fa fa-reorder",
                "label": "<i class=\"fa fa-reorder\"> Reorder"
            },
            {
                "value": "fa fa-reply",
                "label": "<i class=\"fa fa-reply\"> Reply"
            },
            {
                "value": "fa fa-reply-all",
                "label": "<i class=\"fa fa-reply-all\"> Reply-all"
            },
            {
                "value": "fa fa-retweet",
                "label": "<i class=\"fa fa-retweet\"> Retweet"
            },
            {
                "value": "fa fa-road",
                "label": "<i class=\"fa fa-road\"> Road"
            },
            {
                "value": "fa fa-rocket",
                "label": "<i class=\"fa fa-rocket\"> Rocket"
            },
            {
                "value": "fa fa-rss",
                "label": "<i class=\"fa fa-rss\"> Rss"
            },
            {
                "value": "fa fa-rss-square",
                "label": "<i class=\"fa fa-rss-square\"> Rss-square"
            },
            {
                "value": "fa fa-search",
                "label": "<i class=\"fa fa-search\"> Search"
            },
            {
                "value": "fa fa-search-minus",
                "label": "<i class=\"fa fa-search-minus\"> Search-minus"
            },
            {
                "value": "fa fa-search-plus",
                "label": "<i class=\"fa fa-search-plus\"> Search-plus"
            },
            {
                "value": "fa fa-send",
                "label": "<i class=\"fa fa-send\"> Send"
            },
            {
                "value": "fa fa-send-o",
                "label": "<i class=\"fa fa-send-o\"> Send-o"
            },
            {
                "value": "fa fa-server",
                "label": "<i class=\"fa fa-server\"> Server"
            },
            {
                "value": "fa fa-share",
                "label": "<i class=\"fa fa-share\"> Share"
            },
            {
                "value": "fa fa-share-alt",
                "label": "<i class=\"fa fa-share-alt\"> Share-alt"
            },
            {
                "value": "fa fa-share-alt-square",
                "label": "<i class=\"fa fa-share-alt-square\"> Share-alt-square"
            },
            {
                "value": "fa fa-share-square",
                "label": "<i class=\"fa fa-share-square\"> Share-square"
            },
            {
                "value": "fa fa-share-square-o",
                "label": "<i class=\"fa fa-share-square-o\"> Share-square-o"
            },
            {
                "value": "fa fa-shield",
                "label": "<i class=\"fa fa-shield\"> Shield"
            },
            {
                "value": "fa fa-ship",
                "label": "<i class=\"fa fa-ship\"> Ship"
            },
            {
                "value": "fa fa-shopping-cart",
                "label": "<i class=\"fa fa-shopping-cart\"> Shopping-cart"
            },
            {
                "value": "fa fa-sign-in",
                "label": "<i class=\"fa fa-sign-in\"> Sign-in"
            },
            {
                "value": "fa fa-sign-out",
                "label": "<i class=\"fa fa-sign-out\"> Sign-out"
            },
            {
                "value": "fa fa-signal",
                "label": "<i class=\"fa fa-signal\"> Signal"
            },
            {
                "value": "fa fa-sitemap",
                "label": "<i class=\"fa fa-sitemap\"> Sitemap"
            },
            {
                "value": "fa fa-sliders",
                "label": "<i class=\"fa fa-sliders\"> Sliders"
            },
            {
                "value": "fa fa-smile-o",
                "label": "<i class=\"fa fa-smile-o\"> Smile-o"
            },
            {
                "value": "fa fa-soccer-ball-o",
                "label": "<i class=\"fa fa-soccer-ball-o\"> Soccer-ball-o"
            },
            {
                "value": "fa fa-sort",
                "label": "<i class=\"fa fa-sort\"> Sort"
            },
            {
                "value": "fa fa-sort-alpha-asc",
                "label": "<i class=\"fa fa-sort-alpha-asc\"> Sort-alpha-asc"
            },
            {
                "value": "fa fa-sort-alpha-desc",
                "label": "<i class=\"fa fa-sort-alpha-desc\"> Sort-alpha-desc"
            },
            {
                "value": "fa fa-sort-amount-asc",
                "label": "<i class=\"fa fa-sort-amount-asc\"> Sort-amount-asc"
            },
            {
                "value": "fa fa-sort-amount-desc",
                "label": "<i class=\"fa fa-sort-amount-desc\"> Sort-amount-desc"
            },
            {
                "value": "fa fa-sort-asc",
                "label": "<i class=\"fa fa-sort-asc\"> Sort-asc"
            },
            {
                "value": "fa fa-sort-desc",
                "label": "<i class=\"fa fa-sort-desc\"> Sort-desc"
            },
            {
                "value": "fa fa-sort-down",
                "label": "<i class=\"fa fa-sort-down\"> Sort-down"
            },
            {
                "value": "fa fa-sort-numeric-asc",
                "label": "<i class=\"fa fa-sort-numeric-asc\"> Sort-numeric-asc"
            },
            {
                "value": "fa fa-sort-numeric-desc",
                "label": "<i class=\"fa fa-sort-numeric-desc\"> Sort-numeric-desc"
            },
            {
                "value": "fa fa-sort-up",
                "label": "<i class=\"fa fa-sort-up\"> Sort-up"
            },
            {
                "value": "fa fa-space-shuttle",
                "label": "<i class=\"fa fa-space-shuttle\"> Space-shuttle"
            },
            {
                "value": "fa fa-spinner",
                "label": "<i class=\"fa fa-spinner\"> Spinner"
            },
            {
                "value": "fa fa-spoon",
                "label": "<i class=\"fa fa-spoon\"> Spoon"
            },
            {
                "value": "fa fa-square",
                "label": "<i class=\"fa fa-square\"> Square"
            },
            {
                "value": "fa fa-square-o",
                "label": "<i class=\"fa fa-square-o\"> Square-o"
            },
            {
                "value": "fa fa-star",
                "label": "<i class=\"fa fa-star\"> Star"
            },
            {
                "value": "fa fa-star-half",
                "label": "<i class=\"fa fa-star-half\"> Star-half"
            },
            {
                "value": "fa fa-star-half-empty",
                "label": "<i class=\"fa fa-star-half-empty\"> Star-half-empty"
            },
            {
                "value": "fa fa-star-half-full",
                "label": "<i class=\"fa fa-star-half-full\"> Star-half-full"
            },
            {
                "value": "fa fa-star-half-o",
                "label": "<i class=\"fa fa-star-half-o\"> Star-half-o"
            },
            {
                "value": "fa fa-star-o",
                "label": "<i class=\"fa fa-star-o\"> Star-o"
            },
            {
                "value": "fa fa-street-view",
                "label": "<i class=\"fa fa-street-view\"> Street-view"
            },
            {
                "value": "fa fa-suitcase",
                "label": "<i class=\"fa fa-suitcase\"> Suitcase"
            },
            {
                "value": "fa fa-sun-o",
                "label": "<i class=\"fa fa-sun-o\"> Sun-o"
            },
            {
                "value": "fa fa-support",
                "label": "<i class=\"fa fa-support\"> Support"
            },
            {
                "value": "fa fa-tablet",
                "label": "<i class=\"fa fa-tablet\"> Tablet"
            },
            {
                "value": "fa fa-tachometer",
                "label": "<i class=\"fa fa-tachometer\"> Tachometer"
            },
            {
                "value": "fa fa-tag",
                "label": "<i class=\"fa fa-tag\"> Tag"
            },
            {
                "value": "fa fa-tags",
                "label": "<i class=\"fa fa-tags\"> Tags"
            },
            {
                "value": "fa fa-tasks",
                "label": "<i class=\"fa fa-tasks\"> Tasks"
            },
            {
                "value": "fa fa-taxi",
                "label": "<i class=\"fa fa-taxi\"> Taxi"
            },
            {
                "value": "fa fa-terminal",
                "label": "<i class=\"fa fa-terminal\"> Terminal"
            },
            {
                "value": "fa fa-thumb-tack",
                "label": "<i class=\"fa fa-thumb-tack\"> Thumb-tack"
            },
            {
                "value": "fa fa-thumbs-down",
                "label": "<i class=\"fa fa-thumbs-down\"> Thumbs-down"
            },
            {
                "value": "fa fa-thumbs-o-down",
                "label": "<i class=\"fa fa-thumbs-o-down\"> Thumbs-o-down"
            },
            {
                "value": "fa fa-thumbs-o-up",
                "label": "<i class=\"fa fa-thumbs-o-up\"> Thumbs-o-up"
            },
            {
                "value": "fa fa-thumbs-up",
                "label": "<i class=\"fa fa-thumbs-up\"> Thumbs-up"
            },
            {
                "value": "fa fa-ticket",
                "label": "<i class=\"fa fa-ticket\"> Ticket"
            },
            {
                "value": "fa fa-times",
                "label": "<i class=\"fa fa-times\"> Times"
            },
            {
                "value": "fa fa-times-circle",
                "label": "<i class=\"fa fa-times-circle\"> Times-circle"
            },
            {
                "value": "fa fa-times-circle-o",
                "label": "<i class=\"fa fa-times-circle-o\"> Times-circle-o"
            },
            {
                "value": "fa fa-tint",
                "label": "<i class=\"fa fa-tint\"> Tint"
            },
            {
                "value": "fa fa-toggle-down",
                "label": "<i class=\"fa fa-toggle-down\"> Toggle-down"
            },
            {
                "value": "fa fa-toggle-left",
                "label": "<i class=\"fa fa-toggle-left\"> Toggle-left"
            },
            {
                "value": "fa fa-toggle-off",
                "label": "<i class=\"fa fa-toggle-off\"> Toggle-off"
            },
            {
                "value": "fa fa-toggle-on",
                "label": "<i class=\"fa fa-toggle-on\"> Toggle-on"
            },
            {
                "value": "fa fa-toggle-right",
                "label": "<i class=\"fa fa-toggle-right\"> Toggle-right"
            },
            {
                "value": "fa fa-toggle-up",
                "label": "<i class=\"fa fa-toggle-up\"> Toggle-up"
            },
            {
                "value": "fa fa-trash",
                "label": "<i class=\"fa fa-trash\"> Trash"
            },
            {
                "value": "fa fa-trash-o",
                "label": "<i class=\"fa fa-trash-o\"> Trash-o"
            },
            {
                "value": "fa fa-tree",
                "label": "<i class=\"fa fa-tree\"> Tree"
            },
            {
                "value": "fa fa-trophy",
                "label": "<i class=\"fa fa-trophy\"> Trophy"
            },
            {
                "value": "fa fa-truck",
                "label": "<i class=\"fa fa-truck\"> Truck"
            },
            {
                "value": "fa fa-tty",
                "label": "<i class=\"fa fa-tty\"> Tty"
            },
            {
                "value": "fa fa-umbrella",
                "label": "<i class=\"fa fa-umbrella\"> Umbrella"
            },
            {
                "value": "fa fa-university",
                "label": "<i class=\"fa fa-university\"> University"
            },
            {
                "value": "fa fa-unlock",
                "label": "<i class=\"fa fa-unlock\"> Unlock"
            },
            {
                "value": "fa fa-unlock-alt",
                "label": "<i class=\"fa fa-unlock-alt\"> Unlock-alt"
            },
            {
                "value": "fa fa-unsorted",
                "label": "<i class=\"fa fa-unsorted\"> Unsorted"
            },
            {
                "value": "fa fa-upload",
                "label": "<i class=\"fa fa-upload\"> Upload"
            },
            {
                "value": "fa fa-user",
                "label": "<i class=\"fa fa-user\"> User"
            },
            {
                "value": "fa fa-user-plus",
                "label": "<i class=\"fa fa-user-plus\"> User-plus"
            },
            {
                "value": "fa fa-user-secret",
                "label": "<i class=\"fa fa-user-secret\"> User-secret"
            },
            {
                "value": "fa fa-user-times",
                "label": "<i class=\"fa fa-user-times\"> User-times"
            },
            {
                "value": "fa fa-users",
                "label": "<i class=\"fa fa-users\"> Users"
            },
            {
                "value": "fa fa-video-camera",
                "label": "<i class=\"fa fa-video-camera\"> Video-camera"
            },
            {
                "value": "fa fa-volume-down",
                "label": "<i class=\"fa fa-volume-down\"> Volume-down"
            },
            {
                "value": "fa fa-volume-off",
                "label": "<i class=\"fa fa-volume-off\"> Volume-off"
            },
            {
                "value": "fa fa-volume-up",
                "label": "<i class=\"fa fa-volume-up\"> Volume-up"
            },
            {
                "value": "fa fa-warning",
                "label": "<i class=\"fa fa-warning\"> Warning"
            },
            {
                "value": "fa fa-wheelchair",
                "label": "<i class=\"fa fa-wheelchair\"> Wheelchair"
            },
            {
                "value": "fa fa-wifi",
                "label": "<i class=\"fa fa-wifi\"> Wifi"
            },
            {
                "value": "fa fa-wrench",
                "label": "<i class=\"fa fa-wrench\"> Wrench"
            }
        ]
    })
    .constant('icons2', {
        icons: [
            { "value": "fas fa-ad", "label": "<i class=\"fas fa-ad\"> Ad" },
            { "value": "fas fa-address-book", "label": "<i class=\"fas fa-address-book\"> Address-Book" },
            { "value": "fas fa-address-card", "label": "<i class=\"fas fa-address-card\"> Address-Card" },
            { "value": "fas fa-adjust", "label": "<i class=\"fas fa-adjust\"> Adjust" },
            { "value": "fas fa-air-freshener", "label": "<i class=\"fas fa-air-freshener\"> Air-Freshener" },
            { "value": "fas fa-align-center", "label": "<i class=\"fas fa-align-center\"> Align-Center" },
            { "value": "fas fa-align-justify", "label": "<i class=\"fas fa-align-justify\"> Align-Justify" },
            { "value": "fas fa-align-left", "label": "<i class=\"fas fa-align-left\"> Align-Left" },
            { "value": "fas fa-align-right", "label": "<i class=\"fas fa-align-right\"> Align-Right" },
            { "value": "fas fa-allergies", "label": "<i class=\"fas fa-allergies\"> Allergies" },
            { "value": "fas fa-ambulance", "label": "<i class=\"fas fa-ambulance\"> Ambulance" },
            {
                "value": "fas fa-american-sign-language-interpreting",
                "label": "<i class=\"fas fa-american-sign-language-interpreting\"> American-Sign-Language-Interpreting"
            },
            { "value": "fas fa-anchor", "label": "<i class=\"fas fa-anchor\"> Anchor" },
            { "value": "fas fa-angle-double-down", "label": "<i class=\"fas fa-angle-double-down\"> Angle-Double-Down" },
            { "value": "fas fa-angle-double-left", "label": "<i class=\"fas fa-angle-double-left\"> Angle-Double-Left" },
            {
                "value": "fas fa-angle-double-right",
                "label": "<i class=\"fas fa-angle-double-right\"> Angle-Double-Right"
            },
            { "value": "fas fa-angle-double-up", "label": "<i class=\"fas fa-angle-double-up\"> Angle-Double-Up" },
            { "value": "fas fa-angle-down", "label": "<i class=\"fas fa-angle-down\"> Angle-Down" },
            { "value": "fas fa-angle-left", "label": "<i class=\"fas fa-angle-left\"> Angle-Left" },
            { "value": "fas fa-angle-right", "label": "<i class=\"fas fa-angle-right\"> Angle-Right" },
            { "value": "fas fa-angle-up", "label": "<i class=\"fas fa-angle-up\"> Angle-Up" },
            { "value": "fas fa-angry", "label": "<i class=\"fas fa-angry\"> Angry" },
            { "value": "fas fa-ankh", "label": "<i class=\"fas fa-ankh\"> Ankh" },
            { "value": "fas fa-apple-alt", "label": "<i class=\"fas fa-apple-alt\"> Apple-Alt" },
            { "value": "fas fa-archive", "label": "<i class=\"fas fa-archive\"> Archive" },
            { "value": "fas fa-archway", "label": "<i class=\"fas fa-archway\"> Archway" },
            {
                "value": "fas fa-arrow-alt-circle-down",
                "label": "<i class=\"fas fa-arrow-alt-circle-down\"> Arrow-Alt-Circle-Down"
            },
            {
                "value": "fas fa-arrow-alt-circle-left",
                "label": "<i class=\"fas fa-arrow-alt-circle-left\"> Arrow-Alt-Circle-Left"
            },
            {
                "value": "fas fa-arrow-alt-circle-right",
                "label": "<i class=\"fas fa-arrow-alt-circle-right\"> Arrow-Alt-Circle-Right"
            },
            {
                "value": "fas fa-arrow-alt-circle-up",
                "label": "<i class=\"fas fa-arrow-alt-circle-up\"> Arrow-Alt-Circle-Up"
            },
            { "value": "fas fa-arrow-circle-down", "label": "<i class=\"fas fa-arrow-circle-down\"> Arrow-Circle-Down" },
            { "value": "fas fa-arrow-circle-left", "label": "<i class=\"fas fa-arrow-circle-left\"> Arrow-Circle-Left" },
            {
                "value": "fas fa-arrow-circle-right",
                "label": "<i class=\"fas fa-arrow-circle-right\"> Arrow-Circle-Right"
            },
            { "value": "fas fa-arrow-circle-up", "label": "<i class=\"fas fa-arrow-circle-up\"> Arrow-Circle-Up" },
            { "value": "fas fa-arrow-down", "label": "<i class=\"fas fa-arrow-down\"> Arrow-Down" },
            { "value": "fas fa-arrow-left", "label": "<i class=\"fas fa-arrow-left\"> Arrow-Left" },
            { "value": "fas fa-arrow-right", "label": "<i class=\"fas fa-arrow-right\"> Arrow-Right" },
            { "value": "fas fa-arrow-up", "label": "<i class=\"fas fa-arrow-up\"> Arrow-Up" },
            { "value": "fas fa-arrows-alt", "label": "<i class=\"fas fa-arrows-alt\"> Arrows-Alt" },
            { "value": "fas fa-arrows-alt-h", "label": "<i class=\"fas fa-arrows-alt-h\"> Arrows-Alt-H" },
            { "value": "fas fa-arrows-alt-v", "label": "<i class=\"fas fa-arrows-alt-v\"> Arrows-Alt-V" },
            {
                "value": "fas fa-assistive-listening-systems",
                "label": "<i class=\"fas fa-assistive-listening-systems\"> Assistive-Listening-Systems"
            },
            { "value": "fas fa-asterisk", "label": "<i class=\"fas fa-asterisk\"> Asterisk" },
            { "value": "fas fa-at", "label": "<i class=\"fas fa-at\"> At" },
            { "value": "fas fa-atlas", "label": "<i class=\"fas fa-atlas\"> Atlas" },
            { "value": "fas fa-atom", "label": "<i class=\"fas fa-atom\"> Atom" },
            { "value": "fas fa-audio-description", "label": "<i class=\"fas fa-audio-description\"> Audio-Description" },
            { "value": "fas fa-award", "label": "<i class=\"fas fa-award\"> Award" },
            // { "value": "fas fa-baby", "label": "<i class=\"fas fa-baby\"> Baby" },
            // { "value": "fas fa-baby-carriage", "label": "<i class=\"fas fa-baby-carriage\"> Baby-Carriage" },
            { "value": "fas fa-backspace", "label": "<i class=\"fas fa-backspace\"> Backspace" },
            { "value": "fas fa-backward", "label": "<i class=\"fas fa-backward\"> Backward" },
            { "value": "fas fa-bacon", "label": "<i class=\"fas fa-bacon\"> Bacon" },
            { "value": "fas fa-balance-scale", "label": "<i class=\"fas fa-balance-scale\"> Balance-Scale" },
            { "value": "fas fa-ban", "label": "<i class=\"fas fa-ban\"> Ban" },
            { "value": "fas fa-band-aid", "label": "<i class=\"fas fa-band-aid\"> Band-Aid" },
            { "value": "fas fa-barcode", "label": "<i class=\"fas fa-barcode\"> Barcode" },
            { "value": "fas fa-bars", "label": "<i class=\"fas fa-bars\"> Bars" },
            { "value": "fas fa-baseball-ball", "label": "<i class=\"fas fa-baseball-ball\"> Baseball-Ball" },
            { "value": "fas fa-basketball-ball", "label": "<i class=\"fas fa-basketball-ball\"> Basketball-Ball" },
            { "value": "fas fa-bath", "label": "<i class=\"fas fa-bath\"> Bath" },
            { "value": "fas fa-battery-empty", "label": "<i class=\"fas fa-battery-empty\"> Battery-Empty" },
            { "value": "fas fa-battery-full", "label": "<i class=\"fas fa-battery-full\"> Battery-Full" },
            { "value": "fas fa-battery-half", "label": "<i class=\"fas fa-battery-half\"> Battery-Half" },
            { "value": "fas fa-battery-quarter", "label": "<i class=\"fas fa-battery-quarter\"> Battery-Quarter" },
            {
                "value": "fas fa-battery-three-quarters",
                "label": "<i class=\"fas fa-battery-three-quarters\"> Battery-Three-Quarters"
            },
            { "value": "fas fa-bed", "label": "<i class=\"fas fa-bed\"> Bed" },
            { "value": "fas fa-beer", "label": "<i class=\"fas fa-beer\"> Beer" },
            { "value": "fas fa-bell", "label": "<i class=\"fas fa-bell\"> Bell" },
            { "value": "fas fa-bell-slash", "label": "<i class=\"fas fa-bell-slash\"> Bell-Slash" },
            { "value": "fas fa-bezier-curve", "label": "<i class=\"fas fa-bezier-curve\"> Bezier-Curve" },
            { "value": "fas fa-bible", "label": "<i class=\"fas fa-bible\"> Bible" },
            { "value": "fas fa-bicycle", "label": "<i class=\"fas fa-bicycle\"> Bicycle" },
            { "value": "fas fa-binoculars", "label": "<i class=\"fas fa-binoculars\"> Binoculars" },
            { "value": "fas fa-biohazard", "label": "<i class=\"fas fa-biohazard\"> Biohazard" },
            { "value": "fas fa-birthday-cake", "label": "<i class=\"fas fa-birthday-cake\"> Birthday-Cake" },
            { "value": "fas fa-blender", "label": "<i class=\"fas fa-blender\"> Blender" },
            { "value": "fas fa-blender-phone", "label": "<i class=\"fas fa-blender-phone\"> Blender-Phone" },
            { "value": "fas fa-blind", "label": "<i class=\"fas fa-blind\"> Blind" },
            // { "value": "fas fa-blog", "label": "<i class=\"fas fa-blog\"> Blog" },
            { "value": "fas fa-bold", "label": "<i class=\"fas fa-bold\"> Bold" },
            { "value": "fas fa-bolt", "label": "<i class=\"fas fa-bolt\"> Bolt" },
            { "value": "fas fa-bomb", "label": "<i class=\"fas fa-bomb\"> Bomb" },
            { "value": "fas fa-bone", "label": "<i class=\"fas fa-bone\"> Bone" },
            { "value": "fas fa-bong", "label": "<i class=\"fas fa-bong\"> Bong" },
            { "value": "fas fa-book", "label": "<i class=\"fas fa-book\"> Book" },
            { "value": "fas fa-book-dead", "label": "<i class=\"fas fa-book-dead\"> Book-Dead" },
            // { "value": "fas fa-book-medical", "label": "<i class=\"fas fa-book-medical\"> Book-Medical" },
            { "value": "fas fa-book-open", "label": "<i class=\"fas fa-book-open\"> Book-Open" },
            { "value": "fas fa-book-reader", "label": "<i class=\"fas fa-book-reader\"> Book-Reader" },
            { "value": "fas fa-bookmark", "label": "<i class=\"fas fa-bookmark\"> Bookmark" },
            { "value": "fas fa-bowling-ball", "label": "<i class=\"fas fa-bowling-ball\"> Bowling-Ball" },
            { "value": "fas fa-box", "label": "<i class=\"fas fa-box\"> Box" },
            { "value": "fas fa-box-open", "label": "<i class=\"fas fa-box-open\"> Box-Open" },
            { "value": "fas fa-boxes", "label": "<i class=\"fas fa-boxes\"> Boxes" },
            { "value": "fas fa-braille", "label": "<i class=\"fas fa-braille\"> Braille" },
            { "value": "fas fa-brain", "label": "<i class=\"fas fa-brain\"> Brain" },
            // { "value": "fas fa-bread-slice", "label": "<i class=\"fas fa-bread-slice\"> Bread-Slice" },
            { "value": "fas fa-briefcase", "label": "<i class=\"fas fa-briefcase\"> Briefcase" },
            { "value": "fas fa-briefcase-medical", "label": "<i class=\"fas fa-briefcase-medical\"> Briefcase-Medical" },
            { "value": "fas fa-broadcast-tower", "label": "<i class=\"fas fa-broadcast-tower\"> Broadcast-Tower" },
            { "value": "fas fa-broom", "label": "<i class=\"fas fa-broom\"> Broom" },
            { "value": "fas fa-brush", "label": "<i class=\"fas fa-brush\"> Brush" },
            { "value": "fas fa-bug", "label": "<i class=\"fas fa-bug\"> Bug" },
            { "value": "fas fa-building", "label": "<i class=\"fas fa-building\"> Building" },
            { "value": "fas fa-bullhorn", "label": "<i class=\"fas fa-bullhorn\"> Bullhorn" },
            { "value": "fas fa-bullseye", "label": "<i class=\"fas fa-bullseye\"> Bullseye" },
            { "value": "fas fa-burn", "label": "<i class=\"fas fa-burn\"> Burn" },
            { "value": "fas fa-bus", "label": "<i class=\"fas fa-bus\"> Bus" },
            { "value": "fas fa-bus-alt", "label": "<i class=\"fas fa-bus-alt\"> Bus-Alt" },
            { "value": "fas fa-business-time", "label": "<i class=\"fas fa-business-time\"> Business-Time" },
            { "value": "fas fa-calculator", "label": "<i class=\"fas fa-calculator\"> Calculator" },
            { "value": "fas fa-calendar", "label": "<i class=\"fas fa-calendar\"> Calendar" },
            { "value": "fas fa-calendar-alt", "label": "<i class=\"fas fa-calendar-alt\"> Calendar-Alt" },
            { "value": "fas fa-calendar-check", "label": "<i class=\"fas fa-calendar-check\"> Calendar-Check" },
            // { "value": "fas fa-calendar-day", "label": "<i class=\"fas fa-calendar-day\"> Calendar-Day" },
            { "value": "fas fa-calendar-minus", "label": "<i class=\"fas fa-calendar-minus\"> Calendar-Minus" },
            { "value": "fas fa-calendar-plus", "label": "<i class=\"fas fa-calendar-plus\"> Calendar-Plus" },
            { "value": "fas fa-calendar-times", "label": "<i class=\"fas fa-calendar-times\"> Calendar-Times" },
            { "value": "fas fa-calendar-week", "label": "<i class=\"fas fa-calendar-week\"> Calendar-Week" },
            { "value": "fas fa-camera", "label": "<i class=\"fas fa-camera\"> Camera" },
            { "value": "fas fa-camera-retro", "label": "<i class=\"fas fa-camera-retro\"> Camera-Retro" },
            { "value": "fas fa-campground", "label": "<i class=\"fas fa-campground\"> Campground" },
            { "value": "fas fa-candy-cane", "label": "<i class=\"fas fa-candy-cane\"> Candy-Cane" },
            { "value": "fas fa-cannabis", "label": "<i class=\"fas fa-cannabis\"> Cannabis" },
            { "value": "fas fa-capsules", "label": "<i class=\"fas fa-capsules\"> Capsules" },
            { "value": "fas fa-car", "label": "<i class=\"fas fa-car\"> Car" },
            { "value": "fas fa-car-alt", "label": "<i class=\"fas fa-car-alt\"> Car-Alt" },
            { "value": "fas fa-car-battery", "label": "<i class=\"fas fa-car-battery\"> Car-Battery" },
            { "value": "fas fa-car-crash", "label": "<i class=\"fas fa-car-crash\"> Car-Crash" },
            { "value": "fas fa-car-side", "label": "<i class=\"fas fa-car-side\"> Car-Side" },
            { "value": "fas fa-caret-down", "label": "<i class=\"fas fa-caret-down\"> Caret-Down" },
            { "value": "fas fa-caret-left", "label": "<i class=\"fas fa-caret-left\"> Caret-Left" },
            { "value": "fas fa-caret-right", "label": "<i class=\"fas fa-caret-right\"> Caret-Right" },
            { "value": "fas fa-caret-square-down", "label": "<i class=\"fas fa-caret-square-down\"> Caret-Square-Down" },
            { "value": "fas fa-caret-square-left", "label": "<i class=\"fas fa-caret-square-left\"> Caret-Square-Left" },
            {
                "value": "fas fa-caret-square-right",
                "label": "<i class=\"fas fa-caret-square-right\"> Caret-Square-Right"
            },
            { "value": "fas fa-caret-square-up", "label": "<i class=\"fas fa-caret-square-up\"> Caret-Square-Up" },
            { "value": "fas fa-caret-up", "label": "<i class=\"fas fa-caret-up\"> Caret-Up" },
            { "value": "fas fa-carrot", "label": "<i class=\"fas fa-carrot\"> Carrot" },
            { "value": "fas fa-cart-arrow-down", "label": "<i class=\"fas fa-cart-arrow-down\"> Cart-Arrow-Down" },
            { "value": "fas fa-cart-plus", "label": "<i class=\"fas fa-cart-plus\"> Cart-Plus" },
            { "value": "fas fa-cash-register", "label": "<i class=\"fas fa-cash-register\"> Cash-Register" },
            { "value": "fas fa-cat", "label": "<i class=\"fas fa-cat\"> Cat" },
            { "value": "fas fa-certificate", "label": "<i class=\"fas fa-certificate\"> Certificate" },
            { "value": "fas fa-chair", "label": "<i class=\"fas fa-chair\"> Chair" },
            { "value": "fas fa-chalkboard", "label": "<i class=\"fas fa-chalkboard\"> Chalkboard" },
            {
                "value": "fas fa-chalkboard-teacher",
                "label": "<i class=\"fas fa-chalkboard-teacher\"> Chalkboard-Teacher"
            },
            { "value": "fas fa-charging-station", "label": "<i class=\"fas fa-charging-station\"> Charging-Station" },
            { "value": "fas fa-chart-area", "label": "<i class=\"fas fa-chart-area\"> Chart-Area" },
            { "value": "fas fa-chart-bar", "label": "<i class=\"fas fa-chart-bar\"> Chart-Bar" },
            { "value": "fas fa-chart-line", "label": "<i class=\"fas fa-chart-line\"> Chart-Line" },
            { "value": "fas fa-chart-pie", "label": "<i class=\"fas fa-chart-pie\"> Chart-Pie" },
            { "value": "fas fa-check", "label": "<i class=\"fas fa-check\"> Check" },
            { "value": "fas fa-check-circle", "label": "<i class=\"fas fa-check-circle\"> Check-Circle" },
            { "value": "fas fa-check-double", "label": "<i class=\"fas fa-check-double\"> Check-Double" },
            { "value": "fas fa-check-square", "label": "<i class=\"fas fa-check-square\"> Check-Square" },
            { "value": "fas fa-cheese", "label": "<i class=\"fas fa-cheese\"> Cheese" },
            { "value": "fas fa-chess", "label": "<i class=\"fas fa-chess\"> Chess" },
            { "value": "fas fa-chess-bishop", "label": "<i class=\"fas fa-chess-bishop\"> Chess-Bishop" },
            { "value": "fas fa-chess-board", "label": "<i class=\"fas fa-chess-board\"> Chess-Board" },
            { "value": "fas fa-chess-king", "label": "<i class=\"fas fa-chess-king\"> Chess-King" },
            { "value": "fas fa-chess-knight", "label": "<i class=\"fas fa-chess-knight\"> Chess-Knight" },
            { "value": "fas fa-chess-pawn", "label": "<i class=\"fas fa-chess-pawn\"> Chess-Pawn" },
            { "value": "fas fa-chess-queen", "label": "<i class=\"fas fa-chess-queen\"> Chess-Queen" },
            { "value": "fas fa-chess-rook", "label": "<i class=\"fas fa-chess-rook\"> Chess-Rook" },
            {
                "value": "fas fa-chevron-circle-down",
                "label": "<i class=\"fas fa-chevron-circle-down\"> Chevron-Circle-Down"
            },
            {
                "value": "fas fa-chevron-circle-left",
                "label": "<i class=\"fas fa-chevron-circle-left\"> Chevron-Circle-Left"
            },
            {
                "value": "fas fa-chevron-circle-right",
                "label": "<i class=\"fas fa-chevron-circle-right\"> Chevron-Circle-Right"
            },
            { "value": "fas fa-chevron-circle-up", "label": "<i class=\"fas fa-chevron-circle-up\"> Chevron-Circle-Up" },
            { "value": "fas fa-chevron-down", "label": "<i class=\"fas fa-chevron-down\"> Chevron-Down" },
            { "value": "fas fa-chevron-left", "label": "<i class=\"fas fa-chevron-left\"> Chevron-Left" },
            { "value": "fas fa-chevron-right", "label": "<i class=\"fas fa-chevron-right\"> Chevron-Right" },
            { "value": "fas fa-chevron-up", "label": "<i class=\"fas fa-chevron-up\"> Chevron-Up" },
            { "value": "fas fa-child", "label": "<i class=\"fas fa-child\"> Child" },
            { "value": "fas fa-church", "label": "<i class=\"fas fa-church\"> Church" },
            { "value": "fas fa-circle", "label": "<i class=\"fas fa-circle\"> Circle" },
            { "value": "fas fa-circle-notch", "label": "<i class=\"fas fa-circle-notch\"> Circle-Notch" },
            { "value": "fas fa-city", "label": "<i class=\"fas fa-city\"> City" },
            { "value": "fas fa-clinic-medical", "label": "<i class=\"fas fa-clinic-medical\"> Clinic-Medical" },
            { "value": "fas fa-clipboard", "label": "<i class=\"fas fa-clipboard\"> Clipboard" },
            { "value": "fas fa-clipboard-check", "label": "<i class=\"fas fa-clipboard-check\"> Clipboard-Check" },
            { "value": "fas fa-clipboard-list", "label": "<i class=\"fas fa-clipboard-list\"> Clipboard-List" },
            { "value": "fas fa-clock", "label": "<i class=\"fas fa-clock\"> Clock" },
            { "value": "fas fa-clone", "label": "<i class=\"fas fa-clone\"> Clone" },
            { "value": "fas fa-closed-captioning", "label": "<i class=\"fas fa-closed-captioning\"> Closed-Captioning" },
            { "value": "fas fa-cloud", "label": "<i class=\"fas fa-cloud\"> Cloud" },
            {
                "value": "fas fa-cloud-download-alt",
                "label": "<i class=\"fas fa-cloud-download-alt\"> Cloud-Download-Alt"
            },
            { "value": "fas fa-cloud-meatball", "label": "<i class=\"fas fa-cloud-meatball\"> Cloud-Meatball" },
            { "value": "fas fa-cloud-moon", "label": "<i class=\"fas fa-cloud-moon\"> Cloud-Moon" },
            { "value": "fas fa-cloud-moon-rain", "label": "<i class=\"fas fa-cloud-moon-rain\"> Cloud-Moon-Rain" },
            { "value": "fas fa-cloud-rain", "label": "<i class=\"fas fa-cloud-rain\"> Cloud-Rain" },
            {
                "value": "fas fa-cloud-showers-heavy",
                "label": "<i class=\"fas fa-cloud-showers-heavy\"> Cloud-Showers-Heavy"
            },
            { "value": "fas fa-cloud-sun", "label": "<i class=\"fas fa-cloud-sun\"> Cloud-Sun" },
            { "value": "fas fa-cloud-sun-rain", "label": "<i class=\"fas fa-cloud-sun-rain\"> Cloud-Sun-Rain" },
            { "value": "fas fa-cloud-upload-alt", "label": "<i class=\"fas fa-cloud-upload-alt\"> Cloud-Upload-Alt" },
            { "value": "fas fa-cocktail", "label": "<i class=\"fas fa-cocktail\"> Cocktail" },
            { "value": "fas fa-code", "label": "<i class=\"fas fa-code\"> Code" },
            { "value": "fas fa-code-branch", "label": "<i class=\"fas fa-code-branch\"> Code-Branch" },
            { "value": "fas fa-coffee", "label": "<i class=\"fas fa-coffee\"> Coffee" },
            { "value": "fas fa-cog", "label": "<i class=\"fas fa-cog\"> Cog" },
            { "value": "fas fa-cogs", "label": "<i class=\"fas fa-cogs\"> Cogs" },
            { "value": "fas fa-coins", "label": "<i class=\"fas fa-coins\"> Coins" },
            { "value": "fas fa-columns", "label": "<i class=\"fas fa-columns\"> Columns" },
            { "value": "fas fa-comment", "label": "<i class=\"fas fa-comment\"> Comment" },
            { "value": "fas fa-comment-alt", "label": "<i class=\"fas fa-comment-alt\"> Comment-Alt" },
            { "value": "fas fa-comment-dollar", "label": "<i class=\"fas fa-comment-dollar\"> Comment-Dollar" },
            { "value": "fas fa-comment-dots", "label": "<i class=\"fas fa-comment-dots\"> Comment-Dots" },
            { "value": "fas fa-comment-medical", "label": "<i class=\"fas fa-comment-medical\"> Comment-Medical" },
            { "value": "fas fa-comment-slash", "label": "<i class=\"fas fa-comment-slash\"> Comment-Slash" },
            { "value": "fas fa-comments", "label": "<i class=\"fas fa-comments\"> Comments" },
            { "value": "fas fa-comments-dollar", "label": "<i class=\"fas fa-comments-dollar\"> Comments-Dollar" },
            { "value": "fas fa-compact-disc", "label": "<i class=\"fas fa-compact-disc\"> Compact-Disc" },
            { "value": "fas fa-compass", "label": "<i class=\"fas fa-compass\"> Compass" },
            { "value": "fas fa-compress", "label": "<i class=\"fas fa-compress\"> Compress" },
            {
                "value": "fas fa-compress-arrows-alt",
                "label": "<i class=\"fas fa-compress-arrows-alt\"> Compress-Arrows-Alt"
            },
            { "value": "fas fa-concierge-bell", "label": "<i class=\"fas fa-concierge-bell\"> Concierge-Bell" },
            { "value": "fas fa-cookie", "label": "<i class=\"fas fa-cookie\"> Cookie" },
            { "value": "fas fa-cookie-bite", "label": "<i class=\"fas fa-cookie-bite\"> Cookie-Bite" },
            { "value": "fas fa-copy", "label": "<i class=\"fas fa-copy\"> Copy" },
            { "value": "fas fa-copyright", "label": "<i class=\"fas fa-copyright\"> Copyright" },
            { "value": "fas fa-couch", "label": "<i class=\"fas fa-couch\"> Couch" },
            { "value": "fas fa-credit-card", "label": "<i class=\"fas fa-credit-card\"> Credit-Card" },
            { "value": "fas fa-crop", "label": "<i class=\"fas fa-crop\"> Crop" },
            { "value": "fas fa-crop-alt", "label": "<i class=\"fas fa-crop-alt\"> Crop-Alt" },
            { "value": "fas fa-cross", "label": "<i class=\"fas fa-cross\"> Cross" },
            { "value": "fas fa-crosshairs", "label": "<i class=\"fas fa-crosshairs\"> Crosshairs" },
            { "value": "fas fa-crow", "label": "<i class=\"fas fa-crow\"> Crow" },
            { "value": "fas fa-crown", "label": "<i class=\"fas fa-crown\"> Crown" },
            { "value": "fas fa-crutch", "label": "<i class=\"fas fa-crutch\"> Crutch" },
            { "value": "fas fa-cube", "label": "<i class=\"fas fa-cube\"> Cube" },
            { "value": "fas fa-cubes", "label": "<i class=\"fas fa-cubes\"> Cubes" },
            { "value": "fas fa-cut", "label": "<i class=\"fas fa-cut\"> Cut" },
            { "value": "fas fa-database", "label": "<i class=\"fas fa-database\"> Database" },
            { "value": "fas fa-deaf", "label": "<i class=\"fas fa-deaf\"> Deaf" },
            { "value": "fas fa-democrat", "label": "<i class=\"fas fa-democrat\"> Democrat" },
            { "value": "fas fa-desktop", "label": "<i class=\"fas fa-desktop\"> Desktop" },
            { "value": "fas fa-dharmachakra", "label": "<i class=\"fas fa-dharmachakra\"> Dharmachakra" },
            { "value": "fas fa-diagnoses", "label": "<i class=\"fas fa-diagnoses\"> Diagnoses" },
            { "value": "fas fa-dice", "label": "<i class=\"fas fa-dice\"> Dice" },
            { "value": "fas fa-dice-d20", "label": "<i class=\"fas fa-dice-d20\"> Dice-D20" },
            { "value": "fas fa-dice-d6", "label": "<i class=\"fas fa-dice-d6\"> Dice-D6" },
            { "value": "fas fa-dice-five", "label": "<i class=\"fas fa-dice-five\"> Dice-Five" },
            { "value": "fas fa-dice-four", "label": "<i class=\"fas fa-dice-four\"> Dice-Four" },
            { "value": "fas fa-dice-one", "label": "<i class=\"fas fa-dice-one\"> Dice-One" },
            { "value": "fas fa-dice-six", "label": "<i class=\"fas fa-dice-six\"> Dice-Six" },
            { "value": "fas fa-dice-three", "label": "<i class=\"fas fa-dice-three\"> Dice-Three" },
            { "value": "fas fa-dice-two", "label": "<i class=\"fas fa-dice-two\"> Dice-Two" },
            {
                "value": "fas fa-digital-tachograph",
                "label": "<i class=\"fas fa-digital-tachograph\"> Digital-Tachograph"
            },
            { "value": "fas fa-directions", "label": "<i class=\"fas fa-directions\"> Directions" },
            { "value": "fas fa-divide", "label": "<i class=\"fas fa-divide\"> Divide" },
            { "value": "fas fa-dizzy", "label": "<i class=\"fas fa-dizzy\"> Dizzy" },
            { "value": "fas fa-dna", "label": "<i class=\"fas fa-dna\"> Dna" },
            { "value": "fas fa-dog", "label": "<i class=\"fas fa-dog\"> Dog" },
            { "value": "fas fa-dollar-sign", "label": "<i class=\"fas fa-dollar-sign\"> Dollar-Sign" },
            { "value": "fas fa-dolly", "label": "<i class=\"fas fa-dolly\"> Dolly" },
            { "value": "fas fa-dolly-flatbed", "label": "<i class=\"fas fa-dolly-flatbed\"> Dolly-Flatbed" },
            { "value": "fas fa-donate", "label": "<i class=\"fas fa-donate\"> Donate" },
            { "value": "fas fa-door-closed", "label": "<i class=\"fas fa-door-closed\"> Door-Closed" },
            { "value": "fas fa-door-open", "label": "<i class=\"fas fa-door-open\"> Door-Open" },
            { "value": "fas fa-dot-circle", "label": "<i class=\"fas fa-dot-circle\"> Dot-Circle" },
            { "value": "fas fa-dove", "label": "<i class=\"fas fa-dove\"> Dove" },
            { "value": "fas fa-download", "label": "<i class=\"fas fa-download\"> Download" },
            { "value": "fas fa-drafting-compass", "label": "<i class=\"fas fa-drafting-compass\"> Drafting-Compass" },
            { "value": "fas fa-dragon", "label": "<i class=\"fas fa-dragon\"> Dragon" },
            { "value": "fas fa-draw-polygon", "label": "<i class=\"fas fa-draw-polygon\"> Draw-Polygon" },
            { "value": "fas fa-drum", "label": "<i class=\"fas fa-drum\"> Drum" },
            { "value": "fas fa-drum-steelpan", "label": "<i class=\"fas fa-drum-steelpan\"> Drum-Steelpan" },
            { "value": "fas fa-drumstick-bite", "label": "<i class=\"fas fa-drumstick-bite\"> Drumstick-Bite" },
            { "value": "fas fa-dumbbell", "label": "<i class=\"fas fa-dumbbell\"> Dumbbell" },
            { "value": "fas fa-dumpster", "label": "<i class=\"fas fa-dumpster\"> Dumpster" },
            { "value": "fas fa-dumpster-fire", "label": "<i class=\"fas fa-dumpster-fire\"> Dumpster-Fire" },
            { "value": "fas fa-dungeon", "label": "<i class=\"fas fa-dungeon\"> Dungeon" },
            { "value": "fas fa-edit", "label": "<i class=\"fas fa-edit\"> Edit" },
            { "value": "fas fa-egg", "label": "<i class=\"fas fa-egg\"> Egg" },
            { "value": "fas fa-eject", "label": "<i class=\"fas fa-eject\"> Eject" },
            { "value": "fas fa-ellipsis-h", "label": "<i class=\"fas fa-ellipsis-h\"> Ellipsis-H" },
            { "value": "fas fa-ellipsis-v", "label": "<i class=\"fas fa-ellipsis-v\"> Ellipsis-V" },
            { "value": "fas fa-envelope", "label": "<i class=\"fas fa-envelope\"> Envelope" },
            { "value": "fas fa-envelope-open", "label": "<i class=\"fas fa-envelope-open\"> Envelope-Open" },
            {
                "value": "fas fa-envelope-open-text",
                "label": "<i class=\"fas fa-envelope-open-text\"> Envelope-Open-Text"
            },
            { "value": "fas fa-envelope-square", "label": "<i class=\"fas fa-envelope-square\"> Envelope-Square" },
            { "value": "fas fa-equals", "label": "<i class=\"fas fa-equals\"> Equals" },
            { "value": "fas fa-eraser", "label": "<i class=\"fas fa-eraser\"> Eraser" },
            { "value": "fas fa-ethernet", "label": "<i class=\"fas fa-ethernet\"> Ethernet" },
            { "value": "fas fa-euro-sign", "label": "<i class=\"fas fa-euro-sign\"> Euro-Sign" },
            { "value": "fas fa-exchange-alt", "label": "<i class=\"fas fa-exchange-alt\"> Exchange-Alt" },
            { "value": "fas fa-exclamation", "label": "<i class=\"fas fa-exclamation\"> Exclamation" },
            {
                "value": "fas fa-exclamation-circle",
                "label": "<i class=\"fas fa-exclamation-circle\"> Exclamation-Circle"
            },
            {
                "value": "fas fa-exclamation-triangle",
                "label": "<i class=\"fas fa-exclamation-triangle\"> Exclamation-Triangle"
            },
            { "value": "fas fa-expand", "label": "<i class=\"fas fa-expand\"> Expand" },
            { "value": "fas fa-expand-arrows-alt", "label": "<i class=\"fas fa-expand-arrows-alt\"> Expand-Arrows-Alt" },
            { "value": "fas fa-external-link-alt", "label": "<i class=\"fas fa-external-link-alt\"> External-Link-Alt" },
            {
                "value": "fas fa-external-link-square-alt",
                "label": "<i class=\"fas fa-external-link-square-alt\"> External-Link-Square-Alt"
            },
            { "value": "fas fa-eye", "label": "<i class=\"fas fa-eye\"> Eye" },
            { "value": "fas fa-eye-dropper", "label": "<i class=\"fas fa-eye-dropper\"> Eye-Dropper" },
            { "value": "fas fa-eye-slash", "label": "<i class=\"fas fa-eye-slash\"> Eye-Slash" },
            { "value": "fas fa-fast-backward", "label": "<i class=\"fas fa-fast-backward\"> Fast-Backward" },
            { "value": "fas fa-fast-forward", "label": "<i class=\"fas fa-fast-forward\"> Fast-Forward" },
            { "value": "fas fa-fax", "label": "<i class=\"fas fa-fax\"> Fax" },
            { "value": "fas fa-feather", "label": "<i class=\"fas fa-feather\"> Feather" },
            { "value": "fas fa-feather-alt", "label": "<i class=\"fas fa-feather-alt\"> Feather-Alt" },
            { "value": "fas fa-female", "label": "<i class=\"fas fa-female\"> Female" },
            { "value": "fas fa-fighter-jet", "label": "<i class=\"fas fa-fighter-jet\"> Fighter-Jet" },
            { "value": "fas fa-file", "label": "<i class=\"fas fa-file\"> File" },
            { "value": "fas fa-file-alt", "label": "<i class=\"fas fa-file-alt\"> File-Alt" },
            { "value": "fas fa-file-archive", "label": "<i class=\"fas fa-file-archive\"> File-Archive" },
            { "value": "fas fa-file-audio", "label": "<i class=\"fas fa-file-audio\"> File-Audio" },
            { "value": "fas fa-file-code", "label": "<i class=\"fas fa-file-code\"> File-Code" },
            { "value": "fas fa-file-contract", "label": "<i class=\"fas fa-file-contract\"> File-Contract" },
            { "value": "fas fa-file-csv", "label": "<i class=\"fas fa-file-csv\"> File-Csv" },
            { "value": "fas fa-file-download", "label": "<i class=\"fas fa-file-download\"> File-Download" },
            { "value": "fas fa-file-excel", "label": "<i class=\"fas fa-file-excel\"> File-Excel" },
            { "value": "fas fa-file-export", "label": "<i class=\"fas fa-file-export\"> File-Export" },
            { "value": "fas fa-file-image", "label": "<i class=\"fas fa-file-image\"> File-Image" },
            { "value": "fas fa-file-import", "label": "<i class=\"fas fa-file-import\"> File-Import" },
            { "value": "fas fa-file-invoice", "label": "<i class=\"fas fa-file-invoice\"> File-Invoice" },
            {
                "value": "fas fa-file-invoice-dollar",
                "label": "<i class=\"fas fa-file-invoice-dollar\"> File-Invoice-Dollar"
            },
            { "value": "fas fa-file-medical", "label": "<i class=\"fas fa-file-medical\"> File-Medical" },
            { "value": "fas fa-file-medical-alt", "label": "<i class=\"fas fa-file-medical-alt\"> File-Medical-Alt" },
            { "value": "fas fa-file-pdf", "label": "<i class=\"fas fa-file-pdf\"> File-Pdf" },
            { "value": "fas fa-file-powerpoint", "label": "<i class=\"fas fa-file-powerpoint\"> File-Powerpoint" },
            { "value": "fas fa-file-prescription", "label": "<i class=\"fas fa-file-prescription\"> File-Prescription" },
            { "value": "fas fa-file-signature", "label": "<i class=\"fas fa-file-signature\"> File-Signature" },
            { "value": "fas fa-file-upload", "label": "<i class=\"fas fa-file-upload\"> File-Upload" },
            { "value": "fas fa-file-video", "label": "<i class=\"fas fa-file-video\"> File-Video" },
            { "value": "fas fa-file-word", "label": "<i class=\"fas fa-file-word\"> File-Word" },
            { "value": "fas fa-fill", "label": "<i class=\"fas fa-fill\"> Fill" },
            { "value": "fas fa-fill-drip", "label": "<i class=\"fas fa-fill-drip\"> Fill-Drip" },
            { "value": "fas fa-film", "label": "<i class=\"fas fa-film\"> Film" },
            { "value": "fas fa-filter", "label": "<i class=\"fas fa-filter\"> Filter" },
            { "value": "fas fa-fingerprint", "label": "<i class=\"fas fa-fingerprint\"> Fingerprint" },
            { "value": "fas fa-fire", "label": "<i class=\"fas fa-fire\"> Fire" },
            { "value": "fas fa-fire-alt", "label": "<i class=\"fas fa-fire-alt\"> Fire-Alt" },
            { "value": "fas fa-fire-extinguisher", "label": "<i class=\"fas fa-fire-extinguisher\"> Fire-Extinguisher" },
            { "value": "fas fa-first-aid", "label": "<i class=\"fas fa-first-aid\"> First-Aid" },
            { "value": "fas fa-fish", "label": "<i class=\"fas fa-fish\"> Fish" },
            { "value": "fas fa-fist-raised", "label": "<i class=\"fas fa-fist-raised\"> Fist-Raised" },
            { "value": "fas fa-flag", "label": "<i class=\"fas fa-flag\"> Flag" },
            { "value": "fas fa-flag-checkered", "label": "<i class=\"fas fa-flag-checkered\"> Flag-Checkered" },
            { "value": "fas fa-flag-usa", "label": "<i class=\"fas fa-flag-usa\"> Flag-Usa" },
            { "value": "fas fa-flask", "label": "<i class=\"fas fa-flask\"> Flask" },
            { "value": "fas fa-flushed", "label": "<i class=\"fas fa-flushed\"> Flushed" },
            { "value": "fas fa-folder", "label": "<i class=\"fas fa-folder\"> Folder" },
            { "value": "fas fa-folder-minus", "label": "<i class=\"fas fa-folder-minus\"> Folder-Minus" },
            { "value": "fas fa-folder-open", "label": "<i class=\"fas fa-folder-open\"> Folder-Open" },
            { "value": "fas fa-folder-plus", "label": "<i class=\"fas fa-folder-plus\"> Folder-Plus" },
            { "value": "fas fa-font", "label": "<i class=\"fas fa-font\"> Font" },
            { "value": "fas fa-football-ball", "label": "<i class=\"fas fa-football-ball\"> Football-Ball" },
            { "value": "fas fa-forward", "label": "<i class=\"fas fa-forward\"> Forward" },
            { "value": "fas fa-frog", "label": "<i class=\"fas fa-frog\"> Frog" },
            { "value": "fas fa-frown", "label": "<i class=\"fas fa-frown\"> Frown" },
            { "value": "fas fa-frown-open", "label": "<i class=\"fas fa-frown-open\"> Frown-Open" },
            { "value": "fas fa-funnel-dollar", "label": "<i class=\"fas fa-funnel-dollar\"> Funnel-Dollar" },
            { "value": "fas fa-futbol", "label": "<i class=\"fas fa-futbol\"> Futbol" },
            { "value": "fas fa-gamepad", "label": "<i class=\"fas fa-gamepad\"> Gamepad" },
            { "value": "fas fa-gas-pump", "label": "<i class=\"fas fa-gas-pump\"> Gas-Pump" },
            { "value": "fas fa-gavel", "label": "<i class=\"fas fa-gavel\"> Gavel" },
            { "value": "fas fa-gem", "label": "<i class=\"fas fa-gem\"> Gem" },
            { "value": "fas fa-genderless", "label": "<i class=\"fas fa-genderless\"> Genderless" },
            { "value": "fas fa-ghost", "label": "<i class=\"fas fa-ghost\"> Ghost" },
            { "value": "fas fa-gift", "label": "<i class=\"fas fa-gift\"> Gift" },
            { "value": "fas fa-gifts", "label": "<i class=\"fas fa-gifts\"> Gifts" },
            { "value": "fas fa-glass-cheers", "label": "<i class=\"fas fa-glass-cheers\"> Glass-Cheers" },
            { "value": "fas fa-glass-martini", "label": "<i class=\"fas fa-glass-martini\"> Glass-Martini" },
            { "value": "fas fa-glass-martini-alt", "label": "<i class=\"fas fa-glass-martini-alt\"> Glass-Martini-Alt" },
            { "value": "fas fa-glass-whiskey", "label": "<i class=\"fas fa-glass-whiskey\"> Glass-Whiskey" },
            { "value": "fas fa-glasses", "label": "<i class=\"fas fa-glasses\"> Glasses" },
            { "value": "fas fa-globe", "label": "<i class=\"fas fa-globe\"> Globe" },
            { "value": "fas fa-globe-africa", "label": "<i class=\"fas fa-globe-africa\"> Globe-Africa" },
            { "value": "fas fa-globe-americas", "label": "<i class=\"fas fa-globe-americas\"> Globe-Americas" },
            { "value": "fas fa-globe-asia", "label": "<i class=\"fas fa-globe-asia\"> Globe-Asia" },
            { "value": "fas fa-globe-europe", "label": "<i class=\"fas fa-globe-europe\"> Globe-Europe" },
            { "value": "fas fa-golf-ball", "label": "<i class=\"fas fa-golf-ball\"> Golf-Ball" },
            { "value": "fas fa-gopuram", "label": "<i class=\"fas fa-gopuram\"> Gopuram" },
            { "value": "fas fa-graduation-cap", "label": "<i class=\"fas fa-graduation-cap\"> Graduation-Cap" },
            { "value": "fas fa-greater-than", "label": "<i class=\"fas fa-greater-than\"> Greater-Than" },
            {
                "value": "fas fa-greater-than-equal",
                "label": "<i class=\"fas fa-greater-than-equal\"> Greater-Than-Equal"
            },
            { "value": "fas fa-grimace", "label": "<i class=\"fas fa-grimace\"> Grimace" },
            { "value": "fas fa-grin", "label": "<i class=\"fas fa-grin\"> Grin" },
            { "value": "fas fa-grin-alt", "label": "<i class=\"fas fa-grin-alt\"> Grin-Alt" },
            { "value": "fas fa-grin-beam", "label": "<i class=\"fas fa-grin-beam\"> Grin-Beam" },
            { "value": "fas fa-grin-beam-sweat", "label": "<i class=\"fas fa-grin-beam-sweat\"> Grin-Beam-Sweat" },
            { "value": "fas fa-grin-hearts", "label": "<i class=\"fas fa-grin-hearts\"> Grin-Hearts" },
            { "value": "fas fa-grin-squint", "label": "<i class=\"fas fa-grin-squint\"> Grin-Squint" },
            { "value": "fas fa-grin-squint-tears", "label": "<i class=\"fas fa-grin-squint-tears\"> Grin-Squint-Tears" },
            { "value": "fas fa-grin-stars", "label": "<i class=\"fas fa-grin-stars\"> Grin-Stars" },
            { "value": "fas fa-grin-tears", "label": "<i class=\"fas fa-grin-tears\"> Grin-Tears" },
            { "value": "fas fa-grin-tongue", "label": "<i class=\"fas fa-grin-tongue\"> Grin-Tongue" },
            {
                "value": "fas fa-grin-tongue-squint",
                "label": "<i class=\"fas fa-grin-tongue-squint\"> Grin-Tongue-Squint"
            },
            { "value": "fas fa-grin-tongue-wink", "label": "<i class=\"fas fa-grin-tongue-wink\"> Grin-Tongue-Wink" },
            { "value": "fas fa-grin-wink", "label": "<i class=\"fas fa-grin-wink\"> Grin-Wink" },
            { "value": "fas fa-grip-horizontal", "label": "<i class=\"fas fa-grip-horizontal\"> Grip-Horizontal" },
            { "value": "fas fa-grip-lines", "label": "<i class=\"fas fa-grip-lines\"> Grip-Lines" },
            {
                "value": "fas fa-grip-lines-vertical",
                "label": "<i class=\"fas fa-grip-lines-vertical\"> Grip-Lines-Vertical"
            },
            { "value": "fas fa-grip-vertical", "label": "<i class=\"fas fa-grip-vertical\"> Grip-Vertical" },
            { "value": "fas fa-guitar", "label": "<i class=\"fas fa-guitar\"> Guitar" },
            { "value": "fas fa-h-square", "label": "<i class=\"fas fa-h-square\"> H-Square" },
            { "value": "fas fa-hamburger", "label": "<i class=\"fas fa-hamburger\"> Hamburger" },
            { "value": "fas fa-hammer", "label": "<i class=\"fas fa-hammer\"> Hammer" },
            { "value": "fas fa-hamsa", "label": "<i class=\"fas fa-hamsa\"> Hamsa" },
            { "value": "fas fa-hand-holding", "label": "<i class=\"fas fa-hand-holding\"> Hand-Holding" },
            {
                "value": "fas fa-hand-holding-heart",
                "label": "<i class=\"fas fa-hand-holding-heart\"> Hand-Holding-Heart"
            },
            { "value": "fas fa-hand-holding-usd", "label": "<i class=\"fas fa-hand-holding-usd\"> Hand-Holding-Usd" },
            { "value": "fas fa-hand-lizard", "label": "<i class=\"fas fa-hand-lizard\"> Hand-Lizard" },
            {
                "value": "fas fa-hand-middle-finger",
                "label": "<i class=\"fas fa-hand-middle-finger\"> Hand-Middle-Finger"
            },
            { "value": "fas fa-hand-paper", "label": "<i class=\"fas fa-hand-paper\"> Hand-Paper" },
            { "value": "fas fa-hand-peace", "label": "<i class=\"fas fa-hand-peace\"> Hand-Peace" },
            { "value": "fas fa-hand-point-down", "label": "<i class=\"fas fa-hand-point-down\"> Hand-Point-Down" },
            { "value": "fas fa-hand-point-left", "label": "<i class=\"fas fa-hand-point-left\"> Hand-Point-Left" },
            { "value": "fas fa-hand-point-right", "label": "<i class=\"fas fa-hand-point-right\"> Hand-Point-Right" },
            { "value": "fas fa-hand-point-up", "label": "<i class=\"fas fa-hand-point-up\"> Hand-Point-Up" },
            { "value": "fas fa-hand-pointer", "label": "<i class=\"fas fa-hand-pointer\"> Hand-Pointer" },
            { "value": "fas fa-hand-rock", "label": "<i class=\"fas fa-hand-rock\"> Hand-Rock" },
            { "value": "fas fa-hand-scissors", "label": "<i class=\"fas fa-hand-scissors\"> Hand-Scissors" },
            { "value": "fas fa-hand-spock", "label": "<i class=\"fas fa-hand-spock\"> Hand-Spock" },
            { "value": "fas fa-hands", "label": "<i class=\"fas fa-hands\"> Hands" },
            { "value": "fas fa-hands-helping", "label": "<i class=\"fas fa-hands-helping\"> Hands-Helping" },
            { "value": "fas fa-handshake", "label": "<i class=\"fas fa-handshake\"> Handshake" },
            { "value": "fas fa-hanukiah", "label": "<i class=\"fas fa-hanukiah\"> Hanukiah" },
            { "value": "fas fa-hard-hat", "label": "<i class=\"fas fa-hard-hat\"> Hard-Hat" },
            { "value": "fas fa-hashtag", "label": "<i class=\"fas fa-hashtag\"> Hashtag" },
            { "value": "fas fa-hat-wizard", "label": "<i class=\"fas fa-hat-wizard\"> Hat-Wizard" },
            { "value": "fas fa-haykal", "label": "<i class=\"fas fa-haykal\"> Haykal" },
            { "value": "fas fa-hdd", "label": "<i class=\"fas fa-hdd\"> Hdd" },
            { "value": "fas fa-heading", "label": "<i class=\"fas fa-heading\"> Heading" },
            { "value": "fas fa-headphones", "label": "<i class=\"fas fa-headphones\"> Headphones" },
            { "value": "fas fa-headphones-alt", "label": "<i class=\"fas fa-headphones-alt\"> Headphones-Alt" },
            { "value": "fas fa-headset", "label": "<i class=\"fas fa-headset\"> Headset" },
            { "value": "fas fa-heart", "label": "<i class=\"fas fa-heart\"> Heart" },
            { "value": "fas fa-heart-broken", "label": "<i class=\"fas fa-heart-broken\"> Heart-Broken" },
            { "value": "fas fa-heartbeat", "label": "<i class=\"fas fa-heartbeat\"> Heartbeat" },
            { "value": "fas fa-helicopter", "label": "<i class=\"fas fa-helicopter\"> Helicopter" },
            { "value": "fas fa-highlighter", "label": "<i class=\"fas fa-highlighter\"> Highlighter" },
            { "value": "fas fa-hiking", "label": "<i class=\"fas fa-hiking\"> Hiking" },
            { "value": "fas fa-hippo", "label": "<i class=\"fas fa-hippo\"> Hippo" },
            { "value": "fas fa-history", "label": "<i class=\"fas fa-history\"> History" },
            { "value": "fas fa-hockey-puck", "label": "<i class=\"fas fa-hockey-puck\"> Hockey-Puck" },
            { "value": "fas fa-holly-berry", "label": "<i class=\"fas fa-holly-berry\"> Holly-Berry" },
            { "value": "fas fa-home", "label": "<i class=\"fas fa-home\"> Home" },
            { "value": "fas fa-horse", "label": "<i class=\"fas fa-horse\"> Horse" },
            { "value": "fas fa-horse-head", "label": "<i class=\"fas fa-horse-head\"> Horse-Head" },
            { "value": "fas fa-hospital", "label": "<i class=\"fas fa-hospital\"> Hospital" },
            { "value": "fas fa-hospital-alt", "label": "<i class=\"fas fa-hospital-alt\"> Hospital-Alt" },
            { "value": "fas fa-hospital-symbol", "label": "<i class=\"fas fa-hospital-symbol\"> Hospital-Symbol" },
            { "value": "fas fa-hot-tub", "label": "<i class=\"fas fa-hot-tub\"> Hot-Tub" },
            { "value": "fas fa-hotdog", "label": "<i class=\"fas fa-hotdog\"> Hotdog" },
            { "value": "fas fa-hotel", "label": "<i class=\"fas fa-hotel\"> Hotel" },
            { "value": "fas fa-hourglass", "label": "<i class=\"fas fa-hourglass\"> Hourglass" },
            { "value": "fas fa-hourglass-end", "label": "<i class=\"fas fa-hourglass-end\"> Hourglass-End" },
            { "value": "fas fa-hourglass-half", "label": "<i class=\"fas fa-hourglass-half\"> Hourglass-Half" },
            { "value": "fas fa-hourglass-start", "label": "<i class=\"fas fa-hourglass-start\"> Hourglass-Start" },
            { "value": "fas fa-house-damage", "label": "<i class=\"fas fa-house-damage\"> House-Damage" },
            { "value": "fas fa-hryvnia", "label": "<i class=\"fas fa-hryvnia\"> Hryvnia" },
            { "value": "fas fa-i-cursor", "label": "<i class=\"fas fa-i-cursor\"> I-Cursor" },
            { "value": "fas fa-ice-cream", "label": "<i class=\"fas fa-ice-cream\"> Ice-Cream" },
            { "value": "fas fa-icicles", "label": "<i class=\"fas fa-icicles\"> Icicles" },
            { "value": "fas fa-id-badge", "label": "<i class=\"fas fa-id-badge\"> Id-Badge" },
            { "value": "fas fa-id-card", "label": "<i class=\"fas fa-id-card\"> Id-Card" },
            { "value": "fas fa-id-card-alt", "label": "<i class=\"fas fa-id-card-alt\"> Id-Card-Alt" },
            { "value": "fas fa-igloo", "label": "<i class=\"fas fa-igloo\"> Igloo" },
            { "value": "fas fa-image", "label": "<i class=\"fas fa-image\"> Image" },
            { "value": "fas fa-images", "label": "<i class=\"fas fa-images\"> Images" },
            { "value": "fas fa-inbox", "label": "<i class=\"fas fa-inbox\"> Inbox" },
            { "value": "fas fa-indent", "label": "<i class=\"fas fa-indent\"> Indent" },
            { "value": "fas fa-industry", "label": "<i class=\"fas fa-industry\"> Industry" },
            { "value": "fas fa-infinity", "label": "<i class=\"fas fa-infinity\"> Infinity" },
            { "value": "fas fa-info", "label": "<i class=\"fas fa-info\"> Info" },
            { "value": "fas fa-info-circle", "label": "<i class=\"fas fa-info-circle\"> Info-Circle" },
            { "value": "fas fa-italic", "label": "<i class=\"fas fa-italic\"> Italic" },
            { "value": "fas fa-jedi", "label": "<i class=\"fas fa-jedi\"> Jedi" },
            { "value": "fas fa-joint", "label": "<i class=\"fas fa-joint\"> Joint" },
            { "value": "fas fa-journal-whills", "label": "<i class=\"fas fa-journal-whills\"> Journal-Whills" },
            { "value": "fas fa-kaaba", "label": "<i class=\"fas fa-kaaba\"> Kaaba" },
            { "value": "fas fa-key", "label": "<i class=\"fas fa-key\"> Key" },
            { "value": "fas fa-keyboard", "label": "<i class=\"fas fa-keyboard\"> Keyboard" },
            { "value": "fas fa-khanda", "label": "<i class=\"fas fa-khanda\"> Khanda" },
            { "value": "fas fa-kiss", "label": "<i class=\"fas fa-kiss\"> Kiss" },
            { "value": "fas fa-kiss-beam", "label": "<i class=\"fas fa-kiss-beam\"> Kiss-Beam" },
            { "value": "fas fa-kiss-wink-heart", "label": "<i class=\"fas fa-kiss-wink-heart\"> Kiss-Wink-Heart" },
            { "value": "fas fa-kiwi-bird", "label": "<i class=\"fas fa-kiwi-bird\"> Kiwi-Bird" },
            { "value": "fas fa-landmark", "label": "<i class=\"fas fa-landmark\"> Landmark" },
            { "value": "fas fa-language", "label": "<i class=\"fas fa-language\"> Language" },
            { "value": "fas fa-laptop", "label": "<i class=\"fas fa-laptop\"> Laptop" },
            { "value": "fas fa-laptop-code", "label": "<i class=\"fas fa-laptop-code\"> Laptop-Code" },
            { "value": "fas fa-laptop-medical", "label": "<i class=\"fas fa-laptop-medical\"> Laptop-Medical" },
            { "value": "fas fa-laugh", "label": "<i class=\"fas fa-laugh\"> Laugh" },
            { "value": "fas fa-laugh-beam", "label": "<i class=\"fas fa-laugh-beam\"> Laugh-Beam" },
            { "value": "fas fa-laugh-squint", "label": "<i class=\"fas fa-laugh-squint\"> Laugh-Squint" },
            { "value": "fas fa-laugh-wink", "label": "<i class=\"fas fa-laugh-wink\"> Laugh-Wink" },
            { "value": "fas fa-layer-group", "label": "<i class=\"fas fa-layer-group\"> Layer-Group" },
            { "value": "fas fa-leaf", "label": "<i class=\"fas fa-leaf\"> Leaf" },
            { "value": "fas fa-lemon", "label": "<i class=\"fas fa-lemon\"> Lemon" },
            { "value": "fas fa-less-than", "label": "<i class=\"fas fa-less-than\"> Less-Than" },
            { "value": "fas fa-less-than-equal", "label": "<i class=\"fas fa-less-than-equal\"> Less-Than-Equal" },
            { "value": "fas fa-level-down-alt", "label": "<i class=\"fas fa-level-down-alt\"> Level-Down-Alt" },
            { "value": "fas fa-level-up-alt", "label": "<i class=\"fas fa-level-up-alt\"> Level-Up-Alt" },
            { "value": "fas fa-life-ring", "label": "<i class=\"fas fa-life-ring\"> Life-Ring" },
            { "value": "fas fa-lightbulb", "label": "<i class=\"fas fa-lightbulb\"> Lightbulb" },
            { "value": "fas fa-link", "label": "<i class=\"fas fa-link\"> Link" },
            { "value": "fas fa-lira-sign", "label": "<i class=\"fas fa-lira-sign\"> Lira-Sign" },
            { "value": "fas fa-list", "label": "<i class=\"fas fa-list\"> List" },
            { "value": "fas fa-list-alt", "label": "<i class=\"fas fa-list-alt\"> List-Alt" },
            { "value": "fas fa-list-ol", "label": "<i class=\"fas fa-list-ol\"> List-Ol" },
            { "value": "fas fa-list-ul", "label": "<i class=\"fas fa-list-ul\"> List-Ul" },
            { "value": "fas fa-location-arrow", "label": "<i class=\"fas fa-location-arrow\"> Location-Arrow" },
            { "value": "fas fa-lock", "label": "<i class=\"fas fa-lock\"> Lock" },
            { "value": "fas fa-lock-open", "label": "<i class=\"fas fa-lock-open\"> Lock-Open" },
            {
                "value": "fas fa-long-arrow-alt-down",
                "label": "<i class=\"fas fa-long-arrow-alt-down\"> Long-Arrow-Alt-Down"
            },
            {
                "value": "fas fa-long-arrow-alt-left",
                "label": "<i class=\"fas fa-long-arrow-alt-left\"> Long-Arrow-Alt-Left"
            },
            {
                "value": "fas fa-long-arrow-alt-right",
                "label": "<i class=\"fas fa-long-arrow-alt-right\"> Long-Arrow-Alt-Right"
            },
            { "value": "fas fa-long-arrow-alt-up", "label": "<i class=\"fas fa-long-arrow-alt-up\"> Long-Arrow-Alt-Up" },
            { "value": "fas fa-low-vision", "label": "<i class=\"fas fa-low-vision\"> Low-Vision" },
            { "value": "fas fa-luggage-cart", "label": "<i class=\"fas fa-luggage-cart\"> Luggage-Cart" },
            { "value": "fas fa-magic", "label": "<i class=\"fas fa-magic\"> Magic" },
            { "value": "fas fa-magnet", "label": "<i class=\"fas fa-magnet\"> Magnet" },
            { "value": "fas fa-mail-bulk", "label": "<i class=\"fas fa-mail-bulk\"> Mail-Bulk" },
            { "value": "fas fa-male", "label": "<i class=\"fas fa-male\"> Male" },
            { "value": "fas fa-map", "label": "<i class=\"fas fa-map\"> Map" },
            { "value": "fas fa-map-marked", "label": "<i class=\"fas fa-map-marked\"> Map-Marked" },
            { "value": "fas fa-map-marked-alt", "label": "<i class=\"fas fa-map-marked-alt\"> Map-Marked-Alt" },
            { "value": "fas fa-map-marker", "label": "<i class=\"fas fa-map-marker\"> Map-Marker" },
            { "value": "fas fa-map-marker-alt", "label": "<i class=\"fas fa-map-marker-alt\"> Map-Marker-Alt" },
            { "value": "fas fa-map-pin", "label": "<i class=\"fas fa-map-pin\"> Map-Pin" },
            { "value": "fas fa-map-signs", "label": "<i class=\"fas fa-map-signs\"> Map-Signs" },
            { "value": "fas fa-marker", "label": "<i class=\"fas fa-marker\"> Marker" },
            { "value": "fas fa-mars", "label": "<i class=\"fas fa-mars\"> Mars" },
            { "value": "fas fa-mars-double", "label": "<i class=\"fas fa-mars-double\"> Mars-Double" },
            { "value": "fas fa-mars-stroke", "label": "<i class=\"fas fa-mars-stroke\"> Mars-Stroke" },
            { "value": "fas fa-mars-stroke-h", "label": "<i class=\"fas fa-mars-stroke-h\"> Mars-Stroke-H" },
            { "value": "fas fa-mars-stroke-v", "label": "<i class=\"fas fa-mars-stroke-v\"> Mars-Stroke-V" },
            { "value": "fas fa-mask", "label": "<i class=\"fas fa-mask\"> Mask" },
            { "value": "fas fa-medal", "label": "<i class=\"fas fa-medal\"> Medal" },
            { "value": "fas fa-medkit", "label": "<i class=\"fas fa-medkit\"> Medkit" },
            { "value": "fas fa-meh", "label": "<i class=\"fas fa-meh\"> Meh" },
            { "value": "fas fa-meh-blank", "label": "<i class=\"fas fa-meh-blank\"> Meh-Blank" },
            { "value": "fas fa-meh-rolling-eyes", "label": "<i class=\"fas fa-meh-rolling-eyes\"> Meh-Rolling-Eyes" },
            { "value": "fas fa-memory", "label": "<i class=\"fas fa-memory\"> Memory" },
            { "value": "fas fa-menorah", "label": "<i class=\"fas fa-menorah\"> Menorah" },
            { "value": "fas fa-mercury", "label": "<i class=\"fas fa-mercury\"> Mercury" },
            { "value": "fas fa-meteor", "label": "<i class=\"fas fa-meteor\"> Meteor" },
            { "value": "fas fa-microchip", "label": "<i class=\"fas fa-microchip\"> Microchip" },
            { "value": "fas fa-microphone", "label": "<i class=\"fas fa-microphone\"> Microphone" },
            { "value": "fas fa-microphone-alt", "label": "<i class=\"fas fa-microphone-alt\"> Microphone-Alt" },
            {
                "value": "fas fa-microphone-alt-slash",
                "label": "<i class=\"fas fa-microphone-alt-slash\"> Microphone-Alt-Slash"
            },
            { "value": "fas fa-microphone-slash", "label": "<i class=\"fas fa-microphone-slash\"> Microphone-Slash" },
            { "value": "fas fa-microscope", "label": "<i class=\"fas fa-microscope\"> Microscope" },
            { "value": "fas fa-minus", "label": "<i class=\"fas fa-minus\"> Minus" },
            { "value": "fas fa-minus-circle", "label": "<i class=\"fas fa-minus-circle\"> Minus-Circle" },
            { "value": "fas fa-minus-square", "label": "<i class=\"fas fa-minus-square\"> Minus-Square" },
            { "value": "fas fa-mitten", "label": "<i class=\"fas fa-mitten\"> Mitten" },
            { "value": "fas fa-mobile", "label": "<i class=\"fas fa-mobile\"> Mobile" },
            { "value": "fas fa-mobile-alt", "label": "<i class=\"fas fa-mobile-alt\"> Mobile-Alt" },
            { "value": "fas fa-money-bill", "label": "<i class=\"fas fa-money-bill\"> Money-Bill" },
            { "value": "fas fa-money-bill-alt", "label": "<i class=\"fas fa-money-bill-alt\"> Money-Bill-Alt" },
            { "value": "fas fa-money-bill-wave", "label": "<i class=\"fas fa-money-bill-wave\"> Money-Bill-Wave" },
            {
                "value": "fas fa-money-bill-wave-alt",
                "label": "<i class=\"fas fa-money-bill-wave-alt\"> Money-Bill-Wave-Alt"
            },
            { "value": "fas fa-money-check", "label": "<i class=\"fas fa-money-check\"> Money-Check" },
            { "value": "fas fa-money-check-alt", "label": "<i class=\"fas fa-money-check-alt\"> Money-Check-Alt" },
            { "value": "fas fa-monument", "label": "<i class=\"fas fa-monument\"> Monument" },
            { "value": "fas fa-moon", "label": "<i class=\"fas fa-moon\"> Moon" },
            { "value": "fas fa-mortar-pestle", "label": "<i class=\"fas fa-mortar-pestle\"> Mortar-Pestle" },
            { "value": "fas fa-mosque", "label": "<i class=\"fas fa-mosque\"> Mosque" },
            { "value": "fas fa-motorcycle", "label": "<i class=\"fas fa-motorcycle\"> Motorcycle" },
            { "value": "fas fa-mountain", "label": "<i class=\"fas fa-mountain\"> Mountain" },
            { "value": "fas fa-mouse-pointer", "label": "<i class=\"fas fa-mouse-pointer\"> Mouse-Pointer" },
            { "value": "fas fa-mug-hot", "label": "<i class=\"fas fa-mug-hot\"> Mug-Hot" },
            { "value": "fas fa-music", "label": "<i class=\"fas fa-music\"> Music" },
            { "value": "fas fa-network-wired", "label": "<i class=\"fas fa-network-wired\"> Network-Wired" },
            { "value": "fas fa-neuter", "label": "<i class=\"fas fa-neuter\"> Neuter" },
            { "value": "fas fa-newspaper", "label": "<i class=\"fas fa-newspaper\"> Newspaper" },
            { "value": "fas fa-not-equal", "label": "<i class=\"fas fa-not-equal\"> Not-Equal" },
            { "value": "fas fa-notes-medical", "label": "<i class=\"fas fa-notes-medical\"> Notes-Medical" },
            { "value": "fas fa-object-group", "label": "<i class=\"fas fa-object-group\"> Object-Group" },
            { "value": "fas fa-object-ungroup", "label": "<i class=\"fas fa-object-ungroup\"> Object-Ungroup" },
            { "value": "fas fa-oil-can", "label": "<i class=\"fas fa-oil-can\"> Oil-Can" },
            { "value": "fas fa-om", "label": "<i class=\"fas fa-om\"> Om" },
            { "value": "fas fa-otter", "label": "<i class=\"fas fa-otter\"> Otter" },
            { "value": "fas fa-outdent", "label": "<i class=\"fas fa-outdent\"> Outdent" },
            { "value": "fas fa-pager", "label": "<i class=\"fas fa-pager\"> Pager" },
            { "value": "fas fa-paint-brush", "label": "<i class=\"fas fa-paint-brush\"> Paint-Brush" },
            { "value": "fas fa-paint-roller", "label": "<i class=\"fas fa-paint-roller\"> Paint-Roller" },
            { "value": "fas fa-palette", "label": "<i class=\"fas fa-palette\"> Palette" },
            { "value": "fas fa-pallet", "label": "<i class=\"fas fa-pallet\"> Pallet" },
            { "value": "fas fa-paper-plane", "label": "<i class=\"fas fa-paper-plane\"> Paper-Plane" },
            { "value": "fas fa-paperclip", "label": "<i class=\"fas fa-paperclip\"> Paperclip" },
            { "value": "fas fa-parachute-box", "label": "<i class=\"fas fa-parachute-box\"> Parachute-Box" },
            { "value": "fas fa-paragraph", "label": "<i class=\"fas fa-paragraph\"> Paragraph" },
            { "value": "fas fa-parking", "label": "<i class=\"fas fa-parking\"> Parking" },
            { "value": "fas fa-passport", "label": "<i class=\"fas fa-passport\"> Passport" },
            { "value": "fas fa-pastafarianism", "label": "<i class=\"fas fa-pastafarianism\"> Pastafarianism" },
            { "value": "fas fa-paste", "label": "<i class=\"fas fa-paste\"> Paste" },
            { "value": "fas fa-pause", "label": "<i class=\"fas fa-pause\"> Pause" },
            { "value": "fas fa-pause-circle", "label": "<i class=\"fas fa-pause-circle\"> Pause-Circle" },
            { "value": "fas fa-paw", "label": "<i class=\"fas fa-paw\"> Paw" },
            { "value": "fas fa-peace", "label": "<i class=\"fas fa-peace\"> Peace" },
            { "value": "fas fa-pen", "label": "<i class=\"fas fa-pen\"> Pen" },
            { "value": "fas fa-pen-alt", "label": "<i class=\"fas fa-pen-alt\"> Pen-Alt" },
            { "value": "fas fa-pen-fancy", "label": "<i class=\"fas fa-pen-fancy\"> Pen-Fancy" },
            { "value": "fas fa-pen-nib", "label": "<i class=\"fas fa-pen-nib\"> Pen-Nib" },
            { "value": "fas fa-pen-square", "label": "<i class=\"fas fa-pen-square\"> Pen-Square" },
            { "value": "fas fa-pencil-alt", "label": "<i class=\"fas fa-pencil-alt\"> Pencil-Alt" },
            { "value": "fas fa-pencil-ruler", "label": "<i class=\"fas fa-pencil-ruler\"> Pencil-Ruler" },
            { "value": "fas fa-people-carry", "label": "<i class=\"fas fa-people-carry\"> People-Carry" },
            { "value": "fas fa-pepper-hot", "label": "<i class=\"fas fa-pepper-hot\"> Pepper-Hot" },
            { "value": "fas fa-percent", "label": "<i class=\"fas fa-percent\"> Percent" },
            { "value": "fas fa-percentage", "label": "<i class=\"fas fa-percentage\"> Percentage" },
            { "value": "fas fa-person-booth", "label": "<i class=\"fas fa-person-booth\"> Person-Booth" },
            { "value": "fas fa-phone", "label": "<i class=\"fas fa-phone\"> Phone" },
            { "value": "fas fa-phone-slash", "label": "<i class=\"fas fa-phone-slash\"> Phone-Slash" },
            { "value": "fas fa-phone-square", "label": "<i class=\"fas fa-phone-square\"> Phone-Square" },
            { "value": "fas fa-phone-volume", "label": "<i class=\"fas fa-phone-volume\"> Phone-Volume" },
            { "value": "fas fa-piggy-bank", "label": "<i class=\"fas fa-piggy-bank\"> Piggy-Bank" },
            { "value": "fas fa-pills", "label": "<i class=\"fas fa-pills\"> Pills" },
            { "value": "fas fa-pizza-slice", "label": "<i class=\"fas fa-pizza-slice\"> Pizza-Slice" },
            { "value": "fas fa-place-of-worship", "label": "<i class=\"fas fa-place-of-worship\"> Place-Of-Worship" },
            { "value": "fas fa-plane", "label": "<i class=\"fas fa-plane\"> Plane" },
            { "value": "fas fa-plane-arrival", "label": "<i class=\"fas fa-plane-arrival\"> Plane-Arrival" },
            { "value": "fas fa-plane-departure", "label": "<i class=\"fas fa-plane-departure\"> Plane-Departure" },
            { "value": "fas fa-play", "label": "<i class=\"fas fa-play\"> Play" },
            { "value": "fas fa-play-circle", "label": "<i class=\"fas fa-play-circle\"> Play-Circle" },
            { "value": "fas fa-plug", "label": "<i class=\"fas fa-plug\"> Plug" },
            { "value": "fas fa-plus", "label": "<i class=\"fas fa-plus\"> Plus" },
            { "value": "fas fa-plus-circle", "label": "<i class=\"fas fa-plus-circle\"> Plus-Circle" },
            { "value": "fas fa-plus-square", "label": "<i class=\"fas fa-plus-square\"> Plus-Square" },
            { "value": "fas fa-podcast", "label": "<i class=\"fas fa-podcast\"> Podcast" },
            { "value": "fas fa-poll", "label": "<i class=\"fas fa-poll\"> Poll" },
            { "value": "fas fa-poll-h", "label": "<i class=\"fas fa-poll-h\"> Poll-H" },
            { "value": "fas fa-poo", "label": "<i class=\"fas fa-poo\"> Poo" },
            { "value": "fas fa-poo-storm", "label": "<i class=\"fas fa-poo-storm\"> Poo-Storm" },
            { "value": "fas fa-poop", "label": "<i class=\"fas fa-poop\"> Poop" },
            { "value": "fas fa-portrait", "label": "<i class=\"fas fa-portrait\"> Portrait" },
            { "value": "fas fa-pound-sign", "label": "<i class=\"fas fa-pound-sign\"> Pound-Sign" },
            { "value": "fas fa-power-off", "label": "<i class=\"fas fa-power-off\"> Power-Off" },
            { "value": "fas fa-pray", "label": "<i class=\"fas fa-pray\"> Pray" },
            { "value": "fas fa-praying-hands", "label": "<i class=\"fas fa-praying-hands\"> Praying-Hands" },
            { "value": "fas fa-prescription", "label": "<i class=\"fas fa-prescription\"> Prescription" },
            {
                "value": "fas fa-prescription-bottle",
                "label": "<i class=\"fas fa-prescription-bottle\"> Prescription-Bottle"
            },
            {
                "value": "fas fa-prescription-bottle-alt",
                "label": "<i class=\"fas fa-prescription-bottle-alt\"> Prescription-Bottle-Alt"
            },
            { "value": "fas fa-print", "label": "<i class=\"fas fa-print\"> Print" },
            { "value": "fas fa-procedures", "label": "<i class=\"fas fa-procedures\"> Procedures" },
            { "value": "fas fa-project-diagram", "label": "<i class=\"fas fa-project-diagram\"> Project-Diagram" },
            { "value": "fas fa-puzzle-piece", "label": "<i class=\"fas fa-puzzle-piece\"> Puzzle-Piece" },
            { "value": "fas fa-qrcode", "label": "<i class=\"fas fa-qrcode\"> Qrcode" },
            { "value": "fas fa-question", "label": "<i class=\"fas fa-question\"> Question" },
            { "value": "fas fa-question-circle", "label": "<i class=\"fas fa-question-circle\"> Question-Circle" },
            { "value": "fas fa-quidditch", "label": "<i class=\"fas fa-quidditch\"> Quidditch" },
            { "value": "fas fa-quote-left", "label": "<i class=\"fas fa-quote-left\"> Quote-Left" },
            { "value": "fas fa-quote-right", "label": "<i class=\"fas fa-quote-right\"> Quote-Right" },
            { "value": "fas fa-quran", "label": "<i class=\"fas fa-quran\"> Quran" },
            { "value": "fas fa-radiation", "label": "<i class=\"fas fa-radiation\"> Radiation" },
            { "value": "fas fa-radiation-alt", "label": "<i class=\"fas fa-radiation-alt\"> Radiation-Alt" },
            { "value": "fas fa-rainbow", "label": "<i class=\"fas fa-rainbow\"> Rainbow" },
            { "value": "fas fa-random", "label": "<i class=\"fas fa-random\"> Random" },
            { "value": "fas fa-receipt", "label": "<i class=\"fas fa-receipt\"> Receipt" },
            { "value": "fas fa-recycle", "label": "<i class=\"fas fa-recycle\"> Recycle" },
            { "value": "fas fa-redo", "label": "<i class=\"fas fa-redo\"> Redo" },
            { "value": "fas fa-redo-alt", "label": "<i class=\"fas fa-redo-alt\"> Redo-Alt" },
            { "value": "fas fa-registered", "label": "<i class=\"fas fa-registered\"> Registered" },
            { "value": "fas fa-reply", "label": "<i class=\"fas fa-reply\"> Reply" },
            { "value": "fas fa-reply-all", "label": "<i class=\"fas fa-reply-all\"> Reply-All" },
            { "value": "fas fa-republican", "label": "<i class=\"fas fa-republican\"> Republican" },
            { "value": "fas fa-restroom", "label": "<i class=\"fas fa-restroom\"> Restroom" },
            { "value": "fas fa-retweet", "label": "<i class=\"fas fa-retweet\"> Retweet" },
            { "value": "fas fa-ribbon", "label": "<i class=\"fas fa-ribbon\"> Ribbon" },
            { "value": "fas fa-ring", "label": "<i class=\"fas fa-ring\"> Ring" },
            { "value": "fas fa-road", "label": "<i class=\"fas fa-road\"> Road" },
            { "value": "fas fa-robot", "label": "<i class=\"fas fa-robot\"> Robot" },
            { "value": "fas fa-rocket", "label": "<i class=\"fas fa-rocket\"> Rocket" },
            { "value": "fas fa-route", "label": "<i class=\"fas fa-route\"> Route" },
            { "value": "fas fa-rss", "label": "<i class=\"fas fa-rss\"> Rss" },
            { "value": "fas fa-rss-square", "label": "<i class=\"fas fa-rss-square\"> Rss-Square" },
            { "value": "fas fa-ruble-sign", "label": "<i class=\"fas fa-ruble-sign\"> Ruble-Sign" },
            { "value": "fas fa-ruler", "label": "<i class=\"fas fa-ruler\"> Ruler" },
            { "value": "fas fa-ruler-combined", "label": "<i class=\"fas fa-ruler-combined\"> Ruler-Combined" },
            { "value": "fas fa-ruler-horizontal", "label": "<i class=\"fas fa-ruler-horizontal\"> Ruler-Horizontal" },
            { "value": "fas fa-ruler-vertical", "label": "<i class=\"fas fa-ruler-vertical\"> Ruler-Vertical" },
            { "value": "fas fa-running", "label": "<i class=\"fas fa-running\"> Running" },
            { "value": "fas fa-rupee-sign", "label": "<i class=\"fas fa-rupee-sign\"> Rupee-Sign" },
            { "value": "fas fa-sad-cry", "label": "<i class=\"fas fa-sad-cry\"> Sad-Cry" },
            { "value": "fas fa-sad-tear", "label": "<i class=\"fas fa-sad-tear\"> Sad-Tear" },
            { "value": "fas fa-satellite", "label": "<i class=\"fas fa-satellite\"> Satellite" },
            { "value": "fas fa-satellite-dish", "label": "<i class=\"fas fa-satellite-dish\"> Satellite-Dish" },
            { "value": "fas fa-save", "label": "<i class=\"fas fa-save\"> Save" },
            { "value": "fas fa-school", "label": "<i class=\"fas fa-school\"> School" },
            { "value": "fas fa-screwdriver", "label": "<i class=\"fas fa-screwdriver\"> Screwdriver" },
            { "value": "fas fa-scroll", "label": "<i class=\"fas fa-scroll\"> Scroll" },
            { "value": "fas fa-sd-card", "label": "<i class=\"fas fa-sd-card\"> Sd-Card" },
            { "value": "fas fa-search", "label": "<i class=\"fas fa-search\"> Search" },
            { "value": "fas fa-search-dollar", "label": "<i class=\"fas fa-search-dollar\"> Search-Dollar" },
            { "value": "fas fa-search-location", "label": "<i class=\"fas fa-search-location\"> Search-Location" },
            { "value": "fas fa-search-minus", "label": "<i class=\"fas fa-search-minus\"> Search-Minus" },
            { "value": "fas fa-search-plus", "label": "<i class=\"fas fa-search-plus\"> Search-Plus" },
            { "value": "fas fa-seedling", "label": "<i class=\"fas fa-seedling\"> Seedling" },
            { "value": "fas fa-server", "label": "<i class=\"fas fa-server\"> Server" },
            { "value": "fas fa-shapes", "label": "<i class=\"fas fa-shapes\"> Shapes" },
            { "value": "fas fa-share", "label": "<i class=\"fas fa-share\"> Share" },
            { "value": "fas fa-share-alt", "label": "<i class=\"fas fa-share-alt\"> Share-Alt" },
            { "value": "fas fa-share-alt-square", "label": "<i class=\"fas fa-share-alt-square\"> Share-Alt-Square" },
            { "value": "fas fa-share-square", "label": "<i class=\"fas fa-share-square\"> Share-Square" },
            { "value": "fas fa-shekel-sign", "label": "<i class=\"fas fa-shekel-sign\"> Shekel-Sign" },
            { "value": "fas fa-shield-alt", "label": "<i class=\"fas fa-shield-alt\"> Shield-Alt" },
            { "value": "fas fa-ship", "label": "<i class=\"fas fa-ship\"> Ship" },
            { "value": "fas fa-shipping-fast", "label": "<i class=\"fas fa-shipping-fast\"> Shipping-Fast" },
            { "value": "fas fa-shoe-prints", "label": "<i class=\"fas fa-shoe-prints\"> Shoe-Prints" },
            { "value": "fas fa-shopping-bag", "label": "<i class=\"fas fa-shopping-bag\"> Shopping-Bag" },
            { "value": "fas fa-shopping-basket", "label": "<i class=\"fas fa-shopping-basket\"> Shopping-Basket" },
            { "value": "fas fa-shopping-cart", "label": "<i class=\"fas fa-shopping-cart\"> Shopping-Cart" },
            { "value": "fas fa-shower", "label": "<i class=\"fas fa-shower\"> Shower" },
            { "value": "fas fa-shuttle-van", "label": "<i class=\"fas fa-shuttle-van\"> Shuttle-Van" },
            { "value": "fas fa-sign", "label": "<i class=\"fas fa-sign\"> Sign" },
            { "value": "fas fa-sign-in-alt", "label": "<i class=\"fas fa-sign-in-alt\"> Sign-In-Alt" },
            { "value": "fas fa-sign-language", "label": "<i class=\"fas fa-sign-language\"> Sign-Language" },
            { "value": "fas fa-sign-out-alt", "label": "<i class=\"fas fa-sign-out-alt\"> Sign-Out-Alt" },
            { "value": "fas fa-signal", "label": "<i class=\"fas fa-signal\"> Signal" },
            { "value": "fas fa-signature", "label": "<i class=\"fas fa-signature\"> Signature" },
            { "value": "fas fa-sim-card", "label": "<i class=\"fas fa-sim-card\"> Sim-Card" },
            { "value": "fas fa-sitemap", "label": "<i class=\"fas fa-sitemap\"> Sitemap" },
            { "value": "fas fa-skating", "label": "<i class=\"fas fa-skating\"> Skating" },
            { "value": "fas fa-skiing", "label": "<i class=\"fas fa-skiing\"> Skiing" },
            { "value": "fas fa-skiing-nordic", "label": "<i class=\"fas fa-skiing-nordic\"> Skiing-Nordic" },
            { "value": "fas fa-skull", "label": "<i class=\"fas fa-skull\"> Skull" },
            { "value": "fas fa-skull-crossbones", "label": "<i class=\"fas fa-skull-crossbones\"> Skull-Crossbones" },
            { "value": "fas fa-slash", "label": "<i class=\"fas fa-slash\"> Slash" },
            { "value": "fas fa-sleigh", "label": "<i class=\"fas fa-sleigh\"> Sleigh" },
            { "value": "fas fa-sliders-h", "label": "<i class=\"fas fa-sliders-h\"> Sliders-H" },
            { "value": "fas fa-smile", "label": "<i class=\"fas fa-smile\"> Smile" },
            { "value": "fas fa-smile-beam", "label": "<i class=\"fas fa-smile-beam\"> Smile-Beam" },
            { "value": "fas fa-smile-wink", "label": "<i class=\"fas fa-smile-wink\"> Smile-Wink" },
            { "value": "fas fa-smog", "label": "<i class=\"fas fa-smog\"> Smog" },
            { "value": "fas fa-smoking", "label": "<i class=\"fas fa-smoking\"> Smoking" },
            { "value": "fas fa-smoking-ban", "label": "<i class=\"fas fa-smoking-ban\"> Smoking-Ban" },
            { "value": "fas fa-sms", "label": "<i class=\"fas fa-sms\"> Sms" },
            { "value": "fas fa-snowboarding", "label": "<i class=\"fas fa-snowboarding\"> Snowboarding" },
            { "value": "fas fa-snowflake", "label": "<i class=\"fas fa-snowflake\"> Snowflake" },
            { "value": "fas fa-snowman", "label": "<i class=\"fas fa-snowman\"> Snowman" },
            { "value": "fas fa-snowplow", "label": "<i class=\"fas fa-snowplow\"> Snowplow" },
            { "value": "fas fa-socks", "label": "<i class=\"fas fa-socks\"> Socks" },
            { "value": "fas fa-solar-panel", "label": "<i class=\"fas fa-solar-panel\"> Solar-Panel" },
            { "value": "fas fa-sort", "label": "<i class=\"fas fa-sort\"> Sort" },
            { "value": "fas fa-sort-alpha-down", "label": "<i class=\"fas fa-sort-alpha-down\"> Sort-Alpha-Down" },
            { "value": "fas fa-sort-alpha-up", "label": "<i class=\"fas fa-sort-alpha-up\"> Sort-Alpha-Up" },
            { "value": "fas fa-sort-amount-down", "label": "<i class=\"fas fa-sort-amount-down\"> Sort-Amount-Down" },
            { "value": "fas fa-sort-amount-up", "label": "<i class=\"fas fa-sort-amount-up\"> Sort-Amount-Up" },
            { "value": "fas fa-sort-down", "label": "<i class=\"fas fa-sort-down\"> Sort-Down" },
            { "value": "fas fa-sort-numeric-down", "label": "<i class=\"fas fa-sort-numeric-down\"> Sort-Numeric-Down" },
            { "value": "fas fa-sort-numeric-up", "label": "<i class=\"fas fa-sort-numeric-up\"> Sort-Numeric-Up" },
            { "value": "fas fa-sort-up", "label": "<i class=\"fas fa-sort-up\"> Sort-Up" },
            { "value": "fas fa-spa", "label": "<i class=\"fas fa-spa\"> Spa" },
            { "value": "fas fa-space-shuttle", "label": "<i class=\"fas fa-space-shuttle\"> Space-Shuttle" },
            { "value": "fas fa-spider", "label": "<i class=\"fas fa-spider\"> Spider" },
            { "value": "fas fa-spinner", "label": "<i class=\"fas fa-spinner\"> Spinner" },
            { "value": "fas fa-splotch", "label": "<i class=\"fas fa-splotch\"> Splotch" },
            { "value": "fas fa-spray-can", "label": "<i class=\"fas fa-spray-can\"> Spray-Can" },
            { "value": "fas fa-square", "label": "<i class=\"fas fa-square\"> Square" },
            { "value": "fas fa-square-full", "label": "<i class=\"fas fa-square-full\"> Square-Full" },
            { "value": "fas fa-square-root-alt", "label": "<i class=\"fas fa-square-root-alt\"> Square-Root-Alt" },
            { "value": "fas fa-stamp", "label": "<i class=\"fas fa-stamp\"> Stamp" },
            { "value": "fas fa-star", "label": "<i class=\"fas fa-star\"> Star" },
            { "value": "fas fa-star-and-crescent", "label": "<i class=\"fas fa-star-and-crescent\"> Star-And-Crescent" },
            { "value": "fas fa-star-half", "label": "<i class=\"fas fa-star-half\"> Star-Half" },
            { "value": "fas fa-star-half-alt", "label": "<i class=\"fas fa-star-half-alt\"> Star-Half-Alt" },
            { "value": "fas fa-star-of-david", "label": "<i class=\"fas fa-star-of-david\"> Star-Of-David" },
            { "value": "fas fa-star-of-life", "label": "<i class=\"fas fa-star-of-life\"> Star-Of-Life" },
            { "value": "fas fa-step-backward", "label": "<i class=\"fas fa-step-backward\"> Step-Backward" },
            { "value": "fas fa-step-forward", "label": "<i class=\"fas fa-step-forward\"> Step-Forward" },
            { "value": "fas fa-stethoscope", "label": "<i class=\"fas fa-stethoscope\"> Stethoscope" },
            { "value": "fas fa-sticky-note", "label": "<i class=\"fas fa-sticky-note\"> Sticky-Note" },
            { "value": "fas fa-stop", "label": "<i class=\"fas fa-stop\"> Stop" },
            { "value": "fas fa-stop-circle", "label": "<i class=\"fas fa-stop-circle\"> Stop-Circle" },
            { "value": "fas fa-stopwatch", "label": "<i class=\"fas fa-stopwatch\"> Stopwatch" },
            { "value": "fas fa-store", "label": "<i class=\"fas fa-store\"> Store" },
            { "value": "fas fa-store-alt", "label": "<i class=\"fas fa-store-alt\"> Store-Alt" },
            { "value": "fas fa-stream", "label": "<i class=\"fas fa-stream\"> Stream" },
            { "value": "fas fa-street-view", "label": "<i class=\"fas fa-street-view\"> Street-View" },
            { "value": "fas fa-strikethrough", "label": "<i class=\"fas fa-strikethrough\"> Strikethrough" },
            { "value": "fas fa-stroopwafel", "label": "<i class=\"fas fa-stroopwafel\"> Stroopwafel" },
            { "value": "fas fa-subscript", "label": "<i class=\"fas fa-subscript\"> Subscript" },
            { "value": "fas fa-subway", "label": "<i class=\"fas fa-subway\"> Subway" },
            { "value": "fas fa-suitcase", "label": "<i class=\"fas fa-suitcase\"> Suitcase" },
            { "value": "fas fa-suitcase-rolling", "label": "<i class=\"fas fa-suitcase-rolling\"> Suitcase-Rolling" },
            { "value": "fas fa-sun", "label": "<i class=\"fas fa-sun\"> Sun" },
            { "value": "fas fa-superscript", "label": "<i class=\"fas fa-superscript\"> Superscript" },
            { "value": "fas fa-surprise", "label": "<i class=\"fas fa-surprise\"> Surprise" },
            { "value": "fas fa-swatchbook", "label": "<i class=\"fas fa-swatchbook\"> Swatchbook" },
            { "value": "fas fa-swimmer", "label": "<i class=\"fas fa-swimmer\"> Swimmer" },
            { "value": "fas fa-swimming-pool", "label": "<i class=\"fas fa-swimming-pool\"> Swimming-Pool" },
            { "value": "fas fa-synagogue", "label": "<i class=\"fas fa-synagogue\"> Synagogue" },
            { "value": "fas fa-sync", "label": "<i class=\"fas fa-sync\"> Sync" },
            { "value": "fas fa-sync-alt", "label": "<i class=\"fas fa-sync-alt\"> Sync-Alt" },
            { "value": "fas fa-syringe", "label": "<i class=\"fas fa-syringe\"> Syringe" },
            { "value": "fas fa-table", "label": "<i class=\"fas fa-table\"> Table" },
            { "value": "fas fa-table-tennis", "label": "<i class=\"fas fa-table-tennis\"> Table-Tennis" },
            { "value": "fas fa-tablet", "label": "<i class=\"fas fa-tablet\"> Tablet" },
            { "value": "fas fa-tablet-alt", "label": "<i class=\"fas fa-tablet-alt\"> Tablet-Alt" },
            { "value": "fas fa-tablets", "label": "<i class=\"fas fa-tablets\"> Tablets" },
            { "value": "fas fa-tachometer-alt", "label": "<i class=\"fas fa-tachometer-alt\"> Tachometer-Alt" },
            { "value": "fas fa-tag", "label": "<i class=\"fas fa-tag\"> Tag" },
            { "value": "fas fa-tags", "label": "<i class=\"fas fa-tags\"> Tags" },
            { "value": "fas fa-tape", "label": "<i class=\"fas fa-tape\"> Tape" },
            { "value": "fas fa-tasks", "label": "<i class=\"fas fa-tasks\"> Tasks" },
            { "value": "fas fa-taxi", "label": "<i class=\"fas fa-taxi\"> Taxi" },
            { "value": "fas fa-teeth", "label": "<i class=\"fas fa-teeth\"> Teeth" },
            { "value": "fas fa-teeth-open", "label": "<i class=\"fas fa-teeth-open\"> Teeth-Open" },
            { "value": "fas fa-temperature-high", "label": "<i class=\"fas fa-temperature-high\"> Temperature-High" },
            { "value": "fas fa-temperature-low", "label": "<i class=\"fas fa-temperature-low\"> Temperature-Low" },
            { "value": "fas fa-tenge", "label": "<i class=\"fas fa-tenge\"> Tenge" },
            { "value": "fas fa-terminal", "label": "<i class=\"fas fa-terminal\"> Terminal" },
            { "value": "fas fa-text-height", "label": "<i class=\"fas fa-text-height\"> Text-Height" },
            { "value": "fas fa-text-width", "label": "<i class=\"fas fa-text-width\"> Text-Width" },
            { "value": "fas fa-th", "label": "<i class=\"fas fa-th\"> Th" },
            { "value": "fas fa-th-large", "label": "<i class=\"fas fa-th-large\"> Th-Large" },
            { "value": "fas fa-th-list", "label": "<i class=\"fas fa-th-list\"> Th-List" },
            { "value": "fas fa-theater-masks", "label": "<i class=\"fas fa-theater-masks\"> Theater-Masks" },
            { "value": "fas fa-thermometer", "label": "<i class=\"fas fa-thermometer\"> Thermometer" },
            { "value": "fas fa-thermometer-empty", "label": "<i class=\"fas fa-thermometer-empty\"> Thermometer-Empty" },
            { "value": "fas fa-thermometer-full", "label": "<i class=\"fas fa-thermometer-full\"> Thermometer-Full" },
            { "value": "fas fa-thermometer-half", "label": "<i class=\"fas fa-thermometer-half\"> Thermometer-Half" },
            {
                "value": "fas fa-thermometer-quarter",
                "label": "<i class=\"fas fa-thermometer-quarter\"> Thermometer-Quarter"
            },
            {
                "value": "fas fa-thermometer-three-quarters",
                "label": "<i class=\"fas fa-thermometer-three-quarters\"> Thermometer-Three-Quarters"
            },
            { "value": "fas fa-thumbs-down", "label": "<i class=\"fas fa-thumbs-down\"> Thumbs-Down" },
            { "value": "fas fa-thumbs-up", "label": "<i class=\"fas fa-thumbs-up\"> Thumbs-Up" },
            { "value": "fas fa-thumbtack", "label": "<i class=\"fas fa-thumbtack\"> Thumbtack" },
            { "value": "fas fa-ticket-alt", "label": "<i class=\"fas fa-ticket-alt\"> Ticket-Alt" },
            { "value": "fas fa-times", "label": "<i class=\"fas fa-times\"> Times" },
            { "value": "fas fa-times-circle", "label": "<i class=\"fas fa-times-circle\"> Times-Circle" },
            { "value": "fas fa-tint", "label": "<i class=\"fas fa-tint\"> Tint" },
            { "value": "fas fa-tint-slash", "label": "<i class=\"fas fa-tint-slash\"> Tint-Slash" },
            { "value": "fas fa-tired", "label": "<i class=\"fas fa-tired\"> Tired" },
            { "value": "fas fa-toggle-off", "label": "<i class=\"fas fa-toggle-off\"> Toggle-Off" },
            { "value": "fas fa-toggle-on", "label": "<i class=\"fas fa-toggle-on\"> Toggle-On" },
            { "value": "fas fa-toilet", "label": "<i class=\"fas fa-toilet\"> Toilet" },
            { "value": "fas fa-toilet-paper", "label": "<i class=\"fas fa-toilet-paper\"> Toilet-Paper" },
            { "value": "fas fa-toolbox", "label": "<i class=\"fas fa-toolbox\"> Toolbox" },
            { "value": "fas fa-tools", "label": "<i class=\"fas fa-tools\"> Tools" },
            { "value": "fas fa-tooth", "label": "<i class=\"fas fa-tooth\"> Tooth" },
            { "value": "fas fa-torah", "label": "<i class=\"fas fa-torah\"> Torah" },
            { "value": "fas fa-torii-gate", "label": "<i class=\"fas fa-torii-gate\"> Torii-Gate" },
            { "value": "fas fa-tractor", "label": "<i class=\"fas fa-tractor\"> Tractor" },
            { "value": "fas fa-trademark", "label": "<i class=\"fas fa-trademark\"> Trademark" },
            { "value": "fas fa-traffic-light", "label": "<i class=\"fas fa-traffic-light\"> Traffic-Light" },
            { "value": "fas fa-train", "label": "<i class=\"fas fa-train\"> Train" },
            { "value": "fas fa-tram", "label": "<i class=\"fas fa-tram\"> Tram" },
            { "value": "fas fa-transgender", "label": "<i class=\"fas fa-transgender\"> Transgender" },
            { "value": "fas fa-transgender-alt", "label": "<i class=\"fas fa-transgender-alt\"> Transgender-Alt" },
            { "value": "fas fa-trash", "label": "<i class=\"fas fa-trash\"> Trash" },
            { "value": "fas fa-trash-alt", "label": "<i class=\"fas fa-trash-alt\"> Trash-Alt" },
            { "value": "fas fa-trash-restore", "label": "<i class=\"fas fa-trash-restore\"> Trash-Restore" },
            { "value": "fas fa-trash-restore-alt", "label": "<i class=\"fas fa-trash-restore-alt\"> Trash-Restore-Alt" },
            { "value": "fas fa-tree", "label": "<i class=\"fas fa-tree\"> Tree" },
            { "value": "fas fa-trophy", "label": "<i class=\"fas fa-trophy\"> Trophy" },
            { "value": "fas fa-truck", "label": "<i class=\"fas fa-truck\"> Truck" },
            { "value": "fas fa-truck-loading", "label": "<i class=\"fas fa-truck-loading\"> Truck-Loading" },
            { "value": "fas fa-truck-monster", "label": "<i class=\"fas fa-truck-monster\"> Truck-Monster" },
            { "value": "fas fa-truck-moving", "label": "<i class=\"fas fa-truck-moving\"> Truck-Moving" },
            { "value": "fas fa-truck-pickup", "label": "<i class=\"fas fa-truck-pickup\"> Truck-Pickup" },
            { "value": "fas fa-tshirt", "label": "<i class=\"fas fa-tshirt\"> Tshirt" },
            { "value": "fas fa-tty", "label": "<i class=\"fas fa-tty\"> Tty" },
            { "value": "fas fa-tv", "label": "<i class=\"fas fa-tv\"> Tv" },
            { "value": "fas fa-umbrella", "label": "<i class=\"fas fa-umbrella\"> Umbrella" },
            { "value": "fas fa-umbrella-beach", "label": "<i class=\"fas fa-umbrella-beach\"> Umbrella-Beach" },
            { "value": "fas fa-underline", "label": "<i class=\"fas fa-underline\"> Underline" },
            { "value": "fas fa-undo", "label": "<i class=\"fas fa-undo\"> Undo" },
            { "value": "fas fa-undo-alt", "label": "<i class=\"fas fa-undo-alt\"> Undo-Alt" },
            { "value": "fas fa-universal-access", "label": "<i class=\"fas fa-universal-access\"> Universal-Access" },
            { "value": "fas fa-university", "label": "<i class=\"fas fa-university\"> University" },
            { "value": "fas fa-unlink", "label": "<i class=\"fas fa-unlink\"> Unlink" },
            { "value": "fas fa-unlock", "label": "<i class=\"fas fa-unlock\"> Unlock" },
            { "value": "fas fa-unlock-alt", "label": "<i class=\"fas fa-unlock-alt\"> Unlock-Alt" },
            { "value": "fas fa-upload", "label": "<i class=\"fas fa-upload\"> Upload" },
            { "value": "fas fa-user", "label": "<i class=\"fas fa-user\"> User" },
            { "value": "fas fa-user-alt", "label": "<i class=\"fas fa-user-alt\"> User-Alt" },
            { "value": "fas fa-user-alt-slash", "label": "<i class=\"fas fa-user-alt-slash\"> User-Alt-Slash" },
            { "value": "fas fa-user-astronaut", "label": "<i class=\"fas fa-user-astronaut\"> User-Astronaut" },
            { "value": "fas fa-user-check", "label": "<i class=\"fas fa-user-check\"> User-Check" },
            { "value": "fas fa-user-circle", "label": "<i class=\"fas fa-user-circle\"> User-Circle" },
            { "value": "fas fa-user-clock", "label": "<i class=\"fas fa-user-clock\"> User-Clock" },
            { "value": "fas fa-user-cog", "label": "<i class=\"fas fa-user-cog\"> User-Cog" },
            { "value": "fas fa-user-edit", "label": "<i class=\"fas fa-user-edit\"> User-Edit" },
            { "value": "fas fa-user-friends", "label": "<i class=\"fas fa-user-friends\"> User-Friends" },
            { "value": "fas fa-user-graduate", "label": "<i class=\"fas fa-user-graduate\"> User-Graduate" },
            { "value": "fas fa-user-injured", "label": "<i class=\"fas fa-user-injured\"> User-Injured" },
            { "value": "fas fa-user-lock", "label": "<i class=\"fas fa-user-lock\"> User-Lock" },
            { "value": "fas fa-user-md", "label": "<i class=\"fas fa-user-md\"> User-Md" },
            { "value": "fas fa-user-minus", "label": "<i class=\"fas fa-user-minus\"> User-Minus" },
            { "value": "fas fa-user-ninja", "label": "<i class=\"fas fa-user-ninja\"> User-Ninja" },
            { "value": "fas fa-user-nurse", "label": "<i class=\"fas fa-user-nurse\"> User-Nurse" },
            { "value": "fas fa-user-plus", "label": "<i class=\"fas fa-user-plus\"> User-Plus" },
            { "value": "fas fa-user-secret", "label": "<i class=\"fas fa-user-secret\"> User-Secret" },
            { "value": "fas fa-user-shield", "label": "<i class=\"fas fa-user-shield\"> User-Shield" },
            { "value": "fas fa-user-slash", "label": "<i class=\"fas fa-user-slash\"> User-Slash" },
            { "value": "fas fa-user-tag", "label": "<i class=\"fas fa-user-tag\"> User-Tag" },
            { "value": "fas fa-user-tie", "label": "<i class=\"fas fa-user-tie\"> User-Tie" },
            { "value": "fas fa-user-times", "label": "<i class=\"fas fa-user-times\"> User-Times" },
            { "value": "fas fa-users", "label": "<i class=\"fas fa-users\"> Users" },
            { "value": "fas fa-users-cog", "label": "<i class=\"fas fa-users-cog\"> Users-Cog" },
            { "value": "fas fa-utensil-spoon", "label": "<i class=\"fas fa-utensil-spoon\"> Utensil-Spoon" },
            { "value": "fas fa-utensils", "label": "<i class=\"fas fa-utensils\"> Utensils" },
            { "value": "fas fa-vector-square", "label": "<i class=\"fas fa-vector-square\"> Vector-Square" },
            { "value": "fas fa-venus", "label": "<i class=\"fas fa-venus\"> Venus" },
            { "value": "fas fa-venus-double", "label": "<i class=\"fas fa-venus-double\"> Venus-Double" },
            { "value": "fas fa-venus-mars", "label": "<i class=\"fas fa-venus-mars\"> Venus-Mars" },
            { "value": "fas fa-vial", "label": "<i class=\"fas fa-vial\"> Vial" },
            { "value": "fas fa-vials", "label": "<i class=\"fas fa-vials\"> Vials" },
            { "value": "fas fa-video", "label": "<i class=\"fas fa-video\"> Video" },
            { "value": "fas fa-video-slash", "label": "<i class=\"fas fa-video-slash\"> Video-Slash" },
            { "value": "fas fa-vihara", "label": "<i class=\"fas fa-vihara\"> Vihara" },
            { "value": "fas fa-volleyball-ball", "label": "<i class=\"fas fa-volleyball-ball\"> Volleyball-Ball" },
            { "value": "fas fa-volume-down", "label": "<i class=\"fas fa-volume-down\"> Volume-Down" },
            { "value": "fas fa-volume-mute", "label": "<i class=\"fas fa-volume-mute\"> Volume-Mute" },
            { "value": "fas fa-volume-off", "label": "<i class=\"fas fa-volume-off\"> Volume-Off" },
            { "value": "fas fa-volume-up", "label": "<i class=\"fas fa-volume-up\"> Volume-Up" },
            { "value": "fas fa-vote-yea", "label": "<i class=\"fas fa-vote-yea\"> Vote-Yea" },
            { "value": "fas fa-vr-cardboard", "label": "<i class=\"fas fa-vr-cardboard\"> Vr-Cardboard" },
            { "value": "fas fa-walking", "label": "<i class=\"fas fa-walking\"> Walking" },
            { "value": "fas fa-wallet", "label": "<i class=\"fas fa-wallet\"> Wallet" },
            { "value": "fas fa-warehouse", "label": "<i class=\"fas fa-warehouse\"> Warehouse" },
            { "value": "fas fa-water", "label": "<i class=\"fas fa-water\"> Water" },
            { "value": "fas fa-weight", "label": "<i class=\"fas fa-weight\"> Weight" },
            { "value": "fas fa-weight-hanging", "label": "<i class=\"fas fa-weight-hanging\"> Weight-Hanging" },
            { "value": "fas fa-wheelchair", "label": "<i class=\"fas fa-wheelchair\"> Wheelchair" },
            { "value": "fas fa-wifi", "label": "<i class=\"fas fa-wifi\"> Wifi" },
            { "value": "fas fa-wind", "label": "<i class=\"fas fa-wind\"> Wind" },
            { "value": "fas fa-window-close", "label": "<i class=\"fas fa-window-close\"> Window-Close" },
            { "value": "fas fa-window-maximize", "label": "<i class=\"fas fa-window-maximize\"> Window-Maximize" },
            { "value": "fas fa-window-minimize", "label": "<i class=\"fas fa-window-minimize\"> Window-Minimize" },
            { "value": "fas fa-window-restore", "label": "<i class=\"fas fa-window-restore\"> Window-Restore" },
            { "value": "fas fa-wine-bottle", "label": "<i class=\"fas fa-wine-bottle\"> Wine-Bottle" },
            { "value": "fas fa-wine-glass", "label": "<i class=\"fas fa-wine-glass\"> Wine-Glass" },
            { "value": "fas fa-wine-glass-alt", "label": "<i class=\"fas fa-wine-glass-alt\"> Wine-Glass-Alt" },
            { "value": "fas fa-won-sign", "label": "<i class=\"fas fa-won-sign\"> Won-Sign" },
            { "value": "fas fa-wrench", "label": "<i class=\"fas fa-wrench\"> Wrench" },
            { "value": "fas fa-x-ray", "label": "<i class=\"fas fa-x-ray\"> X-Ray" },
            { "value": "fas fa-yen-sign", "label": "<i class=\"fas fa-yen-sign\"> Yen-Sign" },
            { "value": "fas fa-yin-yang", "label": "<i class=\"fas fa-yin-yang\"> Yin-Yang" }
        ]
    })
    .constant('environments', {
        data: [{
            name: 'Development',
            value: 'development',
            selected: true,
            disabled: true
        }, {
            name: 'Test',
            value: 'test',
            selected: true,
            disabled: true
        }, {
            name: 'Production',
            value: 'production',
            selected: true,
            disabled: false
        }]
    })
    .value('guidEmpty', '00000000-0000-0000-0000-000000000000')

    .value('emailRegex', /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/);