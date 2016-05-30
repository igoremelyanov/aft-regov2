@Integration

Feature: Security
	As a software tester I want Security Controller POST requests refactored  
	So that proper status code are shown

Scenario: Validate Response status code for valid parameters
	Given Anonymous request allowed, valid IP address and brandName
	When I try to send request to VerifyIp end point and get Response
		Then I should see Status Code 200
		Then I should see successfull VerifyIp response 

Scenario: Validate Response error code for valid IpAddress and invalid brand
	Given Anonymous request allowed, valid IP address and invalid brand
	When I try to send request to VerifyIp end point and get Response
		Then I should see Status Code 400
		Then I should see localisation code "InvalidBrandCode" for error message

Scenario: Validate Response localisation code for invalid IpAddress and valid brandID
	Given Anonymous request allowed, invalid IP address and valid brand
	When I try to send request to VerifyIp end point and get Response
		Then I should see Status Code 400
		Then I should see localisation code "InvalidIpAddress" for error message

Scenario: Validate Response localisation code for invalid IpAddress and invalid brandID
	Given Anonymous request allowed, invalid IP address and invalid brand
	When I try to send request to VerifyIp end point and get Response
		Then I should see Status Code 400
		Then I should see localisation code "InvalidBrandCode" for error message

Scenario: Validate Response localisation code for empty value for IpAddress and empty value for brandID
	Given Anonymous request allowed, empty value for IP address and empty value for brand name
	When I try to send request to VerifyIp end point and get Response
		Then I should see Status Code 400
		Then I should see localisation code "InvalidBrandCode" for error message

Scenario: Validate Response localisation code for invalid IpAddress with redirect URL and valid brand
	Given Anonymous request allowed, "http://google.com" as redirect URL, valid IP address and valid brand
	When I try to send request to VerifyIp end point and get Response
		Then I should see Status Code 200
		Then I should see redirect URL "http://google.com"

Scenario: Validate Response localisation code for invalid IpAddress with blocking type and valid brand
	Given Anonymous request allowed, "Login/Registration" as blocking type, valid IP address and valid brand
	When I try to send request to VerifyIp end point and get Response
		Then I should see Status Code 200
		Then I should see blocking type "Login/Registration"
	
