/*global progressHub */
/*global client */
/*global server */
/*global username */
"use strict";

function StartInvoicing() {
    var progressNotifier = $.connection.progressHub;

    progressNotifier.client.initializeDeleteAll = function() {
        $(".progress-bar").css("width", "0");
        $(".progress-bar").attr("aria-valuenow", "0");
        console.log("start");
    };

    progressNotifier.client.deleteProgress = function(progr) {
        $(".progress-bar").css("width", progr + "%");
        $(".progress-bar").attr("aria-valuenow", progr + "%");
        console.log(progr);
    };

    progressNotifier.client.completeDeleteAll = function() {
        $(".progress-bar").css("width", "100%");
        $(".progress-bar").attr("aria-valuenow", "100%");
        console.log("stop");
        progressNotifier.server.stop(username);
    };

    $.connection.hub.start().done(function() {
        progressNotifier.server.start(username);
    });
}

$("body").on("click",
    "#passwordConfirm",
    function() {
        StartInvoicing();
    }
);