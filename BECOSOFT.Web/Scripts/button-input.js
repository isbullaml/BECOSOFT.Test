$(".button-input button").on("click", function (e) {
    e.preventDefault();
    var inputParent = $(this).closest(".button-input");
    $(inputParent).find("button.active").removeClass("active");
    $(inputParent).find("button.latest-active").removeClass("latest-active");
    $(inputParent).find("input").val("");

    var val = $(this).data("value");
    var hiddenSelector = inputParent.data("hidden-input");
    $(hiddenSelector).val(val);
    $(this).addClass("active");
});

$(".button-input input").on("change paste keyup", function () {
    var inputParent = $(this).closest(".button-input");
    var latestButton = $(inputParent).find("button.active");
    if (latestButton.length > 0) {
        latestButton.addClass("latest-active");
        latestButton.removeClass("active");
    }

    var val = $(this).val();
    var hiddenSelector = inputParent.data("hidden-input");
    $(hiddenSelector).val(val);
    if (val === "") {
        $(inputParent).find("button.latest-active").trigger("click");
    }
});