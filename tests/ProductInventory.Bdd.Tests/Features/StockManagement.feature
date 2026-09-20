Feature: Managing product stock
  As an inventory manager
  I want to adjust product stock levels
  So that available quantities stay accurate

  Scenario: Decrementing stock within available quantity succeeds
    Given a product with 20 units in stock
    When I decrement its stock by 5
    Then the response status should be 200
    And the product stock should be 15

  Scenario: Decrementing stock below zero is rejected
    Given a product with 5 units in stock
    When I decrement its stock by 10
    Then the response status should be 409
    And the product stock should be 5

  Scenario: Adding to stock succeeds
    Given a product with 10 units in stock
    When I add 15 units to its stock
    Then the response status should be 200
    And the product stock should be 25
