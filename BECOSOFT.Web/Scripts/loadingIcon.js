var loadingIcon = {};
var loaderVisible = false;

/**
 * Sets a loading icon in the chosen resultId and then performs an ajax call.
 * @param {string} resultId
 * @param {string} url
 * @param {settings} settings
 * @param {function(data, string, jqXHR)} [done = function() {}]
 * @param {function(jqXHR, string, error)} [fail = function() {}]
 * @param {function(data|jqXHR, string, jqXHR|error)} [always = function() {}]
 * @param {string} text
 */
loadingIcon.ajax = function(resultId, url, settings, done = function () { }, fail = function () { }, always = function () { }, text = "") {
    loadingIcon.callback(resultId, function () {
        becosoft.ajax(url, settings, done, fail, always);
    }, text);
};

/**
 * Sets a loading icon in the chosen resultId and then performs a callback.
 * @param {string} resultId
 * @param {function()} callback
 * @param {string} text
 */
loadingIcon.callback = function(resultId, callback, text = "") {
    var loadingIcon = $("#loading-icon").html();
    var resultHtml = $(loadingIcon);
    if (text !== "") {
        resultHtml.find("#loading-icon-text").text(text);
    }

    $(resultId).html(resultHtml);
    callback();
};

/**
 * Display a loadingscreen on the current page
 */
loadingIcon.showLoadingScreen = function(container = null) {
    var loadingIcon = $("#loading-icon").html();
    let elem =
        '<div id="loading-screen" style="height: 100%; width: 100%; background: rgba(0, 0, 0, 0.2); position: fixed; left: 0; top: 0; z-index: 99999; padding-top: 10rem; color: black !important;">'
            + '<div class="card py-4" style="margin: 0 auto;">'
            + loadingIcon
            + '</div>'
            + '</div>';
    loaderVisible = true;
    if (container != null) {
        container.append(elem);
    } else {
        $("#page-content").append(elem);
    }
};

/**
 * Hides the loadingScreen
 */
loadingIcon.hideLoadingScreen = async function () {
    await new Promise(r => setTimeout(r, 1000));
    $("#loading-screen").remove();
    loaderVisible = false;
};

/**
 * Hides the loadingScreen (sync)
 */
loadingIcon.hideLoadingScreenSync = function() {
    $("#loading-screen").remove();
    loaderVisible = false;
};

loadingIcon.isLoadingScreenVisible = function() {
    return loaderVisible;
};

/**
* Sets a loading icon in the chosen resultId and then performs an ajax call.
* @param {string} resultId
* @param {string} url
* @param {settings} settings
* @param {function(data, string, jqXHR)} [done = function() {}]
* @param {function(jqXHR, string, error)} [fail = function() {}]
* @param {function(data|jqXHR, string, jqXHR|error)} [always = function() {}]
*/
loadingIcon.ajaxWithLoadingScreen = function (url, settings, done = function () { }, fail = function () { }, always = function () { }) {
    loadingIcon.showLoadingScreen();

    function alwaysCall() {
        try {
            always();
        } catch (e) { }
        loadingIcon.hideLoadingScreenSync();
    }
    becosoft.ajax(url, settings, done, fail, alwaysCall);
}