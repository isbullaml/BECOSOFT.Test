///<reference path="functions.js"/>

/**
 * Handles a key down event
 * @param {jQueryEvent} e
 */
function handleKeyDown(e) {
    if (e.ctrlKey) { //CTRL
        switch (e.keyCode) {
        case 38: //UP
            handleCtrlUp(e);
            break;
        case 40: //DOWN
            handleCtrlDown(e);
            break;
        case 46: //DEL
            handleCtrlDel(e);
            break;
        case 49: //&
            handleCtrl1(e);
            break;
        case 50: //é
            handleCtrl2(e);
            break;
        case 51: //"
            handleCtrl3(e);
            break;
        case 52: //'
            handleCtrl4(e);
            break;
        case 70: //F
            handleCtrlF(e);
            break;
        }
    } else {
        switch (e.keyCode) {
        case 9: //TAB
            handleTab(e);
            break;
        case 13: //TAB
            handleBaseEnter(e);
            break;
        }
    }
}

/**
 * Handles a TAB event
 * @param {jQueryEvent} e
 */
function handleTab(e) {
    var target = e.target;
    var id = target.id;
    if (id.includes("VATGroupID")) {
        var lastRow = $(target).closest("tbody.sortable").is(":last-child");
        if (lastRow) {
            e.preventDefault();
            $("#defaultSearchField").focus();
        }
    }

    if (id === "RepresentativeID") {
        e.preventDefault();
        var detailTabLink = $("#detailTab a");

        if (detailTabLink.hasClass("disabled")) {
            $("#DocumentTypeID").focus();
        } else {
            detailTabLink.trigger("click");
        }
    }
}

/**
 * Handles an ENTER event
 * @param {jQueryEvent} e
 */
function handleBaseEnter(e) {
    if (!hasOpenModal()) {
        e.stopPropagation();
        e.preventDefault();
        var elements = $(":input:visible:not(:disabled)");
        var currentElement = $(":focus");
        var id = currentElement.attr("id");

        if (id.includes("VATGroupID")) {
            var lastRow = $(e.target).closest("tbody.sortable").is(":last-child");
            if (lastRow) {
                e.preventDefault();
                $("#defaultSearchField").focus();
            }
            return;
        }
        
        if (id === "RepresentativeID") {
            e.preventDefault();
            var detailTabLink = $("#detailTab a");

            if (detailTabLink.hasClass("disabled")) {
                $("#DocumentTypeID").focus();
            } else {
                detailTabLink.trigger("click");
            }
            return;
        }
         
        $(":input:visible:not(:disabled):eq(" + (elements.index($(":focus")) + 1) + ")").focus();
    }
}

/**
 * Handles a CTRL + UP event
 * @param {jQueryEvent} e
 */
function handleCtrlUp(e) {
    var target = e.target;
    var currentRow = $(target).closest("tbody.sortable");
    if (currentRow.length > 0) {
        e.preventDefault();
        currentRow.prev("tbody.sortable").before(currentRow);
        updateIndices();
        currentRow.find("[name$='.Amount']").focus();
    }
}

/**
 * Handles a CTRL + DOWN event
 * @param {jQueryEvent} e
 */
function handleCtrlDown(e) {
    var target = e.target;
    var currentRow = $(target).closest("tbody.sortable");
    if (currentRow.length > 0) {
        e.preventDefault();
        currentRow.next("tbody.sortable").after(currentRow);
        updateIndices();
        currentRow.find("[name$='.Amount']").focus();
        return;
    }
}

/**
 * Handles a CTRL + DELETE event
 * @param {jQueryEvent} e
 */
function handleCtrlDel(e) {
    var target = e.target;
    var currentRow = $(target).closest("tbody.sortable");
    if (currentRow.length > 0) {
        e.preventDefault();
        var deleteButton = currentRow.find(".btn-delete-r");
        if (deleteButton.length > 0) {
            deleteButton.trigger("click");
            $("#defaultSearchField").focus();
        }
    }
}

/**
 * Handles a CTRL + & or CTRL + 1 event
 * @param {jQueryEvent} e
 */
function handleCtrl1(e) {
    e.preventDefault();
    var generalTabLink = $("#generalTab a");
    if (!generalTabLink.hasClass("disabled")) {
        generalTabLink.trigger("click");
    }
}

/**
 * Handles a CTRL + é or CTRL + 2 event
 * @param {jQueryEvent} e
 */
function handleCtrl2(e) {
    e.preventDefault();
    var detailTabLink = $("#detailTab a");
    if (!detailTabLink.hasClass("disabled")) {
        detailTabLink.trigger("click");
    }
}

/**
 * Handles a CTRL + " or CTRL + 3 event
 * @param {jQueryEvent} e
 */
function handleCtrl3(e) {
    e.preventDefault();
    var followupTabLink = $("#followupTab a");
    if (!followupTabLink.hasClass("disabled")) {
        followupTabLink.trigger("click");
    }
}

/**
 * Handles a CTRL + ' or CTRL + 4 event
 * @param {jQueryEvent} e
 */
function handleCtrl4(e) {
    e.preventDefault();
    var paymentsTabLink = $("#paymentsTab a");
    if (!paymentsTabLink.hasClass("disabled")) {
        paymentsTabLink.trigger("click");
    }
}

/**
 * Handles a CTRL + F event
 * @param {jQueryEvent} e
 */
function handleCtrlF(e) {
    e.preventDefault();

    var detailTabLink = $("#detailTab a");
    var generalTabLink = $("#generalTab a");

    if (hasOpenModal()) {
        var contactFilterModal = $("#contactFilterModal");
        if (contactFilterModal.length > 0) {
            $("#Filter_SearchQuery").focus();
            return;
        }

        var combinedFilterModal = $("#combinedFilterModal");
        if (combinedFilterModal.length > 0) {
            if ($("#filterMatrixCollapse").hasClass("show")) {
                if ($("#Matrices_Filter_SearchQuery").is(":focus")) {
                    $("#filterArticleCollapse").collapse("show");
                } else {
                    $("#Matrices_Filter_SearchQuery").focus();
                }
            }

            if ($("#filterArticleCollapse").hasClass("show")) {
                if ($("#Articles_Filter_SearchQuery").is(":focus")) {
                    $("#filterMatrixCollapse").collapse("show");
                } else {
                    $("#Articles_Filter_SearchQuery").focus();
                }
            }

            return;
        }

        return;
    }

    if (detailTabLink.hasClass("active")) {
        if ($("#defaultSearchField").is(":focus")) {
            $("#searchTypeSelector button:not(.active)").trigger("click");
        }
        $("#defaultSearchField").focus();
    }

    if (generalTabLink.hasClass("active")) {
        $("#contactSearchField").focus();
    }
}