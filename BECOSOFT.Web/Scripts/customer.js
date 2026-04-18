$("#validateVat").on("click", validateVatNumber);
$("#Accounting_VatNumber").on("input", changeVatNumber);
$("#CountryID").on("change", changeVatNumber);
$("#CountryID").on("change", checkProvinces);
$("#PostalCode").on("focusout", updatePlaceProvince);
$("#modal-container").on("click", ".select-place", modalSelectPlace);

function validateVatNumber() {
    var url = $("#vatValidationUrl").val();
    var countryUrl = $("#countryDataUrl").val();
    var vatInputField = $("#Accounting_VatNumber");
    var vatNumber = vatInputField.val();
    var countryId = $("#CountryID").val();
    var $this = $(this);

    var loadingIcon = $("#loader-icon").html();
    var dangerIcon = $("#danger-icon").html();
    var successIcon = $("#success-icon").html();

    $this.html("<span class='spinning d-inline-flex'>" + loadingIcon + "</span>");
    becosoft.ajax(url + "?vatNumberString=" + vatNumber + "&countryId=" + countryId, { async: true }, function (result) {
        if (result === false || result["IsValid"] === false) {
            $this.removeClass("btn-success");
            $this.removeClass("btn-outline-secondary");
            $this.addClass("btn-danger disabled");
            $this.html(dangerIcon);
            vatInputField.addClass("bcs-input-invalid");
            $(vatInputField.siblings()[0].find("button")).text(result["ValidationResult"]["Errors"]["0"]["Value"]);
        } else {
            var viesValidation = result["ViesResponse"];
            var validatedatNumber = result["VatNumber"];

            $("#CompanyName").val(viesValidation["Name"]);
            $("#Street").val(viesValidation["Address"]["Street"]);
            $("#HouseNumber").val(viesValidation["Address"]["Number"]);
            $("#PostalCode").val(viesValidation["Address"]["PostalCode"]);
            $("#Place").val(viesValidation["Address"]["Place"]);
            $("#Accounting_VatNumber").val(validatedatNumber["ValidatedVatNumber"]);

            becosoft.ajax(countryUrl + "?countryCode=" + viesValidation["Address"]["CountryCode"], { async: true }, function (countryResult) {
                if (result !== false) {
                    $("#CountryID").val(countryResult["Id"]);
                    checkProvinces();
                    if (countryResult["Iso"] === "BE") {
                        $("#Accounting_VatCodeID").val(2);
                    } else if (countryResult["IntraComm"] === true) {
                        $("#Accounting_VatCodeID").val(3);
                    } else {
                        $("#Accounting_VatCodeID").val(6);
                    }
                    updatePlaceProvince(false);
                }
            });

            $this.removeClass("btn-danger");
            $this.removeClass("btn-outline-secondary");
            $this.addClass("btn-success disabled");
            $this.html(successIcon);
            vatInputField.addClass("bcs-input-valid");
        }
    });
}

function changeVatNumber() {
    var button = $("#validateVat");
    var vatInputField = $("#Accounting_VatNumber");
    var validationMessage = $("#vatValidationMessage").val();
    button.removeClass("btn-success btn-danger disabled");
    button.addClass("btn-outline-secondary");
    button.empty().append(validationMessage);
    vatInputField.removeClass("bcs-input-valid bcs-input-invalid");
}

function updatePlaceProvince(updatePlace = true) {
    var countryId = $("#CountryID").val();
    var postalCode = $("#PostalCode").val();
    var url = $("#placeDataUrl").val();

    becosoft.ajax(url + "?postalCode=" + postalCode + "&countryId=" + countryId + "&onlyProvince=" + !updatePlace, { async: true }, function (result) {
        if (result !== false && result.length > 0) {
            $("#ProvinceID").val(parseInt(result["0"]["ProvinceID"]));
            if (result.length === 1) {
                var row = result["0"];
                if (updatePlace) {
                    $("#Place").val(row["Name"]);
                    $("#PlaceID").val(parseInt(row["PlaceID"]));
                }
            } else if (updatePlace) {
                showModal(result);
            }
        } else {
            $("#ProvinceID").val(0);
        }
    });
}

function showModal(result) {
    var element = $("#modal-container");
    if (element.data("executing")) {
        return;
    }
    element.data("executing", true);
    element.on("hidden.bs.modal",
        function () {
            element.removeData("executing");
            element.empty();
            $("#Accounting_VatNumber").focus();
        });
    element.html(result);
    element.children("div").first().modal("show");
}

function modalSelectPlace(place) {
    $("#Place").val(place);
    updatePlaceProvince(false);
    var element = $("#modal-container");
    var modal = element.children("div").first();
    modal.on("hidden.bs.modal",
        function () {
            element.removeData("executing");
            element.empty();
            $("#Accounting_VatNumber").focus();
        });
    modal.modal("hide");
}

function checkProvinces() {
    if ($("#CountryID").val() === "1") {
        $("#ProvinceID").prop("disabled", false);
    } else {
        $("#ProvinceID").val(0);
        $("#ProvinceID").prop("disabled", true);
    }
}

$(document).ready(function () {
    checkProvinces();
});