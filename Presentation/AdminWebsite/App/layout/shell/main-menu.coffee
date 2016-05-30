define (require) ->
    security = require "security/security"
    i18n = require "i18next"

# In R1.0 we will not have dashboard
#    home:
#        title: i18n.t "app:sidebar.home"
#        icon: "fa-home"
#        submenu: 
#        
#            dashboard:  
#                title: i18n.t "app:sidebar.dashboard"
#                path: "home/dashboard"


                
    admin:
        title: i18n.t "app:sidebar.admin"
        icon: "fa-user"
        submenu:
        
            adminManager:
                title: i18n.t "app:sidebar.adminManager"
                security: [
                    ["AdminManager/View"]
                ]
                container: [
                    title: "Users"
                    path: "admin/admin-manager/list"
                ]
                
            roleManager:
                title: i18n.t "app:sidebar.roleManager"
                security: [
                    ["RoleManager/View"]
                ]
                container: [
                    title: "Roles"
                    path: "admin/role-manager/list"
                ]
                
            adminIpRegulations:
                title: i18n.t "app:sidebar.adminIpRegulation"  
                security: [
                    ["BackendIpRegulationManager/View"]
                ]
                container: [
                    title: "Admin web site IP regulations"
                    path: "admin/ip-regulations/admin/list"
                ]
                
            brandIpRegulations:
                title: i18n.t "app:sidebar.brandIpRegulation"  
                security: [
                    ["BrandIpRegulationManager/View"]
                ]
                container: [
                    title: "Member web site IP regulations"
                    path: "admin/ip-regulations/brand/list"
                ]                
                
            currencyManager:
                title: i18n.t "app:currencies.currencyManager"
                security: [
                    ["CurrencyManager/View"]
                ]
                container: [
                    title: i18n.t "app:currencies.currencies"
                    path: "currency-manager/list"
                ]              
                
            cultureManager:
                title: i18n.t "app:language.manager"
                security: [
                    ["LanguageManager/View"]
                ]
                container: [
                    title: i18n.t "app:common.languages"
                    path: "culture-manager/list"
                ]                            

            countryManager:
                title: i18n.t "app:country.manager"
                security: [
                    ["CountryManager/View"]
                ]
                container: [
                    title: i18n.t "app:common.countries"
                    path: "country-manager/list"
                ] 
                
            contentTranslation:
                title: i18n.t "app:contenttranslation.title"
                security: [
                    ["TranslationManager/View"]
                ]
                container: [
                    title: i18n.t "app:contenttranslation.title"
                    path: 'brand/translation-manager/list'
                ]                                 
            
            idDocumentsSettings:
                title: "Identification Document Settings"
                security: [
                    ["IdentificationDocumentSettings/View"]
                ]
                container: [
                    title: "Identification Document Settings"
                    path: "admin/identification-document-settings/list"
                ]  
            
            adminActivityLog:
                title: i18n.t "app:admin.adminActivityLog.title"
                security: [
                    ["AdminActivityLog/View"]
                ]
                container: [
                    title: i18n.t "app:admin.adminActivityLog.title"
                    path: "admin/admin-activity-log/list"
                ]                                  

            adminAuthenticationLog:
                title: i18n.t "app:admin.authenticationLog.adminTitle"
                security: [
                    ["AdminAuthenticationLog/View"]
                ]
                container: [
                    title: i18n.t "app:admin.authenticationLog.adminTitle"
                    path: "admin/admin-authentication-log/list"
                ]

            memberAuthenticationLog:
                title: i18n.t "app:admin.authenticationLog.memberTitle"
                security: [
                    ["MemberAuthenticationLog/View"]
                ]
                container: [
                    title: i18n.t "app:admin.authenticationLog.memberTitle"
                    path: "admin/member-authentication-log/list"
                ]

            errorManager:
                title: i18n.t "app:sidebar.errorsManager"
                security: [
                    ["ErrorManager/View"]
                ]

            baseSettings:
                title: i18n.t "app:sidebar.baseSetting"
                security: [
                    ["BaseSetting/View"]
                ]
                
            ipControl:
                title: i18n.t "app:sidebar.ipControl"
                security: [
                    ["IPControl/View"]
                ]                
                
            gameList:
                title: i18n.t "app:sidebar.gameList"
                security: [
                    ["GameList/View"]
                ]
                
            domainControl:
                title: i18n.t "app:sidebar.domainControl"
                security: [
                    ["DomainControl/View"]
                ]
                
    playerManager:
        title: i18n.t "app:sidebar.playerManager"
        icon: "fa-gamepad"
        submenu:
        
            playerManager:
                title: i18n.t "app:playerManager.playerManager"
                security: [
                    ["PlayerManager/View"]
                ]
                container: [
                    title: i18n.t "app:playerManager.container.players"
                    path: "player-manager/list"
                ]
                
            paymentLevelSettings:
                title: i18n.t "app:playerManager.paymentLevelSettings.title"
                security: [
                    ["PaymentLevelSettings/View"]
                ]
                container: [
                    title: i18n.t "app:playerManager.paymentLevelSettings.title"
                    path: "player-manager/payment-level-settings/list"
                ]

    # In R1.0 we will not have wallets
    #wallet:
    #    title: i18n.t "app:wallet.menu.wallet"
    #    icon: "fa-credit-card"
    #    submenu:
    #        walletManager:
    #            title: i18n.t "app:wallet.menu.walletManager"
    #            security: [
    #                ["WalletManager/View"]
    #            ]
    #            container: [
    #                title: i18n.t "app:wallet.menu.walletManager"
    #                path: "wallet/manager/list"
    #            ]
    
    withdrawal:
        title: "Withdrawal"
        icon: "fa-credit-card"
        submenu:
            #verificationQueue:
            #    title: i18n.t "Verification Queue"
            #    security: [
            #        ["OfflineWithdrawalWagerCheck/View"]
            #    ]
            #    container: [
            #        title: i18n.t "Verification Queue"
            #        path: "withdrawal/verification-queue-list"
            #    ]
            #onHoldQueue:
            #    title: i18n.t "On Hold Queue"
            #    security: [
            #        ["OfflineWithdrawalWagerCheck/View"]
            #    ]
            #    container: [
            #        title: "On Hold Queue"
            #        path: "withdrawal/on-hold-queue-list"
            #    ]
            offlineWithdrawalAcceptance:
                title: i18n.t "Acceptance Queue"
                security: [
                    ["OfflineWithdrawalAcceptance/View"]
                ]
                container: [
                    title: "Acceptance Queue"
                    path: "withdrawal/accept-queue-list"
                ]
                
            offlineWithdrawalApproval:
                title: i18n.t "Release Queue"
                security: [
                    ["OfflineWithdrawalAcceptance/View"]
                ]
                container: [
                    title: "Release Queue"
                    path: "withdrawal/release-queue-list"
                ]
    payment:
        title: i18n.t "app:sidebar.payment"
        icon: "fa-credit-card"
        submenu:
            banks:
                title: i18n.t "app:payment.banks"
                security: [
                    ["Banks/View"]
                ]
                container: [
                    title: i18n.t "app:payment.banks"
                    path: "payments/banks/list"
                ]

            bankAccounts:
                title: i18n.t "app:payment.bankAccounts"
                security: [
                    ["BankAccounts/View"]
                ]
                container: [
                    title: i18n.t "app:payment.bankAccounts"
                    path: "payments/bank-accounts/list"
                ]
                
            playerBankAccountVerify:
                title: i18n.t "app:sidebar.playerBankAccountVerify"
                security: [
                    ["PlayerBankAccount/View"]
                ]
                container: [
                    title: i18n.t "app:sidebar.playerBankAccountVerify"
                    path: "payments/player-bank-accounts/pending-list"
                ]                

            offlineDepositRequests:
                title: i18n.t "app:common.offlineDepositConfirm"
                security: [
                    ["OfflineDepositRequests/View"]
                ]
                container: [
                    title: i18n.t "app:common.offlineDepositConfirm"
                    path: "player-manager/offline-deposit/requests"
                ]

            playerDepositVerify:
                title: i18n.t "app:sidebar.playerDepositVerify"
                security: [
                    ["DepositVerification/View"]
                ]
                container: [
                    title: i18n.t "app:sidebar.playerDepositVerify"
                    path: "player-manager/offline-deposit/verifyRequests"
                ]

            playerDepositApprove:
                title: i18n.t "app:sidebar.playerDepositApprove"
                security: [
                    ["DepositApproval/View"]
                ]
                container: [
                    title: i18n.t "app:sidebar.playerDepositApprove"
                    path: "player-manager/offline-deposit/approveRequests"
                ]

            levelManager:
                title: i18n.t "app:payment.levelManager"
                security: [
                    ["PaymentLevelManager/View"]
                ]
                container: [
                    title: i18n.t "app:payment.levelManager"
                    path: "payments/level-manager/list"
                ]
            paymentSettings:
                title: i18n.t "app:payment.paymentSettings"
                security: [
                    ["PaymentSettings/View"]
                ]
                container: [
                    title: i18n.t "app:payment.paymentSettings"
                    path: "payments/settings/list"
                ]
            # In R1.0 we will not have transfersettings
            #transfersettings:
            #    title: i18n.t "app:payment.transfersettings"
            #    security: [
            #        ["TransferSettings/View"]
            #    ]
            #    container: [
            #        title: i18n.t "app:payment.transfersettings"
            #        path: "payments/transfer-settings/list"
            #    ]
             paymentGatewaySettings:
                title: i18n.t "app:payment.paymentGatewaySettings"
                security: [
                    ["PaymentGatewaySettings/View"]
                ]
                container: [
                    title: i18n.t "app:payment.paymentGatewaySettings"
                    path: "payments/payment-gateway-settings/list"
                ]                

    bonus:
        title: i18n.t "app:sidebar.bonus"
        icon: "fa-gift"
        submenu:
        
            bonusTemplateManager:
                title: i18n.t "app:bonus.templateManager.bonusTemplateManager"
                security: [
                    ["BonusTemplateManager/View"]
                ]
                container: [
                    title: i18n.t "app:bonus.templateManager.templates"
                    path: "bonus/template-manager/list"
                ]
        
            bonusManager:
                title: i18n.t "app:bonus.bonusManager.bonusManager"
                security: [
                    ["BonusManager/View"]
                ]
                container: [
                    title: i18n.t "app:bonus.bonusManager.bonuses"
                    path: "bonus/bonus-manager/list"
                ]

 
    report:
        title: i18n.t "app:sidebar.report"
        icon: "fa-bar-chart-o"
        submenu:
        
            playerReports:
                title: i18n.t "app:report.playerReports.title"
                security: [
                    ["PlayerReport/View"]
                    #["PlayerBetHistoryReport/View"]
                ]
                container: [
                    title: i18n.t "app:report.playerReports.title"
                    path: "reports/list"
                    data: "player"
                ]

            paymentReports:
                title: i18n.t "app:report.paymentReports.title"
                security: [
                    ["DepositReport/View"]
                ]
                container: [
                    title: i18n.t "app:report.paymentReports.title"
                    path: "reports/list"
                    data: "payment"
                ]

            #This is handled through UGS - AFTREGO-4642
            #gameReports:
            #    title: i18n.t "app:report.gameReports.title"
            #    container: [
            #        title: i18n.t "app:report.gameReports.title"
            #        path: "reports/list"
            #        data: "game"
            #    ]

            #securityReports:
            #    title: i18n.t "app:report.securityReports.title"
            #    container: [
            #        title: i18n.t "app:report.securityReports.title"
            #        path: "reports/list"
            #        data: "security"
            #    ]

            brandReports:
                title: i18n.t "app:report.brandReports.title"
                security: [
                    ["BrandReport/View"]
                    ["LicenseeReport/View"]
                    #["LanguageReport/View"]
                    ["VipLevelReport/View"]
                ]
                container: [
                    title: i18n.t "app:report.brandReports.title"
                    path: "reports/list"
                    data: "brand"
                ]

            bonusReports:
                title: i18n.t "app:report.bonusReports.title"
                container: [
                    title: i18n.t "app:report.bonusReports.title"
                    path: "reports/list"
                    data: "bonus"
                ]


    affiliate:
        title: i18n.t "app:sidebar.affiliate"
        icon: "fa-smile-o"  


    #fraud:
    #    title: i18n.t "app:sidebar.fraud"
    #    icon: "fa-credit-card"
    #    
    #    submenu:
    #        fraudManager:
    #            title: i18n.t "app:fraud.menu.manager"
    #            security: [
    #                ["FraudManager/View"]
    #            ]
    #            container: [
    #                title: i18n.t "app:fraud.manager.title.list"
    #                path: "fraud/manager/list"
    #            ]
    #        wagerManager:
    #            title: "Auto Wager Check Configuration"
    #            security: [
    #                ["WagerConfiguration/View"]
    #            ]
    #            container: [
    #                title: i18n.t "Auto Wager Check Configuration Manager"
    #                path: "fraud/wager/list"
    #            ]
    #        duplicateManager:
    #            title: "Duplicate Mechanism"
    #            #security: [
    #            #    ["DuplicateMechanism/View"]
    #            #]
    #            container: [
    #                title: i18n.t "Duplicate Mechanism Configuration Manager"
    #                path: "fraud/duplicate-mechanism/list"
    #            ]
    #        verificationManager:
    #            title: "Auto Verification Configuration"
    #            security: [
    #                ["AutoVerificationConfiguration/View"]
    #            ]
    #            container: [
    #                title: i18n.t "Auto Verification Configuration Manager"
    #                path: "fraud/verification/list"
    #            ]
    #        riskProfileCheckManager:
    #            title: "Risk Profile Check Configuration"
    #            security: [
    #                ["RiskProfileCheckConfiguration/View"]
    #            ]
    #            container: [
    #                title: i18n.t "Risk Profile Check Configuration Manager"
    #                path: "fraud/risk-profile-check/list"
    #            ]
    #        signUpFraudTypes:
    #            title: "Sign Up Fraud Types"
    #            security: [
    #                ["SignUpFraudTypes/View"]
    #            ]
    #            container: [
    #                title: i18n.t "Sign Up Fraud Types"
    #                path: "fraud/signup-fraud-types/list"
    #            ]
    #        signUpQueue:
    #            title: "Sign Up Queue"
    #            security: [
    #                ["RiskProfileCheckConfiguration/View"]
    #            ]
    #            container: [
    #                title: "Sign Up Queue"
    #                path: "fraud/sign-up-queue/list"
    #            ]

    brand:
        title: i18n.t "app:common.brand"
        icon: "fa-tags"
        submenu:
        
            brandManager:
                title: i18n.t "app:brand.brandManager"
                security: [
                    ["BrandManager/View"]
                ]
                container: [
                    title: i18n.t "app:brand.brands"
                    path: "brand/brand-manager/list"
                ]
                
             vipManager:
                title: i18n.t "app:vipLevel.manager"
                security: [
                    ["VipLevelManager/View"]
                ]
                container: [
                    title: i18n.t "app:vipLevel.levels"
                    path: "vip-manager/list"
                ]                
                
            supportedProducts:
                title: i18n.t "app:product.supportedProducts"
                security: [
                    ["SupportedProducts/View"]
                ]
                container: [
                    title: i18n.t "app:product.supportedProducts"
                    path: 'brand/product-manager/list'
                ]
                
            supportedCurrencies:
                title: i18n.t "app:currencies.supportedCurrencies"
                security: [
                    ["SupportedCurrencies/View"]
                ]
                container: [
                    title: i18n.t "app:currencies.supportedCurrencies"
                    path: 'brand/currency-manager/list'
                ]
                
            supportedCountries:
                title: i18n.t "app:country.supportedCountries"
                security: [
                    ["SupportedCountries/View"]
                ]
                container: [
                    title: i18n.t "app:country.supportedCountries"
                    path: 'brand/country-manager/list'
                ]
                
            supportedCultures:
                title: i18n.t "app:language.supportedLanguages"
                security: [
                    ["SupportedLanguages/View"]
                ]
                container: [
                    title: i18n.t "app:language.supportedLanguages"
                    path: 'brand/culture-manager/list'
                ]
                
            #currencyExchange:
            #    title: i18n.t "app:currencies.currencyExchange"
            #    security: [
            #        ["ExchangeRateManager/View"]
            #    ]
            #    container: [
            #        title: i18n.t "app:currencies.currencyExchange"
            #        path: 'brand/currencyexchange-manager/list'
            #    ]                

    currencyManagerAdv:
        title: i18n.t "app:currencies.currencyManager"
        icon: "fa-tags"
        submenu:
        
            

            exchangeRate:
                title: i18n.t "app:currencies.exchangeRate"                

    licensee:
        title: i18n.t "app:common.licensee"
        icon: "fa-certificate"
        submenu:
            
            licenseeManager:
                title: i18n.t "app:licensee.manager"
                security: [
                    ["LicenseeManager/View"]
                ]
                container: [
                    title: i18n.t "app:common.licensees"
                    path: "licensee-manager/list"
                ]
                
    messaging:
        title: i18n.t "app:messaging.messaging"
        icon: "fa-envelope-o"
        submenu:

            messageTemplateManager:
                title: i18n.t "app:messageTemplates.manager"
                security: [
                    ["MessageTemplateManager/View"]
                ]
                container: [
                    title: i18n.t "app:messageTemplates.messageTemplates"
                    path: "messaging/message-templates/list"
                ]
                
            massMessages:
                title: i18n.t "app:messaging.massMessage.massMessages"
                security: [
                    ["MassMessageTool/Send"]
                ]
                container: [
                    title: i18n.t "app:messaging.massMessage.history"
                    path: "messaging/mass-message/history"
                ]
    products:
        title: i18n.t "product.product"
        icon: "fa-puzzle-piece"
        submenu:
            games:
                title: i18n.t "product.gamesManager"
                security: [
                    ["GameManager/View"]
                ]
                container: [
                    title: i18n.t "product.gamesManager"
                    path: "product/games-manager/list"                ]
            products:
                title: i18n.t "product.productManager"
                security: [
                    ["ProductManager/View"]
                ]
                container: [
                    title: i18n.t "product.productManager"
                    path: "product/products-manager/list"          ]
            betLevels:
                title: i18n.t "product.betLevels"
                security: [
                    ["BetLevels/View"]
                ]
                container: [
                    title: i18n.t "product.betLevels"
                    path: "product/bet-levels/list"            ]

