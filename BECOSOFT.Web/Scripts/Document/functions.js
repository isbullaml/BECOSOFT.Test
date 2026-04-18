///<reference path="~/Scripts/bootstrap.js" />
///<reference path="~/Scripts/moment.js" />
///<reference path="~/Scripts/jquery-ui-1.13.1.js" />
///<reference path="~/Scripts/Document/details.js" />
///<reference path="~/Scripts/Document/contact.js" />
/*global generalTitle */

/**
 * The initial events 
 */

function doLoadEvents() {
    var contactId = $("#ContactID").val();
    if (contactId !== undefined && contactId !== null && contactId !== "0") {
        updateContactData(false);
        $("#detailTab a").removeClass("disabled");
         $("#importTab a").removeClass("disabled");
    } else {
        $("#DocumentTypeID").focus();
    }
    updateDocumentTypeOptions();

    if (numberDecimalSeparator === undefined || numberGroupSeparator === undefined) {
        const cultureInfo = $("#culture-info");
        numberDecimalSeparator = cultureInfo.data("numberdecimalseparator") == null ? "," : cultureInfo.data("numberdecimalseparator");
        numberGroupSeparator = cultureInfo.data("numbergroupseparator") == null ? "." : cultureInfo.data("numbergroupseparator");
    }
    fillTotals();

    $("#detailsTable").sortable({
        items: ".sortable",
        handle : ".arrow",
        update: updateIndices
    });
};

/**
 * Updates the expiration date based on the current payment condition.
 */
function updateDates() {
    var selectedValue = $("#PaymentConditionID option:selected");
    var days = selectedValue.data("days");
    var months = selectedValue.data("months");
    var endOfMonth = selectedValue.data("endofmonth");
    var date = $("#Date").val();
    var expirationDate = window.moment(date);
    expirationDate.add(parseInt(days), "days");
    expirationDate.add(parseInt(months), "months");
    if (endOfMonth === "true") {
        expirationDate.endOf("month");
    }
    $("#ExpirationDate").val(expirationDate.format("YYYY-MM-DD"));
}

/**
 * Updates the document type.
 * This will also update the document number-template and the document details
 */
function updateDocumentType() {
    updateTitle();
    const id = parseInt($("#DocumentTypeID").val(), 10);
    updateDocumentTypeOptions();
    var url = urls.documentTypeInfo;
    becosoft.ajax(url + "/" + id,
        {
            type: "GET",
            async: true
        },
        function(result) {
            if ($("#DocumentNumberTemplateID").length !== 0) {
                updateDocumentNumbers();
            }
            var currentCurrencyID = result["CurrencyID"].toString();
            $("#CurrencyID").val(currentCurrencyID);
            $("#currencySign").html($("#CurrencyID option:selected").text());
            fillTotals();
            var prevCountryId = parseInt($("#CountryID"));
            if (prevCountryId !== result["CountryID"]) {
                //updateDetails();
                //updateMatrixDetails();
                updateDetails();
            }
        });
}


/*
 * Updates visibility of certain elements based on the selected document type
 */
function updateDocumentTypeOptions() {
    const id = parseInt($("#DocumentTypeID").val(), 10);
    const options = $(document).find('div[data-document-type-options]');
    const hideExpirationDateDocumentTypeIDs = options.find('div[data-document-type-hide-expirationdate]')
        .attr('data-document-type-hide-expirationdate').split(',').map(function (d) { return parseInt(d, 10); });
    if (hideExpirationDateDocumentTypeIDs.includes(id)) {
        $(document).find('div[data-container-expiration-date]').addClass('d-none');
    } else {
        $(document).find('div[data-container-expiration-date]').removeClass('d-none');
    }
}

/**
 * Updates the document number example and number
 */
function updateDocumentNumbers() {
    var id = $("#DocumentTypeID").val();
    if (id === undefined || id === 0) {
        return;
    }
    var url = urls.documentNumber;
    $("#DocumentNumberTemplateID").html("");
    becosoft.ajax(url + "/" + id, {
        type: "GET",
        async: true
    }, function (result) {
        var selected = result["SelectedTemplate"];
        if (result["Templates"].length === 0) {
            $("#DocumentNumberTemplateID").attr("disabled", "disabled");
        } else {
            if (result["Templates"].length === 1) {
                $("#DocumentNumberTemplateID").attr("disabled", "disabled");
            } else {
                $("#DocumentNumberTemplateID").removeAttr("disabled");
            }
            $.each(result["Templates"], function () {
                var prefix = this["Prefix"];
                var length = this["Length"];
                var freePositions = parseInt(length) - prefix.toString().length;
                var example = "".padEnd(freePositions, "0");
                var option = $("<option>", { value: this["DocumentNumberTemplateID"] }).text(this["Prefix"]).data("example", example);
                if (selected !== null && this["DocumentNumberTemplateID"] === selected["Id"]) {
                    option.css("background-color", "#d3d3d3");
                }
                $("#DocumentNumberTemplateID").append(option);
            });
        }
        if (selected !== null && selected !== undefined && selected !== 0) {
            $("#DocumentNumberTemplateID").val(selected["Id"]);
        }
        updateDocumentNumberTemplate();
    });
}

/**
 * Updates the document number template
 */
function updateDocumentNumberTemplate() {
    var example = $("#DocumentNumberTemplateID option:selected").data("example");
    if (example === undefined) {
        example = "1";
    }
    $("#DocumentNumber").val(example);
}

/**
 * Updates the example document number to the latest number
 */
function updateToLatestDocumentNumber() {
    var id = $("#DocumentNumberTemplateID").val();
    var url = urls.documentNumber;
    becosoft.ajax(url + "/" + id, {
        type: "GET",
        async: true
    }, function (templates) {
        $("#DocumentNumberTemplateID").html("");
        $.each(templates, function () {
            $("#DocumentNumberTemplateID").append($("<option>", { value: this["DocumentNumberTemplateID"] }).text(this["Prefix"]));
        });

    });
}

/**
 * Sets the title of the page. Example: [DocumentType] ([Documentnumber]) - [Contact]
 */
function updateTitle() {
    var title = window.generalTitle + ": " + $("#DocumentTypeID option:selected").text() + " (" + window.documentNumber + ") - " + $("#Contact_DisplayName").val();
    document.title = title;
    $("div.bcs-titlebar-left > a.bcs-titlebar-title").text(title);
}

/**
 * Saves the document as a concept.
 * @param {jQueryClickEvent} e
 */
function saveAsConcept(e) {
    e.preventDefault();
    //TODO: Parse form and submit with concept type
}

/**
 * Go to the concepts page.
 * @param {jQueryClickEvent} e
 */
function goToConcepts(e) {
    e.preventDefault();
    //TODO: Go to concept index with the current concept type in the filter.
}

/**
 * Highlight a row in a table.
 * @param {jQueryObject} parent - The parent to search in
 * @param {number} index - The new index
 * @param {jQuerySelector} rowSelector - The selector for the rows
 */
function highlightRow(parent, index, rowSelector) {
    var rowLength = parent.find(rowSelector).length;
    if ((index + 1) > rowLength) {
        index = 0;
    }

    var newRow = parent.find(rowSelector + ":eq(" + index + ")");
    if (newRow.length > 0) {
        // Remove other highlights
        parent.find(".highlight-row").removeClass("highlight-row");

        // Highlight your target
        newRow.addClass("highlight-row");
    }
}

/**
 * Gets the index of the new detail.
 * @returns {number} Position
 */
function getLastPosition() {
    if ($("#detailsTable .sortable").length === 0) { return 0; }
    return $("#detailsTable .sortable:last").data("position") + 1;
}

/**
 * Checks if the search-type is article.
 * @returns {boolean} isArticle
 */
function isArticleSearchType() {
    var articleToggle = $("#searchTypeSelector_Article");
    if (articleToggle.length > 0) {
        return articleToggle.hasClass("active");
    }
    return false;
}