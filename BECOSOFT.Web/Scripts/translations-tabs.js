$(function () {
    $("[data-translations]").each(function () {
        var currentTranslationsOverview = $(this);
        var addBtn = $(currentTranslationsOverview).find("a[data-action='addTranslation']");
        addBtn.on("click", function (e) {
            e.preventDefault();

            loadTranslationRow(currentTranslationsOverview);
        });
    });

    reIndexTranslations();
    toggleTranslationActionButtons($("[data-translations]"));
    checkLanguageIDs();

    $("[data-translations] li:first a").trigger("click");
});


function loadTranslationRow(currentTranslationsOverview) {
    var btn = $(currentTranslationsOverview).find("a[data-action='addTranslation']");
    btn.addClass("disabled");
    var url = btn.attr("href");

    if (url === undefined || url === null || url.length === 0) {
        return;
    }

    var tabListItem = btn.parent();
    var parent = btn.closest("[data-translations]");
    var container = parent.find("[data-translation-container]");
    var translations = container.find("[data-translation]");

    var useHtmlEditor = $(currentTranslationsOverview).data("html") === 1;
    var currentIndex = translations.length - 1;
    var languagesPresent = "";
    $(translations).each(function () {
        var languageValue = $(this).find("[data-language]").val();
        if (languagesPresent === "") {
            languagesPresent = languageValue;
        } else {
            languagesPresent += "," + languageValue;
        }
    });

    var htmlFieldPrefix = $(currentTranslationsOverview).find("> [data-translation-container] > [data-translation]:first [id$='__HtmlFieldPrefix']").val();

    url += (url.split("?")[1] ? "&" : "?") + "index=" + currentIndex;
    url += (url.split("?")[1] ? "&" : "?") + "languagesPresent=" + languagesPresent;
    url += (url.split("?")[1] ? "&" : "?") + "htmlFieldPrefix=" + htmlFieldPrefix;

    becosoft.ajax(url, { async: true }, function (result) {
        if (result === null || result === undefined || result.length === 0) {
            var error = $("#errorAllLanguageAreInUse").text();
            alert(error);
            btn.removeClass("disabled");
            return;
        }

        var tabView = $(result.TabView);
        tabListItem.before(tabView);
        var partialView = result.TranslationView;
        var editor = $(partialView);
        if (translations.length === 0) {
            container.append(editor);
        } else {
            var lastTranslations = translations.last();
            lastTranslations.after(editor);
        }

        if (useHtmlEditor) {
            enableHtmlEditor(row);
        }

        reIndexTranslations();
        toggleTranslationActionButtons(currentTranslationsOverview);

        bindTranslationEvents(editor, tabView);

        tabView.find("a").trigger("click");
    });
}

function bindTranslationEvents(editor, tab) {
    var deleteBtn = editor.find("button[data-action='delete']");
    deleteBtn.off("click").on("click", function (deleteClickEvent) {
        let translationTabContents = editor.closest("[data-translations]");
        deleteClickEvent.preventDefault();
        var btn = $(this);
        var page = btn.closest(".tab-pane");
        var tabID = page.attr("aria-labelledby");
        var tab = $("#" + tabID).parent();
        tab.remove();
        page.remove();
        reIndexTranslations();
        toggleTranslationActionButtons(translationTabContents);
        var lastBtn = $("[data-translation-header] li:not([data-action='add-translation']):last a");
        if (lastBtn && lastBtn.length > 0) {
            lastBtn.trigger("click");
        } else {
            var addBtn = $("[data-translation-header] li[data-action='add-translation'] a");
            addBtn.trigger("click");
        }
    });

    var tabLink = tab.find("a");
    tabLink.on("mousedown", function (tabClickEvent) {
        if (tabClickEvent.button === 1) {
            tabClickEvent.preventDefault();
            deleteBtn.trigger("click");
        }
    });

    var languageSelector = editor.find("select[id$='LanguageID']");
    languageSelector.on("change", function () {
        var input = $(this);
        var language = input.find("option:selected").text();
        var page = input.closest(".tab-pane");
        var tabID = page.attr("aria-labelledby");
        var tab = $("#" + tabID);
        var text = tab.find("span[data-content='language']");
        text.text(language);
        checkLanguageIDs();
    });

    if (typeof bindExtraTranslationEvents === 'function') {
        bindExtraTranslationEvents(editor, tab);
    }
}

function checkLanguageIDs() {
    var languageInputs = $("select[data-language]");
    var languageValues = languageInputs.map(function () { return { LanguageID: $(this).val(), Input: $(this) }; }).toArray();
    var values = languageValues.map((kvp) => {
        return {
            count: 1,
            languageID: kvp.LanguageID
        };
    }).reduce((a, b) => {
        a[b.languageID] = (a[b.languageID] || 0) + b.count;
        return a;
    }, {});

    var duplicates = Object.keys(values).filter((a) => values[a] > 1);
    $(languageValues).each(function () {
        var val = this;
        var page = val.Input.closest(".tab-pane");
        var tabID = page.attr("aria-labelledby");
        var link = $("#" + tabID);
        var icon = link.find("span[data-content='icon']");
        if (duplicates.indexOf(val.LanguageID) >= 0) {
            icon.removeClass("d-none");
        } else {
            icon.addClass("d-none");
        }
    });

    return duplicates.length > 0;
}

function reIndexTranslations() {
    $("[data-translations]").each(function () {
        var index = 0;
        $(this).find(">[data-translation-container] > [data-translation]").each(function () {
            var id = $(this).attr("id");
            var tabControls = $('[aria-controls=' + id + ']');
            var split = id.split("-");
            split[split.length - 1] = index + "";
            var newId = split.join("-");
            $(this).attr("id", newId);
            tabControls.attr("aria-controls", newId);
            tabControls.attr("href", '#' + newId);


            var labelledby = $(this).attr("aria-labelledby");
            var tabId = $('#' + labelledby);
            split = labelledby.split("-");
            split[split.length - 2] = index + "";
            var newLabelledBy = split.join("-");
            $(this).attr("aria-labelledby", newLabelledBy);
            tabId.attr("id", newLabelledBy);


            $(this).find("[id*='Translations_']").each(function () {
                var id = $(this).attr("id");
                var split = id.split("_");
                split[split.length - 3] = index + "";
                $(this).attr("id", split.join("_"));


                var name = $(this).attr("name");
                split = name.split("[");
                var lastPart = split[split.length - 1];
                var split2 = lastPart.split("]");
                split2[0] = index;
                split[split.length - 1] = split2.join("]");
                name = split.join("[");
                $(this).attr("name", name);
            });

            $(this).find("[for*='Translations_']").each(function () {
                var id = $(this).attr("for");
                var split = id.split("_");
                split[split.length - 3] = index + "";
                $(this).attr("for", split.join('_'));
            });

            $(this).find("[data-valmsg-for*='Translations[']").each(function () {
                var name = $(this).attr("data-valmsg-for");
                var split = name.split("[");
                var lastPart = split[split.length - 1];
                var split2 = lastPart.split("]");
                split2[0] = index;
                split[split.length - 1] = split2.join("]");
                name = split.join("[");
                $(this).attr("data-valmsg-for", name);
            });
            index += 1;
        });


    });

    var containers = $("[data-translations] .tab-pane");
    containers.each(function () {
        var editor = $(this);
        var tabID = editor.attr("aria-labelledby");
        var tab = $("#" + tabID);
        bindTranslationEvents(editor, tab);
    });
}

function toggleTranslationActionButtons(currentTranslationsOverview) {
    toggleTranslationDeleteButton(currentTranslationsOverview);
    toggleTranslationAddButton(currentTranslationsOverview);
}

function toggleTranslationDeleteButton(currentTranslationsOverview) {
    nrOfRows = $(currentTranslationsOverview).find("[data-translation-container] [data-translation]").length;
    if (nrOfRows === 1) {
        $(currentTranslationsOverview).find("[data-action='delete'], [data-action='removeTranslation']").addClass("disabled").attr("disabled", "disabled");
    }
    else {
        $(currentTranslationsOverview).find("[data-action='delete'], [data-action='removeTranslation']").removeClass("disabled").removeAttr("disabled", "disabled");
    }
}

function toggleTranslationAddButton(currentTranslationsOverview) {
    nrOfRows = $(currentTranslationsOverview).find("[data-translation-container] [data-translation]").length;
    nrOfLanguages = $(currentTranslationsOverview).find("[data-translation-container] [data-translation]:first [data-language] option").length;

    if (nrOfRows === nrOfLanguages) {
        $(currentTranslationsOverview).find("a[data-action='addTranslation']").addClass("disabled").attr("disabled", "disabled");
    }
    else {
        $(currentTranslationsOverview).find("a[data-action='addTranslation']").removeClass("disabled").removeAttr("disabled", "disabled");
    }
}