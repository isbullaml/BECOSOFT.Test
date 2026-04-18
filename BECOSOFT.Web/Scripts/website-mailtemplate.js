function bindExtraTranslationEvents(editor, tab) {
    var placeholderSearch = editor.find("input[data-action='placeholder-search']");
    var placeholderRows = editor.find("li[data-action='copy']");

    placeholderSearch.on("keyup", function () {
        var input = $(this);
        var search = input.val().toLowerCase();

        $.each(placeholderRows, function () {
            var row = $(this);
            var searchTerms = row.data("search-terms").toLowerCase();
            if (searchTerms.indexOf(search) < 0) {
                row.addClass("d-none");
            } else {
                row.removeClass("d-none");
            }
        });
    });

    var textarea = editor.find("textarea");
    if (textarea) {
        textarea.on("click keydown keyup change", function () {
            let area = $(this);
            let selectionStart = area.prop("selectionStart");
            let selectionEnd = area.prop("selectionEnd");
            area.data("selection-start", selectionStart);
            area.data("selection-end", selectionEnd);
            editor.data("focused-input", area);
        });
    }

    var titleInput = editor.find("[id$='__Subject']");
    if (titleInput) {
        titleInput.on("click keydown keyup change", function () {
            let input = $(this);
            let selectionStart = input.prop("selectionStart");
            let selectionEnd = input.prop("selectionEnd");
            input.data("selection-start", selectionStart);
            input.data("selection-end", selectionEnd);
            editor.data("focused-input", input);
        });
    }

    placeholderRows.on("click", function (e) {
        var input = editor.data("focused-input");
        var placeholder = $(this).attr("data-placeholder");
        var isCondition = $(this).attr("data-condition") === 'True';
        var start = input.data("selection-start");
        var end = input.data("selection-end");
        var text = input[0].value.substring(start, end);
        if (isCondition) {
            placeholder = placeholder.replace("...", text);
        }

        input[0].setRangeText(placeholder, start, end);
    });
}

$(function () {
    $(document).on("click", ".tab-pane button[data-action]", function (e) {
        e.preventDefault();
        let btn = $(this);
        let action = btn.data("action");

        if (action === "showExample" || action === "showCode") {
            let container = btn.closest(".tab-pane");
            let textarea = container.find("textarea");
            let example = container.find(".example");
            btn.addClass("d-none");

            if (action === "showExample") {
                let otherActionBtn = container.find("[data-action='showCode']");
                otherActionBtn.removeClass("d-none");
                textarea.addClass("d-none");
                let html = textarea.val();
                example.html(html);
                example.removeClass("d-none");
            } else {
                let otherActionBtn = container.find("[data-action='showExample']");
                otherActionBtn.removeClass("d-none");
                textarea.removeClass("d-none");
                example.addClass("d-none");
            }
        }
    });
});