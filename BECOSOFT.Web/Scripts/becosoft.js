var becosoft = {
    ajaxQueue: []
};

/**
 * Internal AJAX request which automatically handles 401 (Unauthorized) headers.
 * @param {string} url
 * @param {settings} settings
 * @param {function(data, string, jqXHR)} [done = function() {}]
 * @param {function(jqXHR, string, error)} [fail = function() {}]
 * @param {function(data|jqXHR, string, jqXHR|error)} [always = function() {}]
 */
becosoft.ajax = function (url, settings, done = function () { }, fail = function () { }, always = function () { }) {
    $.ajax(url, settings)
        .done(done)
        .fail(function (request, string, error) {
            if (request.status === 401) {
                becosoft.ajaxQueue.push(() => becosoft.ajax(url, settings, done, fail, always));
                var loginModalIsVisible = $("#loginModal.show").length > 0;
                if (!loginModalIsVisible || becosoft.ajaxQueue.length === 1) {
                    becosoft.openLoginModal(function () {
                        while (becosoft.ajaxQueue.length > 0) {
                            var task = becosoft.ajaxQueue.shift();
                            task();
                        }
                    });
                }
            } else {
                console.log("Something went wrong");
                console.log(error);
                console.log(url);
                console.log(settings);
                fail(request, string, error);
            }
        })
        .always(always);

    /**
     * Opens a login modal to do authentication.
     * @param {function()} callbackOnSuccess
     * @param {function()} callbackOnFailure
     */
    becosoft.openLoginModal = function (callbackOnSuccess, callbackOnFailure) {
        $("#login-modal-container").html("");
        $(document).find('#loginModal').remove();
        let url = $("#login-modal-container").data("url");
        let verificationUrl = $("#login-modal-container").data("verification-url");
        becosoft.ajax(url, { type: "GET" }, function (result) {
            openModal("#login-modal-container", result, true, function (container, modal) {
                modal.find("form").on("submit", function (e) {
                    let form = $(this);
                    e.preventDefault();
                    $.ajax(verificationUrl, { type: "GET" }).done(function (verificationResult) {
                        if (verificationResult === 0) {
                            let action = form.attr("action");
                            let formData = form.serialize();
                            $.ajax({
                                data: formData,
                                type: "POST",
                                url: action,
                                dataType: "json",
                                async: true,
                                success: function (innerResult) {
                                    if (Number.isInteger(innerResult) && innerResult !== 0) {
                                        $("#loginModal").on("hidden.bs.modal", function () {
                                            $("#login-modal-container").html("");
                                        });

                                        closeCurrentModal();

                                        let activeUser = parseInt($("#login-modal-container").data("user"));
                                        if (activeUser !== innerResult) {
                                            let baseUrl = $("#login-modal-container").data("base-url");
                                            window.location = baseUrl;
                                        } else {
                                            callbackOnSuccess();
                                        }
                                    } else if ('View' in innerResult) {
                                        const view = $(innerResult.View);
                                        const dataContainer = view.find('div[data-login-container]');
                                        form.find('div[data-login-container]').replaceWith(dataContainer);
                                    } else {
                                        console.log("Authentication failed");
                                        $("#error-container").html(innerResult);
                                        callbackOnFailure();
                                    }
                                }
                            });
                        } else {
                            closeCurrentModal();

                            let activeUser = parseInt($("#login-modal-container").data("user"));
                            if (activeUser !== verificationResult) {
                                let baseUrl = $("#login-modal-container").data("base-url");
                                window.location = baseUrl;
                            } else {
                                callbackOnSuccess();
                            }
                        }
                    });

                });
            });
        });
    };

}; /**
  * @typedef {Object} Button
  * @property {number} position - The position of the button
  * @property {string} text - The displayed text of the button
  * @property {string} class - The classes of the button
  * @property {function()} onClick - The onClick function of the button
  *
  *
  */

/**
 * Opens a question modal with provided information.
 * @param {string} title - The HTML formatted title of the modal 
 * @param {string} text - The HTML formatted text of the modal 
 * @param {{position: number, text: string, class: string, onClick: function(), keepOpen: boolean}[]} buttons - The definition of the buttons to add
 * @param {function()} onClose - The function which is called when the modal is closed
 * @param {boolean} allowBackdrop - Allow the modal to be closed when clicked beside it
 * @param {boolean} onClose - Allow the modal to be closed with the 'Esc' key
 * @param {boolean} useLargeModal - Make the modal larger
 */
becosoft.openQuestionModal = function (title, text, buttons, onClose = function () { }, allowEscapeKey = true, allowBackdrop = true, useLargeModal = false) {
    var modal = $("#questionModal");
    var buttonGroup = modal.find("#buttonGroup").html("");
    buttons.sort((a, b) => a.position - b.position);
    modal.find(".modal-title").html(title);
    modal.find(".modal-body").html(text);
    modal.find(".modal-dialog").toggleClass("modal-lg", useLargeModal);
    for (var button of buttons) {
        const buttonFunction = button["onClick"];
        const keepOpen = button["keepOpen"];
        buttonGroup.append($(`<div class="${button["class"]}">${button["text"]}</div>`).on("click", function () {
            buttonFunction.call();
            if (!keepOpen) { modal.data("perform-exit", false).modal("toggle"); }
        }));
    }
    modal.data("perform-exit", true).on("hidden.bs.modal", () => {
        if (modal.data("perform-exit")) { onClose.call(); }
    });
    modal.modal({ backdrop: allowBackdrop ? true : "static", keyboard: allowEscapeKey });
};

/**
 * Opens the question modal with a single close button.
 * @param {string} titleResource - The resourcename of the title of the modal
 * @param {string} textResource - The resourcename of the text of the modal
 */
becosoft.openInfoModal = function (titleResource, textResource) {
    var modal = $("#questionModal");
    modal.find(".modal-title").html("");
    modal.find(".modal-body").html("");
    modal.find("#buttonGroup").html("")
        .append("<div class='hidden'></div>")
        .append("<div class='btn btn-sm btn-primary w-25 p-2'><svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 64 16\"><path fill=\"#FFFFFF\" d=\"M43.3,0.6c0.6,0.6,0.6,1.6,0,2.2L30.8,15.4c-0.6,0.6-1.6,0.6-2.2,0l-6.3-6.3c-0.6-0.6-0.6-1.6,0-2.2s1.6-0.6,2.2,0l5.2,5.2L41.1,0.6C41.7,0,42.7,0,43.3,0.6L43.3,0.6z\" /></svg ></div>")
        .on("click", function () { modal.data("perform-exit", false).modal("toggle"); });
    modal.modal({ backdrop: true, keyboard: true });
    loadingIcon.callback("#questionModal .modal-body",
        function (e) {
            getTranslations([titleResource, textResource],
                function (e) {
                    var modal = $("#questionModal");
                    modal.find(".modal-title").html(e[titleResource]);
                    modal.find(".modal-body").html(e[textResource]);
                });

        });
};