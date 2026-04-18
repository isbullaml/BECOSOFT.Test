function bindArticleInfoPopoverEvents() {
        $(".article-popover-loading").remove();
    $("*[data-poload]").on("click", function () {
        var e = $(this);
        e.append("<i class='fas fa-sync spinning article-popover-loading'></i>");
        e.unbind("click");
        becosoft.ajax(e.data("poload"), { type: "GET" }, function (result) {
            e.popover({
                content: result,
                html: true,
                sanitize:false,
                placement: "auto",
                container: "body",
                trigger: "click",
                template: '<div class="popover" role="tooltip"><div class="arrow"></div><h3 class="popover-header"></h3><div class="popover-body p-0"></div></div>'
            }).popover("show");
        });
    }).on("show.bs.popover", function (e) {
        $("[rel=popover]").not(e.target).removeData('bs.popover').popover("destroy");
        $(".popover").remove();
        $(".article-popover-loading").remove();
    }).on("shown.bs.popover", function (e) {
        var popover = $("#" + $(e.target).attr("aria-describedby"));
        popover.css("z-index", "11");
        popover[0].scrollIntoView(false);
        $('span[rel=popoverImage]').popover({
            html: true,
            trigger: 'hover',
            placement: 'right',
            content: function () { return '<img height="300" src="' + $(this).data('imguri') + '" alt="Image not available" />'; }
        });
    });
}

//Close popover on click
$(document).on('click', function (e) {
    $('[data-toggle="popover"], [data-original-title]').each(function () {
        //the 'is' for buttons that trigger popups
        //the 'has' for icons within a button that triggers a popup
        if (!$(this).is(e.target) && $(this).has(e.target).length === 0 && $('.popover').has(e.target).length === 0) {
            (($(this).popover('hide').data('bs.popover') || {}).inState || {}).click = false;  // fix for BS 3.3.6
        }
    });
});