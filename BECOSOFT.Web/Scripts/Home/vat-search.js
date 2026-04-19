$(function () {

    var form = $("#vatForm");
    var input = $("#VatNumber");

    var validator = form.data("validator");
    if (validator) {
        validator.settings.onkeyup = function (element) {
            $(element).valid();
        };
    }

    input.on("input", function () {
        var value = $(this).val().trim();

        if (value === "") {
            $("#vatResultContainer").hide();
            $("#vatResultMessage").text("");
        }
    });

    form.on("submit", function (e) {
        e.preventDefault();

        var $form = $(this);

        if (!$form.valid()) {
            $("#vatResultContainer").hide();
            return;
        }

        $("#vat-loader").show();
        $("#vatResultContainer").hide();

        becosoft.ajax(
            "/Home/Search",
            {
                type: "POST",
                data: $form.serialize()
            },
            function (result) {

                $("#vat-loader").hide();
                $("#vatResultContainer").show();

                if (result.IsSuccess) {
                    $("#vatResultMessage")
                        .removeClass("text-danger")
                        .addClass("text-success")
                        .text(result.Data.FullAddress);
                } else {
                    $("#vatResultMessage")
                        .removeClass("text-success")
                        .addClass("text-danger")
                        .text(result.Message);
                }
            },
            function () {

                $("#vat-loader").hide();

                $("#vatResultContainer").show();
                $("#vatResultMessage")
                    .removeClass("text-success")
                    .addClass("text-danger")
                    .text("Something went wrong");
            }
        );
    });

});