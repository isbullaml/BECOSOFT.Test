$("#SelectedTemplateIDs").multiSelectSearch("", "");

$(function () {
    $(".top-level").each(function () {
        var subrights = $(this).closest(".rights-group").find(".subrights");
        if ($(this).is(":checked")) {
            subrights.find("input").removeAttr("disabled");
        } else {
            subrights.find("input").attr("disabled", "disabled");
        }
    });

    $(".popover-example").trigger("input");
    $("#EnableDueDate").trigger("change");
});

$("#EnableDueDate").on("change", function() {
    if ($(this).is(":checked")) {
        $("#DueDate").removeAttr("disabled");
    } else {
        $("#DueDate").attr("disabled", "disabled");
    }
});

$(".user-rights-group").on("click", function (e) {
    e.preventDefault();
    var id = $(this).data("id");
    var title = $(this).text();

    $("#user-rights-group-title").text(title);

    $("#user-rights-group-list .active").removeClass("active");
    $(this).addClass("active");

    $("#user-rights-group-settings > :not(.d-none)").addClass("d-none");
    $("#user-rights-group-settings #" + id).removeClass("d-none");
});

$(".top-level").on("change", function () {
    var subrights = $(this).closest(".rights-group").find(".subrights");
    var menuItem = $(".user-rights-group.active");
    if ($(this).is(":checked")) {
        subrights.find("input").removeAttr("disabled");
        menuItem.removeClass("user-rights-group-disabled");
        menuItem.addClass("user-rights-group-enabled");
    } else {
        subrights.find("input").attr("disabled", "disabled");
        menuItem.addClass("user-rights-group-disabled");
        menuItem.removeClass("user-rights-group-enabled");
    }
});

$(".other-right").on("change", function () {
    var menuItem = $(".user-rights-group.active");
    if ($(".other-right").is(":checked")) {
        menuItem.removeClass("user-rights-group-disabled");
        menuItem.addClass("user-rights-group-enabled");
    } else {
        menuItem.addClass("user-rights-group-disabled");
        menuItem.removeClass("user-rights-group-enabled");
    }
});

$("form").on("submit", function (e) {
    var isPasswordDisabled = $('#Password').prop('disabled');
    var disabled = $("form").find(":input:disabled").removeAttr("disabled");
    disabled.removeAttr("disabled");

    if (isPasswordDisabled) {
        $("#Password").attr('disabled', 'disabled');
    }

    if (!$("#EnableDueDate").is(":checked")) {
        $("#DueDate").attr('disabled', 'disabled');
    }
    console.log($("form"));
    return true;
});

$(".popover-example").on("input", function() {
    var url = $(this).val();
    var id = "#" + $(this).attr("id") + "Img";
    var img = $(id);
    img.data("img", url);
});

$(function () {
    $(document).on("change", ":file", function () {
        $("[data-valmsg-for='Picture.UploadedPicture']").text("");
        var input = $(this);
        var label = input.val().replace(/\\/g, "/").replace(/.*\//, "");
        var extension = label.replace(/^.*\./, "");
        var file = input.get(0).files[0];
        if (file.size > (4 * 1024 * 1024)) {
            $("[data-valmsg-for='Picture.UploadedPicture']").text(window.tooBigError);
            return;
        }
        if (extension !== "jpeg" && extension !== "jpg" && extension !== "png") {
            $("[data-valmsg-for='Picture.UploadedPicture']").text(window.invalidExtensionError);
            return;
        }
        var reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = function () {
            var byteArray = reader.result.split(",")[1];
            var data = { imageData: byteArray, width: window.imageWidth, height: window.imageHeight };
            becosoft.ajax(window.scaleUrl, { type: "POST", data: data }, function(result) {
                $("#SamplePicture").attr("src", "data:image;base64," + result);
                $("#Picture_Picture").val(result);
            });
        };
    });

    $("a[rel=popover]").popover({
        html: true,
        trigger: "hover",
        content: function () {
            return '<img src="' + $(this).data("img") + '" width="164px" height="auto" />';
        }
    }).on("show.bs.popover", function(e) {
        $(e.target).data("bs.popover").tip().css({ "max-width": "200px" });
    });
});

$(".check-all-1").on("change", function () {
    var subcheckboxes = $(this).closest(".rights-group").find(".sub-checkbox-1");
    if ($(this).is(":checked")) {
        subcheckboxes.find("input").attr("checked", "checked");
    } else {
        subcheckboxes.find("input").removeAttr("checked");
    }
});

$(".check-all-2").on("change", function () {
    var subcheckboxes = $(this).closest(".rights-group").find(".sub-checkbox-2");
    if ($(this).is(":checked")) {
        subcheckboxes.find("input").attr("checked", "checked");
    } else {
        subcheckboxes.find("input").removeAttr("checked");
    }
});

$(".check-all-3").on("change", function () {
    var subcheckboxes = $(this).closest(".rights-group").find(".sub-checkbox-3");
    if ($(this).is(":checked")) {
        subcheckboxes.find("input").attr("checked", "checked");
    } else {
        subcheckboxes.find("input").removeAttr("checked");
    }
});

$(".check-all-4").on("change", function () {
    var subcheckboxes = $(this).closest(".rights-group").find(".sub-checkbox-4");
    if ($(this).is(":checked")) {
        subcheckboxes.find("input").attr("checked", "checked");
    } else {
        subcheckboxes.find("input").removeAttr("checked");
    }
});

$(".check-all-5").on("change", function () {
    var subcheckboxes = $(this).closest(".rights-group").find(".sub-checkbox-5");
    if ($(this).is(":checked")) {
        subcheckboxes.find("input").attr("checked", "checked");
    } else {
        subcheckboxes.find("input").removeAttr("checked");
    }
});