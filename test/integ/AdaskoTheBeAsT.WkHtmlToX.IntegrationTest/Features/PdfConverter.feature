Feature: PdfConverter

Scenario Outline: Convert pdf
	Given I have SynchronizedPdfConverter
    And I have sample html to convert '<filename>'
    And I created HtmlToPdfDocument
	When I convert html to pdf <repetitions> times
	Then proper pdf should be created

    Examples:
    | filename    | repetitions |
    | Simple.html | 1           |
    | Simple.html | 5           |
    | Simple.html | 10          |
    | Large.html  | 10          |

Scenario: Convert pdf raises lifecycle callbacks
	Given I have SynchronizedPdfConverter with callback tracking
    And I have sample html to convert 'Simple.html'
    And I created HtmlToPdfDocument
	When I convert html to pdf 1 times
	Then pdf lifecycle callbacks should be raised

