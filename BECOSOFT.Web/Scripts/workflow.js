function bindWorkflowEvents() {
    $("[data-action]").on("click",
        "[id='saveButton']",
        function(e) {
            e.preventDefault();
            let elem = $(this);
            let form = elem.parents("form");

            let data = {
                WorkflowID: $("#WorkflowID").val(),
                Description: $("#Description").val(),
                IsTemplate: $("#IsTemplate").val(),
                DocumentTypeID: $("#DocumentTypeID").val(),
                DocumentID: $("#DocumentID").val(),
                ProjectManagerID: $("#ProjectManagerID").val(),
                CreationDate: $("#CreationDate").val(),
                WorkflowItems: [],
                __RequestVerificationToken: form.find("[name='__RequestVerificationToken']").val(),
            };
            let items = $("#workflow-items").children("[data-root]");
            $.each(items,
                function(i, item) {
                    let el = $(item);
                    let id = el.attr("id");
                    let wfItem = {
                        Level: parseInt(el.data("root")) || 0,
                        WorkflowItemID: $(`[name$='${id}.WorkflowItemID']`).val() || 0,
                        WorkflowID: $(`[name$='${id}.WorkflowID']`).val() || 0,
                        Description: $(`[name$='${id}.Description']`).val(),
                        ProjectManagerComment: $(`[name$='${id}.ProjectManagerComment']`).val(),
                        Comment: $(`[name$='${id}.Comment']`).val(),
                        ParentID: $(`[name$='${id}.ParentID']`).val(),
                        ProjectManagerID: $(`[name$='${id}.ProjectManagerID']`).val(),
                        TemporaryParentItemID: el.data("parent"),
                        TemporaryItemID: id,
                        EnableDeadline: $(`[name$='${id}.EnableDeadline']:checked`).length > 0,
                        Deadline: $(`[name$='${id}.Deadline']`).val(),
                        ExecutionBy: $(`[name$='${id}.ExecutionBy']`).val()
                    };
                    data.WorkflowItems.push(wfItem);
                });
            let url = form.attr("action");
            becosoft.ajax(url,
                { type: "POST", data: data },
                function(result) {
                    if (result.IsSuccess === true) {
                        window.location = result.Url;
                    } else if (result.IsSuccess === false) {
                        $("#page-content").html(result.View);
                        bindWorkflowEvents();
                    }
                    console.log(result);
                });
        });

    $("#workflow-item-container").on("change",
        "select[data-project-manager]",
        function(e) {
            let elem = $(this);
            let pmValue = elem.val();
            let root = elem.parents("div[data-root]:first");
            let items = $(getItemAndChildren(root, false));
            $.each(items.find("select[data-project-manager]"), function(i, e) {
                var elem = $(e);
                if ((parseInt(elem.val()) || 0) === 0) {
                    elem.val(pmValue);
                }
            });
        }
    );

    $("#workflow-item-container").on("click",
        "a[data-workflowaction]",
        function(e) {
            e.preventDefault();
            let elem = $(this);
            let action = elem.data("workflowaction");
            let url = elem.attr("href");
            let root = elem.parents("[data-root]:first");
            switch (action) {
            case "removeitem":
            {
                let textsContainer = $("#main-content").find("[data-texts]:first");
                let deleteConfirmation = textsContainer.find("[data-text='deleteConfirmation']:first").text();
                console.log(textsContainer, deleteConfirmation);
                if (confirm(deleteConfirmation)) {
                    let elementsToRemove = getItemAndChildren(root);
                    elementsToRemove.push(root);
                    $.each(elementsToRemove,
                        function(i, element) {
                            element.remove();
                        });
                }
                return;
            }
            case "additem":
            case "addchild":
            {
                let findResult = findAnchorElement(root, action);
                let insert = findResult.insert;
                let anchorElement = findResult.anchor;

                becosoft.ajax(url,
                    { async: true },
                    function(result) {
                        if (insert) {
                            let newRow = $(result);
                            newRow.insertAfter(anchorElement);
                        } else {
                            anchorElement.append(result);
                        }
                    });
                return;
            }
            case "moveleft":
            case "moveright":
            case "moveup":
            case "movedown":
                moveWorkflowItem(root, action);
                break;
            }
        });
}

function moveWorkflowItem(root, action) {
    const DirectionEnum = { left: 1, right: 2, up: 3, down: 4 };
    Object.freeze(DirectionEnum);

    let direction = DirectionEnum[action.replace("move", "")];

    switch (direction) {
    case DirectionEnum.left:
    {
        var rootLevel = parseInt(root.data("root"));
        if (rootLevel === 0) {
            return;
        }
        let levelOffset = -1;
        let parentID = root.data("parent");
        let parent = $("#" + parentID + "");
        //let parentLevel = parseInt(parent.data("root")) || 0;
        let items = getItemAndChildren(root, false);
        $.each(items,
            function(index, item) {
                let elem = $(item);
                updateIndentation(elem, levelOffset);
            });
        let newParentID = rootLevel + levelOffset !== 0 ? parent.data("parent") : "";
        root.data("parent", newParentID);
        updateIndentation(root, levelOffset);
        break;
    }
    case DirectionEnum.right:
    {
        let levelOffset = 1;
        let parentID = root.data("parent");
        let parentChildren;
        if (parentID === "") {
            parentChildren = $("#workflow-items").children("[data-parent='']");
        } else {
            let parent = $("#" + parentID);
            parentChildren = parent.siblings("[data-parent='" + parentID + "']");
        }
        let prevChild;
        $.each(parentChildren,
            function(i, e) {
                if ($(e).attr("id") === root.attr("id")) {
                    return false;
                }
                prevChild = e;
                return true;
            });
        let newParent = $(prevChild);
        if (newParent.length === 0) {
            return;
        }
        let items = getItemAndChildren(root, false);
        $.each(items,
            function(index, item) {
                let elem = $(item);
                updateIndentation(elem, levelOffset);
            });
        let newParentID = newParent.attr("id");
        root.data("parent", newParentID);
        updateIndentation(root, levelOffset);
        break;
    }
    case DirectionEnum.up:
    {
        let parentID = root.data("parent");
        let level = root.data("root");
        let siblings = $("#workflow-items").children("[data-parent='" + parentID + "']");
        let findPreviousSibling = (s) => {
            let prevSibling;
            $.each(s,
                function(i, e) {
                    if ($(e).attr("id") === root.attr("id")) {
                        return false;
                    }
                    prevSibling = e;
                    return true;
                });
            let result = $(prevSibling);
            return result;
        };
        let prev = findPreviousSibling(siblings);
        if (prev.length === 0) {
            siblings = $("#workflow-items").children("[data-root='" + level + "']");
            prev = findPreviousSibling(siblings);
            if (prev.length === 0) {
                return;
            }
            let prevParentID = prev.data("parent");
            root.data("parent", prevParentID);
        }
        let items = getItemAndChildren(root, false);
        console.log(items);
        prev.before(root, items);
        break;
    }
    case DirectionEnum.down:
    {
        let parentID = root.data("parent");
        let level = parseInt(root.data("root")) || 0;
        let siblings = level === 0
            ? $("#workflow-items").children("[data-parent='']")
            : root.siblings("[data-parent='" + parentID + "']");
        let findNextSibling = (s) => {
            let nextSibling;
            let seenFlag = false;
            $.each(s,
                function(i, e) {
                    if ($(e).attr("id") === root.attr("id")) {
                        seenFlag = true;
                        return true;
                    }
                    if (seenFlag) {
                        nextSibling = e;
                        return false;
                    }
                    return true;
                });
            let result = $(nextSibling);
            return result;
        };
        let next = findNextSibling(siblings);
        if (next.length === 0) {
            siblings = $("#workflow-items").children("[data-root='" + level + "']");
            next = findNextSibling(siblings);
            if (next.length === 0) {
                return;
            }
            let nextParentID = next.data("parent");
            root.data("parent", nextParentID);
        }
        let items = getItemAndChildren(root, false);
        console.log(items);
        let nextChildren = getItemAndChildren(next, false);
        let nextLastChild = $(nextChildren).last();
        nextLastChild.after(root, items);
        break;
    }
    default:
    }

}

function updateIndentation(elem, offset) {
    let level = parseInt(elem.data("root"));
    //console.log(level);
    let newLevel = level + offset;
    //if (newLevel)
    elem.data("root", newLevel);
    console.log(elem.data("root"));
    let indentElem = elem.find("[data-indent]:first");
    let indentStyle = newLevel === 0 ? "" : "margin-left: " + (newLevel * 15 - 15) + "px !important";
    indentElem.attr('style', indentStyle);
    //console.log(indentElem);
};

function findAnchorElement(root, action) {
    var lastChild = root;
    var anchorElement = root;
    var insert = true;
    if (root.length === 0 || $("#workflow-items").children("[data-root]").length === 0) {
        anchorElement = $("#workflow-items");
        insert = false;
    } else {
        var searchLevel = parseInt(root.data("root"));
        if (action === "additem" && searchLevel === 0) {
            anchorElement = $("#workflow-items");
            insert = false;
        } else {
            let guid = root.attr("id");
            searchLevel = searchLevel + 1;
            let siblings = root.siblings("[data-root='" + (searchLevel) + "']").filter("[data-parent='" + guid + "']");
            if (action === "additem" && siblings.length === 0) {
                var parentGuid = root.data("parent");
                searchLevel = parseInt(root.data("root"));
                siblings = root.siblings("[data-root='" + (searchLevel) + "']")
                    .filter("[data-parent='" + parentGuid + "']");
            }
            if (siblings.length > 0) {
                while (siblings.length !== 0) {
                    lastChild = siblings.last();
                    guid = lastChild.attr("id");
                    searchLevel = parseInt(lastChild.data("root")) + 1;
                    siblings = lastChild.siblings("[data-root='" + (searchLevel) + "']")
                        .filter("[data-parent='" + guid + "']");
                };
                //console.log(lastChild);
                if (lastChild.length > 0) {
                    anchorElement = lastChild;
                }
            }
        }
    }
    return { insert: insert, anchor: anchorElement };
}

function getItemAndChildren(root, includeParent = true) {
    let guid = root.attr("id");
    let level = parseInt(root.data("root")) + 1;
    let siblings = root.siblings("[data-root='" + level + "']").filter("[data-parent='" + guid + "']");
    let result = getAllItemChildren(siblings);
    if (includeParent === true) {
        result.push(root);
    }
    return result;
}

function getAllItemChildren(s) {
    var elements = [];
    if (s.length === 0) {
        return elements;
    }
    elements.push(...s);
    s.each(function(index, element) {
        let el = $(element);
        let sibGuid = el.attr("id");
        let sibLevel = parseInt(el.data("root")) + 1;
        let sibs = el.siblings("[data-root='" + sibLevel + "']").filter("[data-parent='" + sibGuid + "']");
        var elems = getAllItemChildren(sibs);
        elements.push(...elems);
    });
    return elements;
};