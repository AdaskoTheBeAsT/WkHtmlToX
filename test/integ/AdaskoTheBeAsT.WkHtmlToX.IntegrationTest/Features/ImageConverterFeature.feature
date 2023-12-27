Feature: ImageConverterFeature

Scenario Outline: Convert Image
	Given I have SynchronizedImageConverter
    And I have sample html to convert '<filename>'
    And I created HtmlToImageDocument
	When I convert html to image <repetitions> times
	Then proper image should be created

    Examples:
    | filename    | repetitions |
    | Simple.html | 1           |
    | Simple.html | 5           |
    | Simple.html | 10          |
    | Large.html  | 10          |
