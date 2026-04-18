// Update the errors on the modal
function updateModalErrors(errors) {
    $(".modal span[data-valmsg-for]").text("");
    $(".modal #modalErrors").text("");
    $(".modal .alert-danger").addClass("d-none");
    var modalErrors = false;
    $.each(errors,
        function() {
            var key = this.Key.split(".")[1];
            if (key === undefined || key === null || key === "") {
                key = this.Key;
            }
            var field = $(".modal span[data-valmsg-for='" + key + "']");
            if (field.length !== 1) {
                modalErrors = true;
                $(".modal #modalErrors").append("<li>" + this.Value + "</li>");
            } else {
                field.text(this.Value);
            }
        });

    if (modalErrors) {
        $(".modal .alert-danger").removeClass("d-none");
    }
}

// Update the select from an entity
function updateSelect(name, fieldId, valueField, textField, isSumo, defaultValue, urlParams = "", callback = function () { }) {
    var updateUrl = $("#updateUrl #" + name).val();
    if (urlParams === false) {
        updateUrl = "";
    } else if (urlParams !== "") {
        updateUrl = updateUrl + "?";
        for (var i = 0; i < urlParams.length; i++) {
            updateUrl = updateUrl + urlParams[i];
            if (i < urlParams.length - 1) {
                updateUrl = updateUrl + "&";
            }
        }
    }

    if (updateUrl !== undefined && updateUrl !== null && updateUrl !== "") {
        becosoft.ajax(updateUrl, { async: true }, function(result) {
            $(fieldId).empty();
            if (defaultValue) {
                $(fieldId).append($("<option>", { value: "" }).text("-"));
            }
            $.each(result, function () {
                $(fieldId).append($("<option>", { value: this[valueField] }).text(this[textField]));
            });
            callback();
            if (isSumo === true) {
                if ($(fieldId).length) {
                    $(fieldId)[0].sumo.reload();
                }
            }
        });
    }
}

// Create a modal for adding an entity
function createModal(name, fieldId, valueField, textField, isSumo, defaultValue, callback = function() {}, urlParams = "") {
    var addlink = $("#" + name + "Add");
    var url = addlink.attr("href");
    if (url === undefined || url === null || url.length === 0) {
        return;
    }
    becosoft.ajax(url, { async: true }, function(result) {
        var eventListenersFunction = function() {
            $("#addTranslation").on("click", loadTranslationRow);
            $("#WithHtmlColor").on("change", function() {
                if ($(this).is(":checked")) {
                    $("#htmlColorCell").removeClass("d-none");
                    $("#noColorCell").addClass("d-none");
                } else {
                    $("#htmlColorCell").addClass("d-none");
                    $("#noColorCell").removeClass("d-none");
                }
            });

            $("#" + name + "AddButton").on("click", function(e) {
                e.preventDefault();
                var form = $("#" + name + "AddForm");
                var formData = form.serialize();
                becosoft.ajax(form.attr("action"), {
                    type: "POST",
                    data: formData
                }, function(postResult) {
                    if (postResult.Result) {
                        closeCurrentModal();
                        if (urlParams !== true) {
                            updateSelect(name, fieldId, valueField, textField, isSumo, defaultValue, urlParams, callback);
                        } else {
                            callback();
                        }
                    } else {
                        updateModalErrors(postResult.Errors);
                    }
                });
            });

            $("#modal-close-button").on("click", function(e) {
                e.preventDefault();
                closeCurrentModal();
            });
        };

        openModal("#modal-container", result, false, eventListenersFunction);
    });
}

// Modal for adding color
$("#articleColorAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        if ($(this).data("multiselect") === true) {
            var selections = $("#SelectedColorIDs").val();
            createModal("articleColor",
                "#SelectedColorIDs",
                "ColorID",
                "ColorName",
                false,
                false,
                function() {
                    if (selections !== undefined) {
                        $("#SelectedColorIDs").val(selections);
                    }
                    $("#SelectedColorIDs").multiSelect("refresh");
                });
        } else {
            createModal("articleColor", "#ColorID", "ColorID", "ColorName", true, true);
        }
    }
);

// Modal for adding brand
$("#brandAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        createModal("brand", "#BrandID", "BrandID", "Name", true, true);
    }
);

// Modal for adding season
$("#seasonAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        createModal("season", "#SeasonID", "SeasonID", "Name", false, true,
            function () {
                $(".seasonSelect").each(function () {
                    var id = "#" + $(this).attr("id");
                    updateSelect("season", id, "SeasonID", "Name", false, false);
                });
            });
    }
);

// Modal for adding subseason
$("#subSeasonAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        var seasonId = $("#SeasonID").val();
        var updateParams = false;
        if (seasonId !== undefined && seasonId !== null && seasonId !== "" && seasonId !== 0 && seasonId !== "-") {
            updateParams = ["seasonID=" + seasonId];
        }
        if ($(".seasonSelect").length > 0) {
            updateParams = true;
        }
        createModal("subSeason", "#SubSeasonID", "SubSeasonID", "Name", false, false,
            function () {
                $(".seasonSelect").each(function () {
                    var index = $(this).attr("id").split("__")[0];
                    var rowSeasonId = $(this).val();
                    var id = "#" + index + "__SubSeasonID";
                    var rowUpdateParams = false;
                    if (rowSeasonId !== undefined && rowSeasonId !== null && rowSeasonId !== "" && rowSeasonId !== 0 && rowSeasonId !== "-") {
                        rowUpdateParams = ["seasonID=" + rowSeasonId];
                    }
                    updateSelect("subSeason", id, "SubSeasonID", "Name", false, false, rowUpdateParams, function() {
                        $(id).prepend($("<option class='font-bold' selected>", { value: 0 }).text("-"));
                    });
                });
            }, updateParams);
    }
);

// Modal for adding intrastat
$("#intrastatAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        createModal("intrastat", "#IntrastatID", "IntrastatCodeID", "Description", true, true, function() {}, "");
    }
);

// Modal for adding accountnumberpurchase
$("#accountNumberPurchaseAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        createModal("accountNumberPurchase", "#AccountNumberPurchaseID", "AccountNumberId", "Description", true, true);
    }
);


// Modal for adding accountnumbersale
$("#accountNumberSaleAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        createModal("accountNumberSale", "#AccountNumberSaleID", "AccountNumberId", "Description", true, true);
    }
);

// Modal for adding collections
$("#collectionAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        createModal("collection", "#CollectionID", "CollectionID", "Name", true, true);
    }
);

// Modal for adding accountnumberpurchase
$("#formOfAddressAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        createModal("formOfAddress", "#FormOfAddressID", "FormOfAddressID", "Name", false, true);
    }
);


// Modal for adding accountnumbersale
$("#deliveryConditionAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        createModal("deliveryCondition", "#Accounting_DeliveryConditionID", "ID", "Description", false, true);
    }
);

// Modal for adding collections
$("#contactGroupAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        createModal("contactGroup", "#GroupID", "ContactGroupId", "Name", false, true);
    }
);

// Modal for adding collections
$("#contactSubGroupAdd").on("click",
    function (e) {
        e.preventDefault();
        $(this).blur();
        createModal("contactSubGroup", "#SubGroupID", "contactSubGroupId", "Name", false, true);
    }
);