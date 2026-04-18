$("#vatForm").on("submit", function (e) {
    e.preventDefault();

    var form = $(this);

    if (!form.valid()) {
        return;
    }

    $("#vat-loader").show();
    $("#vatResultContainer").hide();

    becosoft.ajax(
        "/Home/Search",
        {
            type: "POST",
            data: form.serialize()
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
                    .text(result.Message || "Validation failed");
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