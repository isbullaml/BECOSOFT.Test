
var froalaLicenseKey = "xGE6oE4G4H4C11A6B4A4gKTRe1CD1PGb1DESAb1Kd1EBH1Pd1TKoD6C5G5B4G2D3J4B4A5A5==";

function enableHtmlEditor(domElement) {
    var htmlEditors;

    if (domElement != null) {
        if (domElement.is("textarea[data-html='1']")) {
            htmlEditors = domElement;
        }
        else {
            htmlEditors = domElement.find("textarea[data-html='1']");
        }

    }
    else {
        htmlEditors = $("textarea[data-html='1']");
    }

    var apiBasePath = "/api/FroalaApi/";
    var maxVideoSize = 100;
    var videoAllowedTypes = ['webm', 'ogg', 'mp4'];
    var imageAllowedTypes = ['jpeg', 'jpg', 'png', 'webp', 'gif'];
    var fileAllowedTypes = ['application/pdf', 'application/msword', 'application/vnd.ms-excel', 'text/csv', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 'application/json', 'text/plain', 'application/xml' ];
    var maxImageSize = 5;
    var maxFileSize = 20;


    var fileUploadPreset = function (instanceId) {
        return {
            enter: FroalaEditor.ENTER_P,
            imageUpload: true,
            pasteImage: true,
            videoUpload: true,
            fileUpload: true,
            key: froalaLicenseKey,
            zIndex: 2501,
            attribution: false,
            toolbarButtons: {
                moreText: {
                    buttons: ['bold', 'italic', 'underline', 'fontSize', 'textColor', 'strikeThrough', 'subscript', 'superscript', 'fontFamily', , 'backgroundColor', 'inlineClass', 'inlineStyle', 'clearFormatting'],
                    align: 'left',
                    buttonsVisible: 5
                },
                moreParagraph: {
                    buttons: ['align', 'formatOLSimple', 'formatOL', 'formatUL', 'paragraphFormat', 'paragraphStyle', 'lineHeight', 'outdent', 'indent', 'quote'],
                    align: 'left',
                    buttonsVisible: 6
                },
                moreRich: {
                    buttons: ['insertLink', 'insertImage', 'insertVideo', 'insertFile', 'insertTable', 'fontAwesome', 'specialCharacters', 'embedly', , 'insertHR'],
                    align: 'left',
                    buttonsVisible: 5
                },
                moreMisc: {
                    buttons: ['undo', 'redo', 'fullscreen', 'print', 'getPDF', 'spellChecker', 'selectAll', 'html', 'help'],
                    align: 'left',
                    buttonsVisible: 3
                }
            },
            toolbarButtonsXS: [['undo', 'redo'], ['bold', 'italic', 'underline']],
            imageEditButtons: ['imageReplace', 'imageRemove', '|', 'imageLink', 'linkOpen', 'linkEdit', 'linkRemove', '-', 'imageDisplay', 'imageStyle', 'imageAlt', 'imageSize'],
            quickInsertTags: [],
            quickInsertEnabled: false,
            imageUploadURL: apiBasePath + "UploadImage",
            imageUploadMethod: 'POST',
            imageMaxSize: maxImageSize * 1024 * 1024,
            imageAllowedTypes: imageAllowedTypes,
            htmlRemoveTags: ['style', 'base', 'html'],
            // Set a preloader.
            imageManagerPreloader: "/Content/Images/loader.gif",
            // Set a scroll offset (value in pixels).
            imageManagerScrollOffset: 10,
            // Set the load images request URL.
            imageManagerLoadURL: apiBasePath + "LoadImages",
            // Set the load images request type.
            imageManagerLoadMethod: "GET",
            // Set the delete image request URL.
            imageManagerDeleteURL: apiBasePath + "DeleteImage",
            // Set the delete image request type.
            imageManagerDeleteMethod: "DELETE",
            // Additional delete params.
            imageManagerDeleteParams: { param: 'value' },
            videoUploadParam: 'file',
            // Set the video upload URL.
            videoUploadURL: apiBasePath + "UploadVideo",
            // Set request type.
            videoUploadMethod: 'POST',
            // Set max video size to 50MB.
            videoMaxSize: maxVideoSize * 1024 * 1024,
            // Allow to upload MP4, WEBM and OGG
            videoAllowedTypes: videoAllowedTypes,

            // Set the file upload parameter.
            fileUploadParam: 'file',

            // Set the file upload URL.
            fileUploadURL: apiBasePath + 'UploadFile',

            // Set request type.
            fileUploadMethod: 'POST',

            // Set max file size to 20MB.
            fileMaxSize: maxFileSize * 1024 * 1024,

            // Allow to upload any file.
            fileAllowedTypes: fileAllowedTypes,
            events: {
                'video.error': function (error, response) {
                    // Video too text-large.
                    if (error.code == 5) {
                        alert('Error uploading video. Max allowed file size is: ' + maxVideoSize + "MB.");
                    }
                    // Invalid video type.
                    else if (error.code == 6) {
                        alert('Error uploading video. Type not supported. Allowed:  ' + videoAllowedTypes);
                    }
                    // Video can be uploaded only to same domain in IE 8 and IE 9.
                    else if (error.code == 7) { }
                },
                'image.error': function (error, response) {

                    if (error.code == 5) {
                        alert('Error uploading image. Max allowed file size is: ' + maxImageSize + "MB.");
                    }
                    else if (error.code == 6) {
                        alert('Error uploading image. Type not supported. Allowed:  ' + imageAllowedTypes);
                    }
                },
                'file.error': function (error, response) {

                    if (error.code == 5) {
                        alert('Error uploading file. Max allowed file size is: ' + maxFileSize + "MB.");
                    }
                    else if (error.code == 6) {
                        alert('Error uploading file. Type not supported. Allowed:  ' + fileAllowedTypes);
                    }
                },
                'blur': function () {
                    $(instanceId).val(this.html.get());
                    $(instanceId).trigger('change');
                }
            }
        };
    };

    var defaultPreset = function (instanceId) {
        return {
            enter: FroalaEditor.ENTER_P,
            imageUpload: false,
            videoUpload: false,
            fileUpload: false,
            key: froalaLicenseKey,
            zIndex: 2501,
            attribution: false,
            toolbarButtons: {
                moreText: {
                    buttons: ['bold', 'italic', 'underline', 'fontSize', 'textColor', 'strikeThrough', 'subscript', 'superscript', 'fontFamily', , 'backgroundColor', 'inlineClass', 'inlineStyle', 'clearFormatting'],
                    align: 'left',
                    buttonsVisible: 5
                },

                moreParagraph: {
                    buttons: ['align', 'formatOLSimple', 'formatOL', 'formatUL', 'paragraphFormat', 'paragraphStyle', 'lineHeight', 'outdent', 'indent', 'quote'],
                    align: 'left',
                    buttonsVisible: 6
                },

                moreRich: {
                    buttons: ['insertLink', 'insertTable', 'insertImage', 'fontAwesome', 'specialCharacters', 'embedly', 'insertHR'],
                    align: 'left',
                    buttonsVisible: 4
                },

                moreMisc: {
                    buttons: ['undo', 'redo', 'fullscreen', 'print', 'getPDF', 'spellChecker', 'selectAll', 'html', 'help'],
                    align: 'right',
                    buttonsVisible: 2
                }
            },
            toolbarButtonsXS: [['undo', 'redo'], ['bold', 'italic', 'underline']],
            imageInsertButtons: ['imageBack', '|', 'imageByURL'],
            imageEditButtons: [],
            quickInsertEnabled: false,
            quickInsertTags: [],
            htmlRemoveTags: ['style', 'base', 'html'],
            events: {
                'blur': function () {
                    $(instanceId).val(this.html.get());
                    $(instanceId).trigger('change');
                }
            }
        };
    };

    $.each(htmlEditors,
        function () {
            var textarea = $(this);
            var preset = textarea.data('htmleditor-preset');
            var selectedPreset = defaultPreset;
            if (preset == undefined) {
                selectedPreset = defaultPreset;
            }
            else {
                selectedPreset = eval("" + preset + "");
            }
            var id = '#' + textarea.attr('id');
            if (id) {
                var editor = $(id)[0]['data-froala.editor'];

                if (editor && editor.html) {
                    $(id)[0]['data-froala.editor'].html.set(textarea.val());
                }
            }
            editor = new FroalaEditor(id, selectedPreset(id));
        });
}