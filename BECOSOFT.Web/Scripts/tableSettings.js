const indexField = "data-index";
const columnField = "data-column";
var preferencesSaveUrl = "";
var preferencesLoadUrl = "";
var preferencesResetUrl = "";
var preferenceKey = "";

var settingsChanged = false;

function initializeSettings() {
    var content = $("#column-changer-content");
    preferencesSaveUrl = content.attr("data-save-url");
    preferencesLoadUrl = content.attr("data-load-url");
    preferencesResetUrl = content.attr("data-reset-url");
    preferenceKey = content.attr("data-key");
    var html = content.html();
    content.remove();

    $("#column-modal-container").html(html);

    bindTableSettingEvents();
    $("form table td").each(function () {
        var td = $(this);
        var index = td.attr(indexField);
        if (index !== undefined) {
            td.attr(columnField, index);
        }
    });

    loadVisibilitySettings();
    bindColumnButtonEvent();
}

function loadVisibilitySettings() {
    // GET from URL
    if (preferencesLoadUrl === undefined) { return; }
    becosoft.ajax(preferencesLoadUrl, { type: "GET", data: { preferenceKey: preferenceKey } }, function (result) {
        if (result !== "") {
            var data = JSON.parse(result);
            var settings = [];
            $(data).each(function (index, setting) {
                settings.push({
                    Key: setting.ColumnKey,
                    Visible: setting.Visible,
                    Position: setting.ColumnIndex,
                    PreviousPosition: setting.ColumnKey,
                    Delta: setting.ColumnKey - setting.ColumnIndex
                });
            });

            applyVisibilitySettings(settings);
            sortColumnsList(settings);
        }
    });
}

function getVisibilitySettings() {
    var settings = [];

    $("#column-changer-rows input").map(function (index, element) {
        var input = $(element);
        var parentDiv = input.closest("div");
        var delta = index - parseInt(parentDiv.attr(indexField));

        settings.push({
            Key: parseInt(parentDiv.attr(columnField)),
            Visible: input.prop("checked"),
            Position: index,
            PreviousPosition: parseInt(parentDiv.attr(indexField)),
            Delta: delta
        });
    });

    return settings;
}

function applyVisibilitySettings(settings) {
    var firstSwap = false;

    settings.sort(function (a, b) {
        return Math.abs(b.Delta) - Math.abs(a.Delta);
    });

    settings.forEach(function (setting) {
        const div = $("#column-changer-rows").find(getDataSelector(columnField, setting.Key));
        const input = div.find("input");
        const actualElement = $(getDataSelector(columnField, setting.Key));
        const actualPosition = parseInt(actualElement.attr(indexField));

        if (setting.Visible) {
            input.prop("checked", true);
            div.removeClass("unchecked-item");
            $(getDataSelector(columnField, setting.Key), "form table").show();
        } else {
            input.prop("checked", false);
            div.addClass("unchecked-item");
            $(getDataSelector(columnField, setting.Key), "form table").hide();
        }

        if (firstSwap && setting.Delta !== 0) {
            if (setting.Position === actualPosition) {
                return;
            }
        }

        firstSwap = swapColumns("table", setting.PreviousPosition, setting.Position);
    });

    reIndexColumns();
    sortColumnsList(settings);
    settingsChanged = true;
}

function saveVisibilitySettings() {
    var preferences = [];
    $(getVisibilitySettings()).each(function () {
        var setting = this;
        preferences.push({
            ColumnIndex: setting.Position,
            ColumnKey: setting.Key,
            ColumnName: "Web",
            Visible: setting.Visible
        });
    });

    // Post to URL
    becosoft.ajax(preferencesSaveUrl, { type: "POST", data: { preferenceKey: preferenceKey, preferences: preferences } });
    applyVisibilitySettings(getVisibilitySettings());
    closeCurrentModal();
}

function resetVisibilitySettings() {
    becosoft.ajax(preferencesResetUrl, { type: "POST", data: { preferenceKey: preferenceKey } }, function () {
        $("#column-changer-rows .sortable").each(function (index, element) {
            const div = $(element).find(".custom-control");
            const input = div.find("input");
            const defaultVisible = div.data("visible").toLowerCase() === "true";

            if (defaultVisible) {
                input.prop("checked", true);
                div.removeClass("unchecked-item");
            } else {
                input.prop("checked", false);
                div.addClass("unchecked-item");
            }
        });

        applyVisibilitySettings(getVisibilitySettings());
    });
}

function sortColumnsList(settings) {
    var rowContainer = $("#column-changer-rows");
    var newRowContainer = $("<div id='column-changer-rows'></div>");
    // Add all divs to an array
    var divs = [];

    rowContainer.children("div").each(function () {
        var elem = $(this);
        divs.push(elem);
    });

    // Sort the divs in descending order
    divs.sort(function (a, b) {
        var aSettingPosition = settings.filter(function (item) {
            return parseInt(item.Key) === parseInt(a.find(".custom-control").attr(columnField));
        })[0];

        var bSettingPosition = settings.filter(function (item) {
            return parseInt(item.Key) === parseInt(b.find(".custom-control").attr(columnField));
        })[0];

        var aTh = $("th" + getDataSelector(columnField, aSettingPosition.Key));
        var aIndex = aTh.attr(indexField);

        var bTh = $("th" + getDataSelector(columnField, bSettingPosition.Key));
        var bIndex = bTh.attr(indexField);
        return parseInt(aIndex) - parseInt(bIndex);
    });

    // Add them into the ul in order
    for (var j = 0; j < divs.length; j++) {
        newRowContainer.append(divs[j]);
    }

    rowContainer.replaceWith(newRowContainer);
    bindSortableList();
    reIndexColumns();
}

//Bindings

function bindSortableList() {
    $("#column-changer-rows").sortable({
        items: "div.sortable",
        handle: ".table-menu-sortable",
        //update: function() {
        //    var settings = getVisibilitySettings();
        //    applyVisibilitySettings(settings);
        //}
    }).disableSelection();
}

function bindTableSettingEvents() {
    $("#column-changer-modal a.hide-all").on("click", function (e) {
        e.preventDefault();
        var link = $(this);
        link.blur();

        $("#column-changer-rows input").prop("checked", false);

        //applyVisibilitySettings(getVisibilitySettings());
        return false;
    });

    $("#column-changer-modal a.show-all").on("click", function (e) {
        e.preventDefault();
        var link = $(this);
        link.blur();

        $("#column-changer-rows input").prop("checked", true);

        //applyVisibilitySettings(getVisibilitySettings());
        return false;
    });

    $("#column-changer-modal a.reset-all").on("click", function (e) {
        e.preventDefault();
        var link = $(this);
        link.blur();
        resetVisibilitySettings();
        //$("button[type=submit].active").focus();
        //$("#paginationForm").submit();
        closeCurrentModal();
        return false;
    });

    bindSortableList();
}

function bindColumnButtonEvent() {
    $("#column-changer-button").off("click").on("click", function (e) {
        e.preventDefault();
        openModal("#column-modal-container");
    });
}

// Utilities

function swapColumns(table, from, to) {
    if (from === to) {
        return false;
    }

    $("tr:not(.totals-row):not(.text-center)", table).each(function () {
        var cols = $(this).find("th, td");
        var elemToPlace = cols.filter(getDataSelector(indexField, from));
        var elemTo = cols.filter(getDataSelector(indexField, to));

        if (from > to) {
            elemToPlace.detach().insertBefore(elemTo);
        } else {
            elemToPlace.detach().insertAfter(elemTo);
        }
        reIndexColumns();
    });

    return true;
}

function reIndexColumns() {
    var beforeFieldsCount = $("tr th.fixed-column-before").length;
    $("tr:not(.totals-row)", "table").each(function () {
        $(this).children("th, td").each(function () {
            var elem = $(this);
            if (!elem.hasClass("fixed-column")) {
                var index = elem.index() - beforeFieldsCount;
                elem.attr(indexField, index);
            }
        });
    });


    $("#column-changer-rows .sortable").each(function (index, element) {
        $(element).find(".custom-control").attr(indexField, index);
    });
}

function getDataSelector(key, value) {
    return `[${key}='${value}']`;
}