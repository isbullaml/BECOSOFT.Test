/*global progressHub */
/*global client */
/*global server */
/*global completedWeb */
/*global completedSql */
"use strict";

function StartInvoicing() {
    var progressNotifier = $.connection.progressHub;
    $.uiLock();

    progressNotifier.client.updateWebProgress = function(prog) {
        UpdateProgressWeb(prog);
    };
    progressNotifier.client.updateSqlProgress = function(prog) {
        UpdateProgressSQL(prog);
    };


    progressNotifier.client.sendSqlCompleted = function(pw) {
        UpdateProgressSQL(100);
        $("#sqlPwLoader").hide();
        $("#sqlPw").text(pw);
        $("#sqlPw").show();
        completedSql = true;
        if (completedWeb && completedSql) Done();
    };

    progressNotifier.client.sendWebCompleted = function(pw) {
        UpdateProgressWeb(100);
        $("#webPwLoader").hide();
        $("#webPw").text(pw);
        $("#webPw").show();
        completedWeb = true;
        if (completedWeb && completedSql) Done();
    };

    progressNotifier.client.done = Done;

    progressNotifier.client.failed = Done;

    $.connection.hub.start().done(function() {
        progressNotifier.server.start(username);
    });
}

(function($) {
    $.extend({
        uiLock: function() {
            $("a").on("click.myDisable", function(e) { e.preventDefault(); });
            $("<div>").attr("id", "uiLockId").css({
                'position': "absolute",
                'top': 0,
                'left': 0,
                'z-index': 1000,
                'width': "100%",
                'height': "100%"
            }).html("").appendTo("body");
        },
        uiUnlock: function() {
            $("#uiLockId").remove();
            $("a").off("click.myDisable");
        }
    });
})(jQuery);

function UpdateProgressWeb(prog) {
    $("#webprogtext").text(prog);
    $("#webprog .progress-bar").css("width", prog + "%");
    $("#webprog .progress-bar").attr("aria-valuenow", prog + "%");
}

function UpdateProgressSQL(prog) {
    $("#sqlprogtext").text(prog);
    $("#sqlprog .progress-bar").css("width", prog + "%");
    $("#sqlprog .progress-bar").attr("aria-valuenow", prog + "%");
}

function Done() {
    $.uiUnlock();
    $.connection.progressHub.server.stop(username);
}

$(document).ready(function() {
    $("#sqlPw").hide();
    $("#webPw").hide();
    StartInvoicing();
});