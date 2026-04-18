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
    $('.visibleDesktop').removeClass('text-success text-dark');
    $('.visibleTablet').removeClass('text-success text-dark');
    $('.visibleSmartphone').removeClass('text-success text-dark');
    $('.contentBuilderDeviceRow').removeClass('d-block d-xl-block d-md-block');
    $('.content-page-rows-responsive').removeClass("col-12 col-6 col-2");
    $('.content-page-rows-responsive').css("left", "50%"); //center rows
    $('.content-page-rows-responsive').css("transform", "translate(-50%, -0.1%)"); //center rows
    if (IsCurrentlyVisible("desktop")) {
        $('link[href="/Content/contentbuilder-mobile.less"]').attr("href", "/Content/contentbuilder-desktop.less");
        $('link[href="/Content/contentbuilder-tablet.less"]').attr("href", "/Content/contentbuilder-desktop.less");
        $("[data-status-visiblility-desktop=true]").show();
        $('.visibleDesktop').addClass('text-success');
        $('.content-page-rows-responsive').addClass("col-12");
        $('.contentBuilderDeviceRow').addClass("d-xl-block");
        $('.smallImages').addClass('d-none');
        $('.bigImages').removeClass('d-none');
    } else {
        $('.visibleDesktop').addClass('text-dark');
    }
    if (IsCurrentlyVisible("tablet")) {
        $('link[href="/Content/contentbuilder-desktop.less"]').attr("href", "/Content/contentbuilder-tablet.less");
        $('link[href="/Content/contentbuilder-mobile.less"]').attr("href", "/Content/contentbuilder-tablet.less");
        $("[data-status-visiblility-tablet=true]").show();
        $('.visibleTablet').addClass('text-success');
        $('.content-page-rows-responsive').addClass("col-6");
        $('.contentBuilderDeviceRow').addClass("d-md-block");
        $('.smallImages').addClass('d-none');
        $('.bigImages').removeClass('d-none');
    } else {
        $('.visibleTablet').addClass('text-dark');
    }
    if (IsCurrentlyVisible("mobile")) {
        $('link[href="/Content/contentbuilder-desktop.less"]').attr("href", "/Content/contentbuilder-mobile.less");
        $('link[href="/Content/contentbuilder-tablet.less"]').attr("href", "/Content/contentbuilder-mobile.less");
        $("[data-status-visiblility-mobile=true]").show();
        $('.visibleSmartphone').addClass('text-success');
        $('.content-page-rows-responsive').addClass("col-3");
        $('.contentBuilderDeviceRow').addClass("d-block");
        $('.smallImages').removeClass('d-none');
        $('.bigImages').addClass('d-none');
    } else {
        $('.visibleSmartphone').addClass('text-dark');
    }
}

$(window).resize(function () {
    SetPanelHeight();
});

$(document).ready(function () {

    hideMenu();
    ChangeLayoutContentBuilder();
    SetPanelHeight();
    InitBootstrap();
    InitSortable();
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
                InitTextEditor();
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
                InitTextEditor();
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