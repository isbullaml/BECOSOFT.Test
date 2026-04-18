$("#paginationForm a.page-link").on("click", function (e) {
    var btn = $(this);
    var value = btn.attr("value");
    var selectedPage = parseInt(value);
    if (selectedPage < 0) {
        e.preventDefault();
        return false;
    }
    var originalPageSize = parseInt($("#PaginationBlock_OriginalPageSize").val());
    var currentPageSize = parseInt($("#PaginationBlock_PageSize").val());
    if (currentPageSize !== originalPageSize) {
        selectedPage = 0;
    }

    $("#PaginationBlock_Page").val(selectedPage);
    $("#paginationForm").submit();
    return false;
});

function onPageSizeChange() {
    $("#PaginationBlock_Page").val(0);
    $("#paginationForm").submit();
}

$("#sortFieldForm a[type=sort]").on("click", function(e) {
    $(this).attr("data-clicked", true);
    $("#sortFieldForm").submit();
});

$("#sortFieldForm").on("submit",
    function (e) {
        var btn = $(this).closest_descendent("a[data-clicked='true']");
        var value = btn.attr("value");
        if (value === "") {
            e.preventDefault();
            return false;
        }
        var originalVal = $("#SortField").val();
        $("#PaginationBlock_SortField").val(originalVal);
        $("#SearchBlock_SortField").val(originalVal);
        if (originalVal === value) {
            var sortVal = $("#SortOrder").val();
            if (sortVal === "1") {
                sortVal = "0";
            } else {
                sortVal = "1";
            }
            $("#SortOrder").val(sortVal);
            $("#PaginationBlock_SortOrder").val(sortVal);
            $("#SearchBlock_SortOrder").val(sortVal);
        } else {
            $("#SortField").val(value);
            $("#PaginationBlock_SortOrder").val(value);
            $("#SearchBlock_SortOrder").val(value);
        }
        return true;
    });