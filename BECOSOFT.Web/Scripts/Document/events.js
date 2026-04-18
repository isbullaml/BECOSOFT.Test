///<reference path="~/Scripts/Document/contact.js" />
///<reference path="~/Scripts/Document/functions.js" />
///<reference path="~/Scripts/Document/keyBindings.js" />
///<reference path="~/Scripts/Document/details.js" />

//Document
$(window).on("load", doLoadEvents);
$(document).keydown(handleKeyDown);
$("#saveAsConceptButton").on("click", saveAsConcept);
$("#conceptsIndexButton").on("click", goToConcepts);
$("#documentForm").on("submit", function (e) {
    //var focusedElement = $(":focus");
    //if (focusedElement.is(".articleSearch, .matrixSearch, .articleField, .matrixField, #defaultSearchField, #contactSearchField")) {
    //    e.preventDefault();
    //    focusedElement.trigger("blur");
    //    return false;
    //}
    e.preventDefault();
    var form = $(this);
    var formData = {};

    form.serializeArray().map(function (x) {
        if (formData[x.name] === undefined) {
            formData[x.name] = x.value;
        }
    });
    
    var formDetailData = [];

    form.find("tbody.detail-table").each(function(i, e) {
        formDetailData.push(convertDetailToJSON2($(e)));
    });

    formData.DocumentDetails = formDetailData;

    loadingIcon.ajax("",
        form.attr("action"),
        { type: "POST", data: formData }, function (result) {
            if (result.Result) {
                if (result.RedirectUrl && result.RedirectUrl.length !== 0) {
                    window.location.href = result.RedirectUrl;
                } else {
                    window.location.href = $("#backToOverview").attr('href');
                }
            } else {
                var body = "<ul>";
                result.Errors.forEach(function(e) {
                    body += `<li>${e.Key}: ${e.Value}</li>`;
                });

                body += "</ul>";
                $("#saveAlert").find("#alert-text").html(body);
                $("#saveAlert").removeClass("d-none");
            }
        });


    return true;
});

//General
$("#generalTab a").on("shown.bs.tab", function () { $("#contactSearchField").focus(); });
$("#DeliveryAddress_Description").on("change", updateDeliveryAddress);
$("#DocumentNumberTemplateID").on("change", updateDocumentNumberTemplate);
$("#PaymentConditionID").on("change", updateDates);
$("#ContactSearch").on("click", openContactModal);
$("#VATCodeID").on("change", updateDetails);
$("#DocumentTypeID").on("change", updateDocumentType);
$("#contactSearchField").on("change, blur", function () {
    var searchValue = $(this).val();
    //if (searchValue === "") { return; }
    var contactName = $("#Contact_DisplayName").val();
    if (searchValue === contactName) { return; }
    openContactModal();
});

//Details
$("#detailTab a").on("shown.bs.tab", function () { $("#defaultSearchField").focus(); });
$("#searchTypeSelector button").on("click", toggleSearchType);
$("#deleteAllDetails").on("click", deleteAllDetails);
$("#collapseAllDetails").on("click", collapseAllDetails);
$("#defaultSearch").on("click", function () {
    openDetailSearchModal(getLastPosition(), $("#defaultSearchField").val(), isArticleSearchType());
});
$("#defaultSearchField").on("change, blur", function () {
    var searchValue = $(this).val();
    if (searchValue === "") { return; }
    openDetailSearchModal(getLastPosition(), searchValue, isArticleSearchType());
});
$("#detailsTable ").on("click", ".articleSearch", function () {
    var searchValue = $(this).closest(".input-group").find(".articleField").val();
    var position = $(this).closest("tbody.sortable").data("position");
    openDetailSearchModal(position, searchValue, true);
});
$("#detailsTable").on("click", ".matrixSearch", function () {
    var searchValue = $(this).closest(".input-group").find(".articleField").val();
    var position = $(this).closest("tbody.sortable").data("position");
    openDetailSearchModal(position, searchValue, false);
});
$("#detailsTable").on("click", ".btn-delete-r", function () {
    var currentRow = $(this).closest("tbody.sortable");
    currentRow.find("[data-toggle='tooltip']").tooltip("dispose");
    currentRow.remove();
    fillTotals();
    updateIndices();
});
$("#detailsTable").on("click", "input", function () {
    var currentRow = $(this).select();
});
$("#detailsTable").on("change", "[data-type='articledetail'] [data-type='number']", onNumericChange);

$("#detailsTable").on("focus", "[data-type='number']", function (e) {
    onNumericFocus(this);
    this.select();
});
$("#detailsTable").on("change", "[name$='.Amount']", function (e) {
    console.log("change");
    var currentDetail = $(this).closest("tbody.sortable");
    currentDetail.data("changed", true);
    currentDetail.data("focussed", true);
    var id = "#" + $(this).attr("id");
    var previousValue = $(this).data("previous-value");
    var newValue = $(this).val();
    var parsedNewNumber = new Number(newValue).formatDecimal(2);
    $(this).val(parsedNewNumber);
    if (previousValue !== parsedNewNumber) {
        calculatePrice($(this).closest('tr'), id);
    }
});
$("#detailsTable").on("change", "[name$='BasePrice'], [name*='Discount'], [data-type='amount']", function (e) {
    console.log("change");
    var currentDetail = $(this).closest("tbody.sortable");
    currentDetail.data("changed", true);
    currentDetail.data("focussed", true);
    var previousValue = $(this).data("previous-value");
    var newValue = $(this).val();
    var parsedNewNumber;
    if (newValue === "") {
        parsedNewNumber = "";

    } else {
        parsedNewNumber = new Number(newValue).formatDecimal(2);
    }
    $(this).val(parsedNewNumber);
    if (previousValue !== parsedNewNumber) {
        var row = $(this).closest("tbody.detail-table > tr");
        if (row.data('type') !== "articledetail") {
            var id = row.data('id');
            row = currentDetail.find(`tr[data-id='${id}']`);
        }
        calculateRow(row);
    }
});
$("#detailsTable").on("change", "[name$='.VATGroupID']", function () {
    var currentRow = $(this).closest("tbody.sortable")[0];
    calculateRow($(currentRow));
});
$("#detailsTable").on("change", "[name$='.Description']", function () {
    var currentCell = $(this).closest("td");
    var description = $(this).val();
    currentCell.attr("data-original-title", description);
});
$("#detailsTable").popover({
    html: true,
    trigger: "hover",
    selector: '[data-toggle="image-popover"]',
    content: function () { return "<span class='d-block text-center'>" + $(this).data("description") + '</span><img style="max-width:150px;" src="' + $(this).data("image") + '" alt="Image not available" />'; }
});