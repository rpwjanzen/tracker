// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Add some custom components such as always-formatted to 2 decimal digits money one


class MoneyElement extends HTMLInputElement {
    static observedAttributes = ["value"];

    constructor() {
        super();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        console.log(`Attribute ${name} has changed.`);
    }
}

customElements.define("money-element", MoneyElement, { extends: "input" });