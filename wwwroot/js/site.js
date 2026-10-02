// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Evitar que el scroll del mouse cambie los valores en los inputs de tipo number
document.addEventListener("wheel", function(event) {
    if (document.activeElement.type === "number") {
        document.activeElement.blur();
    }
});