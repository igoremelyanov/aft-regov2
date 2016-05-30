@Integration
Feature: Core Game Api
	Calling Core Game API 

Scenario: Get and validate authentication token
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
	When I call to validate token
		Then I will receive successful validation result

Scenario: Get playable balance
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $11
	When the player "testplayer" withdraws $11
		And I get balance
	Then the player's playable balance will be $0

Scenario: Place bet when playable balance is $0 
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $0
	When I bet $5 for game "RL-MOCK"
		Then I will get error code "InsufficientFunds"
	When I get balance
		Then the player's playable balance will be $0
		# in other words the balance will not be changed 
		And requested bet will not be recorded	
		And place bet response balance will be $0

Scenario: Place bet when playable balance is insufficient
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $1000
	When I bet $1500 for game "RL-MOCK"
		Then I will get error code "InsufficientFunds"
	When the player "testplayer" withdraws $1000
		And I get balance
	Then the player's playable balance will be $0
		# in other words the balance will not be changed
		And requested bet will not be recorded	

Scenario: Place bet when playable balance is sufficient
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $10
	When I get balance
		Then the player's playable balance will be $10
	When I bet $5 for game "RL-MOCK"
		Then I will get error code "NoError"
	When the player "testplayer" withdraws $5
		And I get balance
	Then the player's playable balance will be $0
		And requested bet will be recorded	
		And place bet response balance will be $5
	
Scenario: Place and win bet
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $20
	When I bet $15 for game "RL-MOCK"
		Then I will get error code "NoError"
	When I win $25
		Then I will get error code "NoError"
	When the player "testplayer" withdraws $30
		And I get balance
		Then the player's playable balance will be $0
		And requested bet will be recorded

Scenario: Place and win multiple bets
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $20
	When I bet $15 for game "RL-MOCK" 
	#20-15=5
		Then I will get error code "NoError"
	When I win multiple amounts:
		| Amount|
		| 3.0   |
		| 12.0   |
		| 27.0   |
		Then I will get error code "NoError"
		And win bet response balance will be $47
		# 5 + 3 + 12 + 27 = 47
	When the player "testplayer" withdraws $47
		And I get balance
	Then the player's playable balance will be $0
		And requested bet will be recorded


Scenario: Place and "win" less than what was placed
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $20
	When I bet $15 for game "RL-MOCK"
		Then I will get error code "NoError"
	When I win $5
		Then I will get error code "NoError"
	When the player "testplayer" withdraws $10
		And I get balance
	Then the player's playable balance will be $0
		And requested bet will be recorded

Scenario: Place and lose bet
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $20
	When I bet $15 for game "RL-MOCK"
		Then I will get error code "NoError"
	When I lose the bet
		Then I will get error code "NoError"
	When the player "testplayer" withdraws $5
		And I get balance
	Then the player's playable balance will be $0
		And requested bet will be recorded

Scenario: Get free bet
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $20
	When I get free bet for $15 for game "RL-MOCK"
		Then I will get error code "NoError"
	When the player "testplayer" withdraws $35
		And I get balance
	Then the player's playable balance will be $0
		And requested bet will be recorded

Scenario: Settle batch of bets
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $20
	When I place bets for game "RL-MOCK" for amount:
		| Amount|
		| 5.0   |
		| 5.0   |
		| 5.0   |
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $5
	When I settle the following bets with "MOCK_CASINO_CLIENT_ID_SECURITY_KEY":
		| Type | Amount |
		| WIN  | 400.0   |
		| LOSE |  0.0   |
		| WIN  |  32.0   |
		# note that lose bet must always be of amount=0
		Then I will get error code "NoError"
	When the player "testplayer" withdraws $437
		And I get balance
	Then the player's playable balance will be $0
		# expect balance: 20 - 5 - 5 - 5 + 400 + 32 = 437
		And requested bets will be recorded


Scenario: Settle batch of bets for multiple players
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $20
	When I call to validate token
		Then I will receive successful validation result
	When "testplayer" bets $5 for game "RL-MOCK"
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $15
	Given I get authentication token for player "demoplayer" with password "123456"
		And the player "demoplayer" main balance is $70
	When I call to validate token
		Then I will receive successful validation result
	When "demoplayer" bets $45 for game "RL-MOCK"
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $25
	When I batch settle the following bets with "MOCK_CASINO_CLIENT_ID_SECURITY_KEY":
		| PlayerName     | Type | Amount |
		| testplayer | WIN  | 400.0  |
		| demoplayer | WIN  |  3.0   |
		| testplayer | WIN  | 543.0  |
		| demoplayer | WIN  |  2.0   |
		| testplayer | WIN  | 234.0  |
		| demoplayer | WIN  |  7.0   |
		| testplayer | WIN  | 664.0  |
		| demoplayer | WIN  |  11.0  |
		# note that lose bet must always be of amount=0
		Then I will get error code "NoError"
	When the player "testplayer" withdraws $1856
		And the player "demoplayer" withdraws $48
	Then the response's playable balance for "testplayer" will be $1856
		And the response's playable balance for "demoplayer" will be $48

Scenario: Settle batch of bets with invalid security key
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $20
	When I place bets for game "RL-MOCK" for amount:
		| Amount|
		| 5.0   |
		| 5.0   |
		| 5.0   |
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $5
	When I settle the following bets with "INVALID_SECURITY_KEY":
		| Type | Amount |
		| WIN  | 400.0   |
		| LOSE |  0.0   |
		| WIN  |  32.0   |
		# note that lose bet must always be of amount=0
		Then I will get error code "InvalidArguments"
	When the player "testplayer" withdraws $5
		And I get balance
	Then the player's playable balance will be $0
		And requested bets will be recorded


Scenario: Getting bet history
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $100
	When I bet $15 for game "RL-MOCK"
		Then I will get error code "NoError"
	When I bet $5 for game "RL-MOCK"
		Then I will get error code "NoError"
	When I get history for game "RL-MOCK"
		Then I will see the bet IDs in the history
	When the player "testplayer" withdraws $80
		And I get balance
	Then the player's playable balance will be $0

Scenario: Cancel transaction of placing bet
	Given I get authorization token for game provider "MOCK_CASINO_CLIENT_ID" with secret "MOCK_CLIENT_SECRET"
		And I get authentication token for player "testplayer" with password "123456"
		And the player "testplayer" main balance is $100
	When I bet $15 for game "RL-MOCK"
		Then I will get error code "NoError"
	When I cancel the last transaction
		Then I will get error code "NoError"
	When the player "testplayer" withdraws $100
		And I get balance
	Then the player's playable balance will be $0

#
# UNCOMMENT WHEN WALLET ADJUST IS DONE		
#
#Scenario: Adjust transaction by adding amount
#	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
#		And I validate the token
#		And the player "testplayer" main balance is $100
#	When I bet $15
#		Then I will get error code "NoError"
#	When I adjust transaction with $5
#		Then I will get error code "NoError"
#	When I get balance
#		# 100 - 15 + 5 = 90
#		Then the player's playable balance will be $90
#
#Scenario: Adjust transaction by removing amount
#	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
#		And I validate the token
#		And the player "testplayer" main balance is $100
#	When I bet $15
#		Then I will get error code "NoError"
#	When I adjust transaction with $-5
#		Then I will get error code "NoError"
#	When I get balance
#		# 100 - 15 - 5 = 80
#		Then the player's playable balance will be $80
#