import { Given, When, Then, DataTable } from '@badeball/cypress-cucumber-preprocessor';

Given(`I have SynchronizedImageConverter`, () => {
    // [Given] Sets up the initial state of the system.
});

Given(`I have sample html to convert {string}`, (arg0: string) => {
    // [Given] Sets up the initial state of the system.
});

Given(`I created HtmlToImageDocument`, () => {
    // [Given] Sets up the initial state of the system.
});

When(`I convert html to image {any} times`, (arg0: any) => {
    // [When] Describes the action or event that triggers the scenario.
});

Then(`proper image should be created`, () => {
    // [Then] Describes the expected outcome or result of the scenario.
});

Given(`I have SynchronizedImageConverter with callback tracking`, () => {
    // [Given] Sets up the initial state of the system.
});

When(`I convert html to image {int} times`, (arg0: number) => {
    // [When] Describes the action or event that triggers the scenario.
});

Then(`image lifecycle callbacks should be raised`, () => {
    // [Then] Describes the expected outcome or result of the scenario.
});