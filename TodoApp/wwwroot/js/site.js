// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Shows/hides the rename text box next to a "Rename" button.
function toggleRename(button) {
    var form = button.nextElementSibling;
    form.style.display = form.style.display === "none" ? "flex" : "none";
}
