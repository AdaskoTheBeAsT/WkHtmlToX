Feature: MultipleConversionFeature

Scenario: Convert same html twice should give same results
    Given I have SynchronizedPdfConverter
    And I have complex html
    When I convert first time
    And I convert same html second time
    Then I should obtain files with same length
