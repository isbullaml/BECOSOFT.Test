/// <reference path="bootstrap.js"/>
/// <reference path="~/Scripts/becosoft.js" />
/// <reference path="~/Scripts/loadingIcon.js" />

var numberDecimalSeparator;
var numberGroupSeparator;
var translations = {};
Number.prototype.formatDecimal = function (c, d, t) {
    var n = this,
        c = isNaN(c = Math.abs(c)) ? 2 : c,
        d = d == undefined ? numberDecimalSeparator : d,
        t = t == undefined ? numberGroupSeparator : t,
        s = n < 0 ? "-" : "",
        i = String(parseInt(n = Math.abs(Number(n) || 0).toFixed(c))),
        j = (j = i.length) > 3 ? j % 3 : 0;
    return s +
        (j ? i.substr(0, j) + t : "") +
        i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) +
        (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
};

String.prototype.formatWith = function () {
    var args = arguments;
    return this.replace(/\{\{|\}\}|\{(\d+)\}/g, function (m, n) {
        if (m == "{{") { return "{"; }
        if (m == "}}") { return "}"; }
        var result = args[n];
        if (result === undefined || result === null) { result = ""; }
        return result;
    });
};

//Automatically format all inputs marked as a decimal
$(document).on("change", "input[data-format-decimal]", function () {
    var value = $(this).val();
    var possibleDecimalSeparator = "";
    if (numberDecimalSeparator === ",") {
        possibleDecimalSeparator = "[\\.]+";
    } else {
        possibleDecimalSeparator = "[\\,]+";
    }
    var regex = new RegExp(possibleDecimalSeparator, 'g');
    var formattedValue = value.replace(regex, numberDecimalSeparator);
    $(this).val(formattedValue);
});

$(function () {

    // make room for eventual fixed tabs bar
    if ($(".bcs-tabsbar-fixed").length) {
        $(".bcs-content").css("top", "5rem");
    }

    // culture info
    const cultureInfo = $("#culture-info");
    numberDecimalSeparator = cultureInfo.data("numberdecimalseparator") == null ? "," : cultureInfo.data("numberdecimalseparator");
    numberGroupSeparator = cultureInfo.data("numbergroupseparator") == null ? "." : cultureInfo.data("numbergroupseparator");

    //const menuCookie = getCookie("menu-visible");
    //if ($(window).width() < 1400) {
    //    $("#accordion svg").addClass("d-none");
    //}

    //if (menuCookie && !$("#mobile-menu").is(":visible")) {
    //    if (menuCookie === "1") {
    //        showMenu();
    //    } else {
    //        hideMenu();
    //    }
    //} else {
    //    if ($(window).width() > 1400) {
    //        showMenu();
    //    } else {
    //        hideMenu();
    //    }
    //}

    var websiteAndWarehouseLoader = function () {
        //Dashboards-menu load
        var websiteContainer = $("#websiteContainer");
        if (websiteContainer.length || websiteContainer.length) {
            var websiteUrl = websiteContainer.data("url");
            becosoft.ajax(websiteUrl,
                { type: "GET" },
                function (result) {
                    websiteContainer.html(result);
                },
                function () { },
                loadActiveItem);
        }
        var warehouseContainer = $("#warehouseMenuContainer");
        if (warehouseContainer.length || warehouseContainer.length) {
            var warehouseUrl = warehouseContainer.data("url");
            becosoft.ajax(warehouseUrl,
                { type: "GET" },
                function (result) {
                    warehouseContainer.html(result);
                    if (warehouseContainer.children('div').length <= 1) {
                        warehouseContainer.addClass("d-none");
                    } else {
                        warehouseContainer.removeClass("d-none");
                    }
                },
                function () { },
                loadActiveItem);
        }
    };


    //Dashboards-menu load
    var dashboardMenu = $("#collapseDashboard");
    var dashboardMenuMobile = $("#collapseDashboardMobile");
    if (dashboardMenu.length || dashboardMenuMobile.length) {

        var dashboardUrl = dashboardMenu.data("url");
        becosoft.ajax(dashboardUrl,
            { type: "GET" },
            function (result) {
                dashboardMenu.html(result);
                dashboardMenuMobile.html(result);
            },
            function () {

            }, websiteAndWarehouseLoader);
    } else {
        websiteAndWarehouseLoader();
    }

    //Focus correct input
    $(".focus :input").focus();

    // User picture
    // TODO: Fix
    //var picture = $("#header-userpicture");
    //var userPictureUrl = picture.data("url");
    //var userid = picture.data("userid");
    //becosoft.ajax(userPictureUrl + "/" + userid, { type: "GET" }, function (result) {
    //    if (result.length > 0) {
    //        picture.attr("src", result);
    //        $("#header-userpicture-placeholder").addClass("disabled");
    //    }
    //});

    // Make table links clickable and go to the details page
    $("#main-content").on("click", "table tr.index-row[data-href] td:not(.fixed-column-after):not(.fixed-column-before)", function (e) {
        e.preventDefault();
        var indexRow = $(this).closest("tr.index-row[data-href]");
        var url = indexRow.data("href");
        var newTab = indexRow.data("new-tab");
        //console.log(url);
        var openInNewTab = (parseInt(newTab, 10) || 0) === 1;
        if (url) {
            if (openInNewTab) {
                window.open(url, "_blank");
            } else {
                window.location = url;
            }
        }
        return false;
    });
});

// Reset settings

$("#resetSettingsButton").on("click", function (e) {
    e.preventDefault();
    var url = $(this).attr("href");
    becosoft.ajax(url, { type: "POST" });
});


// Expire caches
$("#expireCachesButton").on("click", function (e) {
    e.preventDefault();
    var url = $(this).attr("href");
    becosoft.ajax(url, { type: "POST" });
});

// Update reports
$("#updateReportsButton").on("click", function (e) {
    e.preventDefault();
    var url = $(this).attr("href");
    becosoft.ajax(url, { type: "POST" });
});

// Added for menu collapse

//function hideMenu() {
//    if (window.fromCrm === undefined || window.fromCrm === 'True') { return; }
//    $(".sidebar").addClass("sidebar-hide");
//    $("#accordion").addClass("d-none");
//    $(".nav-item-brand .hvr-icon-up").addClass("hvr-icon-down");
//    $(".nav-item-brand .hvr-icon-up").removeClass("hvr-icon-up");
//}

//function showMenu() {
//    if (window.fromCrm === undefined || window.fromCrm === 'True') { return; }
//    $(".sidebar").removeClass("sidebar-hide");
//    $("#accordion").removeClass("d-none");
//    $(".nav-item-brand .hvr-icon-down").addClass("hvr-icon-up");
//    $(".nav-item-brand .hvr-icon-down").removeClass("hvr-icon-down");
//}

function loadActiveItem() {

    var navMatches = [];
    var domainlessWindowLocation = window.location.pathname;
    $(".bcs-sidebar-list .bcs-sidebar-item").each(function () {

        // if this element's href attribute starts with "/", skip it
        if ($(this).is('.accordion-toggle:not([href^="/"])')) {
            return true;
        }

        const href = $(this).attr("href");
        if (href === "/") {
            if (domainlessWindowLocation !== "/") {
                return;
            }
        } else {
            if (!domainlessWindowLocation.includes(href)) {
                return;
            }
        }
        navMatches.push($(this));
    });

    const matchElement = $(navMatches.sort(function (a, b) {
        const indexer = function (href) {
            const minLength = Math.min(domainlessWindowLocation.length, href.length);
            var maxIndexMatch = 0;
            for (var i = 0; i < minLength; i++) {
                if (window.location.pathname[i] === href[i]) {
                    maxIndexMatch = i;
                } else { break; }
            }

            return maxIndexMatch;
        };

        return indexer(b.attr("href")) - indexer(a.attr("href"));
    })).first();

    if (matchElement.length !== 0) {
        const link = matchElement[0];
        link.addClass("bcs-sidebar-item-active");
        const collapsePanel = link.closest(".panel-collapse");
        if (collapsePanel.length > 0) {
            if (!collapsePanel.hasClass("show")) {
                const collapseLink = collapsePanel.siblings(".accordion-toggle");
                collapseLink.click();
            }
        } else {
            $(link).parent().addClass("bcs-sidebar-item-active");
        }
    }
}

$(document).on('click',
    'label[data-selectAllCheckboxes="table"]',
    function () {
        const el = $(this);
        const table = el.closest("table");
        const inputs = table.find('input:checkbox[id^="table-checkbox-"]');
        inputs.trigger('click');
        table.find('input[data-selectAllCheckboxes="table"]').trigger('click');
    });

// Utilities

/**
 * Checks if a value is numeric.
 * @param {number} n the number
 * @returns {boolean} whether the number is numeric
 */
function isNumeric(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

$("#showHelp").on("click", showHelp);
$(document).keydown(handleGlobalKeyDown);

/**
 * Handles a key down event
 * @param {jQueryEvent} e the event
 */
function handleGlobalKeyDown(e) {
    if (e.shiftKey) { //SHIFT
        if (e.keyCode === 9) { // + TAB
            if (hasOpenModal()) {
                handleShiftTabInModal(e);
                return;
            }
        }
    }

    if (e.ctrlKey) { //CTRL
        switch (e.keyCode) {
            case 70: // + F
                handleGeneralCtrlF(e);
                break;
            case 72: // + H
                handleCtrlH(e);
                break;
            case 78: // + N
                handleCtrlN(e); // TODO: This will not work, we can't capture browser shortcuts. How do we fix this? (ALT for all shortcuts / key only 'N' when not in input field / ...)
                break;
            case 83: // + S
                handleCtrlS(e);
                break;
        }
    }

    if (e.keyCode === 9) { //TAB
        if (hasOpenModal()) {
            handleTabInModal(e);
            return;
        }
    }


    if (e.keyCode === 27) { //ESC
        let currentModal = getCurrentModal();

        //Don't allowed closing on ESC when keyboard is disabled on modal
        if (currentModal.data("bs.modal")._config.keyboard) {
            closeCurrentModal();
        }
        return;
    }
}
/**
 * Shows the help modal.
 */
function showHelp() {
    if ($("#helpModal").hasClass("show")) {
        closeCurrentModal();
    } else {
        openModal("#help-modal-container");
    }
}

/**
 * Handles a CTRL + F event.
 * This goes to the search field.
 * @param {jQueryEvent} e the event
 */
function handleGeneralCtrlF(e) {
    var searchField = $("#searchField");
    if (searchField.length > 0) {
        e.preventDefault();
        searchField.focus();
    }
}


/**
 * Handles a CTRL + H event.
 * This show the help-page.
 * @param {jQueryEvent} e the event
 */
function handleCtrlH(e) {
    e.preventDefault();
    showHelp();
}

/**
 * Handles a CTRL + S event.
 * This triggers a click on the save-button if available.
 * @param {jQueryEvent} e the event
 */
function handleCtrlS(e) {
    var saveButton = $("#saveButton");
    if (saveButton.length > 0) {
        e.preventDefault();
        saveButton.trigger("click");
    }
}

/**
 * Handles a CTRL + N event.
 * This triggers a click on the new-button if available.
 * @param {jQueryEvent} e the event
 */
function handleCtrlN(e) {
    var createButton = $("#createButton");
    if (createButton.length > 0) {
        e.preventDefault();
        createButton[0].click();
    }
}

/**
 * Handles a SHIFT + TAB-keystroke while a modal is open, to keep the focus in the modal.
 * @param {jQueryEvent} e the event
 */
function handleShiftTabInModal(e) {
    e.preventDefault();
    var currentElement = $(".modal.show :focus");
    var tabbableInputs = $(".modal.show *:tabbable:visible");
    var index = tabbableInputs.index(currentElement);
    var previousIndex = index - 1;

    if (previousIndex < 0) {
        previousIndex = tabbableInputs.length - 1;
    }

    tabbableInputs.eq(previousIndex).focus();
}

/**
 * Handles a TAB-keystroke while a modal is open, to keep the focus in the modal.
 * @param {jQueryEvent} e the event
 */
function handleTabInModal(e) {
    e.preventDefault();
    var currentElement = $(".modal.show :focus");
    var tabbableInputs = $(".modal.show *:tabbable:visible");
    var index = tabbableInputs.index(currentElement);
    var nextIndex = index + 1;

    if (nextIndex >= tabbableInputs.length) {
        nextIndex = 0;
    }

    tabbableInputs.eq(nextIndex).focus();
}

/**
 * Checks if a current modal is open.
 * @returns {boolean} Value indicating whether a modal is open.
 */
function hasOpenModal() {
    return $(".modal.show").length > 0;
}

/**
 * Opens a modal.
 * If the modalContainerSelector is not specified, it opens the default modal in #modal-container.
 * If forceClose is set to true, it closes the previous modal before opening the new one.
 * @param {jQuerySelector} modalContainerSelector the selector for the modal container
 * @param {string} modalHtml the html for the modal
 * @param {boolean} forceClose force close other modals
 * @param {function()} eventListenersFunction an event listener function
 * @param {string} focusFieldSelector selector for the field to focus
 */
function openModal(modalContainerSelector = "", modalHtml = "", forceClose = false, eventListenersFunction = () => { }, focusFieldSelector = "input:first") {
    var currentModal = $(".modal.show");
    if (currentModal.length > 0) {
        if (!forceClose) {
            return;
        }

        currentModal.modal("hide");
    }

    if (modalContainerSelector.length === 0) {
        modalContainerSelector = "#modal-container";
    }

    var modalContainer = $(modalContainerSelector);

    if (modalHtml.length > 0) {
        modalContainer.html(modalHtml);
    }

    var modalDialog = modalContainer.find(".modal").first();

    modalDialog.on("shown.bs.modal", function () {
        eventListenersFunction(modalContainer, modalDialog);
        modalDialog.find(focusFieldSelector).focus();
    });

    modalDialog.modal("show");
}

//Export buttons
$(function () {
    $("#index-export-btn").on("click", function (e) {
        e.preventDefault();
        var url = $(this).attr("href");

        becosoft.ajax(url, {
            type: "POST",
            async: true,
            success: function (result) {
                window.open(result.DownloadUri, "_blank");
            }
        });
    });
});

// Utilities


/**
 * Closes the current open modal.
 */
function getCurrentModal() {
    return $(".modal.show").first();
}


/**
 * Closes the current open modal.
 */
function closeCurrentModal() {
    var modalDialog = getCurrentModal();
    modalDialog.modal("hide");
}

/**
 * Sets the current value as data-previous-value on the current input.
 */
function onNumericFocus(currentInput = null) {
    var inputField = $(this);
    if (currentInput !== null) {
        inputField = $(currentInput);
    }
    var value = inputField.val();
    inputField.data("previous-value", value);
}

/**
 * Checks if the current numeric input value is valid, otherwise use the data-previous-value.
 * This event has to be combined with the onNumericFocus-function.
 */
function onNumericChange() {
    var inputField = $(this);
    var previousValue = inputField.data("previous-value");
    var newValue = inputField.val();
    var cultureDecimalSeparator = $("#culture-info").data("numberdecimalseparator");
    newValue = newValue.replace(cultureDecimalSeparator, ".");
    if (!isNumeric(newValue)) {
        inputField.val(previousValue);
    }
}

/**
 * Sets a cookie in the browser.
 * @param {string} name  the name of the cookie
 * @param {{}} value the value of the cookie
 * @param {number} days the amount of days the cookie should be stored
 */
function setCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }

    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

/**
 * Retrieves a cookie from the browser.
 * @param {string} name the name of the cookie
 * @returns {any} The value of the cookie
 */
function getCookie(name) {
    var nameEqualizer = name + "=";
    var cookieList = document.cookie.split(";");
    for (var i = 0; i < cookieList.length; i++) {
        var c = cookieList[i];
        while (c.charAt(0) === " ") c = c.substring(1, c.length);
        if (c.indexOf(nameEqualizer) === 0) return c.substring(nameEqualizer.length, c.length);
    }
    return null;
}

/**
 * Deletes a cookie from the browser.
 * @param {string} name the name of the cookie
 */
function eraseCookie(name) {
    document.cookie = name + "=; Max-Age=-99999999;";
}

/**
 * Downloads a file with a filter.
 * @param {string} url the url to download from
 * @param {{}} formData the data to post
 * @param {function()} progress the progress-handler
 * @param {function()} callback the callback
 * @param {boolean} withOverlay use a download-overlay
 */
function downloadFile(url, formData, progress, callback, withOverlay) {
    var req = new XMLHttpRequest();

    req.open("POST", url, true);
    req.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    req.responseType = "blob";
    req.onload = function () {
        var fileName = req.getResponseHeader("fileName");
        var link = document.createElement("a");
        link.href = window.URL.createObjectURL(req.response);
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        link.remove();
    };
    if (progress !== undefined) { req.addEventListener("progress", progress, false); }
    if (callback !== undefined) { req.addEventListener("load", callback, false); }
    if (withOverlay) {
        req.addEventListener("load", closeCurrentModal, false);
        openModal("#modal-container", "<div class='modal fade'><div class='modal-dialog'><div class='modal-content' id='temp-loader'></div></div></div>", true, function () {
            loadingIcon.callback("#temp-loader", function () {
                req.send(formData);
            });
        });
    } else {
        req.send(formData);
    }

}

/**
 * Downloads a file with a filter with an API-key.
 * @param {string} url
 * @param {string} apiKey
 * @param {function()} progress
 * @param {function()} callback
 * @param {boolean} withOverlay
 */
function downloadApiFile(url, apiKey, progress, callback, withOverlay) {
    var req = new XMLHttpRequest();

    req.open("GET", url, true);
    req.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    req.setRequestHeader("ApiKey", apiKey);
    req.responseType = "blob";
    req.onload = function () {
        var fileName = req.getResponseHeader("fileName");
        var link = document.createElement("a");
        link.href = window.URL.createObjectURL(req.response);
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        link.remove();
    };
    req.addEventListener("progress", progress, false);
    req.addEventListener("load", callback, false);
    if (withOverlay) {
        req.addEventListener("load", closeCurrentModal, false);
        openModal("#modal-container", "<div class='modal fade'><div class='modal-dialog'><div class='modal-content' id='temp-loader'></div></div></div>", true, function () {
            loadingIcon.callback("#temp-loader", function () {
                req.send();
            });
        });
    } else {
        req.send();
    }

}


/**
 * Checks if an element is in the current viewport.
 * @param {string} selector the selector for the item to check
 * @returns {boolean} whether the item in the selector is in the viewport
 */
function isInViewport(selector) {
    var docViewTop = $(window).scrollTop();
    var docViewBottom = docViewTop + $(window).height();

    var elemTop = $(selector).offset().top;
    var elemBottom = elemTop + $(selector).height();

    return (elemBottom <= docViewBottom) && (elemTop >= docViewTop);
}

/**
 * Formats a date string to a string (yyyy-MM-dd) that can be used in the value of <input type="date" />.
 * @param {string} date the date to format
 * @returns {string} the string formatted for a date input
 */
function formatDateStringAsInputString(date) {
    var d = new Date(date);
    var month = "" + (d.getMonth() + 1);
    var day = "" + d.getDate();
    var year = d.getFullYear();

    if (month.length < 2) {
        month = "0" + month;
    }

    if (day.length < 2) {
        day = "0" + day;
    }

    return [year, month, day].join("-");
}

(function ($) {
    $.fn.closest_descendent = function (filter) {
        var $found = $(),
            $currentSet = this; // Current place
        while ($currentSet.length) {
            $found = $currentSet.filter(filter);
            if ($found.length) break;  // At least one match: break loop
            // Get all children of the current set
            $currentSet = $currentSet.children();
        }
        return $found.first(); // Return first match of the collection
    }
})(jQuery);

/**
 * Test if color is equal to the test color.
 * https://stackoverflow.com/a/60689673/383904
 * 
 * @param {String} color A valid CSS color value
 * @param {String} testColor A valid CSS color value
 * @return {Boolean} True if element color matches
 */
function isElPropColor(color, testColor) {
    const ctx = document.createElement('canvas').getContext('2d');
    ctx.fillStyle = testColor;
    ctx.fillRect(0, 0, 1, 1);
    ctx.fillStyle = color;
    ctx.fillRect(1, 0, 1, 1);
    const a = JSON.stringify(Array.from(ctx.getImageData(0, 0, 1, 1).data));
    const b = JSON.stringify(Array.from(ctx.getImageData(1, 0, 1, 1).data));
    ctx.canvas = null;
    return a === b;
}

$("a[change-userprofile-button]").on("click",
    function (e) {
        e.preventDefault();
        var element = $(this);
        if (element.data("executing")) {
            return;
        }
        element.data("executing", true);
        element.addClass("disabled");
        var url = element.attr("href");
        becosoft.ajax(url,
            {
                type: "POST",
                dataType: "json",
                async: true,
                success: function (result) {
                    element.removeData("executing");
                    element.removeClass("disabled");
                    if (result.Success === false) {
                        if (result.NoProfiles === true) {
                            $("#userProfileNoResultModal").modal({ backdrop: true, show: true });
                        }
                        return;
                    }
                    var chooseModal = $("#changeUserProfileModal");
                    var profileSelect = chooseModal.find("select[data-userprofile-select]:first");
                    profileSelect.empty();
                    console.log(profileSelect);
                    if (profileSelect[0].sumo !== undefined) {
                        profileSelect[0].sumo.unload();
                    }
                    console.log(result);
                    let mapped = $.map(result.Profiles, function (user) { return { Key: user.UserID, Value: user.Name } });
                    mapped = mapped.sort((first, second) => {
                        return first.Value > second.Value ? 1 : -1;
                    });

                    $.each(mapped,
                        function (i, t) {
                            profileSelect.append(`<option value=${t.Key}>${t.Value}</option>`);
                        });
                    chooseModal.modal({ backdrop: true, show: true });
                }
            });
    });

$("button[data-modal-changeuserprofile-button]").on("click",
    function (e) {
        e.preventDefault();
        var element = $(this);
        if (element.data("executing")) {
            return;
        }
        element.data("executing", true);
        element.addClass("disabled");
        var url = element.attr("data-userprofile-url");
        becosoft.ajax(url,
            {
                type: "POST",
                data: { userTemplate: $("#changeUserProfileModal").find("select[data-userprofile-select]:first").val() },
                dataType: "json",
                async: true,
                success: function (result) {
                    element.removeData("executing");
                    element.removeClass("disabled");
                    if (result.Success === true) {
                        window.location = result.Url;
                    }
                }
            });
    });


function initMultiSelect(obj, options = {}, placeholder) {
    const sourceElem = obj;
    const name = (sourceElem.attr("name") || sourceElem.attr("id")) + "__multiselect";
    if (options === undefined || options === null) {
        options = {};
    }
    let height = 200;
    if (options["height"] !== undefined) {
        const parsedHeight = parseInt(options["height"], 10) || height;
        height = parsedHeight;
    }
    let charLength = 200;
    if (options["charLength"] !== undefined) {
        const parsedCharLength = parseInt(options["charLength"], 10) || height;
        charLength = parsedCharLength;
    }
    if (options["refresh"] !== undefined && options.refresh) {
        sourceElem.siblings(`div[name='${name}']`).remove();
        sourceElem.removeAttr('data-linked-to');
    }
    var disabled = options["disabled"] !== undefined && options.disabled;
    var collapsible = options["collapsible"] !== undefined && options.collapsible;
    if (sourceElem.siblings(`div[name='${name}']`).length > 0) {
        if (options["deselect"] !== undefined && options.deselect) {
            $(`div[name='${name}']`).find('input[type="checkbox"]:checked').prop('checked', false).trigger('change');
        }
        if (options["select"] !== undefined && options.select) {
            $(`div[name='${name}']`).find('input[type="checkbox"]:not(:checked)').prop('checked', true).trigger('change');
        }
        return;
    }
    const containerUuid = uuidv4();
    const itemContainerName = (name + "_container_" + containerUuid).replace('.', '_').replace('[', '_').replace(']', '_').replace('.', '_').replace('_', '-');
    sourceElem.data('linked-to', name);
    sourceElem.addClass("d-none");
    const isMultiSelect = sourceElem.prop("multiple") !== undefined && sourceElem.prop("multiple");
    const newElem = $(`<div class='list-group p-0 m-0 rounded' ${disabled ? 'disabled' : ''} data-is-multiselect='${isMultiSelect}' name='${name}'></div>`);
    const subContainer = $(`<div class="switching-collapse-${itemContainerName} collapse"></div>`);
    const utils = $('<div class="bcs-list-group-upper row bg-light btn-toolbar p-0 m-0" data-utilities></div>');
    let extraWidth = (collapsible ? 32 : 0);
    if (disabled) {
        extraWidth += 32;
    } else if (isMultiSelect) {
        extraWidth += 96;
    }
    if (collapsible) {
        utils.append(`<button class="btn btn-sm float-left btn-no-outline bcs-list-group-button-collapse" data-toggle="collapse" data-target=".switching-collapse-${itemContainerName}" style="width:32px" type="button"><i class="fa-solid fa-angle-right"></button>`);
    } else {
        subContainer.addClass("show");
    }
    const searchBar = $(`<input type='text' class='rounded-top input-no-focus-visible' style="width:calc(100%${(extraWidth !== 0 ? ` - ${extraWidth}px` : "")})" name='${name}_search' placeholder='' autocomplete="off" />`);

    utils.append(searchBar);
    utils.append('<button class="btn bcs-list-group-button bcs-list-group-button-clear-search float-right btn-no-outline" style="width:32px" data-action="clear-search" type="button"><i class="fa-regular fa-xmark" style="margin-left:-4px;"></i></button>');
    if (isMultiSelect) {
        utils.append('<button class="btn bcs-list-group-button bcs-list-group-button-select-all float-right btn-no-outline" style="width:32px" data-action="+" type="button"><i class="fa-solid fa-square-check" style="margin-left:-4px;"></i></button>');
        utils.append('<button class="btn bcs-list-group-button bcs-list-group-button-deselect-all float-right btn-no-outline" style="width:32px" data-action="-" type="button"><i class="fa-regular fa-square" style="margin-left:-4px;"></i></button>');
    }
    newElem.append(utils);
    const itemContainer = $(`<div class="list-group d-block" style="overflow-y:auto;height:${height}px" data-item-container></div>`);
    subContainer.append(itemContainer);
    newElem.append(subContainer);
    if (collapsible) {
        const collapsedBar = $(`<div class="switching-collapse-${itemContainerName} collapse show"><p style="white-space:nowrap;overflow:hidden;text-overflow:ellipsis;max-width:${charLength}ch" class="mb-0 small" data-collapsed-container-items title=""></p></div>`);
        newElem.append(collapsedBar);
    }
    var optGroupCounter = 0;
    var optionCounter = 0;
    obj.find('option, optgroup').each(function (i, el) {
        const x = $(this);
        const isOptGroup = x.is('optgroup');
        if (isOptGroup) {
            optGroupCounter++;
            optionCounter = 0;
            itemContainer.append(`
                            <label class='list-group-item p-0 pl-2 pr-2 list-group-item-hover list-group-item-dark ms-optgroup-label' data-optgroup='${optGroupCounter}'>
                                ${x.attr('label')}
                            </label>
                        `);
        } else {
            let lbl = x.text();
            if (lbl === "") {
                lbl = "&nbsp;"
            }
            if (isMultiSelect) {
                itemContainer.append(`
                            <label class='list-group-item p-0 pl-4 pr-2 list-group-item-hover' data-select-index="${optionCounter}" data-optgroup-parent="${optGroupCounter}">
                                <input class='form-check-input' type='checkbox' data-checkbox value='${x.val()}' ${disabled ? 'disabled' : ''}>
                                ${lbl}
                            </label>
                        `);
            } else {
                itemContainer.append(`
                            <label class='list-group-item p-0 pl-2 pr-2 list-group-item-hover' data-select-index="${optionCounter}" data-optgroup-parent="${optGroupCounter}" data-value='${x.val()}'>
                                ${lbl}
                            </label>
                        `);
            }
            optionCounter++;
        }
    });
    const infoElem = $("<span class='bcs-list-group-lower bg-light' style='display:flow-root;' data-information></span>");
    subContainer.append(infoElem);
    sourceElem.after(newElem);
    newElem.on('click', 'button[data-action]', function () {
        const actionBtn = $(this);
        const parentDiv = actionBtn.closest('div[data-is-multiselect]');
        const itemContainer = parentDiv.find('div[data-item-container]');
        if (!itemContainer.closest(".collapse").hasClass("show")) {
            parentDiv.find('button[data-toggle="collapse"]').trigger('click');
        }
        const action = actionBtn.data('action');
        if (action === "clear-search") {
            const searchInput = actionBtn.siblings("input[name$='_search']");
            searchInput.val("");
            searchInput.trigger("change");
        } else if (action === "-") {
            itemContainer.find('input[type="checkbox"]:visible:checked').prop('checked', false);
            itemContainer.find('input[type="checkbox"]:visible:not(:checked)');
            handleMultiSelectChangeEvent(actionBtn);
        } else if (action === "+") {
            itemContainer.find('input[type="checkbox"]:visible:not(:checked)').prop('checked', true).trigger('change');
            itemContainer.find('input[type="checkbox"]:visible:checked');
            handleMultiSelectChangeEvent(actionBtn);
        }
    });
    newElem.on('click', 'button[data-toggle="collapse"]', function (e) {
        const actionBtn = $(this);
        const icon = $(this).find("i");
        if (icon.hasClass("rotate-90")) {
            icon.removeClass("rotate-90");
        } else {
            icon.addClass("rotate-90");
        }
        const parentDiv = actionBtn.closest('div[data-is-multiselect]');
        updateCollapsedLabels(parentDiv);
    });
    searchBar.on('change input paste keydown',
        debounce(function () {
            const searchInput = $(this);
            const searchText = searchInput.val().toLowerCase() || "";
            const parentDiv = searchInput.closest('div[data-is-multiselect]');
            const itemContainer = parentDiv.find('div[data-item-container]');
            if (!itemContainer.closest(".collapse").hasClass("show")) {
                parentDiv.find('button[data-toggle="collapse"]').trigger('click');
            }
            const listItems = parentDiv.find('label.list-group-item');
            listItems.each(function (i, el) {
                const it = $(this);
                const lbl = it.text().toLowerCase();
                if (searchText === "" || lbl.indexOf(searchText) >= 0) {
                    it.removeClass('d-none');
                } else {
                    it.addClass('d-none');
                }
            });
            updateMultiSelectStatus(parentDiv);
        }));
    newElem.on('keydown',
        function (e) {
            if (!(e.ctrlKey && e.shiftKey)) { return; }
            const el = $(this);
            const parentDiv = el.closest('div[data-is-multiselect]');
            const itemContainer = parentDiv.find('div[data-item-container]');
            if (e.key === "a") { // a
                e.preventDefault();
                itemContainer.find('input[type="checkbox"]:visible:not(:checked)').prop('checked', true);
                handleMultiSelectChangeEvent(el);
            } else if (e.key === "d") { // d
                e.preventDefault();
                itemContainer.find('input[type="checkbox"]:visible:checked').prop('checked', false);
                handleMultiSelectChangeEvent(el);
            }

        });
    newElem.on('click change',
        'label, input[type="checkbox"]', debounce(function (e) {
            handleMultiSelectChangeEvent($(this));
        }));
    const currentlyChecked = sourceElem.find('option:selected').map(function () { return $(this).val() }) || [];
    if (currentlyChecked.length > 0) {
        if (isMultiSelect) {
            $.each(newElem.find('input[type="checkbox"]'), function () {
                const inp = $(this);
                const inpValue = inp.val();
                if ($.inArray(inpValue, currentlyChecked) >= 0) {
                    inp.prop('checked', true);
                }
            });
            handleMultiSelectChangeEvent(newElem);
        } else {
            newElem.find('label[data-value="' + currentlyChecked[0] + '"]').addClass("active");
            newElem.find('label[data-value="' + currentlyChecked[0] + '"]').siblings('.list-group-item').removeClass("active");
        }
    }
    updateMultiSelectStatus(newElem);

    if (placeholder) {
        $(`input[name="${name}_search"]`).attr('placeholder', placeholder);
    } else {
        setTranslationOnAttribute('General_Search', `input[name="${name}_search"]`, 'placeholder');
    }
}

function updateCollapsedLabels(multiselect) {
    const itemContainer = multiselect.find('div[data-item-container]');
    const collapsedElementContainer = multiselect.find('p[data-collapsed-container-items]');
    const checkedItemLabels = itemContainer.find('input[type="checkbox"]:checked').map(function () {
        const lbl = $(this).parent().text().trim();
        return lbl === "" ? "\" \"" : lbl;
    }).get() || [];
    const labelString = checkedItemLabels.length === 0 ? "" : checkedItemLabels.join(', ');

    collapsedElementContainer.text(labelString);
    collapsedElementContainer.attr('title', labelString);
}

function setTranslationOnAttribute(resourceName, elementID, attributeName) {
    getTranslation(resourceName,
        function (e) {
            $(elementID).attr(attributeName, e);
        },
        elementID);
}

function handleMultiSelectChangeEvent(eventElem) {
    const parentContainer = eventElem.closest('div[data-is-multiselect]');
    const containerName = parentContainer.attr("name");
    const linkedSelect = parentContainer.siblings('select').filter(function () {
        return $(this).data('linked-to') === containerName;
    });
    const itemContainer = parentContainer.find('div[data-item-container]');
    const optGroupLookup = {
        groups: {},
        options: {}
    };
    if (eventElem.is('label') && eventElem.data('optgroup') !== undefined) {
        let optionGroupItems = eventElem.nextUntil('label[data-optgroup]');
        let checked = optionGroupItems.find('input[type="checkbox"]').filter(':checked');
        let unchecked = optionGroupItems.find('input[type="checkbox"]').filter(':not(:checked)');
        if ((checked.length || 0) > (unchecked.length || 0)) {
            checked.prop('checked', false).trigger('change');
        } else {
            unchecked.prop('checked', true).trigger('change');
        }
    }
    itemContainer.find("label[data-optgroup]").each(function (i, el) {
        const item = $(this);
        const optGroupNumber = parseInt(item.data('optgroup'), 10) || 0;
        optGroupLookup.groups[optGroupNumber] = item;
        optGroupLookup.options[optGroupNumber] = [];
    });
    if (optGroupLookup.options[0] === undefined) {
        optGroupLookup.options[0] = [];
        optGroupLookup.groups[0] = undefined;
    }
    itemContainer.children("label[data-optgroup-parent]").each(function (i, el) {
        const item = $(this);
        const optGroupNumber = parseInt(item.attr('data-optgroup-parent'), 10) || 0;
        const optGroupArray = optGroupLookup.options[optGroupNumber];
        optGroupArray.push(this);
    });
    const isMulti = parentContainer.data("is-multiselect");
    if (!isMulti) {
        const lbl = eventElem.is('label') ? eventElem : eventElem.closest('label');
        if (lbl.data('optgroup') === undefined) {
            parentContainer.find('label').removeClass('active');
            eventElem.closest('label:not([data-optgroup])').addClass('active');
            linkedSelect.val(eventElem.closest('label').data('value'));
            linkedSelect.trigger('change');
        }
    } else {
        const checkedData = [];
        const uncheckedData = [];
        $.each(Object.keys(optGroupLookup.groups),
            function (i, el) {
                const grp = optGroupLookup.groups[parseInt(el, 10)];
                const items = $(optGroupLookup.options[grp === undefined ? 0 : parseInt(el, 10)]);
                const checkedItems = items.find('input[type="checkbox"]:checked').map(function (i, el) {
                    return el.closest('label');
                }).sort(function (a, b) {
                    return parseInt($(a).data('select-index'), 10) - parseInt($(b).data('select-index'), 10);
                });;
                const uncheckedItems = items.find('input[type="checkbox"]:not(:checked)').map(function (i, el) {
                    return el.closest('label');
                }).sort(function (a, b) {
                    return parseInt($(a).data('select-index'), 10) - parseInt($(b).data('select-index'), 10);
                });;
                if (checkedItems.length !== 0) {
                    if (grp !== undefined && grp !== null) {
                        checkedData.push(grp.clone());
                    }
                    checkedData.push.apply(checkedData, checkedItems);
                }
                if (uncheckedItems.length !== 0) {
                    if (grp !== undefined && grp !== null) {
                        uncheckedData.push(grp.clone());
                    }
                    uncheckedData.push.apply(uncheckedData, uncheckedItems);
                }
            });
        itemContainer.find('div[data-divider]').detach();
        itemContainer.empty();
        itemContainer.append(checkedData);
        if (checkedData.length > 0 && uncheckedData.length > 0) {
            itemContainer.append($("<div data-divider class='bcs-list-group-divider'></div>"));
        }
        itemContainer.append(uncheckedData);
        const toSetOnSelect = itemContainer.find('input[type="checkbox"]:checked');
        const valArray = toSetOnSelect.map(function () { return $(this).val(); });
        linkedSelect.val(valArray);
        linkedSelect.trigger('change');
    }

    updateMultiSelectStatus(parentContainer);
}

function updateMultiSelectStatus(multiSelect) {
    const informationSpan = multiSelect.find("span[data-information]");
    const isMultiselect = multiSelect.data("is-multiselect");
    if (isMultiselect) {
        const uncheckedItemCount = multiSelect.find('input[type="checkbox"]:not(:checked)').length || 0;
        const checkItemCount = multiSelect.find('input[type="checkbox"]:checked').length || 0;
        const total = uncheckedItemCount + checkItemCount;
        informationSpan.html(`<small class='float-right pr-1'>${checkItemCount}/${total}</small>`);
        informationSpan.removeClass('d-none');
    } else {
        informationSpan.addClass('d-none');
    }
    updateCollapsedLabels(multiSelect);
}

function getTranslation(resourceName, onSuccess, placeholderID) {
    var translation = translations[resourceName];
    if (translation !== undefined) {
        onSuccess(translation);
        return;
    }

    loadingIcon.ajax(placeholderID,
        "/Resource/GetResource",
        { type: "POST", data: { resourceName: resourceName } },
        function (e) {
            onSuccess(e);
        });
    return;
}

function getTranslations(resourceNames, onSuccess, placeholderID) {
    var result = {};
    var unknown = [];
    $(resourceNames).each(function (i, e) {
        var translation = translations[e];
        if (translation !== undefined) {
            result[e] = translation;
        } else {
            unknown.push(e);
        }
    });
    if (unknown.length > 0) {
        loadingIcon.ajax(placeholderID,
            "/Resource/GetResources",
            { type: "POST", data: { resourceNames: unknown } },
            function (e) {
                $.each(e,
                    function (key, value) {
                        translations[key] = value;
                        result[key] = value;
                    });
                onSuccess(result);
            });
    } else {
        onSuccess(result);
    }
}

function setTranslation(resourceName, elementID) {
    getTranslation(resourceName,
        function (e) {
            $(elementID).text(e);
        },
        elementID);
    return;
}

function handleEmailDialogRequest(url, callingElem) {
    if (callingElem.data("mailing") !== undefined && callingElem.data("mailing") === 'true') {
        return;
    }
    callingElem.data("mailing", true);
    loadingIcon.showLoadingScreen();
    becosoft.ajax(url,
        {
            type: "POST",
            dataType: "json",
            async: true
        },
        function (result) {
            const container = $("#emaildialogcontainer");
            container.html("");
            container.html(result);
            const emailModal = container.find("div[data-email-dialog]");
            bindEmailDialogEvents(emailModal, container);
            emailModal.modal({ backdrop: 'static', show: true, keyboard: false });
        },
        undefined,
        function () {
            loadingIcon.hideLoadingScreen();
        });
}

function bindEmailDialogEvents(emailModal, container) {
    emailModal.on('hidden.bs.modal',
        function () {
            $("[data-mailing='true']").removeAttr('data-mailing');
        });
    enableHtmlEditor();
    emailModal.on('click',
        'button[data-send-mail]',
        function (e) {
            e.preventDefault();

            const data = {
                MessageType: emailModal.find('input[name="messagetype"]').val(),
                TypeID: emailModal.find('input[name="typeid"]').val(),
                Id: emailModal.find('input[name="id"]').val(),
                ContactID: emailModal.find('input[name="contactid"]').val(),
                ReplyToEnabled: emailModal.find('input[name="replytoenabled"]').val(),
                RedirectUrl: emailModal.find('input[name="redirecturl"]').val(),
                Subject: emailModal.find('input[name="subject"]').val(),
                Body: emailModal.find('textarea[name="body"]').val(),
                EnableReadNotification: emailModal.find('input[name="enablereadnotification"]').val(),
                UserID: emailModal.find('input[name="userid"]').val(),
                LanguageID: emailModal.find('input[name="languageid"]').val(),
                NoManipulation: emailModal.find('input[name="nomanipulation"]').val(),
                Receivers: [],
                ReceiversCc: [],
                ReceiversBcc: [],
                Attachments: [],
                TemplateAttachments: [],
                Templates: []
            };
            const getEmailAddress = (item, allowEmpty = false) => {
                var address = {
                    DisplayName: item.find('input[name$="displayname"]').val() || '',
                    Address: item.find('input[name$="address"]').val() || ''
                };
                if (!allowEmpty && address.DisplayName === '' && address.Address === '') {
                    return null;
                }
                return address;
            }
            data.Sender = getEmailAddress(emailModal.find('div[data-emaildialog_sender]'), true);
            data.ReplyTo = getEmailAddress(emailModal.find('div[data-emaildialog_replyto]'));
            $.each($("#receivers").find("tr[data-receiver]"),
                function (e) {
                    const row = $(this);
                    var address = getEmailAddress(row);
                    if (address === null) { return; }
                    data.Receivers.push(address);
                });
            $.each($("#receiverscc").find("tr[data-receiver]"),
                function (e) {
                    const row = $(this);
                    var address = getEmailAddress(row);
                    if (address === null) { return; }
                    data.Receivers.push(address);
                });
            $.each($("#receiversbcc").find("tr[data-receiver]"),
                function (e) {
                    const row = $(this);
                    var address = getEmailAddress(row);
                    if (address === null) { return; }
                    data.Receivers.push(address);
                });
            const getAttachment = (item) => {
                return {
                    FileName: item.data("filename"),
                    Base64FileContent: item.data("bytes"),
                    Size: item.data("size"),
                    MimeType: item.data("mimetype")
                };
            };
            var attachmentRows = emailModal.find("table[data-attachment-table] > tbody > tr");
            $.each(attachmentRows,
                function (e) {
                    const row = $(this);
                    const attachment = getAttachment(row);
                    data.Attachments.push(attachment);
                });
            var templateAttachmentRows = emailModal.find("table[data-template-attachment-table] > tbody > tr");
            $.each(templateAttachmentRows,
                function (e) {
                    const row = $(this);
                    const attachment = getAttachment(row);
                    data.TemplateAttachments.push(attachment);
                });
            var templateRows = emailModal.find("div[data-templates]").find("div[data-template]");
            $.each(templateRows,
                function (e) {
                    const templateData = $(this);

                    const template = {
                        Template: templateData.data("template"),
                        Subject: $("<textarea/>").html(templateData.data("subject")).text(),
                        Body: $("<textarea/>").html(templateData.data("body")).text(),
                        TemplateAttachments: []
                    };
                    var templateAttRows = templateData.find('div[data-bytes]');
                    $.each(templateAttRows,
                        function (e) {
                            const row = $(this);
                            const attachment = getAttachment(row);
                            template.TemplateAttachments.push(attachment);
                        });
                    data.Templates.push(template);
                });
            const handleUrl = emailModal.find('div[data-handle-email-dialog-url]').data('handle-email-dialog-url');
            becosoft.ajax(handleUrl,
                {
                    type: "POST",
                    dataType: "json",
                    data: data,
                    async: true
                }, function (result) {
                    if (result.Success === true) {
                        emailModal.modal('hide');
                        $('.modal-backdrop').remove();
                        container.find('div[data-email-confirmation]').modal({ backdrop: true, show: true });
                        return;
                    } else if (result.Success === false) {
                        const errorModal = container.find('div[data-email-error]');
                        errorModal.find('p[data-error-message]').text(result.Error || "");
                        errorModal.modal({ backdrop: true, show: true });
                        return;
                    } else {
                        emailModal.modal('hide');
                        $('.modal-backdrop').remove();
                        container.html("");
                        container.html(result);
                        emailModal = container.find("div[data-email-dialog]");
                        bindEmailDialogEvents(emailModal, container);
                        emailModal.modal({ backdrop: 'static', show: true, keyboard: false });
                    }
                });

        });
    emailModal.on('click',
        'button[data-toggle="collapse"]',
        function (e) {
            emailModal.modal('handleUpdate');
        });
    emailModal.on('click',
        'button[data-cancel-email]',
        function (e) {
            e.preventDefault();
            // ask confirmation to close (get resources from dialog)
            emailModal.modal('hide');
        });
    emailModal.on('click',
        'span[data-download]',
        function (e) {
            e.preventDefault();
            const elem = $(this);
            const row = elem.closest('tr');
            const mimeType = row.data("mimetype") || "application/octet-stream";
            const base64 = row.data("bytes");
            const hrefString = `data:${mimeType};base64,${base64}`;
            const link = document.createElement('a');
            link.href = hrefString;
            link.download = row.data("filename");
            link.click();
        });
    emailModal.on('click',
        'span[data-delete]',
        function (e) {
            e.preventDefault();

            const elem = $(this);
            const row = elem.closest('tr');
            const parentModal = elem.closest('div[data-email-dialog]');
            const deleteQuestion = parentModal.siblings("div[data-delete-attachment-question]");
            const qFileName = deleteQuestion.find('input[data-delete-template-filename]');
            qFileName.val(row.find('input[data-filename]').val());

            deleteQuestion.on('hidden.bs.modal',
                function (e) {
                    const btn = $(e.target);
                    if (btn.attr("data-confirm-delete") !== undefined) {
                        return;
                    }
                    row.remove();
                });
            deleteQuestion.modal({ backdrop: 'static', show: true, keyboard: false });
        });
    emailModal.on('click',
        'span[data-add-file]',
        function (e) {
            e.preventDefault();
            const elem = $(this);
            const parentModal = elem.closest('div[data-email-dialog]');
            parentModal.find('input[name="emaildialog_fileinput"]').trigger('click');
        });
    emailModal.on('change',
        'select[name="emaildialog_templateselection"]',
        function (e) {
            e.preventDefault();
            const elem = $(this);
            const parentModal = elem.closest('div[data-email-dialog]');
            const template = elem.val() || "";
            const templateContainer = parentModal.find("div[data-templates]");
            const templateData = templateContainer.find("div[data-template='" + template + "']");
            if (templateData === undefined || templateData === null) { return; }

            // TODO: ask change verification
            const templateSubject = $("<textarea/>").html(templateData.data("subject")).text();
            const templateBody = $("<textarea/>").html(templateData.data("body")).text();

            parentModal.find('input[emaildialog_subject]').val(templateSubject);
            enableHtmlEditor();
            const templateFiles = templateData.find('div[data-bytes]');
            const templateRow = parentModal.find("tr[data-templaterow]").clone();
            var templateAttachmentRowContainers = parentModal.find('div[data-template-attachments-section]').find('tbody');
            const megaByteSize = parseInt(parentModal.find('div[data-mega-byte-size]').data('mega-byte-size'), 10);
            templateAttachmentRowContainers.children().remove();
            templateFiles.each(function (t) {
                const templateFile = $(this);
                const size = parseInt(templateFile.data("size"), 10) || 0;
                const newRow = templateRow.clone();
                newRow.removeAttr('data-templaterow');
                newRow.data("filename", templateFile.data("filename"));
                newRow.data("size", size);
                newRow.data("mimetype", templateFile.data("mimetype"));
                newRow.data("bytes", templateFile.data("bytes"));
                newRow.find('td[data-filenameholder]').text(templateFile.data("filename"));
                newRow.find('td[data-sizeholder]').text(`${(size / new Number(megaByteSize)).formatDecimal(4)} MB`);
                templateAttachmentRowContainers.append(newRow);
            });
        });
    emailModal.on('change',
        'input[name="emaildialog_fileinput"]',
        function (e) {
            e.preventDefault();
            const elem = $(this);
            const parentModal = elem.closest('div[data-email-dialog]');
            const maxAttachmentSize = parseInt(parentModal.find('div[data-attachment-max-size]').data('attachment-max-size'), 10);
            const files = e.currentTarget.files;
            let totalSize = 0.0;
            $(files).each(function (i, file) {
                totalSize += file.size;
                if (file.size > maxAttachmentSize) {
                    // TODO: show message with max file size
                    return;
                }
            });
            if (totalSize === 0) { return; }
            var existingFileNames = [];
            var attachmentRowContainer = parentModal.find('div[data-attachments-section]').find('tbody');
            var attachmentRows = attachmentRowContainer.find('tr[data-bytes]');
            var templateAttachmentRows = parentModal.find('div[data-template-attachments-section]').find('tr[data-bytes]');
            attachmentRows.each(function () {
                const r = $(this);
                existingFileNames.push(r.data("filename").toLowerCase());
            });
            templateAttachmentRows.each(function () {
                const r = $(this);
                existingFileNames.push(r.data("filename").toLowerCase());
            });
            const templateRow = parentModal.find("tr[data-templaterow]").clone();
            const megaByteSize = parseInt(parentModal.find('div[data-mega-byte-size]').data('mega-byte-size'), 10);
            $(files).each(async function (i, file) {
                if (existingFileNames.filter(efn => efn.includes(file.name.toLowerCase())).length > 0) {
                    // TODO : filename already present already overwrite or add?
                    return;
                }
                const newRow = templateRow.clone();
                newRow.removeAttr('data-templaterow');
                newRow.data("filename", file.name);
                newRow.data("size", file.size);
                newRow.data("mimetype", file.type);
                const dataUrl = await ConvertFileUploadToBase64(file);
                const bytes = dataUrl.split(';base64,')[1];
                newRow.data("bytes", bytes);
                newRow.find('td[data-filenameholder]').text(file.name);
                newRow.find('td[data-sizeholder]').text(`${(file.size / new Number(megaByteSize)).formatDecimal(4)} MB`);
                attachmentRowContainer.append(newRow);
            });
        });
    emailModal.on('click',
        'span[data-add-receiver]',
        function (e) {
            e.preventDefault();
            const elem = $(this);
            const parentModal = elem.closest('div[data-email-dialog]');
            const parentTable = elem.closest('div[id^="receivers"]').find('table');
            const newRow = parentModal.find('tr[data-receiver-templaterow]').clone();
            newRow.removeAttr("data-receiver-templaterow");
            const tbody = parentTable.find('tbody');
            tbody.append(newRow);
        });
    emailModal.on('click',
        'span[data-delete-receiver]',
        function (e) {
            e.preventDefault();

            const elem = $(this);
            const tbody = elem.closest('tbody');
            const row = elem.closest('tr');
            const parentModal = elem.closest('div[data-email-dialog]');
            const deleteQuestion = parentModal.siblings("div[data-delete-receiver-question]");
            const qAddress = deleteQuestion.find('input[data-delete-receiver-address]');
            const qDispName = deleteQuestion.find('input[data-delete-receiver-displayname]');
            qAddress.val(row.find('input[name="address"]').val());
            qDispName.val(row.find('input[name="displayname"]').val());
            if (qAddress.val().trim() === '' && qDispName.val().trim() === '') {
                if (tbody.find('tr').length === 1) {
                    return;
                }
                qAddress.closest('div.input-group').addClass('d-none');
            } else {
                qAddress.closest('div.input-group').removeClass('d-none');
            }

            deleteQuestion.on('hidden.bs.modal',
                function (e) {
                    const btn = $(e.target);
                    if (btn.attr("data-confirm-delete") !== undefined) {
                        return;
                    }
                    row.remove();
                    if (tbody.find('tr').length === 0) {
                        const newRow = parentModal.find('tr[data-receiver-templaterow]').clone();
                        newRow.removeAttr("data-receiver-templaterow");
                        tbody.append(newRow);
                    }
                });
            deleteQuestion.modal({ backdrop: 'static', show: true, keyboard: false });

        });
    emailModal.on('click',
        'span[data-address-book]',
        function (e) {
            e.preventDefault();

            const elem = $(this);
            if (elem.attr('data-open-address-book') !== undefined && elem.attr('data-open-address-book') === 'true') {
                return;
            }
            elem.data('open-address-book', true);
            const emailModalContainer = $("#emaildialogcontainer");
            loadingIcon.showLoadingScreen(emailModalContainer);
            const url = elem.data('address-book-url');
            becosoft.ajax(url,
                {
                    type: "POST",
                    dataType: "json",
                    async: true
                },
                function (result) {
                    const container = $("#emailaddressbookcontainer");
                    container.html("");
                    container.html(result);
                    const addressBookModal = container.find("div[data-email-address-book-dialog]");
                    bindEmailAddressBookEvents(addressBookModal, container);
                    addressBookModal.modal({ backdrop: 'static', show: true, keyboard: false });
                },
                undefined,
                function () {
                    loadingIcon.hideLoadingScreen();
                });
        });
}

function bindEmailAddressBookEvents(addressBookModal, container) {
    addressBookModal.on('hidden.bs.modal',
        function () {
            $("[data-open-address-book='true']").removeAttr('data-open-address-book');
        });
    addressBookModal.on('change',
        'select[name$="EntryType"], select[name$="Branches"]',
        function (e) {
            const elem = $(this);
            const modal = elem.closest('div[data-email-address-book-dialog]');
            const tbody = modal.find('tbody[data-address-book-entries]');
            const query = modal.find('input[name$="SearchQuery"]').val().trim().toLowerCase() || "";
            const filter = parseInt(modal.find('select[name$="EntryType"]').val(), 10);
            const branch = parseInt(modal.find('select[name$="Branches"]').val(), 10);
            applyAddressBookEntryFilter(tbody, query, filter, branch);
        });
    addressBookModal.on('keyup change',
        'input[name$="SearchQuery"]',
        function (e) {
            const elem = $(this);
            const modal = elem.closest('div[data-email-address-book-dialog]');
            const tbody = modal.find('tbody[data-address-book-entries]');
            const query = modal.find('input[name$="SearchQuery"]').val().trim().toLowerCase() || "";
            const filter = parseInt(modal.find('select[name$="EntryType"]').val(), 10);
            const branch = parseInt(modal.find('select[name$="Branches"]').val(), 10);
            applyAddressBookEntryFilter(tbody, query, filter, branch);
        });
    addressBookModal.on('click',
        "button[data-address-book-save]",
        function (e) {
            e.preventDefault();
            const elem = $(this);
            const modal = elem.closest('div[data-email-address-book-dialog]');
            const emailModalContainer = $("#emaildialogcontainer");
            const tbody = modal.find('tbody[data-address-book-entries]');
            const rows = tbody.find('tr');
            const receiverTable = emailModalContainer.find('div[id="receivers"]').find('table');
            const receiverCcTable = emailModalContainer.find('div[id="receiverscc"]').find('table');
            $.each(rows,
                function () {
                    const r = $(this);
                    const toChecked = r.find('input[data-to]').is(":checked");
                    const ccChecked = r.find('input[data-cc]').is(":checked");
                    if (!toChecked && !ccChecked) { return; }
                    const newReceiverRow = emailModalContainer.find('tr[data-receiver-templaterow]').clone();
                    newReceiverRow.removeAttr("data-receiver-templaterow");
                    newReceiverRow.find('input[name="address"]').val(r.find('td[data-email]').text());
                    newReceiverRow.find('input[name="displayname"]').val(r.find('td[data-name]').text());
                    if (toChecked) {
                        receiverTable.find('tbody').append(newReceiverRow.clone());
                    }
                    if (ccChecked) {
                        receiverCcTable.find('tbody').append(newReceiverRow.clone());
                    }
                });
            modal.modal('hide');
        });
}

function applyAddressBookEntryFilter(tbody, query, filter, branch) {
    if (query === "" && filter === 0 && branch === 0) {
        tbody.find('tr.d-none').removeClass('d-none');
    } else {
        tbody.find('tr').each(function () {
            const r = $(this);
            const hasQueryMatch = query !== "" && (r.find('td[data-name]').text().toLowerCase().indexOf(query) >= 0 || r.find('td[data-email]').text().toLowerCase().indexOf(query) >= 0);
            const hasFilterMatch = parseInt(r.find('td[data-type]').attr('data-type'), 10) === filter;
            const hasBranchMatch = r.attr("data-branch-ids").split(',').some(e => parseInt(e, 10) === branch);
            if (hasQueryMatch || hasFilterMatch || hasBranchMatch) {
                r.removeClass('d-none');
            } else {
                r.addClass('d-none');
            }
        });
    }
}


function ConvertFileUploadToBase64(file) {
    var promise = new Promise(function (resolve, reject) {
        var reader = new FileReader();
        reader.onloadend = function (loadEvent) {
            resolve(loadEvent.target.result);
        };
        reader.onerror = function (e) {
            reject(e);
        };
        reader.readAsDataURL(file);
    });

    return promise;
}

function ImageURLToBase64(url) {
    fetch(url)
        .then((response) => response.blob())
        .then((blob) => {
            const reader = new FileReader();
            reader.readAsDataURL(blob);
            reader.onloadend = () => {
                const base64String = reader.result;

                return base64String;
            };
        });
}

// Returns a function, that, as long as it continues to be invoked, will not
// be triggered. The function will be called after it stops being called for
// N milliseconds. If `immediate` is passed, trigger the function on the
// leading edge, instead of the trailing.
//http://davidwalsh.name/javascript-debounce-function
function debounce(func, wait = 100, immediate = false) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        var later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
};

function uuidv4() {
    return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    );
}

function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

$(document).on("click", "a[data-admin-action]",
    function (e) {
        e.preventDefault();
        const el = $(this);
        const href = el.attr("href");
        const reloadUrl = el.attr("data-reload-url");
        becosoft.ajax(href, { type: "GET", async: false }, function () {
            window.location.replace(reloadUrl);
        });
    });

// compact view switches
$(document).on('click', '.compact-view-toggle', function () {
    var table = $(`${$(this).data("target-table")}`);
    table.toggleClass("bcs-table-xs");
});