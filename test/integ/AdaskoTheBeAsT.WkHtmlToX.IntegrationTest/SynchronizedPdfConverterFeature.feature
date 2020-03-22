Feature: SynchronizedPdfConverterFeature

Scenario Outline: Convert pdf
	Given I have SynchronizedPdfConverter
    And I have sample html to convert '<filename>'
    And I created HtmlToPdfDocument
	When I convert html to pdf <repetitions> times
	Then proper pdf should be created

    Examples:
    | filename    | repetitions |
    | Simple.html | 1           |
    | Simple.html | 10          |
    | Simple.html | 100         |
