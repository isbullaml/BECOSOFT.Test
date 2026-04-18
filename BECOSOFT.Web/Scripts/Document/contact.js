/// <reference path="~/Scripts/Document/functions.js" />
/// <reference path="~/Scripts/Document/modalKeyBindings.js" />
/// <reference path="~/Scripts/loadingIcon.js" />
/* global urls */

/**
 * Opens the contact search modal
 */
localStorage.hasContactClicked = false;
function openContactModal() {
    var hasClicked = localStorage.hasContactClicked === "true";
    if (hasClicked) {
        return;
    }
    localStorage.hasContactClicked = true;
    if (hasOpenModal()) {
        return;
    }

    var url = urls.contactModal + "?isPurchase=" + window.isPurchase;

    becosoft.ajax(url, { async: true }, function (result) {
        var contactEventsFunction = function(container) {
            bindContactEvents();
            container.on("shown.bs.modal", function () {
                $("#Filter_SearchQuery").focus();
            });

            var keydownFunc = function (e) {
                modalTableKeyDown(e, "#modal-container table tbody", $("#modal-container"), "tr.contact");
            };

            $(document).on("keydown", keydownFunc);

            $("#filterSubmit").on("click", function (e) {
                e.preventDefault();
                $("#Filter_PageInfo_Page").val(0);
                postContactFilter();
                $(this).blur();
            });

            $("#modal-close-button").on("click", function () {
                closeCurrentModal();
            });

            container.on("hidden.bs.modal", function () {
                $(document).off("keydown", keydownFunc);
            });

            var searchValue = $("#contactSearchField").val();
            if (searchValue !== "" && searchValue !== "0") {
                $("#Filter_SearchQuery").val(searchValue);
                $("#filterSubmit").click();
            }
        };

        openModal("#modal-container", result, false, contactEventsFunction);
        localStorage.hasContactClicked = false;
    });
}

/**
 * Binds the events in the contact modal
 */
function bindContactEvents() {
    $("#paginationForm a[type=submit]").on("click", function (e) {
        e.preventDefault();
        $(this).attr("data-clicked", true);
        $("#paginationForm").submit();
    });

    $("#result").on("submit", "#paginationForm", function (e) {
        e.preventDefault();
        var btn = $(this).find("a[data-clicked='true']");
        var value = btn.attr("value");
        var selectedPage = parseInt(value);
        $("#Filter_PageInfo_Page").val(selectedPage);
        var pageSize = $("#PaginationBlock_PageSize").val();
        $("#Filter_PageInfo_PageSize").val(pageSize);
        postContactFilter();
    });

    $("#paginationForm").on("submit", function (e) {
        e.preventDefault();
        var btn = $(this).find("a[data-clicked='true']");
        var value = btn.attr("value");
        var selectedPage = parseInt(value);
        $("#Filter_PageInfo_Page").val(selectedPage);
        var pageSize = $("#PaginationBlock_PageSize").val();
        $("#Filter_PageInfo_PageSize").val(pageSize);
        postContactFilter();
    });

    $("#sortFieldForm").on("submit", function (e) {
        e.preventDefault();
        var btn = $(this).find("a[data-clicked='true']");
        var value = btn.attr("value");
        if (value === "SaleID") {
            value = "";
        }
        $("#Filter_OrderField").val(value);
        var sortOrder = $("#PaginationBlock_SortOrder").val();
        $("#Filter_SortOrder").val(sortOrder);
        postContactFilter();
    });
}

/**
 * Posts the contact filter in the modal
 */
function postContactFilter() {
    var form = $("#filterForm");
    var formData = form.serialize();
    loadingIcon.callback("#result", function () {
        var xhr = new window.XMLHttpRequest();
        $("#modal-container").on("hide.bs.modal", function () {
            xhr.abort();
        });

        becosoft.ajax(form.attr("action"), {
            type: "POST",
            data: formData,
            xhr: function () {
                return xhr;
            }
        }, function (result) {
            $("#result").html("");
            $("#result").html(result);

            var sortField = $("#PaginationBlock_SortField").val();
            $("#Filter_PageInfo_SortField").val(sortField);

            $(".contact").on("click", function () {
                pickContact($(this).data("contact"));
            });

            var results = $("#result .contact");
            if (results.length === 1) {
                results.first().trigger("click");
            }
            bindContactEvents();
        });
    });
}

/**
 * Picks a contact based on it's ID
 * @param {number} id
 */
function pickContact(id) {
    $("#ContactID").val(id);
    updateContactData();
    updateDetails();
    closeCurrentModal();
}

/**
 * Updates the data for the selected contact.
 * Resets the delivery-info if {@link withDeliveryReset} is true.
 * @param {boolean} [withDeliveryReset=true]
 */
function updateContactData(withDeliveryReset = true) {
    $("#detailTab a").removeClass("disabled");
        $("#importTab a").removeClass("disabled");
    $("#DeliveryAddress_Description").focus();
    updateInvoiceDate();
    updateDeliveryAddresses();
    if (withDeliveryReset) {
        setDeliveryAddressData("");
    }
}

/**
 * Updates the delivery address data from the selected contact.
 */
function updateDeliveryAddress() {
    var contactId = $("#ContactID").val();
    var description = $("#DeliveryAddress_Description").val();
    if (contactId === undefined || contactId === null || contactId === 0 || contactId === "" ||
        description === undefined || description === null || description === 0 || description === "") {
        return;
    }

    var url = urls.deliveryAddressInfo + "?id=" + contactId + "&description=" + description;
    becosoft.ajax(url, { type: "GET", async: true }, function (deliveryAddressInfo) {
        setDeliveryAddressData(deliveryAddressInfo);
    });
}

/**
 * Updates the delivery address dropdown of the current contact.
 * This resets the delivery address dropdown if no contact is selected.
 */
function updateDeliveryAddresses() {
    var contactId = $("#ContactID").val();
    if (contactId === undefined || contactId === null || contactId === 0 || contactId === "") {
        setDeliveryAddresses("");
        return;
    }
    var url = urls.deliveryAddresses + "/" + contactId;
    becosoft.ajax(url, { type: "GET", async: true }, function (deliveryAddresses) {
        setDeliveryAddresses(deliveryAddresses);
    });
}

/**
 * Updates the invoice data of the current contact.
 * This resets all the contact data if no contact is selected.
 */
function updateInvoiceDate() {
    var contactId = $("#ContactID").val();
    if (contactId === undefined || contactId === null || contactId === 0 || contactId === "") {
        setContactData("");
        setAccountingData("");
        return;
    }
    var url = urls.contactInfo + "/" + contactId;
    becosoft.ajax(url, { type: "GET", async: true }, function (contact) {
        setContactData(contact);
        setAccountingData(contact["Accounting"]);
    });
}

/**
 * Updates the accounting data of the current contact.
 * This resets all the accounting data if it is "".
 * @param {Object<>} accountingData
 */
function setAccountingData(accountingData) {
    if (accountingData === "") {
        $(".contactAccountingField").text("");
        return;
    }
    if (accountingData["VatCodeID"] !== 0) {
        $("#VATCodeID").val(accountingData["VatCodeID"]);
    }
    if (accountingData["PaymentConditionID"] !== 0) {
        $("#PaymentConditionID").val(accountingData["PaymentConditionID"]);
        updateDates();
    }
    if (accountingData["DeliveryConditionID"] !== 0) {
        $("#DeliveryConditionID").val(accountingData["DeliveryConditionID"]);
    }
}

/**
 * Sets the contact data of the current contact.
 * This resets all the contact data if it is "".
 * @param {Object<>} contactData
 */
function setContactData(contactData) {
    if (contactData === "") {
        $(".contactDataField").text("");
        return;
    }

    var title = window.generalTitle + ": " + $("#DocumentTypeID option:selected").text() + " (" + window.documentNumber + ") - " + contactData["DisplayName"];
    document.title = title;
    $("h1 .text-muted").text(title);

    $("#contactSearchField").val(contactData["DisplayName"]);
    $("#Contact_DisplayName").val(contactData["DisplayName"]);
    $("#Contact_ContactNumber").text(contactData["ContactNumber"]);
    $("#Contact_Alias").text(contactData["Alias"]);
    $("#Contact_CompanyName").text(contactData["CompanyName"]);
    $("#Contact_Name").text(contactData["Preposition"] + " " + contactData["LastName"]);
    $("#Contact_FirstName").text(contactData["FirstName"]);
    $("#Contact_Street").text(contactData["Street"]);
    $("#Contact_HouseNumber").text(contactData["HouseNumber"]);
    $("#Contact_Box").text(contactData["Box"]);
    $("#Contact_AddressLine").text(contactData["AddressLine"]);
    $("#Contact_PostalCode").text(contactData["PostalCode"]);
    $("#Contact_Place").text(contactData["Place"]);
    $("#Contact_Country").text(contactData["CountryName"]);
}

/**
 * Sets the delivery address dropdown of the current contact.
 * @param {Array<>} deliveryAddresses
 */
function setDeliveryAddresses(deliveryAddresses) {
    $("#DeliveryAddress_Description").empty();
    $("#DeliveryAddress_Description").append($("<option>", { value: "" }).text("-"));
    if (deliveryAddresses === "") {
        return;
    }
    $.each(deliveryAddresses, function () {
        $("#DeliveryAddress_Description").append($("<option>", { value: this["Description"] }).text(this["Description"]));
    }
    );
}

/**
 * Sets the delivery address data of the current contact.
 * This resets all the delivery address data (except the CountryID) if it is "".
 * @param {Object<>} deliveryAddressData
 */
function setDeliveryAddressData(deliveryAddressData) {
    if (deliveryAddressData === "") {
        $(".deliveryAddressDataField:not(#CountryID)").val("");
        return;
    }

    $("#CompanyName").val(deliveryAddressData["Company"]);
    $("#DeliveryAddress_Addressee").val(deliveryAddressData["Addressee"]);
    $("#Street").val(deliveryAddressData["Street"]);
    $("#HouseNumber").val(deliveryAddressData["HouseNumber"]);
    $("#Box").val(deliveryAddressData["Box"]);
    $("#AddressLine").val(deliveryAddressData["AddressLine"]);
    $("#CountryID").val(deliveryAddressData["CountryID"]);
    $("#PostalCode").val(deliveryAddressData["PostalCode"]);
    $("#Place").val(deliveryAddressData["Place"]);
    $("#DeliveryAddress_Telephone").val(deliveryAddressData["Telephone"]);
    $("#Location").val(deliveryAddressData["Location"]);
    $("#DeliveryAddress_Mobile").val(deliveryAddressData["Mobile"]);
    $("#DeliveryAddress_Email").val(deliveryAddressData["Email"]);
}