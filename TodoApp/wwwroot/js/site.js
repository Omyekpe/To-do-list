// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Shows/hides the small form that sits right after a toggle button
// (used for both "Rename" and "+ Add" buttons).
function toggleInline(button) {
    var form = button.nextElementSibling;
    form.style.display = form.style.display === "none" ? "flex" : "none";
}

// Kept as an alias so any existing onclick="toggleRename(this)" markup still works.
function toggleRename(button) {
    toggleInline(button);
}

// Wires up drag-and-drop reordering for a <ol>/<ul> of draggable <li> rows.
// On drop, writes the row order (from each row's data-id) into the hidden
// input and submits the form, so the server persists it like any other action.
function initDragReorder(listId, formId, inputId) {
    var list = document.getElementById(listId);
    if (!list) return;

    var dragging = null;

    list.addEventListener("dragstart", function (e) {
        dragging = e.target.closest("li");
        dragging.classList.add("dragging");
    });

    list.addEventListener("dragend", function () {
        if (dragging) dragging.classList.remove("dragging");
        dragging = null;
    });

    list.addEventListener("dragover", function (e) {
        e.preventDefault();
        var target = e.target.closest("li");
        if (!target || target === dragging) return;

        var rect = target.getBoundingClientRect();
        var after = e.clientY - rect.top > rect.height / 2;
        list.insertBefore(dragging, after ? target.nextSibling : target);
    });

    list.addEventListener("drop", function (e) {
        e.preventDefault();

        var ids = Array.from(list.querySelectorAll("li")).map(function (li) {
            return li.dataset.id;
        });

        document.getElementById(inputId).value = ids.join(",");
        document.getElementById(formId).submit();
    });
}
