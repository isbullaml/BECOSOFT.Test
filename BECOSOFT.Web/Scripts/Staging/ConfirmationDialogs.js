/*global waitingDialog */
/*global url*/
/*global waitMessage */

"use strict";
$(function() {
    var $href = "";

    $(".btn-wait").unbind("click").click(function(e) {
        e.preventDefault();
        $href = this.href;
        var element = $(this);
        var icon = element.find("img").prop("outerHTML");
        var text = element.text();
        var loadingIcon = $("#loader-icon");
        element.html(loadingIcon + text);
        becosoft.ajax($href, {
            data: {
                id: element.data("id"),
                actionType: element.data("action")
            },
            type: "POST",
            dataType: "text",
            async: true,
            success: function(result) {
                if (result !== null) {
                    var modalContainer = $("#modal-container");
                    modalContainer.html(result);
                    modalContainer.children("div").first().modal("show");
                }
                element.html(icon + text);
            }
        });
    });

    $(".alert-dismissible button").click(function(e) {
        e.preventDefault();
        $(".alert-dismissible").hide();
    });

    $(document).on("click",
        "#confirmAction",
        function(e) {
            e.preventDefault();
            $("#confirmModal").hide();
            window.waitingDialog.show(waitMessage);
            becosoft.ajax($href, {
                type: "POST",
                success: function() {
                    location.reload();
                },
                error: function(response) {
                    clearModal();
                    $(".form-error").text(response.responseText);
                }
            });
            return false;
        }
    );

    $(".btn-delete-all").unbind("click").click(function(e) {
        e.preventDefault();
        $href = this.href;
        var element = $(this);
        var icon = element.find("img").prop("outerHTML");
        var text = element.text();
        var loadingIcon = $("#loader-icon");
        element.html(loadingIcon + text);
        becosoft.ajax($href, {
            data: {
                id: element.data("id")
            },
            type: "GET",
            dataType: "text",
            async: true,
            success: function(result) {
                if (result !== null) {
                    var modalContainer = $("#modal-container");
                    modalContainer.html(result);
                    modalContainer.children("div").first().modal("show");
                }
                element.html(icon + text);
            }
        });
    });

    $(".btn-delete").unbind("click").click(function(e) {
        e.preventDefault();
        $href = this.href;
        var element = $(this);
        var icon = element.find("img").prop("outerHTML");
        var text = element.text();
        var loadingIcon = $("#loader-icon");
        element.html(loadingIcon + text);
        becosoft.ajax($href, {
            data: {
                id: element.data("id"),
                actionType: element.data("action")
            },
            type: "POST",
            dataType: "text",
            async: true,
            success: function(result) {
                if (result !== null) {
                    var modalContainer = $("#modal-container");
                    modalContainer.html(result);
                    modalContainer.children("div").first().modal("show");
                }
                element.html(icon + text);
            }
        });
    });

    $(document).on("submit", "#passwordModal",
        function(e) {
            var $pw = $("#passwordInput").val();
            e.preventDefault();
            $("#passwordModal").hide();
            window.waitingDialog.show(waitMessage);
            becosoft.ajax($href, {
                type: "POST",
                data: { "password": $pw },
                success: function() {
                    location.reload();
                },
                error: function(response) {
                    clearModal();
                    $(".form-error").text(response.statusText);
                }
            });
            return false;
        });

    function clearModal() {
        $("#modal-container").modal("hide");
        $("body").removeClass("modal-open");
        $(".modal-backdrop").remove();
    }
});