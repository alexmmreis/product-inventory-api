Feature: Creating products
  As an inventory manager
  I want to create new products with valid data
  So that only well-formed products enter the catalog

  Scenario: Creating a product with valid data succeeds
    Given I have a valid product named "Bluetooth Speaker" priced at 59.99 with 40 units in stock
    When I submit the product for creation
    Then the response status should be 201
    And the created product should have a 6-digit identifier

  Scenario: Creating a product with a negative price is rejected
    Given I have a product named "Broken Product" priced at -10 with 5 units in stock
    When I submit the product for creation
    Then the response status should be 400
