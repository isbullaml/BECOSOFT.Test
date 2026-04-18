
function initializeTranslations() {
    updateHtmlToMultipleTranslations();

    $('.translations').each(function () {
        var currentTranslationsOverview = $(this);
        toggleTranslationActionButtons(currentTranslationsOverview);
    });
    enableHtmlEditor();
    setFocusToTranslationField();
}

function setFocusToTranslationField(currentTranslations) {
    updateHtmlToMultipleTranslations();
    bindEvents();

    //first set focus on the first error-cell
    var focusCell;

    if (currentTranslations === undefined)
        focusCell = $('.input-validation-error').first();
    else
        focusCell = $(currentTranslations).find('.input-validation-error').first();

    //then on the first focus-cell
    if (focusCell === null || focusCell === undefined || focusCell.length === 0) {
        if (currentTranslations === undefined)
            focusCell = $(".translations > tbody > tr:last > .focus-translation-cell > input");
        else
            focusCell = $(currentTranslations).find(" > tbody > tr:last > .focus-translation-cell > input").first();
    }

    focusCell.focus();
}

function tabHandler(e) {
    if (e.which === 9 && e.shiftKey === false) {
        //remove the handler
        $(e.delegateTarget).off("keydown", tabHandler);

        loadTranslationRow(e.delegateTarget.closest('.translations'));

        e.preventDefault();
    }
}

function loadTranslationRow(currentTranslationsOverview) {
    var btn = $(currentTranslationsOverview).find('button[data-action="addTranslation"]');
    var url = btn.attr('data-url');

    if (url === undefined || url === null || url.length === 0) { // Fallback for old views
        var td = btn.closest("td");
        var hiddenUrl = td.find("#translationUrl");
        url = hiddenUrl.val();
    }

    if (url === undefined || url === null || url.length === 0) {
        return;
    }

    var currentIndex = $(currentTranslationsOverview).find(" > tbody > tr:last").attr("id");
    if (currentIndex === undefined) {
        currentIndex = -1;
    }
    var languagesPresent = "";
    $(currentTranslationsOverview).find(" > tbody > tr").each(function () {
        var languageValue = $(this).find("td").find("select").val();
        if (languagesPresent === "") {
            languagesPresent = languageValue;
        } else {
            languagesPresent += "," + languageValue;
        }
    });

    var htmlFieldPrefix = $(currentTranslationsOverview).find('> tbody > tr:first [id$="__HtmlFieldPrefix"]').val();

    url += (url.split('?')[1] ? '&' : '?') + 'index=' + currentIndex;
    url += (url.split('?')[1] ? '&' : '?') + 'languagesPresent=' + languagesPresent;
    url += (url.split('?')[1] ? '&' : '?') + 'htmlFieldPrefix=' + htmlFieldPrefix;

    becosoft.ajax(url, { async: true }, function (partialView) {
        if (partialView === null || partialView === undefined || partialView.length === 0) {
            return;
        }
        var lastRow = $(currentTranslationsOverview).find(" > tbody > tr:last");
        var lastLanguageID = $('[id$=LanguageID]', lastRow).val();
        var row = $(partialView);
        if (lastRow.length === 0) {
            $(currentTranslationsOverview).find(" > tbody").append(row);
        } else {
            lastRow.after(row);
        }

        setFocusToTranslationField(currentTranslationsOverview);

        reIndexTranslations();
        toggleTranslationActionButtons(currentTranslationsOverview);
        enableHtmlEditor(row);

    });
}

function toggleTranslationActionButtons(currentTranslationsOverview) {
    toggleTranslationDeleteButton(currentTranslationsOverview);
    toggleTranslationAddButton(currentTranslationsOverview);
}

function toggleTranslationDeleteButton(currentTranslationsOverview) {
    nrOfRows = $(currentTranslationsOverview).find(" > tbody > tr").length;
    if (nrOfRows === 1) {
        $(currentTranslationsOverview).find("button[data-action='removeTranslation']").addClass('disabled').attr('disabled', 'disabled');
    }
    else {
        $(currentTranslationsOverview).find("button[data-action='removeTranslation']").removeClass('disabled').removeAttr('disabled', 'disabled');
    }
}

function toggleTranslationAddButton(currentTranslationsOverview) {
    nrOfRows = $(currentTranslationsOverview).find(" > tbody > tr").length;
    nrOfLanguages = $(currentTranslationsOverview).find(" > tbody > tr:first > td > select option").length;

    if (nrOfRows === nrOfLanguages) {
        $(currentTranslationsOverview).find("button[data-action='addTranslation']").addClass('disabled').attr('disabled', 'disabled');
    }
    else {
        $(currentTranslationsOverview).find("button[data-action='addTranslation']").removeClass('disabled').removeAttr('disabled', 'disabled');
    }
}

function bindEvents() {
    //remove the handler form all input
    $(".translations > tbody > tr > .last-translation-cell > input").off("keydown", tabHandler);

    //add a handler to the last inputs
    $('.translations').each(function () {
        var tabCell = $(this).find(' > tbody > tr:last > .last-translation-cell > input');
        tabCell.on('keydown', tabHandler);
    });
}

$("#saver").on("click",
    function (e) {
        e.preventDefault();
        reIndexTranslations();
        $("#saver").closest("form").submit();
    });

function reIndexTranslations() {
    $(".translations").each(function () {
        var index = 0;
        $(this).find(">tbody>tr").each(function () {
            $(this).attr("id", index);

            $(this).find("[id*='Translations_']").each(function () {
                var id = $(this).attr("id");
                var split = id.split("_");
                split[split.length - 3] = index + "";
                $(this).attr("id", split.join('_'));


                var name = $(this).attr("name");
                split = name.split("[");
                var lastPart = split[split.length - 1];
                var split2 = lastPart.split("]");
                split2[0] = index;
                split[split.length - 1] = split2.join("]");
                name = split.join("[");
                $(this).attr("name", name);
            });
            index += 1;
        });
    });
}

function updateHtmlToMultipleTranslations() {
    $('#Translations').addClass('translations');
    $('[name="btn-translation-deleter"]').attr('data-action', 'removeTranslation').removeAttr('name');
    $('button#addTranslation').attr('data-action', 'addTranslation').removeAttr('id'); //TODO: .attr('data-url', $("#translationUrl").val()) why do we need this?
}

$(document)
    .on("click", "button[data-action='removeTranslation']", function (e) {

        var currentTranslationsOverview = $(this).closest('.translations');

        var rows = $(currentTranslationsOverview).find(" > tbody > tr").length;
        if (rows === 1) {
            alert("At least one language is required and cannot be removed");
            return;
        }

        $(this).closest("tr").remove();
        reIndexTranslations();

        toggleTranslationActionButtons(currentTranslationsOverview);
        setFocusToTranslationField();
    })
    .on('click', 'button[data-action="addTranslation"], button#addTranslation', function (e) {
        loadTranslationRow($(this).closest('.translations'));
    })
    .ready(function () {
        initializeTranslations();
    });