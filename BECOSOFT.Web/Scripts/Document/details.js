///<reference path="functions.js"/>

/**
 * Updates the indices of the details.
 */
function updateIndices() {
    $("#detailsTable input[name$='Sequence']").each(function (index, element) {
        $(element).val(index);
        $(this).closest("div.sortable").find(".sequence").html(index);

        var row = $(this).closest("tbody.sortable");

        $(row).find("[id*='DocumentDetails_']").each(function () {
            var id = $(this).attr("id");
            var split = id.split("_");
            $(this).attr("id", id.replace(split[1], index));
        });

        $(row).find("[name*='DocumentDetails[']").each(function () {
            var name = $(this).attr("name");
            var split = name.split("[")[1].split("]")[0];
            $(this).attr("name", name.replace(split, index));
        });

        $(row).find(".arrow").each(function () {
            var target = $(this).attr("data-target");
            var split = target.split("_");
            $(this).attr("data-target", target.replace(split[1], index));
        });

        $(row).find(".collapse").each(function () {
            var id = $(this).attr("id");
            var split = id.split("_");
            $(this).attr("id", id.replace(split[1], index));
        });
        
        $(row).data("position", index);
    });
}

/**
 * Toggle the search-type.
 * @param {jQueryEvent} e
 */
function toggleSearchType(e) {
    e.preventDefault();
    $("#searchTypeSelector button.active").removeClass("active");
    $(this).addClass("active");
}

/**
 * Deletes all the details after confirmation.
 * @param {jQueryEvent} e
 */
function deleteAllDetails(e) {
    e.preventDefault();
    if (hasOpenModal()) {
        return;
    }

    var details = $("#detailsTable .sortable");
    if (details.length !== 0) {
        var deleteAllDetailsEventsFunction = function(container, modal) {
            modal.on("hidden.bs.modal", function () {
                $("#defaultSearchField").focus();
            });
            
            modal.find("#confirmDeleteAll").on("click", function (innerEvent) {
                innerEvent.preventDefault();
                details.remove();
                closeCurrentModal();
                fillTotals();
            });
        };

        openModal("#modal-container", $("#confirmation-dialog-value").html(), false, deleteAllDetailsEventsFunction);
    }
}

/**
 * Collapses all details
 * @param {jQueryEvent} e
 */
function collapseAllDetails(e) {
    e.preventDefault();
    $("#detailsTable .collapse").collapse("hide");
}


/**
 * Opens the detail search-modal
 * @param {number} position
 * @param {string} searchValue
 * @param {boolean} isArticle
 */
localStorage.hasSearchClicked = false;
function openDetailSearchModal(position, searchValue, isArticle) {
    var hasClicked = localStorage.hasSearchClicked === "true";
    if (hasClicked) {
        return;
    }
    localStorage.hasSearchClicked = true;
    if (hasOpenModal()) {
        return;
    }

    becosoft.ajax(urls.detailSearchModal, { async: true }, function (result) {
        var detailSearchEventsFunction = function(container) {
            bindArticleEvents();
            bindMatrixEvents();
            $("#articlesFilterSubmit").on("click", function (e) {
                e.preventDefault();
                $("#Articles_Filter_PageInfo_Page").val(0);
                postArticleFilter(position);
                $(this).blur();
            });

            $("#matricesFilterSubmit").on("click", function (e) {
                e.preventDefault();
                $("#Matrices_Filter_PageInfo_Page").val(0);
                postMatrixFilter(position);
                $(this).blur();
            });

            $("#modal-close-button").on("click", function(e) {
                e.preventDefault();
                closeCurrentModal();
            });

            var keydownFunc = function (e) {
                modalTableKeyDown(e, "#filterArticleCollapse table tbody", container, "tr.article");
                modalTableKeyDown(e, "#filterMatrixCollapse table tbody", container, "tr.matrix");
            };

            $(document).on("keydown", keydownFunc);

            container.on("show.bs.collapse", "#filterMatrixCollapse", function () {
                var articleVal = $("#Articles_Filter_SearchQuery").val();
                $("#Matrices_Filter_SearchQuery").val(articleVal);
                $("#filterArticleCollapse").collapse("hide");
            });

            container.on("shown.bs.collapse", "#filterMatrixCollapse", function () {
                $("#Matrices_Filter_SearchQuery").focus();
            });

            container.on("show.bs.collapse", "#filterArticleCollapse", function () {
                var matrixVal = $("#Matrices_Filter_SearchQuery").val();
                $("#Articles_Filter_SearchQuery").val(matrixVal);
                $("#filterMatrixCollapse").collapse("hide");
            });

            container.on("shown.bs.collapse", "#filterArticleCollapse", function () {
                $("#Articles_Filter_SearchQuery").focus();
            });

            container.on("hidden.bs.modal", function () {
                $(document).off("keydown", keydownFunc);
            });

            if (isArticle) {
                $("#filterMatrixCollapse").collapse("hide");
                $("#filterArticleCollapse").collapse("show");
                container.on("shown.bs.modal", function () {
                    $("#Articles_Filter_SearchQuery").focus();
                });
                if (searchValue !== "" && searchValue !== "0") {
                    $("#Articles_Filter_SearchQuery").val(searchValue);
                    $("#articlesFilterSubmit").click();
                }
            } else {
                $("#filterMatrixCollapse").collapse("show");
                $("#filterArticleCollapse").collapse("hide");
                container.on("shown.bs.modal", function () {
                    $("#Matrices_Filter_SearchQuery").focus();
                });
                if (searchValue !== "" && searchValue !== "0") {
                    $("#Matrices_Filter_SearchQuery").val(searchValue);
                    $("#matricesFilterSubmit").click();
                }
            }

            container.data("position", position);
        };

        openModal("#modal-container", result, false, detailSearchEventsFunction);
        localStorage.hasSearchClicked = false;
    });
}

/**
 * Binds the matrix events in the detail search-modal.
 */
function bindMatrixEvents() {
    $("#matrixResult #paginationForm a[type=submit]").on("click", function (e) {
        e.preventDefault();
        $(this).attr("data-clicked", true);
        $("#matrixResult #paginationForm").submit();
    });

    $("#matrixResult #paginationForm").on("submit", function (e) {
        e.preventDefault();
        var btn = $(this).find("a[data-clicked='true']");
        var value = btn.attr("value");
        var selectedPage = parseInt(value);
        if (selectedPage < 0) {
            e.preventDefault();
            return false;
        }

        $("#Matrices_Filter_PageInfo_Page").val(selectedPage);
        postMatrixFilter();
        return false;
    });
}

/**
 * Binds the article events in the detail search-modal.
 */
function bindArticleEvents() {
    $("#articlesResult #paginationForm a[type=submit]").on("click", function (e) {
        e.preventDefault();
        $(this).attr("data-clicked", true);
        $("#articlesResult #paginationForm").submit();
    });

    $("#articlesResult #paginationForm").on("submit", function (e) {
        e.preventDefault();
        var btn = $(this).find("a[data-clicked='true']");
        var value = btn.attr("value");
        var selectedPage = parseInt(value);
        if (selectedPage < 0) {
            e.preventDefault();
            return false;
        }

        $("#Articles_Filter_PageInfo_Page").val(selectedPage);
        postArticleFilter();
        return false;
    });
}


/**
 * Post the article filter.
 * @param {number} [position=-1]
 */
function postArticleFilter(position = -1) {
    var form = $("#filterArticleForm");
    var formData = {
        SearchQuery: $("#Articles_Filter_SearchQuery").val(),
        RemoveMatrixArticles: $("#Articles_Filter_RemoveMatrixArticles").val(),
        PageInfo: {
            Page: $("#Articles_Filter_PageInfo_Page").val(),
            PageSize: $("#Articles_Filter_PageInfo_PageSize").val(),
        }
    };

    if (position === -1) {
        position = $("#modal-container").data("position");
    }

    $("#articlesResult").html("");
    $("#matrixResult").html("");
    $("#filterMatrixCollapse").collapse("hide");
    loadingIcon.callback("#articlesResult", function () {
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
            $("#articlesResult").html("");
            $("#articlesResult").html(result);
            $(".article").on("click", function () {
                pickArticle($(this).data("article"), position);
            });
            var totalCount = parseInt($("#PaginationBlock_TotalCount").val(), 10);
            if (totalCount === 1) {
                var results = $("#articlesResult .article");
                results.first().trigger("click");
                setTimeout(closeCurrentModal, 500);
            }
            bindArticleEvents();
        });
    });
}

/**
 * Post the matrix filter.
 * @param {number} [position=-1]
 */
function postMatrixFilter(position = -1) {
    var form = $("#filterMatrixForm");
    var formData = {
        SearchQuery: $("#Matrices_Filter_SearchQuery").val(),
        LanguageID: $("#Matrices_Filter_LanguageID").val(),
        PageInfo: {
            Page: $("#Matrices_Filter_PageInfo_Page").val(),
            PageSize: $("#Matrices_Filter_PageInfo_PageSize").val(),
        }
    };

    console.log(position);

    if (position === -1) {
        position = $("#modal-container").data("position");
    }

    $("#articlesResult").html("");
    $("#matrixResult").html("");
    $("#filterArticleCollapse").collapse("hide");
    loadingIcon.callback("#matrixResult", function () {
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
            $("#matrixResult").html("");
            $("#matrixResult").html(result);
            $(".matrix").on("click", function () {
                pickMatrix($(this).data("matrix"), $(this).data("color"), position);
            });
            var totalCount = parseInt($("#PaginationBlock_TotalCount").val(), 10);
            if (totalCount === 1) {
                var results = $("#matrixResult .matrix");
                results.first().trigger("click");
                setTimeout(closeCurrentModal, 500);
            }
            bindMatrixEvents();
        });
    });
}

/**
 * Pick an article from the result.
 * @param {number} id
 * @param {number} position
 */
function pickArticle(id, position) {
    updateArticleData(id, position);
    $("#defaultSearchField").val("");
    closeCurrentModal();
}

/**
 * Pick a matrix from the result.
 * @param {number} id
 * @param {number} colorId
 * @param {number} position
 */
function pickMatrix(id, colorId, position) {
    updateMatrixData(id, colorId, position);
    $("#defaultSearchField").val("");
    closeCurrentModal();
}

/**
 * Loads the detail with the article data from the picked article.
 * @param {number} id
 * @param {number} position
 */
function updateArticleData(id, position) {
    var url = urls.detailArticleLine;
    var vatCode = $("#VATCodeID").val();
    var docType = $("#DocumentTypeID").val();
    var contact = $("#ContactID").val();
    var currencyID = $("#CurrencyID").val() || 0;
    $("#defaultSearchField").attr("disabled", "disabled");
    $("#defaultSearch").attr("disabled", "disabled");
    becosoft.ajax(url + "?index=" + position + "&documentTypeID=" + docType + "&vatCodeID=" + vatCode + "&contactID=" + contact + "&articleID=" + id + "&currencyID=" + currencyID, {
        type: "GET",
        async: true
    }, function (result) {
        var table = $("#detailsTable");
        table.find('[data-toggle="tooltip"]').tooltip("dispose");
        if ($("#detailsTable .sortable").length === position) {
            $(result).insertBefore("#detailsTable tfoot");
        } else {
            var tbody = $("#detailsTable .sortable").eq(position);
            tbody.replaceWith(result);
        }

        var addedBody = $("#detailsTable .sortable").eq(position);

        table.find('[data-toggle="tooltip"]').tooltip({ html: true });
        fillTotals();
        $("#defaultSearchField").val("");
        $("#defaultSearchField").removeAttr("disabled");
        $("#defaultSearch").removeAttr("disabled");
        var strLength = addedBody.find("[name$='Amount']").last().val().length * 2;
        addedBody.find("[name$='Amount']").last().focus();
        addedBody.find("[name$='Amount']").last()[0].setSelectionRange(strLength, strLength);
        ReIndexDetailTabFlow();
    });
}

/**
 * Loads the detail with the matrix data from the picked matrix.
 * @param {number} id
 * @param {number} colorId
 * @param {number} position
 */
function updateMatrixData(id, colorId, position) {
    var url = urls.detailMatrixLine;
    var vatCode = $("#VATCodeID").val();
    var docType = $("#DocumentTypeID").val();
    var contact = $("#ContactID").val();
    var currencyID = $("#CurrencyID").val();
    becosoft.ajax(url + "?index=" + position + "&documentTypeID=" + docType + "&vatCodeID=" + vatCode + "&matrixID=" + id + "&colorID=" + colorId + "&contactID=" + contact + "&currencyID=" + currencyID, {
        type: "GET",
        async: true
    }, function (result) {
        var panel = $("#detailsTable .sortable").eq(position);
        if (panel === undefined || panel === null || panel.length === 0) {
            $(result).insertBefore("#detailsTable tfoot");
        } else {
            panel.replaceWith(result);
        }
        fillTotals();
        $("#defaultSearchField").val("");
        var addedBody = $("#detailsTable .sortable").eq(position);
        addedBody.find("[name$='.Amount']").first().focus();
        ReIndexDetailTabFlow();
    });
}

/**
 * Recalculate the subtotal of an article-row
 * @param {any} input
 * @param {any} idToFocus
 */
function calculateRow(row, currencySign) {
    var articleTable = row.closest("tbody");
    var defaultBaseprice = parseFloat(row.find("[name$='BasePrice']").val().replaceAll(numberGroupSeparator, "").replace(numberDecimalSeparator, '.'));
    var defaultDiscount1 = parseFloat(row.find("[name$='Discount1']").val().replaceAll(numberGroupSeparator, "").replace(numberDecimalSeparator, '.'));
    var price = defaultBaseprice * (100 - defaultDiscount1) / 100;

    if (currencySign === undefined || currencySign === "") {
        currencySign = $("#currencySign").text().trim();
    }
    if (currencySign === undefined || currencySign === "") {
        currencySign = $("#CurrencyID option:selected").text();
    }

    var subTotal = 0.0;

    row.find("input[data-type='amount']").each(function (i, e) {
        var input = $(e);
        var amount = parseFloat(input.val().replaceAll(numberGroupSeparator, "").replace(numberDecimalSeparator, '.'));
        var currentArticleID = input.data("articleid");
        var currentAmountAvailable = row.find(`input[data-type='amountAvailable'][data-articleid='${currentArticleID}']`);
        currentAmountAvailable.val(amount);
        if (amount === 0) { return; }
        var baseprice = articleTable.find(`input[data-property='baseprice'][data-articleid='${currentArticleID}']`).val();
        if ((baseprice || "") === "") {
            baseprice = defaultBaseprice;
        } else {
            baseprice = parseFloat(baseprice.replaceAll(numberGroupSeparator, "").replace(numberDecimalSeparator, '.'));
        }
        var discount1 = articleTable.find(`input[data-property='discount1'][data-articleid='${currentArticleID}']`).val();
        if ((discount1 || "") === "") {
            discount1 = defaultDiscount1;
        } else {
            discount1 = parseFloat(discount1.replaceAll(numberGroupSeparator, "").replace(numberDecimalSeparator, '.'));
        }
        subTotal += amount * baseprice * (100 - discount1) / 100;
    });
    
    row.find("[name$='Price']").html(`${currencySign} ${price.formatDecimal(2)}`);
    row.find("[name$='SubTotal']").html(`${currencySign} ${subTotal.formatDecimal(2)}`);
    row.find("[name$='SubTotal']").attr("data-value", `${subTotal.formatDecimal(2)}`);
    fillTotals();
}

/**
 * Recalculate an article-row
 * @param {any} input
 * @param {any} idToFocus
 */
function calculatePrice(input) {
    var detail = input.closest("tbody.sortable");
    var url = urls.recalculateDetail;
    detail.find('[data-toggle="tooltip"]').tooltip("dispose");

    var data = convertDetailToRecalculateRequest(input);

    becosoft.ajax(url, {
        type: "POST",
        data: {
            viewModel: data
        }
    }, function (result) {
        if (!result) { return; }
        var currentRow = input.closest("tr[data-type='articledetail']");
        var basepriceField = currentRow.find("td[data-type='baseprice']");

        if (basepriceField.data("timestamp") == undefined || basepriceField.data("timestamp") < data["Milliseconds"]) {
            var newValue = result["BasePrice"];
            var parsedNewNumber = new Number(newValue).formatDecimal(2);
            basepriceField.data("timestamp", result["Milliseconds"]);
            basepriceField.find("input").val(parsedNewNumber).trigger("change");
        }
        fillTotals();
    });
}

/**
 * Converts a detail-line to a JSON-object.
 * @param {jQueryObject} input
 * @returns {{}} The JSON-object.
 */
function convertDetailToRecalculateRequest(input) {
    /** public int ArticleID { get; set; }
        public int VatGroupID { get; set; }
        public int DocumentTypeID { get; set; }
        public decimal Discount1 { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }
        */
    var currentRow = input.closest("tr[data-type='articledetail']");
    var articleId = parseInt(input.data('articleid'));
    var vatGroupID = parseInt(currentRow.find('input[data-type="vatgroup"]').val());
    var documentTypeID = parseInt($("#DocumentTypeID").val());
    var discount1 = parseFloat($(`input[data-type=discount1][data-articleid="${articleId}"]`).val().replace(',', '.'));
    var amount = parseFloat(currentRow.find(`input[data-type=amount][data-articleid="${articleId}"]`).val().replace(',', '.'));
    var milliSeconds = Date.now();

    var object = {
        ArticleID: articleId,
        VatGroupID: vatGroupID,
        DocumentTypeID: documentTypeID,
        Discount: discount1,
        Amount: amount,
        Milliseconds: milliSeconds,
    };

    return object;
}

/**
 * Converts a detail-line to a JSON-object.
 * @param {jQueryObject} detail
 * @param {boolean} [withPrefix=false]
 * @returns {{}} The JSON-object.
 */
function convertDetailToJSON(detail) {
    var data = [];
    if (detail.length === 0) {
        return data;
    }

    var position = detail.data("position");

    data = detail.find("input, select").serializeArray();
    data.push({ name: "OldVatGroupID", value: detail.find("[name$='VATGroupID']").data("old") });
    data.push({ name: "SubTotal", value: detail.find("[id$='SubTotal']").attr("data-value") });
    
    var prefix = "DocumentDetails\\[" + position + "\\].";
    var replacer = new RegExp(prefix, "g");

    $(data).each(function() {
        var kvp = $(this)[0];
        kvp.name = kvp.name.replace(replacer, "");
    });

    var object = {};

    for (var key in data) {
        if (data.hasOwnProperty(key)) {
            var pair = data[key];

            if (pair.value !== undefined) {
                let replacedVal = pair.value.replace(",", ".");
                if (isNumeric(replacedVal)) {
                    let numericVal = new Number(replacedVal);
                    if (numericVal % 1 === 0) {
                        let parsedVal = numericVal.formatDecimal(0, numberDecimalSeparator, "").replace(",", ".");
                        object[pair.name] = parsedVal;
                    } else {
                        let parsedVal = numericVal.formatDecimal(2).replace(",", ".");
                        object[pair.name] = parsedVal;
                    }
                } else {
                    object[pair.name] = pair.value;
                }
            } else {
                object[pair.name] = pair.value;
            }
        }
    }
    return object;
}

/**
 * Converts a detail-line to a JSON-object.
 * @param {jQueryObject} detail
 * @param {boolean} [withPrefix=false]
 * @returns {{}} The JSON-object.
 */
function convertDetailToJSON2(detail, sequence) {
    var data = [];
    if (detail.length === 0) {
        return data;
    }

    data = detail.find("input, select").serializeArray();
    data.push({ name: "OldVatGroupID", value: detail.find("[name$='VATGroupID']").data("old") });
    data.push({ name: "SubTotal", value: detail.find("[id$='SubTotal']").attr("data-value") });


    var matrixSizeViewModels = [];
    if (detail.data("ismatrix")) {
        detail.find("tr[data-type='articledetail']").each(function (i, eRow) {
            var row = $(eRow);
            var articleTable = row.closest("tbody");
            var colorID = parseInt(row.data('colorid'));
            var sizes = [];
            row.find("input[data-type='amount']").each(function (j, e) {
                var input = $(e);
                var sizeBarDetailID = parseInt(input.data("sizebardetailid"));
                var articleID = input.data("articleid");
                var amount = parseFloat(input.val().replaceAll(numberGroupSeparator, "").replace(numberDecimalSeparator, '.'));
                var amountAvailable = row.find(`input[data-type='amountAvailable'][data-articleid='${articleID}']`).val();

                var articleData = { ArticleID: articleID, Amount: amount, AvailableAmount: amountAvailable, ColorID: colorID, SizeBarDetailID: sizeBarDetailID };

                if (amount === 0) { return; }
                var baseprice = articleTable.find(`input[data-property='baseprice'][data-articleid='${articleID}']`).val();
                if ((baseprice || "") !== "") {
                    let replacedVal = baseprice.replace(numberGroupSeparator, "").replace(numberDecimalSeparator, ".");
                    if (isNumeric(replacedVal)) {
                        let numericVal = new Number(replacedVal);
                        if (numericVal % 1 === 0) {
                            articleData.BasePrice = numericVal.formatDecimal(0).replaceAll(numberGroupSeparator, "");//.replace(numberDecimalSeparator, '.'));
                        } else {
                            articleData.BasePrice = numericVal.formatDecimal(2).replaceAll(numberGroupSeparator, "");//.replace(numberDecimalSeparator, '.'));
                        }
                    }
                }
                var discount1 = articleTable.find(`input[data-property='discount1'][data-articleid='${articleID}']`).val();
                if ((discount1 || "") !== "") {
                    let replacedVal = discount1.replace(numberGroupSeparator, "").replace(numberDecimalSeparator, ".");
                    if (isNumeric(replacedVal)) {
                        let numericVal = new Number(replacedVal);
                        if (numericVal % 1 === 0) {
                            articleData.Discount1 = numericVal.formatDecimal(0).replaceAll(numberGroupSeparator, "");//.replace(numberDecimalSeparator, '.'));
                        } else {
                            articleData.Discount1 = numericVal.formatDecimal(2).replaceAll(numberGroupSeparator, "");//.replace(numberDecimalSeparator, '.'));
                        }
                    }
                }
                matrixSizeViewModels.push(articleData);
            });
        });
    }
    


    var object = {};

    for (var key in data) {
        if (data.hasOwnProperty(key)) {
            var pair = data[key];

            if (pair.value !== undefined) {
                let replacedVal = pair.value.toString().replace(numberGroupSeparator, "").replace(numberDecimalSeparator, ".");
                if (isNumeric(replacedVal)) {
                    let numericVal = new Number(replacedVal);
                    if (numericVal % 1 === 0) {
                        //let parsedVal = parseInt(numericVal);
                        let parsedVal = numericVal.formatDecimal(0).replace(numberGroupSeparator, "");
                        object[pair.name] = parsedVal;
                    } else {
                        //let parsedVal = parseFloat(numericVal);
                        let parsedVal = numericVal.formatDecimal(2).replace(numberGroupSeparator, "");//.replace(numberDecimalSeparator, ".");
                        object[pair.name] = parsedVal;
                    }
                } else {
                    object[pair.name] = pair.value;
                }
            } else {
                object[pair.name] = pair.value;
            }
        }
    }

    object.MatrixSizeViewModels = matrixSizeViewModels;

    return object;
}

/**
 * Updates all the details
 */
function updateDetails() {
    var details = $("#detailsTable .sortable");
    if (details.length === 0) {
        return;
    }
    var url = urls.recalculateDetails;
    var vatCode = $("#VATCodeID").val();
    var docType = $("#DocumentTypeID").val();
    var contactId = $("#ContactID").val();
    var currencyID = $("#CurrencyID").val() || 0;
    details.find('[data-toggle="tooltip"]').tooltip("dispose");

    var data = [];

    details.each(function() {
        var detail = $(this);
        var json = convertDetailToJSON2(detail);
        data.push(json);
    });

    details.find("input, select").attr("disabled", "disabled");
 
    becosoft.ajax(url, {
        type: "POST",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({
            details: data,
            documentTypeID: docType,
            vatCodeID: vatCode,
            contactID: contactId,
            currencyID: currencyID
        }),
        async: true
    }, function (result) {
        if (!result) {
            return;
        }

        details.remove();

        $.each(result, function(key, value) {
            console.log(value);
            $(value).insertBefore("#detailsTable tfoot");
        });

        fillTotals();
    });
}

/**
 * Fills the total-table.
 */
function fillTotals() {
    //Create the totals-table
    var details = $("#detailsTable tbody.sortable");
    $(".totalDetailCount").text(details.length);

    var url = urls.totalsTable;
    var currencyId = $("#CurrencyID").val();
    var documentTypeID = $("#DocumentTypeID").val();

    var data = [];

    details.each(function() {
        var detail = $(this);
        var json = convertDetailToJSON2(detail);
        data.push(json);
    });

    becosoft.ajax(url, {
        type: "POST",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({
            details: data,
            currencySignID: currencyId,
            documentTypeID: documentTypeID
        }),
        async: true
    }, function (result) {
        $("#totals-container").html(result);
    });

    var totalAmount = 0;
    $(".detail-table").find("[data-type='amount']").each(function (i, t) {
        totalAmount += parseInt($(t).val());
    });
    $("#TotalAmount").html(parseFloat(totalAmount));

}

function ReIndexDetailTabFlow() {
    $(".detail-table").find("[data-type='articledetail']").each(function (i, t) {
        $(t).find('.tab-detail-input').each(function (k, s) {
            $(s).attr('tabindex', i + 1);
        });
    });
    var lines = $(".detail-table").find("[data-type='articledetail']").length;
    $("#defaultSearchField").attr('tabindex', lines + 1);
}
