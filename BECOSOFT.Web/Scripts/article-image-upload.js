
var $imageContainer = $("#image-container");
var imageUploaders = undefined;
var $imageUploaders = undefined;

//function resetFileUploader() {
//    if ($imageUploader !== undefined)
//        $imageUploader.replaceWith($imageUploader.val('').clone(true));

//    imageUploaders = $(".image-uploader");
//    $imageUploaders = $(imageUploaders);
//}
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
        var request = new XMLHttpRequest();
        request.open('GET', src, true);
        request.responseType = 'blob';
        request.onload = function () {
            var reader = new FileReader();
            reader.readAsDataURL(request.response);
            reader.onload = function (loadEvent) {
                var parent = item.closest('.row.sortable');
                var id = parent.find("input[name='ArticleImages.index']").value;
                var content = loadEvent.target.result;
                item.attr("src", content);

                //parent.find('ArticleImages_' + id + '__NewFile').val(content.split(",")[1]);
            };
        };
        request.send();
    });
}

function convertFileUploadToBase64(file) {
    var promise = new Promise(function (resolve, reject) {
        var reader = new FileReader();
        reader.onloadend = function (loadEvent) {
            resolve(loadEvent.target.result);
        };
        reader.onerror = function (e) {
            reject(e);
        };
        reader.readAsDataURL(file);
    });

    return promise;
}

function updateExistingImage(file, position, src) {

    id = position - 1;
    if (id < 0)
        id = 0;

    var existingImg = $('[id="ArticleImages_' + id + '__Position"]').closest('.image-item').find('.upload-card');//$('img[data-position="' + position + '"]');
    existingImg[0].src = src;

    if (src.startsWith('data:'))
        src = src.split(',')[1];

    $('#ArticleImages_' + id + '__NewFile').attr("value", src);
    $('#ArticleImages_' + id + '__FileName').attr("value", file.name);
}

function processNewImage(file, position, callBack) {
    if (file === undefined
        || file === null)
        return;

    var promise = convertFileUploadToBase64(file);

    promise.then(function (result) {
        var existingImg = $('[id="ArticleImages_' + (position - 1) + '__Position"]').closest('.image-item').find('.upload-card');//var existingImg = $('img[data-position="' + position + '"]');

        //No existing image? First add a new row
        if (existingImg.length === 0) {
            new Promise(function (resolve, reject) {
                var articleID = $("#getArticleImageView").closest('form').find('input#ArticleID').val();

                var url = $("#getArticleImageView").attr("data-url");
                url += (url.split('?')[1] ? '&' : '?') + 'position=' + position;
                url += (url.split('?')[1] ? '&' : '?') + 'articleID=' + articleID;

                becosoft.ajax(url, { async: true }, function (partialView) {
                    if (partialView === null || partialView === undefined || partialView.length === 0) {
                        reject();
                    }  else {
                        var lastRow = $("#image-container .image-item:last");
                        if (lastRow.length === 0) {
                            $("#image-container").prepend(partialView);
                        } else {
                            lastRow.after(partialView);
                        }
                        setUploadArea();

                        updateExistingImage(file, position, result);

                        resolve();
                    }
                });
            })
                .finally(function () {
                    //reInitialize the file-uploaders so the new ones are accounted for
                    initFileUploaders();

                    setFocusToTranslationField();

                    lastTranslation = $("#image-container .image-item:last .translations");
                    toggleTranslationActionButtons(lastTranslation);

                    callBack();
                });
        }
        else {
            updateExistingImage(file, position, result);
        }

        //initFileUploaders();
    }, function (err) {
        //alert('Error while converting the image to base64');
    });
}

function processNewImages(e) {
    var imageUploader = e.currentTarget;

    if (imageUploader.files === undefined
        || imageUploader.files === null)
        return;

    position = $(imageUploader).attr('data-position');

    var nrOfFiles = $(imageUploader.files).length;
    var finished = 0;

    var promises = [];


    $(imageUploader.files).each(function () {
        var d = new $.Deferred;
        var result = processNewImage(this, position, function () {
            d.resolve();
        });
        position++;

        promises.push(d.promise());
    });

    //Wait for all promises to be done
    $.when.apply($, promises).done(function () {

        reIndexArticleImages();

        //Iterate all the processed (failed and success) promises
        //
        //for (var i = 0, length = arguments.length; i < length; i++) {
        //    var ele = arguments[i];
        //}
    });
};


$('.upload-drop-zone').on("dragover", function () {
    this.className = 'upload-drop-zone drop';
    return false;
});

$('.upload-drop-zone').on("dragleave", function () {
    this.className = 'upload-drop-zone';
    return false;
});

$('.upload-drop-zone').on(
    'drop',
    function (e) {
        if (e.originalEvent.dataTransfer && e.originalEvent.dataTransfer.files.length) {
            e.preventDefault();
            e.stopPropagation();

            this.className = 'upload-drop-zone';

            var imageUploader = $('.upload-card .image-uploader');
            imageUploader.files = e.originalEvent.dataTransfer.files;
            uploadFiles();
        }
    }
);



function reIndexArticleImages() {
    var listItems = $imageContainer.find(".image-item");
    listItems.each(function () {
        var element = $(this);
        var index = $(".image-item").index(element);


        element.find("[id^='ArticleImages_']").each(function () {
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
                prefix = split.join(".");
                $(this).attr('value', prefix);
            }
        });

        element.find("#ArticleImages_" + index + "__Position").val(index + 1);
        element.find("#ArticleImages_" + index + "__Position").attr('value', index + 1);
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


$(document)
    .on("click", "button[data-action='removeImage']", function (e) {
        e.stopPropagation();
        e.preventDefault();
        $(this).closest(".image-item").remove();
        reIndexArticleImages();

        setUploadArea();

        setFocusToTranslationField();
    })
    .on("click", ".upload-card, .card", function (e) {
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

        $imageUploader = $(this).find('.image-uploader');
        $imageUploader.attr("data-position", currentPosition);

        $imageUploader.trigger("click");

        $(this).removeClass('noclick');
    })
    .ready(function () {

        initFileUploaders();
        ConvertImagesToBase64();

        $imageContainer.sortable({
            items: ".image-item",
            update: reIndexArticleImages,
            //add noclick-class to prevent triggering the upload-popup
            start: function (event, ui) { $(this).addClass('noclick'); },
            //remove the noclick-class on stop te re-enable the upload-popup
            stop: function (event, ui) { $(this).removeClass('noclick'); }
        });
    });