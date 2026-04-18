$("#addVatGroup").on("click", loadVatGroupRow);

$(document).ready(function() {
    var tabCell = $("#VatGroups > tbody > tr:last > .last-vatGroup-cell > input");
    tabCell.keydown(tabHandler);
    var focusCell = $("#VatGroups > tbody > tr:last > .focus-vatGroup-cell > input");
    if (focusCell === null || focusCell === undefined || focusCell.length === 0) {
        focusCell = tabCell;
    }
    focusCell.focus();
});

function tabHandler(e) {
    if (e.which === 9 && e.shiftKey === false) {
        loadTranslationRow();
        e.preventDefault();
    }
}

function loadVatGroupRow() {
    var url = $("#vatGroupUrl").val();
    if (url === undefined || url === null || url.length === 0) {
        return;
    }
    vatGroupReIndex();
    var currentIndex = $("#VatGroups > tbody > tr:last").attr("id");
    if (currentIndex === undefined) {
        currentIndex = -1;
    }
    var countriesPresent = "";
    $("#VatGroups > tbody > tr").each(function () {
        var countryValue = $(this).find("td").find("select").val();
        if (countriesPresent === "") {
            countriesPresent = countryValue;
        } else {
            countriesPresent += "," + countryValue;
        }
    });
    becosoft.ajax(url + "?index=" + currentIndex + "&countriespresent=" + countriesPresent, { async: true }, function (partialView) {
        $("#VatGroups > tbody > tr").each(function () {
            $(this).find("td input:text").each(function () {
                $(this).off("keydown", tabHandler);
            });
        });
        if (partialView === null || partialView === undefined || partialView.length === 0) {
            return;
        }
        var lastRow = $("#VatGroups > tbody > tr:last");
        if (lastRow.length === 0) {
            $("#VatGroups > tbody").append(partialView);
        } else {
            lastRow.after(partialView);
        }
        $("button[name='btn-vatGroup-deleter']").on("click",
            function () {
                var rows = $("#Translations > tbody > tr").length;
                if (rows > 1) {
                    $(this).closest("tr").remove();
                    vatGroupReIndex();
                }
            });
        var tabCell = $("#VatGroups > tbody > tr:last > .last-vatGroup-cell > input");
        tabCell.keydown(tabHandler);
        var focusCell = $("#VatGroups > tbody > tr:last > .focus-vatGroup-cell > input");
        if (focusCell === null || focusCell === undefined || focusCell.length === 0) {
            focusCell = tabCell;
        }
        focusCell.focus();
    });
}

$("button[name='btn-vatGroup-deleter']").on("click",
    function () {
        var rows = $("#Translations > tbody > tr").length;
        if (rows > 1) {
            $(this).closest("tr").remove();
            vatGroupReIndex();
        }
    });
$("#saver").on("click",
    function (e) {
        e.preventDefault();
        vatGroupReIndex();
        $("#saver").closest("form").submit();
    });

$("#VatGroups").on("change", ".countrySelect",
    function () {
        var index = $(this).attr("id").split("__")[0];
        var countryId = $(this).val();
        updateVatGroups(countryId, index);
    }
);

function updateVatGroups(countryId, index) {
    var url = $("#countryUpdateUrl").val();
    if (url === undefined || url === null || url.length === 0) {
        return;
    }
    becosoft.ajax(url + "?countryId=" + countryId, { async: true }, function (result) {
        var vatGroupSelect = $("#" + index + "__VatGroupID");
        vatGroupSelect.html("");
        $.each(result, function () {
            vatGroupSelect.append($("<option>", { value: this["VatGroupID"] }).text(this["VatPercentage"].toFixed(2).replace(".", ",") + "%"));
        });
    });
}

function vatGroupReIndex() {
    var index = 0;
    $("#VatGroups").find(">tbody>tr").each(function () {
        $(this).attr("id", index);
        $(this).find("[id*='VatGroups']").each(function () {
            var id = $(this).attr("id");
            var split = id.split("_");
            $(this).attr("id", id.replace(split[1], index));
        });
        $(this).find("[name*='VatGroups[']").each(function () {
            var name = $(this).attr("name");
            var split = name.split("[")[1].split("]")[0];
            $(this).attr("name", name.replace(split, index));
        });
        index += 1;
    });
}