//Contentbuilder change layout by device click, save status last clicked device to localstorage
function clickDevice(deviceType) {
    var isVisible = IsCurrentlyVisible(deviceType);
    if (isVisible) {
        return;
    }

    localStorage.setItem("Visibledesktop", false);
    localStorage.setItem("Visibletablet", false);
    localStorage.setItem("Visiblemobile", false);
    var localstorageName = "Visible" + deviceType;
    localStorage.setItem(localstorageName, !isVisible);

    ChangeLayoutContentBuilder();
}

//Get and set status last clicked device to localstorage
function IsCurrentlyVisible(deviceType) {
    var localstorageName = "Visible" + deviceType;
    var isVisible = localStorage.getItem(localstorageName);
    if (isVisible === null) {
        if (deviceType === "desktop") {
            localStorage.setItem(localstorageName, true);
        } else {
            localStorage.setItem(localstorageName, false);
        }
    }
    return localStorage.getItem(localstorageName) === "true";
}

//Change layout screen depending on device click, style color devices
// Depending which device clicked = change styling file
function ChangeLayoutContentBuilder() {
    $("[data-status-visiblility-desktop]").hide();
    $("[data-status-visiblility-tablet]").hide();
    $("[data-status-visiblility-mobile]").hide();
    $('.visibleDesktop').removeClass('bcs-opacity-50');
    $('.visibleTablet').removeClass('bcs-opacity-50');
    $('.visibleSmartphone').removeClass('bcs-opacity-50');
    $('.contentBuilderDeviceRow').removeClass('d-block d-xl-block d-md-block');
    $('.content-page-rows-responsive').removeClass("col-12 col-6 col-2");
    $('.content-page-rows-responsive').css("left", "50%"); //center rows
    $('.content-page-rows-responsive').css("transform", "translate(-50%, -0.1%)"); //center rows
    if (IsCurrentlyVisible("desktop")) {
        $('link[href="/Content/contentbuilder-mobile.less"]').attr("href", "/Content/contentbuilder-desktop.less");
        $('link[href="/Content/contentbuilder-tablet.less"]').attr("href", "/Content/contentbuilder-desktop.less");
        $("[data-status-visiblility-desktop=true]").show();

        $('.visibleTablet').removeClass('bcs-opacity-100');
        $('.visibleSmartphone').removeClass('bcs-opacity-100');

        $('.visibleDesktop').addClass('bcs-opacity-100');
        $('.content-page-rows-responsive').addClass("col-12");
        $('.contentBuilderDeviceRow').addClass("d-xl-block");
        $('.smallImages').addClass('d-none');
        $('.bigImages').removeClass('d-none');
    } else {
        $('.visibleDesktop').addClass('bcs-opacity-50');
    }
    if (IsCurrentlyVisible("tablet")) {
        $('link[href="/Content/contentbuilder-desktop.less"]').attr("href", "/Content/contentbuilder-tablet.less");
        $('link[href="/Content/contentbuilder-mobile.less"]').attr("href", "/Content/contentbuilder-tablet.less");
        $("[data-status-visiblility-tablet=true]").show();

        $('.visibleDesktop').removeClass('bcs-opacity-100');
        $('.visibleSmartphone').removeClass('bcs-opacity-100');

        $('.visibleTablet').addClass('bcs-opacity-100');
        $('.content-page-rows-responsive').addClass("col-6");
        $('.contentBuilderDeviceRow').addClass("d-md-block");
        $('.smallImages').addClass('d-none');
        $('.bigImages').removeClass('d-none');
    } else {
        $('.visibleTablet').addClass('bcs-opacity-50');
    }
    if (IsCurrentlyVisible("mobile")) {
        $('link[href="/Content/contentbuilder-desktop.less"]').attr("href", "/Content/contentbuilder-mobile.less");
        $('link[href="/Content/contentbuilder-tablet.less"]').attr("href", "/Content/contentbuilder-mobile.less");
        $("[data-status-visiblility-mobile=true]").show();

        $('.visibleDesktop').removeClass('bcs-opacity-100');
        $('.visibleTablet').removeClass('bcs-opacity-100');

        $('.visibleSmartphone').addClass('bcs-opacity-100');
        $('.content-page-rows-responsive').addClass("col-3");
        $('.contentBuilderDeviceRow').addClass("d-block");
        $('.smallImages').removeClass('d-none');
        if ($('.smallImages').length > 0) {
            $('.bigImages').addClass('d-none');
        }

    } else {
        $('.visibleSmartphone').addClass('bcs-opacity-50');
    }
}

//Copy a row
$('.copy-row').on("click", function () {
    $('#loader').show();
});

$(window).on("resize", function () {
    SetPanelHeight();
});

function SetOriginalImg() {
    var docRow = document.getElementsByClassName("elementRowPosition");
    for (var j = 0; j < docRow.length; j++) {
        var originImg = docRow[j].querySelector('.originalImage');

        if (originImg !== null) {
            docRow[j].querySelector('.imageDisplay').setAttribute('src', originImg.getAttribute('value'));
            docRow[j].querySelector('.imageDisplay').setAttribute('srcset', originImg.getAttribute('value'));
        }
    }
}

//Click empty row = active tab Elements
function clickEmptyRow() {
    $("#nav-rows-tab").removeClass("active");
    $("#nav-element-tab").removeClass("active show");
    $("#nav-rows").removeClass("active show");
    $("#nav-element").removeClass("active show");
    $("#nav-elements-tab").addClass("active");
    $("#elements").addClass("active show");
}

$(function () {
    clickEmptyRow();
    hideMenu();
    ChangeLayoutContentBuilder();

    $('.picture a').on("click", function (e) {
        e.preventDefault();
    });

    SetPanelHeight();
    InitBootstrap();
    InitSortable();

    $("#body-content").show();
    $('#loader').hide();
});

// Add row
$(document).on("click", ".add-row", function (e) {
    e.stopPropagation();
    e.preventDefault();
    var add = $(this).data('container');
    $(add).popover('show');
    var row = $(this).closest('.content-row');
    var rowid;
    if (row.length > 0) {
        rowid = $(row).data('row-id');
    }
    var position = $(row).index() + 2;
    var structureRows = $('.row-structure');
    for (var i = 0; i < structureRows.length; i++) {
        var url = structureRows[i].getAttribute('href');
        var value = url + '&position=' + position + '&rowid=' + rowid;
        structureRows[i] = structureRows[i].setAttribute('href', value);
    };
});

$('body').on('click', function (e) {
    //did not click a popover toggle or popover
    if ($(e.target).data('toggle') !== 'popover'
        && $(e.target).parents('.popover.in').length === 0) {
        $('[data-toggle="popover"]').popover('hide');
    }
});

// Add new row
$(document).on("click", ".row-structure", function (e) {
    e.preventDefault();
    var $this = $(this);
    var url = $this.attr('href');

    var searchParams = new URLSearchParams(url);
    var rowid = 0;
    if (searchParams.has('rowid') === true) {
        rowid = searchParams.get('rowid');
    }

    $.ajax({
        type: "POST",
        url: url,
        beforeSend: function () { $('#loader').show(); },
        success: function (resp) {
            if (rowid === 0) {
                var rows = $("#content-page-rows").find("#rows");
                $(resp).appendTo($(rows));
                LoadContentPanel($("#rows .content-row:last-child"), null, true);
            }
            else {
                $(resp).insertAfter("#rows .content-row[data-row-id=" + rowid + "]");
                LoadContentPanel($("#rows .content-row[data-row-id=" + rowid + "]"), null, true);
            }
            $('[data-toggle="popover"]').popover('hide');
            InitBootstrap();
            $('#loader').hide();
        },
        error: function () {
            $('#loader').hide();
            alert("Er ging iets fout bij het toevoegen van een nieuwe rij.");
        }
    });
});

// Bootstrap intialisers
var InitBootstrap = function () {
    $('[data-toggle="popover"]').popover();
};

/* GENERAL */
var SetPanelHeight = function () {
    var tabContent = $("#content-page-panel").find(".tab-content");
    var height = $(window).height() - 265;
    $(tabContent).css("height", height);
    $('.select2').select2();

    initAmsifySuggestags();
};

var LoadContentPanel = function (contentRow, contentElement, newRow) {
    var contentID = 0;
    var websiteID = 0;
    var localizeID = 0;
    var languageID = 0;
    // If there is no row selected
    if (contentRow === null) {

        var $this = $("#rows");
        $.ajax({
            type: "GET",
            url: $('#rows').data("panel-url"),
            beforeSend: function () { $('#loader').show(); },
            success: function (resp) {
                $("#content-page-panel").empty().append(resp);
                InitSortable();
                SetPanelHeight();
                InitBootstrap();
                $('#loader').hide();
                clickEmptyRow();
            },
            error: function () {
                $('#loader').hide();
            }
        });
    }
    // Element is selected
    else if (contentElement != null) {
        contentID = $(contentRow).data("content-id");
        websiteID = $(contentRow).data("website-id");
        localizeID = $(contentRow).data("localize-id");
        languageID = $(contentRow).data("language-id");
        var $this = $(contentElement);
        var contentRowID = $(contentRow).data("row-id");
        var contentElementID = $($this).data("id");
        var elementType = $($this).data("type");
        $.ajax({
            type: "GET",
            url: '/WebsiteContent/ContentPanel/?WebsiteID=' + websiteID + '&ContentID=' + contentID + '&LocalizeID=' + localizeID + '&LanguageID=' + languageID + '&ContentElementID=' + contentElementID + '&ContentRowID=' + contentRowID + '&ContentElementType=' + elementType,
            beforeSend: function () { $('#loader').show(); },
            success: function (resp) {
                if ($('#content-page-panel').find('.search-new').length) {
                    $(resp).find(".panel-search").collapse("show");
                }
                $("#content-page-panel").empty().append(resp);
                $("#rows .content-row").removeClass("active");
                $(contentRow).addClass("active");
                $("#rows .content-row .element").removeClass("active");
                $this.addClass("active");
                InitSortable();
                enableHtmlEditor();
                InitImageUpload();
                SetPanelHeight();
                $('#loader').hide();
                $('#collapseTitle2').addClass('collapse');
            },
            error: function () {
                $('#loader').hide();
                alert("Er ging iets fout bij het selecteren van het element.");
            }
        });
    }
    // Row is selected
    else {
        contentID = $(contentRow).data("content-id");
        websiteID = $(contentRow).data("website-id");
        localizeID = $(contentRow).data("localize-id");
        languageID = $(contentRow).data("language-id");
        var $this = $(contentRow);
        var contentRowID = $(contentRow).data("row-id");
        $.ajax({
            type: "GET",
            url: '/WebsiteContent/ContentPanel/?WebsiteID=' + websiteID + '&ContentID=' + contentID + '&LocalizeID=' + localizeID + '&LanguageID=' + languageID + '&ContentRowID=' + contentRowID,
            beforeSend: function () { $('#loader').show(); },
            success: function (resp) {
                $("#content-page-panel").empty().append(resp);
                $("#rows .content-row").removeClass("active");
                $("#rows .content-row .element").removeClass("active");
                $this.addClass("active");
                InitSortable();
                enableHtmlEditor();
                InitImageUpload();
                SetPanelHeight();
                $('#loader').hide();
                if (newRow) {
                    // select elements tab
                    $('a[href="#elements"]').tab('show');
                }
            },
            error: function () {
                $('#loader').hide();
                alert("Er ging iets fout bij het selecteren van de rij.");
            }
        });
    }
};

// Update a specific row in the rows overview when the contentpanel is updated
function UpdateContentPanel_OnSuccess(resp) {
    var updatedRowID = $(resp).data("row-id");
    // Validation errors
    if (typeof updatedRowID === "undefined") {
        $("#content-page-panel").find(".body").replaceWith(resp);
        SetPanelHeight();
    }
    else {
        $("#rows").find(".content-row[data-row-id='" + updatedRowID + "']").replaceWith(resp);
        InitSortable();
        $('.field-validation-error').text('');
    }
    ChangeLayoutContentBuilder();
}

// Update a specific row in the rows overview when the contentpanel is updated
function UpdateContentPanel_OnError(resp) {
    alert("Er ging iets fout bij het opslaan.");
    InitSortable();
}

$(function () {
    sessionStorage.setItem('ContentPanelChanged', false);
    sessionStorage.setItem('ContentChangedID', "");
    $('#content-page-panel').on('input', function () {
        sessionStorage.setItem('ContentChangedID', $(this).attr("id"));
        sessionStorage.setItem('ContentPanelChanged', true);
    });
    $('#content-page-panel').on('submit', function () {
        sessionStorage.setItem('ContentChangedID', "");
        sessionStorage.setItem('ContentPanelChanged', false);
    });
});

// Select row from the options
$(document).on("click", ".edit-row", function (e) {
    e.preventDefault();
    LoadContentPanel($(".content-row[data-row-id='" + $(this).data("row-id") + "']"));
});

// Select a row and load the options panel
$(document).on("click", "#rows .content-row", function (e) {
    var inputChanged = sessionStorage.getItem('ContentPanelChanged');
    if (inputChanged === 'true') {
        var confirmed = confirm('1 of meerdere velden zijn veranderd, bent u zeker dat u wilt verdergaan?');
        if (confirmed) {
            sessionStorage.setItem('ContentPanelChanged', false);
            e.target.click();
        } else {
            var elementID = sessionStorage.getItem('ContentChangedID');
            $('#' + elementID).on("focus");
            $('#' + elementID).on("select");
        }
        return;
    } else {
        if (e.target !== e.currentTarget) return;
        e.stopPropagation();
        e.preventDefault();
        LoadContentPanel(this);
    }
});

$(document).on("click", ".new-row", function (e) {
    e.stopPropagation();
    e.preventDefault();
});

// Delete a row
$(document).on("click", ".delete-row", function (e) {
    e.stopPropagation();
    e.preventDefault();
    if (confirm('Weet u zeker dat u deze rij wenst te verwijderen?')) {
        var $this = $(this);
        $.ajax({
            type: "POST",
            url: $this.attr('href'),
            beforeSend: function () { $('#loader').show(); },
            success: function (resp) {
                var rowid = $this.closest(".content-row").data('row-id');
                $(".content-row[data-row-id=" + rowid + "]").remove();
                LoadContentPanel(null);
                InitSortable();
                $('#loader').hide();
            },
            error: function () {
                $('#loader').hide();
                alert("Er ging iets fout bij het verwijderen van deze rij.");
            }
        });
    }
});

// Select an element and load the options panel
$(document).on("click", "#rows .content-row .element", function (e) {
    var inputChanged = sessionStorage.getItem('ContentPanelChanged');
    if (inputChanged === 'true') {
        return;
    } else {
        e.stopPropagation();
        e.preventDefault();
        var row = $(this).closest(".content-row");
        LoadContentPanel(row, this);
    }
});

// Select a device structure in the content panel row options
$(document).on("click", ".device-structures .device-structure", function (e) {
    e.preventDefault();
    var $this = $(this);
    var parent = $this.parent().parent();
    var ddl = $(parent).find("select");
    $(ddl).val($this.data("structure-value"));
    $(parent).find(".device-structure").removeClass("active");
    $this.addClass("active");
});


// Delete an element
$(document).on("click", ".delete-element", function (e) {
    e.stopPropagation();
    e.preventDefault();
    if (confirm('Weet u zeker dat u dit element wenst te verwijderen?')) {
        var $this = $(this);
        var rowID = $this.closest(".content-row").data('row-id');
        $.ajax({
            type: "POST",
            url: $this.attr('href'),
            beforeSend: function () { $('#loader').show(); },
            success: function (resp) {
                $("#rows").find(".content-row[data-row-id='" + rowID + "']").replaceWith(resp);
                LoadContentPanel(null, null);
                InitSortable();
                $('#loader').hide();
            },
            error: function () {
                $('#loader').hide();
                alert("Er ging iets fout bij het verwijderen van dit element.");
            }
        });
    }
});

// All sortable events
var InitSortable = function () {

    // Change position of row - JQUERY UI
    $("#rows").sortable({
        handle: ".drag-handle",
        placeholder: "ui-state-highlight",
        axis: 'y',
        revert: true,
        cursorAt: {},
        start: function (evt, ui) {
            $(ui.item).addClass("active");
        },
        stop: function (evt, ui) {

        },
        update: function (evt, ui) {
            $.ajax({
                type: "POST",
                url: '/WebsiteContent/ChangeRowPosition/?WebsiteID=' + $(ui.item).data("website-id") + '&languageID=' + $(ui.item).data("language-id") + '&ContentID=' + $(ui.item).data("content-id") + '&ContentRowID=' + $(ui.item).data("row-id") + "&newPosition=" + (parseInt(ui.item.index()) + 1),
                beforeSend: function () { $('#loader').show(); },
                success: function (resp) {
                    LoadContentPanel(ui.item, null);
                    $('#loader').hide();
                },
                error: function (e) {
                    $('#loader').hide();
                    if (e.status.error)
                        alert("Er ging iets fout bij het wijzigen van de positie.");
                    return false;
                }
            });
        }
    }).disableSelection();

    $("#elements").sortable({

        placeholder: "ui-state-highlight",
        connectWith: ".col",
        helper: "clone",
        appendTo: "body",
        items: ".panel-element",
        revert: true,
        cursorAt: {},
        tolerance: "pointer",
        start: function (evt, ui) {
            $(ui.item).addClass("active");
        },
        over: function (evt, ui) {
            $(ui.item).addClass("active");
        },
        update: function (evt, ui) {
            if ($(ui.item).parent().parent().attr('id') === "elements") {
                LoadContentPanel(null, null);
                return;
            }

            $(ui.item).parent().find(".placeholder").remove();
            var colPosition = parseInt($(ui.item).parent().data("col-position")) + 1;
            var rowID = $(ui.item).parent().closest(".content-row").data("row-id");
            var row = $(ui.item).parent().closest(".content-row");
            var col = $(ui.item).parent();
            var position = parseInt($(ui.item).index() + 1);
            $.ajax({
                type: "POST",
                url: $(ui.item).data("url") + "&columnPosition=" + colPosition + "&contentRowID=" + rowID + "&position=" + position,
                beforeSend: function () { $('#loader').show(); },
                success: function (resp) {
                    if (position === 1) {
                        if ($(ui.item).parent().find(".element").length === 0) {
                            $(ui.item).parent().empty().append(resp);
                        }
                        else {
                            $(ui.item).parent().find(".element").before(resp);
                        }

                    } else {
                        $(ui.item).parent().find(".element").eq(position - 2).after(resp);
                    }

                    $(col).find(".panel-element").remove();
                    $(col).removeClass("empty pr-0 pl-0");

                    LoadContentPanel(row, $(resp));
                    SetElementPositions(null, $(col));
                },
                error: function () {
                    $('#loader').hide();
                    alert("Er ging iets fout bij het toevoegen van het element.");
                    return false;
                }
            });

            $('#loader').hide();
        }
    }).disableSelection();

    $(".col").sortable({

        placeholder: "ui-state-highlight",
        connectWith: ".col",
        helper: "clone",
        items: ".element",
        tolerance: "pointer",
        revert: true,
        start: function (evt, ui) {
            $(ui.item).addClass("active");
        },
        stop: function (evt, ui) {

        },
        update: function (evt, ui) {
            if ($(ui.item).hasClass("panel-element")) {
                return;
            }

            if ($(ui.item).parent().parent().attr('id') === "elements") {
                LoadContentPanel(null, null);
                return;
            }
            $('#loader').show();

            if ($(ui.item).parent().find(".placeholder").length) {
                $(ui.item).parent().find(".placeholder").remove();
                $(ui.item).parent().find(".panel-element").remove();
                $(ui.item).parent().removeClass("empty pr-0 pl-0");
            }

            //$(ui.item).removeClass("active");
            LoadContentPanel(null, null);
            SetElementPositions($(ui.sender), $(ui.item).parent());
        }
    }).disableSelection();
};

var SetElementPositions = function (col1, col2) {
    var jsonObj = [];

    if (col1 !== null) {
        var row1Id = $(col1).closest('.content-row').data('row-id');
        $(col1).find('.element').each(function () {
            item = {};
            item["RowId"] = row1Id;
            item["ElementId"] = $(this).data('id');
            item["Type"] = $(this).data('type');
            item["Position"] = parseInt($(this).index()) + 1;
            item["ColumnPosition"] = parseInt($(this).closest('.col').data("col-position")) + 1;

            jsonObj.push(item);
        });
    }

    if (col2 !== null) {
        var row2Id = $(col2).closest('.content-row').data('row-id');
        $(col2).find('.element').each(function () {
            item = {};
            item["RowId"] = row2Id;
            item["ElementId"] = $(this).data('id');
            item["Type"] = $(this).data('type');
            item["Position"] = parseInt($(this).index()) + 1;
            item["ColumnPosition"] = parseInt($(this).closest('.col').data("col-position")) + 1;

            jsonObj.push(item);
        });
    }

    var jsonString = JSON.stringify(jsonObj);
    $.ajax({
        type: "POST",
        url: "/WebsiteContent/ChangeElementPositions?websiteId=" + $("#WebsiteID").val() + "&ContentID=" + $("#ContentID").val() + "&LanguageID=" + $("#LanguageID").val() + "&positionInfoJson=" + jsonString,
        beforeSend: function () { $('#loader').show(); },
        success: function (resp) {
            $('#loader').hide();
        },
        error: function (resp) {
            if (resp.status.error)
                alert("Er ging iets fout bij het wijzigen van de element posities.");
            return false;
        }
    });
};

var InitImageUpload = function () {

    $("[id$='_ImageUpload']", '.websiteContentImageParent').on("change", function (e) {
        var parent = $(this).closest('.websiteContentImageParent');
        e.preventDefault();
        var input = $(this);
        var file = input.get(0).files[0];
        var preview = $(parent).find("#exampleImage");
        var reader = new FileReader();
        ImageExtensionValidator(file, input);

        $(parent).find("#undo-delete-image-button").prop("disabled", true);

        reader.onloadend = function () {
            preview.attr("src", reader.result);
            preview.removeClass("d-none");
            $(parent).find("#DeleteImage").val(false);
            $(parent).find("#delete-image-button").prop("disabled", false);
            var oldSrc = $(parent).find("[id$='_ImageAssetPath']").val();
            if (oldSrc?.length > 0) {
                $(parent).find("#undo-delete-image-button").prop("disabled", false);
            }
        };

        if (file) {
            reader.readAsDataURL(file);
        } else {
            preview.src = "";
        }
    });

    $("#delete-image-button", '.websiteContentImageParent').on("click", function (e) {
        var parent = $(this).closest('.websiteContentImageParent');
        e.preventDefault();

        var logoUpload = $(parent).find("#WebsiteContentImage_ImageUpload");
        var preview = $(parent).find("#exampleImage");

        logoUpload.val("");
        preview.attr("src", "");
        preview.addClass("d-none");
        $(parent).find("[id$='DeleteImage']", '.websiteContentImageParent').val(true);
        $(parent).find("#delete-image-button").prop("disabled", true);
        sessionStorage.setItem('ContentPanelChanged', true);
        sessionStorage.setItem('ContentChangedID', "delete-image-button");
    });

    $("#undo-delete-image-button", '.websiteContentImageParent').on("click", function (e) {
        var parent = $(this).closest('.websiteContentImageParent');
        e.preventDefault();

        var logoUpload = $(parent).find("#WebsiteContentImage_ImageUpload");
        var preview = $(parent).find("#exampleImage");
        var oldSrc = $(parent).find("#ImageAssetPath").val();

        logoUpload.val("");
        $(parent).find("#undo-delete-image-button").prop("disabled", true);
        if (oldSrc.length > 0) {
            preview.attr("src", oldSrc);
            preview.removeClass("d-none");
            $(parent).find("#delete-image-button").prop("disabled", false);
        }

        $(parent).find("#DeleteImage").val(false);
    });

    $("[id$='_ImageMobileUpload']", '.websiteContentImageParent').on("change", function (e) {
        var parent = $(this).closest('.websiteContentImageParent');
        e.preventDefault();
        var input = $(this);
        var file = input.get(0).files[0];
        var preview = $(parent).find("#exampleImageMobile");
        var reader = new FileReader();
        ImageExtensionValidator(file, input);

        $(parent).find("#undo-delete-imagemobile-button").prop("disabled", true);

        reader.onloadend = function () {
            preview.attr("src", reader.result);
            preview.removeClass("d-none");
            $(parent).find("#DeleteImageMobile").val(false);
            $(parent).find("#delete-imagemobile-button").prop("disabled", false);
        };

        if (file) {
            reader.readAsDataURL(file);
        } else {
            preview.src = "";
        }
    });

    $("#delete-imagemobile-button", '.websiteContentImageParent').on("click", function (e) {
        var parent = $(this).closest('.websiteContentImageParent');
        e.preventDefault();

        var logoUpload = $(parent).find("#WebsiteContentImage_ImageMobileUpload");
        var preview = $(parent).find("#exampleImageMobile");

        logoUpload.val("");
        preview.attr("src", "");
        preview.addClass("d-none");
        $(parent).find("[id$='DeleteImageMobile']", '.websiteContentImageParent').val(true);
        $(parent).find("#delete-imagemobile-button").prop("disabled", true);
        sessionStorage.setItem('ContentPanelChanged', true);
        sessionStorage.setItem('ContentChangedID', "delete-imagemobile-button");
    });

    $("#undo-delete-imagemobile-button", '.websiteContentImageParent').on("click", function (e) {
        var parent = $(this).closest('.websiteContentImageParent');
        e.preventDefault();

        var logoUpload = $(parent).find("#WebsiteContentImage_ImageMobileUpload");
        var preview = $(parent).find("#exampleImageMobile");
        var oldSrc = $(parent).find("#ImageMobileAssetPath").val();

        logoUpload.val("");
        $(parent).find("#undo-delete-imagemobile-button").prop("disabled", true);
        if (oldSrc.length > 0) {
            preview.attr("src", oldSrc);
            preview.removeClass("d-none");
            $(parent).find("#delete-imagemobile-button").prop("disabled", false);
        }

        $(parent).find("#DeleteImageMobile").val(false);
    });

    $("[id$='_BackgroundImageUpload']", '.websiteContentText').on("change", function (e) {
        var parent = $(this).closest('.websiteContentText');
        e.preventDefault();
        var input = $(this);
        var file = input.get(0).files[0];
        var preview = $(parent).find("#exampleImage");
        var reader = new FileReader();
        ImageExtensionValidator(file, input);

        $(parent).find("#undo-delete-image-button").prop("disabled", true);

        reader.onloadend = function () {
            preview.attr("src", reader.result);
            preview.removeClass("d-none");
            $(parent).find("#DeleteImage").val(false);
            $(parent).find("#delete-image-button").prop("disabled", false);
        };

        if (file) {
            reader.readAsDataURL(file);
        } else {
            preview.src = "";
        }
    });

    $("#delete-text-button", '.websiteContentText').on("click", function (e) {
        var parent = $(this).closest('.websiteContentText');

        var logoUpload = $(parent).find("#WebsiteContentText_BackgroundImageUpload");
        var preview = $(parent).find("#exampleImage");

        logoUpload.val("");
        preview.attr("src", "");
        preview.addClass("d-none");
        $("#WebsiteContentText_ContentText_DeleteImage").val(true);
        $(parent).find("#delete-text-button").prop("disabled", true);
        sessionStorage.setItem('ContentPanelChanged', true);
        sessionStorage.setItem('ContentChangedID', "delete-text-button");
    });

    $("[id$='_BackgroundImageUpload']", '.websiteContentRow').on("change", function (e) {
        var parent = $(this).closest('.websiteContentRow');
        e.preventDefault();
        var input = $(this);
        var file = input.get(0).files[0];
        var preview = $(parent).find("#exampleImage");
        var reader = new FileReader();
        ImageExtensionValidator(file, input);

        $(parent).find("#undo-delete-image-button").prop("disabled", true);

        reader.onloadend = function () {
            preview.attr("src", reader.result);
            preview.removeClass("d-none");
            $(parent).find("#DeleteImage").val(false);
            $(parent).find("#delete-image-button").prop("disabled", false);
        };

        if (file) {
            reader.readAsDataURL(file);
        } else {
            preview.src = "";
        }
    });

    $("#delete-row-button", '.websiteContentRow').on("click", function (e) {
        var parent = $(this).closest('.websiteContentRow');

        var logoUpload = $(parent).find("#WebsiteContentRow_BackgroundImageUpload");
        var preview = $(parent).find("#exampleImage");

        logoUpload.val("");
        preview.attr("src", "");
        preview.addClass("d-none");
        $("#WebsiteContentRow_ContentRow_DeleteImage").val(true);
        $(parent).find("#delete-row-button").prop("disabled", true);
        sessionStorage.setItem('ContentPanelChanged', true);
        sessionStorage.setItem('ContentChangedID', "delete-row-button");
    });

    $('[data-id=contenttext-generate-ai]').on('click', function () {
        $('#contenttext-ai-modal').modal('show');
    });

    $('[data-id=AI-generator-generate]').on('click', function () {
        var subject = $('[data-id=AI-generator-subject]').val();
        var information = $('[data-id=AI-generator-information]').val();
        var importantKeywords = $('[data-id=AI-generator-important-keywords]').val();
        var additionalKeywords = $('[data-id=AI-generator-additional-keywords]').val();
        var tone = $('[data-id=AI-generator-tone]').val();
        var currentLanguageID = $('#languageID').val();
        var url = "/WebsiteContent/GenerateDescriptionAI/?subject=" + subject + "&information=" + information + "&importantKeywords=" + importantKeywords + "&additionalKeywords=" + additionalKeywords + "&tone=" + tone + "&languageID=" + currentLanguageID;
        
        $.ajax({
            type: "POST",
            url: url,
            beforeSend: function () { $('#loader').show(); },
            success: function (resp) {
                $('[data-id=contentbuilder-text-element] .fr-element.fr-view').text(resp.description);
                $('[data-id=contentbuilder-text-element] .fr-placeholder').text("");
                $('#loader').hide();
                $('#contenttext-ai-modal').modal('hide');
            },
            error: function () {
            }
        });
    });
};

function ImageExtensionValidator(file, input) {
    var allowedExtensions =
        /(\.jpg|\.jpeg|\.png)$/i;

    if (!allowedExtensions.exec(file.name)) {
        input.get(0).value = '';
        alert('Invalid file type');
        return false;
    }
};