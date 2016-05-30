@Integration

Feature: Payment
    As a user I can manage Payment related functionality

#CommonTests
Scenario: Can not execute permission protected payment methods
    Given I am logged in with insufficientPermissions for payment
    Then I can not execute protected payment methods with insufficient permissions

Scenario: Can not execute payment methods using GET
    Given I am logged in and have access token
    Then I am not allowed to execute payment methods using GET

Scenario: Can not execute payment methods with invalid token
    Given I am not logged in and I do not have valid token
    Then I am unauthorized to execute payment methods with invalid token

#End of CommonTests

#PaymentGatewaySettings/Add
Scenario: Can add new PaymentGatewaySettings
    Given I am logged in and have access token
    Then New PaymentGatewaySettings is successfully added

Scenario: Can not add PaymentGatewaySettings with invalid brand
    Given I am logged in and have access token   
    Then I am forbidden to add PaymentGatewaySettings with invalid brand

Scenario: Can not add PaymentGetewaySettings with existed onlinePaymentMethodName
    Given I am logged in and have access token
      Then New PaymentGatewaySettings is successfully added
      When Add PaymentGatewaySettings with existed onlinePaymentMethodName
      Then The PaymentGatewaySettings can not be saved due to OnlinePaymentMethodNameAlreadyExists

Scenario: Can not add PaymentGetewaySettings with the same settings
    Given I am logged in and have access token
      Then New PaymentGatewaySettings is successfully added
      When Add PaymentGatewaySettings with the same settings
      Then The PaymentGatewaySettings can not be saved due to PaymentGatewaySettingAlreadyExists

#PaymentGatewaySettings/Edit
Scenario: Can edit PaymentGatewaySettings
    Given I am logged in and have access token
    When New deactivated PaymentGatewaySettings is created
    Then PaymentGatewaySettings data is successfully edited

Scenario: Can not edit PaymentGatewaySettings with invalid brand
    Given I am logged in and have access token
    When New deactivated PaymentGatewaySettings is created
    Then I am forbidden to edit PaymentGatewaySettings with invalid brand

Scenario: Can not edit PaymentGetewaySettings with wrong Id
    Given I am logged in and have access token
      Then New PaymentGatewaySettings is successfully added
      When Edit the PaymentGatewaySettings with wrong id
      Then The PaymentGatewaySettings can not be saved due to NotFound

#PaymentGatewaySettings/Activate
Scenario: Can activate PaymentGatewaySettings
    Given I am logged in and have access token
    When New deactivated PaymentGatewaySettings is created
    Then PaymentGatewaySettings is successfully activated

Scenario: Can not activate PaymentGatewaySettings with invalid brand
    Given I am logged in and have access token
    When New deactivated PaymentGatewaySettings is created
    Given New user with Activate permission in PaymentGatewaySettings module login for payment    
    Then I am forbidden to activate PaymentGatewaySettings with invalid brand

Scenario: Can not activate PaymentGetewaySettings with wrong Id
    Given I am logged in and have access token
      Then New PaymentGatewaySettings is successfully added
      When Activate the PaymentGatewaySettings with wrong id
      Then The PaymentGatewaySettings can not be activated due to NotFound

#PaymentGatewaySettings/Deactivate
Scenario: Can deactivate PaymentGatewaySettings
    Given I am logged in and have access token
    When New activated PaymentGatewaySettings is created
    Then PaymentGatewaySettings is successfully deactivated

Scenario: Can not deactivate PaymentGatewaySettings with invalid brand
    Given I am logged in and have access token
    When New activated PaymentGatewaySettings is created
    Given New user with Deactivate permission in PaymentGatewaySettings module login for payment    
    Then I am forbidden to deactivate PaymentGatewaySettings with invalid brand

Scenario: Can not deactivate PaymentGetewaySettings with wrong Id
    Given I am logged in and have access token
      Then New PaymentGatewaySettings is successfully added
      When Deactivate the PaymentGatewaySettings with wrong id
      Then The PaymentGatewaySettings can not be deactivated due to NotFound

#PaymentGatewaySettings/GetById
Scenario: Can Get PaymentGatewaySettings
    Given I am logged in and have access token
    Then New PaymentGatewaySettings is successfully added
    Then The PaymentGatewaySettings is visible to me

#PaymentGatewaySettings/GetPaymentGateways
Scenario: Can get PaymentGateways
    Given I am logged in and have access token    
    Then The PaymentGateways is visible to me

Scenario: Can not get PaymentGateways with invalid brand
    Given I am logged in and have access token
    When New deactivated PaymentGatewaySettings is created
    Given New user with View permission in PaymentGatewaySettings module login for payment    
    Then I am forbidden to get PaymentGateways with invalid brand
#End of PaymentGatewaySettingsController

#OnlineDepositController
#OnlineDeposit/GetById
Scenario: Can Get OnlineDeposit
    Given I am logged in and have access token
    When New player is created with default brand
    And New OnlineDeposit is created
    Then The OnlineDeposit is visible to me

 Scenario: Can not get OnlineDeposit with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New OnlineDeposit is created
    Given New user with Verify permission in DepositVerification module login for payment    
    Then I am forbidden to get OnlineDeposit with invalid brand

#OnlineDeposit/Verify
Scenario: Can verify OnlineDeposit
    Given I am logged in and have access token
    When New player is created with default brand
    And New OnlineDeposit is created
    Then OnlineDeposit is successfully verified

Scenario: Can not verify OnlineDeposit with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New OnlineDeposit is created
    Given New user with Verify permission in DepositVerification module login for payment    
    Then I am forbidden to verify OnlineDeposit with invalid brand

#OnlineDeposit/Reject
Scenario: Can reject OnlineDeposit
    Given I am logged in and have access token
    When New player is created with default brand
    And New OnlineDeposit is created
    Then OnlineDeposit is successfully rejected

Scenario: Can not reject OnlineDeposit with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New OnlineDeposit is created
    Given New user with Reject permission in DepositVerification module login for payment    
    Then I am forbidden to reject OnlineDeposit with invalid brand

#OnlineDeposit/Approve
Scenario: Can approve OnlineDeposit
    Given I am logged in and have access token
    When New player is created with default brand
    And New OnlineDeposit is created
    And OnlineDeposit is verified
    Then OnlineDeposit is successfully approved   

Scenario: Can not approve OnlineDeposit with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New OnlineDeposit is created
    Given New user with Approve permission in DepositApproval module login for payment
    Then I am forbidden to approve OnlineDeposit with invalid brand

#End of OnlineDepositController

#BanksController
#Banks/Add
Scenario: Can add new Bank
    Given I am logged in and have access token
    Then New Bank is successfully added

Scenario: Can not add Bank with invalid brand
    Given I am logged in and have access token   
    Then I am forbidden to add Bank with invalid brand

#Banks/Edit
Scenario: Can edit Bank
    Given I am logged in and have access token
    When New Bank is created
    Then Bank data is successfully edited       

Scenario: Can not edit Bank with invalid brand
    Given I am logged in and have access token
    When New Bank is created
    Then I am forbidden to edit Bank with invalid brand

#Banks/GetById
Scenario: Can Get Bank
    Given I am logged in and have access token
    When New Bank is created
    Then The Bank is visible to me

 Scenario: Can not get Bank with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
     When New Bank is created
    Given New user with Verify permission in DepositVerification module login for payment    
    Then I am forbidden to get Bank with invalid brand
#End of BanksController


#BankAccountsController
#BankAccounts/Add
Scenario: Can add new BankAccount
    Given I am logged in and have access token
    Then New BankAccount is successfully added

Scenario: Can not add BankAccount with invalid brand
    Given I am logged in and have access token   
    Then I am forbidden to add BankAccount with invalid brand

#BankAccounts/Edit
Scenario: Can edit BankAccount
    Given I am logged in and have access token
    When New deactivated BankAccount is created
    Then BankAccount data is successfully edited

Scenario: Can not edit BankAccount with invalid brand
    Given I am logged in and have access token
    When New deactivated BankAccount is created
    Then I am forbidden to edit BankAccount with invalid brand

#BankAccounts/GetById
Scenario: Can Get BankAccount
    Given I am logged in and have access token
    When New deactivated BankAccount is created
    Then The BankAccount is visible to me

 Scenario: Can not get BankAccount with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
     When New deactivated BankAccount is created
    Given New user with View permission in BankAccounts module login for payment    
    Then I am forbidden to get BankAccount with invalid brand

#BankAccounts/Activate
Scenario: Can activate BankAccount
    Given I am logged in and have access token
    When New deactivated BankAccount is created
    Then BankAccount is successfully activated

Scenario: Can not activate BankAccount with invalid brand
    Given I am logged in and have access token
    When New deactivated BankAccount is created
    Given New user with Deactivate permission in BankAccounts module login for payment    
    Then I am forbidden to activate BankAccount with invalid brand

#BankAccounts/Deactivate
Scenario: Can deactivate BankAccount
    Given I am logged in and have access token
    When New activated BankAccount is created
    Then BankAccount is successfully deactivated

Scenario: Can not deactivate BankAccount with invalid brand
    Given I am logged in and have access token
    When New activated BankAccount is created
    Given New user with Deactivate permission in BankAccounts module login for payment    
    Then I am forbidden to deactivate BankAccount with invalid brand
#End of BankAccountsController

#PlayerBankAccountsController
#PlayerBankAccount/Verify
Scenario: Can verfiy PlayerBankAccount
    Given I am logged in and have access token
    When New player is created with default brand
    And New pending PlayerBankAccount is created
    Then PlayerBankAccount is successfully verified

Scenario: Can not verfiy PlayerBankAccount with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New pending PlayerBankAccount is created
    Given New user with Verify permission in PlayerBankAccount module login for payment    
    Then I am forbidden to verify PlayerBankAccount with invalid brand

#PlayerBankAccount/Reject
Scenario: Can reject PlayerBankAccount
    Given I am logged in and have access token
    When New player is created with default brand
    And New pending PlayerBankAccount is created
    Then PlayerBankAccount is successfully rejected

Scenario: Can not reject PlayerBankAccount with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New pending PlayerBankAccount is created
    Given New user with Reject permission in PlayerBankAccount module login for payment    
    Then I am forbidden to reject PlayerBankAccount with invalid brand

#End of PlayerBankAccountsController

#OfflineDepositController
#OfflineDeposit/GetById
Scenario: Can Get OfflineDeposit
    Given I am logged in and have access token
    When New player is created with default brand
    And New OfflineDeposit is created
    Then The OfflineDeposit is visible to me

#OfflineDeposit/Create
Scenario: Can create OfflineDeposit
    Given I am logged in and have access token
    When New player is created with default brand  
    And New activated BankAccount is created  
    Then OfflineDeposit is successfully created

Scenario: Can not create OfflineDeposit with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand  
    And New activated BankAccount is created   
    Given New user with Create permission in OfflineDepositRequests module login for payment   
    Then I am forbidden to create OfflineDeposit with invalid brand

#OfflineDeposit/Confirm
Scenario: Can confirm OfflineDeposit
    Given I am logged in and have access token
    When New player is created with default brand    
    And New OfflineDeposit is created
    Then OfflineDeposit is successfully confirmed

Scenario: Can not confirm OfflineDeposit with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New OfflineDeposit is created
    Given New user with Confirm permission in OfflineDepositConfirmation module login for payment
    Then I am forbidden to confirm OfflineDeposit with invalid brand

#OfflineDeposit/Verify
Scenario: Can verfiy OfflineDeposit
    Given I am logged in and have access token
    When New player is created with default brand    
    And New OfflineDeposit is created
    And OfflineDeposit is confirmed
    Then OfflineDeposit is successfully verified

Scenario: Can not verfiy OfflineDeposit with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New OfflineDeposit is created
    And OfflineDeposit is confirmed
    Given New user with Verify permission in DepositVerification module login for payment
    Then I am forbidden to verify OfflineDeposit with invalid brand

#OfflineDeposit/Unverify
Scenario: Can unverifiy OfflineDeposit
    Given I am logged in and have access token
    When New player is created with default brand    
    And New OfflineDeposit is created
    And OfflineDeposit is confirmed
    Then OfflineDeposit is successfully unverified

Scenario: Can not unverifiy OfflineDeposit with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New OfflineDeposit is created
    And OfflineDeposit is confirmed
    Given New user with Unverify permission in DepositVerification module login for payment
    Then I am forbidden to unverify OfflineDeposit with invalid brand

#OfflineDeposit/Approve
Scenario: Can approve OfflineDeposit
    Given I am logged in and have access token
    When New player is created with default brand    
    And New OfflineDeposit is created
    And OfflineDeposit is confirmed
    And OfflineDeposit is verified
    Then OfflineDeposit is successfully approved

Scenario: Can not approve OfflineDeposit with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New OfflineDeposit is created
    And OfflineDeposit is confirmed
    And OfflineDeposit is verified
    Given New user with Approve permission in DepositApproval module login for payment
    Then I am forbidden to approve OfflineDeposit with invalid brand

#OfflineDeposit/Reject
Scenario: Can reject OfflineDeposit
    Given I am logged in and have access token
    When New player is created with default brand    
    And New OfflineDeposit is created
    And OfflineDeposit is confirmed
    And OfflineDeposit is verified
    Then OfflineDeposit is successfully rejected

Scenario: Can not reject OfflineDeposit with invalid brand
    Given I am logged in and have access token
    When New player is created with default brand
    And New OfflineDeposit is created
    And OfflineDeposit is confirmed
    And OfflineDeposit is verified
    Given New user with Reject permission in DepositApproval module login for payment
    Then I am forbidden to reject OfflineDeposit with invalid brand

#End of OfflineDepositController