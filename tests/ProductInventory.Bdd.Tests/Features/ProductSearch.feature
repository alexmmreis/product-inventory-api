Feature: Searching and filtering products
  As a shopper
  I want to search products by name and filter by stock level
  So that I can find what I need

  Scenario: Searching by a partial name returns matching products
    Given a product named "Gaming Headset" exists
    When I search for products matching "headset"
    Then the response status should be 200
    And the results should include "Gaming Headset"

  Scenario: Searching with a term shorter than 2 characters is rejected
    When I search for products matching "a"
    Then the response status should be 400

  Scenario: Filtering by stock range returns products within range
    Given a product named "Limited Stock Item" with 7 units in stock exists
    When I filter products with stock between 5 and 10
    Then the response status should be 200
    And the results should include "Limited Stock Item"
