$(function () {
    $("#possible-values-card").on("click", "button[data-action]", function (e) {
        e.preventDefault();
        let action = $(this).data("action");
        let row = $(this).closest("tr");
        let parent = row.closest("tbody");
        let invertedSorting = $("#InvertSorting").is(":checked");

        switch (action) {
            case "add-possible-value":
                addPossibleValue();
                break;
            case "add-possible-value-translation":
                addPossibleValueTranslation(parent, $(this));
                break;
            case "delete-possible-value":
                deletePossibleValue(parent);
                break;
            case "delete-possible-value-translation":
                deletePossibleValueTranslation(parent, row);
                break;
            case "move-up":
                if (invertedSorting) {
                    moveDown(parent);
                } else {
                    moveUp(parent);
                }
                break;
            case "move-down":
                if (invertedSorting) {
                    moveUp(parent);
                } else {
                    moveDown(parent);
                }
                break;
        }
    });

    $("#possible-values-card").on("change", "input[data-default]", function (e) {
        let currentCheckbox = $(this);
        if (!currentCheckbox.is(":checked")) {
            return;
        }

        let checkBoxes = $("#PossibleValues input[data-default]");
        $.each(checkBoxes, function (index, checkbox) {
            let $checkbox = $(checkbox);
            if (!$checkbox.is(currentCheckbox) && $checkbox.is(":checked")) {
                $checkbox.click();
            }
        });
    });

    $("#possible-values-card").on("change", "input[data-sequence]", function (e) {
        let invertedSorting = $("#InvertSorting").is(":checked");
        let newPosition = parseInt($(this).val());
        let oldPosition = parseInt($(this).attr("data-previous"));
        if (newPosition < 1 || newPosition === oldPosition) {
            return;
        }

        let parent = $(this).closest("tbody");
        let toReplace = $("#PossibleValues tbody[data-sequence='" + newPosition + "']");

        if (invertedSorting) {
            if (newPosition < oldPosition) {
                toReplace.after(parent);
            } else {
                toReplace.before(parent);
            }
        } else {
            if (newPosition < oldPosition) {
                toReplace.before(parent);
            } else {
                toReplace.after(parent);
            }
        }

        reIndexPossibleValues();
    });

    $("#InvertSorting").on("change", function () {
        var arr = $.makeArray($("tbody", "#PossibleValues").detach());
        arr.reverse();
        $("#PossibleValues").append(arr);
        reIndexPossibleValues();
    }).prop("disabled", false);

    function addPossibleValue() {
        let index = $("#PossibleValues tbody[data-sequence").length + 1;
        let invertedSorting = $("#InvertSorting").is(":checked");
        let url = $("#PossibleValues").data("add-value-url") + "?index=" + index;
        becosoft.ajax(url, { type: "GET" }, function (result) {
            if (invertedSorting) {
                $("#PossibleValues thead").after(result);
            } else {
                $("#PossibleValues").append(result);
            }
            reIndexPossibleValues();
        });
    }

    function addPossibleValueTranslation(parent, button) {
        let parentIndex = parent.data("sequence");
        let translationRows = parent.find("tr[data-type='translation']");
        let index = translationRows.length + 1;
        let languagesPresent = "";
        button.prop('disabled', true);
        $.each(translationRows, function (translationIndex, row) {
            let languageValue = $(row).find("select").val();
            if (languagesPresent === "") {
                languagesPresent = languageValue;
            } else {
                languagesPresent += "," + languageValue;
            }
        });

        let url = $("#PossibleValues").data("add-value-translation-url") + "?parentIndex=" + parentIndex + "&index=" + index + "&languagesPresent=" + languagesPresent;
        becosoft.ajax(url, { type: "GET" }, function (result) {
            parent.append(result);
            reIndexPossibleValues();
        }, function () { }, function() {
            button.prop('disabled', false);
        });
    }

    function deletePossibleValue(parent) {
        parent.remove();
        reIndexPossibleValues();
    }

    function deletePossibleValueTranslation(parent, row) {
        let translations = parent.find("tr[data-type='translation']");
        if (translations.length > 1) {
            row.remove();
            reIndexPossibleValues();
        }
    }

    function moveUp(parent) {
        let sequenceInput = parent.find("input[data-sequence]");
        let currentPosition = parseInt(sequenceInput.val());
        let newPosition = currentPosition - 1;
        if (newPosition <= 0) {
            return;
        }
        sequenceInput.val(newPosition);
        sequenceInput.trigger("change");
    }

    function moveDown(parent) {
        let sequenceInput = parent.find("input[data-sequence]");
        let currentPosition = parseInt(sequenceInput.val());
        let newPosition = currentPosition + 1;
        let possibleValueCount = $("#PossibleValues tbody[data-sequence").length + 1;
        if (newPosition > possibleValueCount) {
            return;
        }

        sequenceInput.val(newPosition);
        sequenceInput.trigger("change");
    }

    function reIndexPossibleValues() {
        let parents = $("#PossibleValues tbody");
        let invertedSorting = $("#InvertSorting").is(":checked");
        let parentCount = parents.length;

        $.each(parents, function (index, parent) {
            let sequence;
            if (invertedSorting) {
                sequence = parentCount - index;
            } else {
                sequence = index + 1;
            }

            let $parent = $(parent);
            $parent.attr("data-sequence", sequence);

            let parentRow = $parent.find("tr[data-type='parent']");
            let translationRows = $parent.find("tr[data-type='translation']");

            let sequenceInput = parentRow.find("input[data-sequence]");
            sequenceInput.val(sequence);
            sequenceInput.attr("data-previous", sequence);

            let translationCountSpan = parentRow.find("span[data-translation-count]");
            translationCountSpan.text(translationRows.length);

            $(parentRow).find("[id*='PossibleValues_']").each(function () {
                let id = $(this).attr("id");
                let split = id.split("_");
                let fieldName = split[split.length - 1];

                $(this).attr("id", "PossibleValues_" + index + "__" + fieldName);
                $(this).attr("name", "PossibleValues[" + index + "]." + fieldName);
            });

            $.each(translationRows, function (translationIndex, translation) {
                $(translation).find("[id*='Translations_']").each(function () {
                    let id = $(this).attr("id");
                    let split = id.split("_");
                    let fieldName = split[split.length - 1];

                    $(this).attr("id", "PossibleValues_" + index + "__Translations_" + translationIndex + "__" + fieldName);
                    $(this).attr("name", "PossibleValues[" + index + "].Translations[" + translationIndex + "]." + fieldName);
                });
            });
        });
    }
});