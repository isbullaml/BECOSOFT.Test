function addTopHorizontalScrollbar(table) {

    if (table.length === 0) {
        return;
    }

    // set up scrollbar and table-responsive wrapper
    var topScrollbar = `<div class="bcs-top-horizontal-scrollbar"><div></div></div>`;
    if (table.parent().hasClass("table-responsive") === false) {
        table.wrap(`<div class="table-responsive"></div>`);
    }
    table.parent().before(topScrollbar);

    // redefine for uniqueness to allow multiple on page
    topScrollbar = table.parent().prev();
    topScrollbar.find("div").width(table.width());
    var tableResponsiveContainer = $(topScrollbar).next();

    // link scrollbars
    $(topScrollbar).on("scroll", function () {
        $(tableResponsiveContainer).scrollLeft($(topScrollbar).scrollLeft());
    });
    $(tableResponsiveContainer).on("scroll", function () {
        $(topScrollbar).scrollLeft($(tableResponsiveContainer).scrollLeft());
    });

    // observe size changes
    const resizeObserver = new ResizeObserver(function (e) {
        setTopHorizontalScrollbarWidth(topScrollbar);
    });
    resizeObserver.observe($(table)[0]);
}

function setTopHorizontalScrollbarWidth(scrollbar) {
    var table = scrollbar.next().find("table");
    $(scrollbar).find("div").width(table.width());
}