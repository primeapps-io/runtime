'use strict';

angular.module('primeapps')

    .constant('taskDate', {
        today: new Date().setHours(23, 59, 59, 0),
        tomorrow: new Date().setHours(47, 59, 59, 0),
        nextWeek: new Date().setHours(191, 59, 59, 0),
        future: new Date('2100-12-12').getTime()
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
        before_list_request: 17
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
        { name: 'Before List Request', value: 17 }
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
                tr: 'Tag'
            },
            operators: [
                'contains',
                'not_contain',
                'is',
                'is_not',
                'empty',
                'not_empty'
            ],
            order: 20
        }
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
    .value('guidEmpty', '00000000-0000-0000-0000-000000000000')

    .value('emailRegex', /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/);