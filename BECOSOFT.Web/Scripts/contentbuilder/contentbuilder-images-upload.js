//select2 implementation
$(".select2").select2({});
$(".select2-tagging").select2({
    tags: true
});

$('#ddlContentTags').on('change', function (e) {
    var value = $("#ddlContentTags").select2('data');

    if (value.length === 0) {
        $('#WebsiteContentTagIDs').val('');
        return;
    }
    var valueJoined = value.map(function (item) {
        return item['id'];
    });

    $('#WebsiteContentTagIDs').val(valueJoined.join(","));
});

//Upload image
$("#ImageUpload").on("change", function (e) {
    e.preventDefault();
    var input = $(this);
    var file = input.get(0).files[0];
    var preview = $("#exampleImage");
    var reader = new FileReader();
    $("#undo-delete-image-button").prop("disabled", true);

    reader.onloadend = function () {
        preview.attr("src", reader.result);
        preview.removeClass("d-none");
        $("#DeleteImage").val(false);
        $("#delete-image-button").prop("disabled", false);
        var oldSrc = $("#ImageAssetPath").val();
        if (oldSrc.length > 0) {
            $("#undo-delete-image-button").prop("disabled", false);
        }
    };

    if (file) {
        reader.readAsDataURL(file);
    } else {
        preview.src = "";
    }
});

//delete image
$("#delete-image-button").on("click", function (e) {
    e.preventDefault();

    var logoUpload = $("#ImageUpload");
    var preview = $("#exampleImage");

    logoUpload.val("");
    preview.attr("src", "");
    preview.addClass("d-none");
    $("#DeleteImage").val(true);
    $("#delete-image-button").prop("disabled", true);

    var oldSrc = $("#ImageAssetPath").val();
    if (oldSrc.length > 0) {
        $("#undo-delete-image-button").prop("disabled", false);
    }
});

//undo delete image mobile
$("#undo-delete-image-button").on("click", function (e) {
    e.preventDefault();

    var logoUpload = $("#ImageUpload");
    var preview = $("#exampleImage");
    var oldSrc = $("#ImageAssetPath").val();

    logoUpload.val("");
    $("#undo-delete-image-button").prop("disabled", true);
    if (oldSrc.length > 0) {
        preview.attr("src", oldSrc);
        preview.removeClass("d-none");
        $("#delete-image-button").prop("disabled", false);
    }

    $("#DeleteImage").val(false);
});

//upload image mobile
$("#ImageMobileUpload").on("change", function (e) {
    e.preventDefault();
    var input = $(this);
    var file = input.get(0).files[0];
    var preview = $("#exampleImageMobile");
    var reader = new FileReader();

    $("#undo-delete-imagemobile-button").prop("disabled", true);

    reader.onloadend = function () {
        preview.attr("src", reader.result);
        preview.removeClass("d-none");
        $("#DeleteImageMobile").val(false);
        $("#delete-imagemobile-button").prop("disabled", false);
        var oldSrc = $("#ImageMobileAssetPath").val();
        if (oldSrc.length > 0) {
            $("#undo-delete-imagemobile-button").prop("disabled", false);
        }
    };

    if (file) {
        reader.readAsDataURL(file);
    } else {
        preview.src = "";
    }
});

//delete image mobile
$("#delete-imagemobile-button").on("click", function (e) {
    e.preventDefault();

    var logoUpload = $("#ImageMobileUpload");
    var preview = $("#exampleImageMobile");

    logoUpload.val("");
    preview.attr("src", "");
    preview.addClass("d-none");
    $("#DeleteImageMobile").val(true);
    $("#delete-imagemobile-button").prop("disabled", true);

    var oldSrc = $("#ImageMobileUploadAssetPath").val();
    if (oldSrc.length > 0) {
        $("#undo-delete-imagemobile-button").prop("disabled", false);
    }
});

$("#undo-delete-imagemobile-button").on("click", function (e) {
    e.preventDefault();

    var logoUpload = $("#ImageMobileUpload");
    var preview = $("#exampleImageMobile");
    var oldSrc = $("#ImageMobileAssetPath").val();

    logoUpload.val("");
    $("#undo-delete-imagemobile-button").prop("disabled", true);
    if (oldSrc.length > 0) {
        preview.attr("src", oldSrc);
        preview.removeClass("d-none");
        $("#delete-imagemobile-button").prop("disabled", false);
    }

    $("#DeleteImageMobile").val(false);
});