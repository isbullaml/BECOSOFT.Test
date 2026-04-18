function sortTable(event, options = null) {
    var columnHeader = event.target || event.srcElement;
    var table = columnHeader.closest("table");
    if (table.getAttribute("data-is-sorting-table") === true) {
        return;
    }
    table.setAttribute("data-is-sorting-table", true);
    if (options == null) {
        options = {};
    }
    var rows, switching, i, x, y, shouldSwitch, dir, switchCount = 0;
    var headers = table.querySelector("thead").querySelectorAll("th");
    var headersArray = Array.from(headers);
    var columnIndex = headersArray.findIndex(col => col === event.target);
    switching = true;
    dir = "asc";
    var xRow, yRow;
    while (switching) {
        switching = false;
        rows = table.getElementsByTagName("tr");
        for (i = 1; i < (rows.length - 1); i++) {
            shouldSwitch = false;
            xRow = rows[i];
            yRow = rows[i + 1];
            if (options.stickyRowAttr !== undefined) {
                var isStickyX = xRow.getAttribute(options.stickyRowAttr) != null;
                var isStickyY = yRow.getAttribute(options.stickyRowAttr) != null;
                if (isStickyX || isStickyY) {
                    if (isStickyX && !isStickyY) {
                        shouldSwitch = true;
                        break;
                    } else {
                        break;
                    }
                }
            }
            if (xRow.getElementsByTagName("td")[columnIndex].getAttribute('data') !== null && yRow.getElementsByTagName("td")[columnIndex].getAttribute('data') !== null) {
                x = xRow.getElementsByTagName("td")[columnIndex].getAttribute('data');
                y = yRow.getElementsByTagName("td")[columnIndex].getAttribute('data');
            }
            else {
                x = xRow.getElementsByTagName("td")[columnIndex].innerHTML.toLowerCase();
                y = yRow.getElementsByTagName("td")[columnIndex].innerHTML.toLowerCase();
            }
            var xNumber = parseFloat(x.replace(",", "."));
            var yNumber = parseFloat(y.replace(",", "."));
            if (!isNaN(xNumber) && !isNaN(yNumber)) {
                x = xNumber;
                y = yNumber;
            }
            if (dir === "asc") {
                if (x > y) {
                    shouldSwitch = true;
                    break;
                }
            } else if (dir === "desc") {
                if (x < y) {
                    shouldSwitch = true;
                    break;
                }
            }
        }
        if (shouldSwitch) {
            xRow.parentNode.insertBefore(yRow, xRow);
            switching = true;
            switchCount++;
        } else {
            if (switchCount === 0 && dir === "asc") {
                dir = "desc";
                switching = true;
            }
        }
    }
    table.setAttribute("data-is-sorting-table", false);
}