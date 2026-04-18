var websiteRoleDocumentContainer = undefined;
var websiteRoleDocumentItemIdentifier = undefined;
var websiteRoleDocumentListPrefix = undefined;

$("[data-type=website-role-document-addNew]").on('click', function () {
    var btn = $(this);
    new Promise(function (resolve, reject) {
        var rows = $(websiteRoleDocumentItemIdentifier, websiteRoleDocumentContainer);
        var index = rows.length;
        var url = btn.data("url");
        url += (url.split('?')[1] ? '&' : '?') + 'index=' + index;


        becosoft.ajax(url, { async: true },
            function (partialView) {
                if (partialView === null || partialView === undefined || partialView.length === 0) {
                    reject();
                } else {
                    var lastRow = $(websiteRoleDocumentItemIdentifier + ':last', websiteRoleDocumentContainer);
                    if (lastRow.length === 0) {
                        $(websiteRoleDocumentContainer).prepend(partialView);
                    } else {
                        lastRow.after(partialView);
                    }

                    resolve();
                }
            });
    }).finally(function () {
        bindRowClickEvents();
        reIndexWebsiteRoleDocumentTypes();
    });
});

function bindRowClickEvents() {
    $('[data-action="removeRoleDocument"]').off('click');
    $('[data-action="removeRoleDocument"]').on('click', function (e) {
        e.stopPropagation();
        e.preventDefault();
        $(this).closest(websiteRoleDocumentItemIdentifier).remove();
        reIndexWebsiteRoleDocumentTypes();
    });
}

function reIndexWebsiteRoleDocumentTypes() {
    var listItems = websiteRoleDocumentContainer.find(websiteRoleDocumentItemIdentifier);
    listItems.each(function () {
        var element = $(this);
        var index = $(websiteRoleDocumentItemIdentifier, websiteRoleDocumentContainer).index(element);

        element.find("[id^='" + websiteRoleDocumentListPrefix + "_']").each(function () {
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
    });
}

function initWebsiteRoleDocument() {
    websiteRoleDocumentContainer = $('[data-type="website-role-document-container"]');
    websiteRoleDocumentItemIdentifier = '[data-type="website-role-document-item"]';
    websiteRoleDocumentListPrefix = websiteRoleDocumentContainer.data("list-prefix");

    bindRowClickEvents();
}

$(document).ready(function () {
    initWebsiteRoleDocument();

    $('[data-toggle="tooltip"]').tooltip();
});