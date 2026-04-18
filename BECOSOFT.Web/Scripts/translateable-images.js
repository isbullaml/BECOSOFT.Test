var $imageContainer = undefined;
var imageUploaders = undefined;
var $imageUploaders = undefined;
var imageListPrefix = undefined;


function initFileUploaders() {
    imageUploaders = $(".image-uploader");
    $imageUploaders = $(imageUploaders);
    $imageUploaders.off('change');
    $imageUploaders.on('change', processNewImages);
}

//Convert the image-src to base64
function ConvertImagesToBase64() {
    var listItems = $imageContainer.find("img");

    listItems.each(function () {
        var item = $(this);
        var src = item.attr("src");
        if (src && src.length > 0) {
            if (!src.includes("base64,")) {
                ConvertImageSourceToBase64(src).then(function (result) {
                    let input = item.closest(".image-item").find("input[id$='__Base64File']");
                    input.val(result);
                    item.attr("src", result);
                });
            }
        }
    });

    var inputItems = $imageContainer.find("input[id$='__Base64File']");
    inputItems.each(function () {
        var item = $(this);
        var img = item.closest(".card").find(".upload-card");
        var src = item.val();
        if (src && src.length > 0) {
            img.attr("src", src);
        }
    });
}

function updateExistingImage(position, src, fileName) {
    var id = position - 1;
    if (id < 0) {
        id = 0;
    }

    var existingImg = $('[id="' + imageListPrefix + '_' + id + '__Position"]').closest('.image-item').find('.upload-card');
    existingImg[0].src = src;

    $('#' + imageListPrefix + '_' + id + '__Base64File').val(src);
    var fileNameInput = $('#' + imageListPrefix + '_' + id + '__OriginalFileName');
    if (fileNameInput) {
        fileNameInput.val(fileName);
    }
}

function processNewImage(file, position, callBack) {
    if (file === undefined || file === null) {
        return;
    }

    var promise = ConvertFileUploadToBase64(file);
    var fileName = file.name;

    promise.then(function (result) {
        var existingImg = $('[id="' + imageListPrefix + '_' + (position - 1) + '__Position"]').closest('.image-item').find('.upload-card');
        //No existing image? First add a new row
        if (existingImg.length === 0) {
            new Promise(function (resolve, reject) {
                var url = $("#addImage").attr("data-url");
                url += (url.split('?')[1] ? '&' : '?') + 'position=' + position;

                becosoft.ajax(url,
                    { async: true },
                    function (partialView) {
                        if (partialView === null || partialView === undefined || partialView.length === 0) {
                            reject();
                        } else {
                            var lastRow = $("#image-container .image-item:last");
                            if (lastRow.length === 0) {
                                $("#image-container").prepend(partialView);
                            } else {
                                lastRow.after(partialView);
                            }
                            setUploadArea();

                            updateExistingImage(position, result, fileName);

                            resolve();
                        }
                    });
            }).finally(function () {
                //reInitialize the file-uploaders so the new ones are accounted for
                initFileUploaders();
                if (typeof setFocusToTranslationField === "function") {
                    setFocusToTranslationField();
                }
                var lastTranslation = $("#image-container .image-item:last .translations");

                var id = position - 1;
                if (id < 0) {
                    id = 0;
                }
                var fileNameInput = $('#' + imageListPrefix + '_' + id + '__OriginalFileName');
                if (fileNameInput) {
                    var fileName = fileNameInput.val();
                    var altText = lastTranslation.find("[id$='AlternateText']");
                    if (altText) {
                        altText.val(fileName);
                    }
                }

                toggleTranslationActionButtons(lastTranslation);
                callBack();
            });
        } else {
            updateExistingImage(position, result, fileName);
        }

        //initFileUploaders();
    }, function (err) {
        //alert('Error while converting the image to base64');
    });
}

function processNewImages(e) {
    var imageUploader = e.currentTarget;
    if (imageUploader.files === undefined || imageUploader.files === null) {
        return;
    }

    var position = $(imageUploader).attr('data-position');
    var promises = [];

    $(imageUploader.files).each(function () {
        var d = new $.Deferred;
        var file = this;

        const img = new Image();
        img.src = window.URL.createObjectURL(file);
        img.onload = () => {
            if (img.width < 750) {
                $('#small-image-modal').modal('show');
                $('[data-id="article-image-current-width"]').text(img.width + " px.");
            } else {
                $('#small-image-modal').modal('hide');
            }
            processNewImage(file,
                position,
                function () {
                    d.resolve();
                });

            position++;

            promises.push(d.promise());
        };
    });

    //Wait for all promises to be done
    $.when.apply($, promises).done(function () {
        imageUploader.files = null;
        imageUploader.value = null;
        reIndexImages();
    });
};

$('[data-id=select-other-image]').on('click', function () {
    $('#image-container').find('.image-item:last').remove();
    $(".upload-card").trigger('click');
});

$('[data-id=continue-small-image]').on('click', function () {
    $('#small-image-modal').modal('hide');
});

$(".upload-drop-zone:not(.drop)").on("dragover",
    function () {
        this.classList.add("drop");
        return false;
    });

$(".upload-drop-zone.drop").on("dragleave",
    function () {
        this.classList.remove("drop");
        return false;
    });

$(".upload-drop-zone").on(
    "drop",
    function (e) {
        if (e.originalEvent.dataTransfer && e.originalEvent.dataTransfer.files.length) {
            e.preventDefault();
            e.stopPropagation();

            this.classList.remove("drop");

            var imageUploader = $(".upload-card .image-uploader");
            imageUploader[0].files = e.originalEvent.dataTransfer.files;

            var currentPosition = $(this).parent().find('.card-body [id$="__Position"]').val();
            if (!currentPosition) {
                var position = $imageContainer.find('[id$="__Position"]:last').val();
                if (position === undefined)
                    position = 0;

                currentPosition = parseInt(position) + 1;
            }

            imageUploader.attr("data-position", currentPosition);
            imageUploader.trigger("change");
        }
    }
);


function reIndexImages() {
    var listItems = $imageContainer.find(".image-item");
    listItems.each(function () {
        var element = $(this);
        var index = $(".image-item").index(element);


        element.find("[id^='" + imageListPrefix + "_']").each(function () {
            var id = $(this).attr("id");
            var split = id.split("_");
            split[1] = index + "";
            $(this).attr("id", split.join('_'));


            var name = $(this).attr("name");
            split = name.split("[");
            var secondPart = split[1];
            var split2 = secondPart.split("]");
            split2[0] = index;
            split[1] = split2.join("]");
            name = split.join("[");
            $(this).attr("name", name);

            if (id.endsWith('__HtmlFieldPrefix')) {
                split = name.split(".");
                split.pop();
                var prefix = split.join(".");
                $(this).attr('value', prefix);
            }
        });

        element.find("#" + imageListPrefix + "_" + index + "__Position").val(index + 1);
        element.find("#" + imageListPrefix + "_" + index + "__Position").attr('value', index + 1);
        element.find(".position-text").text(index + 1);

    });
}

function setUploadArea() {
    var listItems = $("#image-container .image-item");
    if (listItems.length < maxImages) {
        $(".upload-part").removeClass("d-none");
    } else {
        $(".upload-part").addClass("d-none");
    }
}

function initImageUploader() {
    $imageContainer = $("#image-container");

    initFileUploaders();
    ConvertImagesToBase64();
    maxImages = $imageContainer.data("max-images");
    imageListPrefix = $imageContainer.data("list-prefix");

    $imageContainer.sortable({
        items: ".image-item",
        update: reIndexImages,
        //add noclick-class to prevent triggering the upload-popup
        start: function (event, ui) { $(this).addClass('noclick'); },
        //remove the noclick-class on stop te re-enable the upload-popup
        stop: function (event, ui) { $(this).removeClass('noclick'); }
    });
}

$(document)
    .on("click",
        "button[data-action='removeImage']",
        function (e) {
            e.stopPropagation();
            e.preventDefault();
            $(this).closest(".image-item").remove();
            reIndexImages();

            setUploadArea();

            if (typeof setFocusToTranslationField === "function") {
                setFocusToTranslationField();
            }
        })
    .on("click",
        ".upload-card",
        function (e) {
            //prevent endless triggering from the click-event bubbling up
            if ($(this).hasClass('noclick'))
                return;

            $(this).addClass('noclick');

            var currentPosition = $(this).parent().find('.card-body [id$="__Position"]').val();
            if (!currentPosition) {
                var position = $imageContainer.find('[id$="__Position"]:last').val();
                if (position === undefined)
                    position = 0;

                currentPosition = parseInt(position) + 1;
            }

            var $imageUploader = $(this).find('.image-uploader');
            $imageUploader.attr("data-position", currentPosition);

            $imageUploader.trigger("click");

            $(this).removeClass('noclick');
        })
    .ready(function () {
        initImageUploader();
        setUploadArea();
    });