///<reference path="functions.js"/>

/**
 * Event handler for key-down events in a modal
 * @param {jQueryEvent} e - The keydown-event
 * @param {jQuerySelector} tableSelector - The selector for the result-table
 * @param {jQueryObject} modalContainer - The modal-container
 * @param {jQuerySelector} selector - The selector for the rows
 */
function modalTableKeyDown(e, tableSelector, modalContainer, selector) {
    var table = $(tableSelector);
    if ((modalContainer.children("div").first().data("bs.modal") || { isShown: false }).isShown === false || table.is(":hidden")) {
        return;
    }

    switch (e.which) {
        case 13: //Enter
            handleEnter(e, table);
            break;
        case 37: //Arrow Left
            handleArrowLeft(e);
            break;
        case 38: //Arrow Up
            handleArrowUp(e, table, selector);
            break;
        case 39: //Arrow Right
            handleArrowRight(e);
            break;
        case 40: //Arrow Down
            handleArrowDown(e, table, selector);
            break;
    }
}

/**
 * Handle an ENTER in the modal. This selects the current row if there are any rows.
 * @param {jQueryEvent} e
 * @param {jQueryObject} table
 */
function handleEnter(e, table) {
    var row = table.find("tr.highlight-row").first();
    if (row.length > 0) {
        e.preventDefault();
        row.trigger("click");
    }
}

/**
 * Handle an ARROW LEFT in the modal. This goes to the previous page in the result.
 * @param {jQueryEvent} e
 */
function handleArrowLeft(e) {
    e.preventDefault();
    var previousLink = $("#paginationPreviousLink");
    if (!previousLink.closest("li").hasClass("disabled")) {
        previousLink.trigger("click");
    }
}

/**
 * Handle an ARROW RIGHT in the modal. This goes to the next page in the result.
 * @param {jQueryEvent} e
 */
function handleArrowRight(e) {
    e.preventDefault();
    var nextLink = $("#paginationNextLink");
    if (!nextLink.closest("li").hasClass("disabled")) {
        nextLink.trigger("click");
    }
}

/**
 * Handle an ARROW UP in the modal. This selects the previous row in the result.
 * @param {jQueryEvent} e
 * @param {jQueryObject} table
 * @param {jQuerySelector} selector
 */
function handleArrowUp(e, table, selector) {
    e.preventDefault();
    var index = 0;
    var currentRow = table.find("tr.highlight-row");
    if (currentRow.length > 0) {
        index = currentRow.index() - 1;
    }
    highlightRow(table, index, selector);
}

/**
 * Handle an ARROW DOWN in the modal. This selects the next row in the result.
 * @param {jQueryEvent} e
 * @param {jQueryObject} table
 * @param {jQuerySelector} selector
 */
function handleArrowDown(e, table, selector) {
    e.preventDefault();
    var index = 0;
    var currentRow = table.find("tr.highlight-row");
    if (currentRow.length > 0) {
        index = currentRow.index() + 1;
    }
    highlightRow(table, index, selector);
}