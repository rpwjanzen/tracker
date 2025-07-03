// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Add some custom components such as always-formatted to 2 decimal digits money one

// HTMX anti-forgery token support
// adds the token (if present) to non-GET AJAX requests
// from: https://khalidabuhakmeh.com/htmx-requests-with-aspnet-core-anti-forgery-tokens
document.addEventListener("htmx:configRequest", (evt) => {
    let httpVerb = evt.detail.verb.toUpperCase();
    if (httpVerb === 'GET') {
        return;
    }
    
    let antiForgery = htmx.config.antiForgery;
    if (antiForgery) {
        if (evt.detail.parameters[antiForgery.formFieldName]) {
            // already specified on form, short circuit
            return;
        }

        if (antiForgery.headerName) {
            evt.detail.headers[antiForgery.headerName] = antiForgery.requestToken;
        } else {
            evt.detail.parameters[antiForgery.formFieldName] = antiForgery.requestToken;
        }
    }
});
