@Integration

Feature: Payment
	As a user I can use player related functionality

Scenario: Validate 200 status code for Payment Controller GET Requests API
	Given User with valid credentials is logged in
	When I try to send GET request to Payment  Controller API <end point>
	Then The received Status Code must be 200 Successful and Response schema validated

Scenario: Validate 401 status code for Payment Controller API
	Given User with invalid credentials is logged in
	When I try to send GET request without having a valid Token to Payment Controller end point
	Then I should see unautorized response as Status Code 401