function loadFeatureRow(currentFeaturesOverview) {
    var btn = $(currentFeaturesOverview).find('#addFeature');
    var url = btn.attr('data-url');

    var useHtmlEditor = $(currentFeaturesOverview).data("html") === 1;
    var currentIndex = $(currentFeaturesOverview).find(" > tbody > tr:last").attr("id");
    if (currentIndex === undefined) {
        currentIndex = -1;
    }
    var featuresPresent = "";
    $(currentFeaturesOverview).find(" > tbody > tr").each(function () {
        var languageValue = $(this).find("td").find("select").val();
        if (featuresPresent === "") {
            featuresPresent = languageValue;
        } else {
            featuresPresent += "," + languageValue;
        }
    });

    var htmlFieldPrefix = $(currentFeaturesOverview).find('> tbody > tr:first [id$="__HtmlFieldPrefix"]').val();

    url += (url.split('?')[1] ? '&' : '?') + 'index=' + currentIndex;
    url += (url.split('?')[1] ? '&' : '?') + 'featuresPresent=' + featuresPresent;
    url += (url.split('?')[1] ? '&' : '?') + 'htmlFieldPrefix=' + htmlFieldPrefix;
    loadingIcon.showLoadingScreen();
    becosoft.ajax(url, { async: true }, function (partialViews) {
        if (partialViews === null || partialViews === undefined || partialViews.length === 0) {
            loadingIcon.hideLoadingScreen();
            return;
        }
        var lastRow = $(currentFeaturesOverview).find(" > tbody > tr:last");
        var row = $(partialViews.featureRow);
        if (lastRow.length === 0) {
            $(currentFeaturesOverview).find(" > tbody").append(row);
        } else {
            lastRow.after(row);
        }

        reIndexFeatures();
        highlightFeatureRow(row);

        $("#featureInfoDiv").append($(partialViews.featureInfo));
        showFutureInfoOfFeatureRow(++currentIndex);
        loadingIcon.hideLoadingScreen();
        toggleFeatureActionButtons(currentFeaturesOverview);

        if (useHtmlEditor) {
            enableHtmlEditor(row);
        }
    });
}

function highlightFeatureRow(row) {
    $('tr[name = "feature-row"]').each(function () {
        $(this).removeClass('highlight');
    });

    $(row).addClass('highlight');
}

function showFutureInfoOfFeatureRow(rowId) {
    let featureInfoId = "featureAdditionalInfo_" + rowId;
    $('.feature-info').each(function () {
        $(this).hide();
    });

    $('#' + featureInfoId).show();
}

function addFutureInfoDiv(featureInfo) {
    $("#existingDiv").append(featureInfo);
}
function toggleFeatureActionButtons(currentFeaturesOverview) {
    toggleFeatureAddButton(currentFeaturesOverview);
}

function toggleFeatureDeleteButton(currentFeaturesOverview) {
    let nrOfRows = $(currentFeaturesOverview).find(" > tbody > tr").length;
    if (nrOfRows === 1) {
        $(currentFeaturesOverview).find("#removeFeature").addClass('disabled').attr('disabled', 'disabled');
    }
    else {
        $(currentFeaturesOverview).find("#removeFeature").removeClass('disabled').removeAttr('disabled', 'disabled');
    }
}

function toggleFeatureAddButton(currentFeaturesOverview) {
    let nrOfRows = $(currentFeaturesOverview).find(" > tbody > tr").length;
    let nrOfFeatures = Number($('#Features').attr('data-feature-count'));

    if (nrOfRows === nrOfFeatures) {
        $(currentFeaturesOverview).find("#addFeature").addClass('disabled').attr('disabled', 'disabled');
    }
    else {
        $(currentFeaturesOverview).find("#addFeature").removeClass('disabled').removeAttr('disabled', 'disabled');
    }
}

$("#saver").on("click",
    function (e) {
        e.preventDefault();
        reIndexFeatures();
        $("#saver").closest("form").submit();
    });

function reIndexFeatures() {
    $(".features").each(function () {
        var index = 0;
        $(this).find(">tbody>tr").each(function () {
            let currentIndex = Number($(this).attr("id"));
            $(this).attr("id", index);

            $(this).find("[name*='InfoTemplateFeatures[']").each(function () {
                var id = $(this).attr("id");
                if (id !== undefined) {
                    var split = id.split("_");
                    split[split.length - 3] = index + "";
                    $(this).attr("id", split.join('_'));
                }

                var name = $(this).attr("name");
                if (name !== undefined) {
                    split = name.split("[");
                    var lastPart = split[split.length - 1];
                    var split2 = lastPart.split("]");
                    split2[0] = index;
                    split[split.length - 1] = split2.join("]");
                    name = split.join("[");
                    $(this).attr("name", name);
                }

                var label = $(this).closest('div.custom-checkbox').find('label');
                var newLabelForValue = $(this).attr("id");
                if (newLabelForValue !== undefined && label && label.length > 0) {
                    label.attr("for", newLabelForValue);
                }
            });

            if (currentIndex !== index) {
                reIndexFeatureInfoDivOfFeature(currentIndex, index);
            }

            index += 1;
        });
    });
}

function reIndexFeatureInfoDivOfFeature(oldId, newId) {
    let featureDiv = $("#featureAdditionalInfo_" + oldId);
    let featureValuesDiv = $("#FeatureValues_" + oldId);

    featureDiv.attr("id", "featureAdditionalInfo_" + newId);
    featureValuesDiv.attr("id", "FeatureValues_" + newId);

    $(featureDiv).find("[name*='InfoTemplateFeatures[" + oldId + "']").each(function () {
        var id = $(this).attr("id");

        let isTranalstionField = $(this).attr("data-isTranslation");
        let idSplitLength = isTranalstionField === '1' ? 9 : 6;
        let nameSplitLength = isTranalstionField === '1' ? 3 : 2;
        let nameLastPartLength = isTranalstionField === '1' ? 3 : 2;

        if (id !== undefined && id !== null) {
            var idSplit = id.split("_");
            idSplit[idSplit.length - idSplitLength] = newId + "";
            $(this).attr("id", idSplit.join('_'));
        }

        var name = $(this).attr("name");
        if (name !== undefined) {
            var split = name.split("[");
            var middlePart = split[split.length - nameLastPartLength];
            var split2 = middlePart.split("]");
            split2[0] = newId;
            split[split.length - nameSplitLength] = split2.join("]");
            name = split.join("[");
            $(this).attr("name", name);
        }
    });
}

function loadFeatureValueRow(currentFeatureValuesOverview) {
    var btn = $(currentFeatureValuesOverview).find('#addFeatureValue');
    var url = btn.attr('data-url');

    var useHtmlEditor = $(currentFeatureValuesOverview).data("html") === 1;
    var featureIndex = $(currentFeatureValuesOverview).attr("data-featureId");
    var currentIndex = $(currentFeatureValuesOverview).find(" > tbody > tr:last").attr("id");
    if (currentIndex === undefined) {
        currentIndex = -1;
    }

    url += (url.split('?')[1] ? '&' : '?') + 'index=' + currentIndex;
    url += (url.split('?')[1] ? '&' : '?') + 'featureIndex=' + featureIndex;

    becosoft.ajax(url, { async: true }, function (partialView) {
        if (partialView === null || partialView === undefined || partialView.length === 0) {
            return;
        }
        var lastRow = $(currentFeatureValuesOverview).find(" > tbody > tr:last");
        var row = $(partialView);
        if (lastRow.length === 0) {
            $(currentFeatureValuesOverview).find(" > tbody").append(row);
        } else {
            lastRow.after(row);
        }

        reIndexFeatureValues();
        if (useHtmlEditor) {
            enableHtmlEditor(row);
        }
    });
}

function reIndexFeatureValues() {
    $(".featureValues").each(function () {
        var index = 0;
        $(this).find(">tbody>tr").each(function () {
            $(this).attr("id", index);

            $(this).find("[name*='FeatureValues[']").each(function () {
                let id = $(this).attr("id");
                let isTranalstionField = $(this).attr("data-isTranslation");
                let idSplitLength = isTranalstionField === '1' ? 6 : 3;
                let nameSplitLength = isTranalstionField === '1' ? 2 : 1;
                let nameLastPartLength = isTranalstionField === '1' ? 2 : 1;

                if (id !== undefined) {
                    var split = id.split("_");
                    split[split.length - idSplitLength] = index + "";
                    $(this).attr("id", split.join('_'));
                }

                var name = $(this).attr("name");
                if (name !== undefined) {
                    split = name.split("[");
                    var lastPart = split[split.length - nameLastPartLength];
                    var split2 = lastPart.split("]");
                    split2[0] = index;
                    split[split.length - nameSplitLength] = split2.join("]");
                    name = split.join("[");
                    $(this).attr("name", name);
                }
            });
            index += 1;
        });
    });
}

$(document)
    .on("click", "#removeFeature", function (e) {
        var currentFeaturesOverview = $(this).closest('.features');
        let row = $(this).closest("tr");
        let rowId = row.attr('id');
        let featureInfoId = "featureAdditionalInfo_" + rowId;
        row.remove();
        $('#' + featureInfoId).remove();

        reIndexFeatures();
        toggleFeatureActionButtons(currentFeaturesOverview);
    })
    .on('click', '#addFeature', function (e) {
        loadFeatureRow($(this).closest('.features'));
    })
    .on("click", "#removeFeatureValue", function (e) {
        var currentFeatureValuesOverview = $(this).closest('.featureValues');
        $(this).closest("tr").remove();
        reIndexFeatureValues();
    })
    .on('click', '#addFeatureValue', function (e) {
        loadFeatureValueRow($(this).closest('.featureValues'));
    })
    .on('click', 'tr[name = "feature-row"]', function (e) {
        let rowId = $(this).attr('id');
        if (rowId !== undefined) {

            highlightFeatureRow(this);

            let featureInfoId = "featureAdditionalInfo_" + rowId;
            $('.feature-info').each(function () {
                $(this).hide();
            });

            $('#' + featureInfoId).show();
        }
    });