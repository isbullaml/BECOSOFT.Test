function bindBackToOverviewEvent() {
    $("#main-content").on("click",
        "#overviewButton",
        function (e) {
            let url = $(this).attr("href");
            e.preventDefault();
            $("#backToOverview-Continue").off("click").on("click", function(btnEvent) {
                btnEvent.preventDefault();
                window.location = url;
            });

            $("#backToOverview-SaveAndContinue").off("click").on("click", function(btnEvent) {
                btnEvent.preventDefault();
                $("#main-content #saveButton").click();
                $("#backToOverviewModal").modal("hide");
            });

            $("#backToOverviewModal").modal("show");
        });
}

function bindMenuItemEvents() {
    $("#main-content").on("click",
        "#saveButton",
        function (e) {
            e.preventDefault();
            let elem = $(this);
            let form = elem.parents("form");

            let data = {
                ID: $("#ID").val(),
                TableDefiningID: $("#TableDefiningID").val(),
                MenuItems: [],
                Translations: [],
                __RequestVerificationToken: form.find("[name='__RequestVerificationToken']").val(),
            };

            let translations = $("#Translations").find("tr[name='translation-row']");
            $.each(translations,
                function(i, item) {
                    let translation = parseTranslation(item, data.ID);
                    data.Translations.push(translation);
                });

            let items = $("#menu-items").children("[data-root]");
            $.each(items,
                function (i, item) {
                    let menuItem = parseMenuItem(item);
                    data.MenuItems.push(menuItem);
                });

            let url = form.attr("action");
            becosoft.ajax(url,
                { type: "POST", data: data },
                function (result) {
                    if (result.IsSuccess === true) {
                        window.location = result.Url;
                    } else if (result.IsSuccess === false) {
                        $("#page-content").html(result.View);
                        bindMenuItemEvents();
                    }
                });
        });

    $("#menu-item-container").on("click",
        "a[data-action]",
        function (e) {
            e.preventDefault();
            let elem = $(this);
            let action = elem.attr("data-action");
            let url = elem.attr("href");
            let root = elem.parents("[data-root]:first");
            switch (action) {
                case "removeitem":
                    {
                        let textsContainer = $("#main-content").find("[data-texts]:first");
                        let deleteConfirmation = textsContainer.find("[data-text='deleteConfirmation']:first").text();
                        if (confirm(deleteConfirmation)) {
                            let elementsToRemove = getAllChildren(root, true);
                            elementsToRemove.push(root);
                            $.each(elementsToRemove,
                                function (i, element) {
                                    element.remove();
                                });
                        }
                        setPositions();
                        return;
                    }
                case "additem":
                case "addchild":
                    {
                        let findResult = findAnchorElement(root, action);
                        let insert = findResult.insert;
                        let anchorElement = findResult.anchor;
                        let urlParams = new URLSearchParams(url);
                        let type = parseInt(urlParams.get("Type"));
                        var menuItems = $("#menu-items").find("[name$='.Type'][value='2']");

                        if (type === 2 && menuItems && menuItems.length > 0) {
                            alert("Only one menu link is allowed");
                            return;
                        }

                        createMenuItemModal(url, null, (result) => {
                            let newRow = $(result);

                            if (insert) {
                                newRow.insertAfter(anchorElement);
                            } else {
                                anchorElement.append(result);
                            }
                            setPositions();
                        });
                        break;
                    }
                case "edititem":
                    {
                        var menuItem = parseMenuItem(root);
                        createMenuItemModal(url, menuItem, (result) => {
                            root.replaceWith(result);
                            setPositions();
                        });
                        break;
                    }
                case "showitem":
                    {
                        let id = root.attr("id");
                        createDisplayModal(url, id);
                        break;
                    }
                case "moveleft":
                case "moveright":
                case "moveup":
                case "movedown":
                    moveMenuItem(root, action, 1);
                    break;
            }
        });
    $("#menu-item-container").on("change",
        ".position-input",
        function (e) {
            var input = $(this);
            var row = input.closest("[data-root]");
            var oldPosition = input.attr("data-previous");
            var newPosition = parseInt(input.val());

            if (oldPosition > newPosition) {
                var amountUp = oldPosition - newPosition;
                moveMenuItem(row, "moveup", amountUp);
            } else if (oldPosition < newPosition) {
                var amountDown = newPosition - oldPosition;
                moveMenuItem(row, "movedown", amountDown);
            }
        });
}

function moveMenuItem(root, action, amount = 1) {
    const DirectionEnum = { left: 1, right: 2, up: 3, down: 4 };
    Object.freeze(DirectionEnum);

    let direction = DirectionEnum[action.replace("move", "")];

    switch (direction) {
        case DirectionEnum.left:
            {
                var rootLevel = parseInt(root.attr("data-root"));
                if (rootLevel === 0) {
                    return;
                }
                let levelOffset = -1;
                let parentID = root.attr("data-parent");
                let parent = $("#" + parentID + "");
                //let parentLevel = parseInt(parent.attr("data-root")) || 0;
                let items = getAllChildren(root, false);
                $.each(items,
                    function (index, item) {
                        let elem = $(item);
                        updateIndentation(elem, levelOffset);
                    });
                let newParentID = rootLevel + levelOffset !== 0 ? parent.attr("data-parent") : "";
                root.attr("data-parent", newParentID);
                updateIndentation(root, levelOffset);
                break;
            }
        case DirectionEnum.right:
            {
                let levelOffset = 1;
                let parentID = root.attr("data-parent");
                let parentChildren;
                if (parentID === "") {
                    parentChildren = $("#menu-items").children("[data-parent='']");
                } else {
                    let parent = $("#" + parentID);
                    parentChildren = parent.siblings("[data-parent='" + parentID + "']");
                }
                let prevChild;
                $.each(parentChildren,
                    function (i, e) {
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
                let items = getAllChildren(root, false);
                $.each(items,
                    function (index, item) {
                        let elem = $(item);
                        updateIndentation(elem, levelOffset);
                    });
                let newParentID = newParent.attr("id");
                root.attr("data-parent", newParentID);
                updateIndentation(root, levelOffset);
                break;
            }
        case DirectionEnum.up:
            {
                let position = parseInt(root.attr("data-position"));
                var nextPosition = position - amount;
                if (nextPosition < 1) {
                    break;
                }

                let parentID = root.attr("data-parent");
                var siblings = $("[data-parent='" + parentID + "']");
                var children = getAllChildren(root, true);

                var prevRow = siblings.filter("[data-position='" + nextPosition + "']");

                if (!prevRow || prevRow.length === 0) {
                    break;
                }

                prevRow.before(children);

                break;
            }
        case DirectionEnum.down:
            {
                let position = parseInt(root.attr("data-position"));
                var prevPosition = position + amount;

                let parentID = root.attr("data-parent");
                var siblings = $("[data-parent='" + parentID + "']");
                var children = getAllChildren(root, true);

                var nextRow = siblings.filter("[data-position='" + prevPosition + "']");

                if (!nextRow || nextRow.length === 0) {
                    break;
                }

                var nextRowChildren = getAllChildren(nextRow, true);
                var nextRowLastChild = nextRowChildren.slice(-1)[0];

                $(nextRowLastChild).after(children);

                break;
            }
        default:
    }

    setPositions();
}

function updateIndentation(elem, offset) {
    let level = parseInt(elem.attr("data-root"));
    let newLevel = level + offset;
    elem.attr("data-root", newLevel);
    let indentElem = elem.find("[data-indent]:first");
    let indentStyle = newLevel === 0 ? "" : "margin-left: " + (newLevel * 15) + "px !important";
    indentElem.attr('style', indentStyle);
};

function findAnchorElement(root, action) {
    var lastChild = root;
    var anchorElement = root;
    var insert = true;
    if (root.length === 0 || $("#menu-items").children("[data-root]").length === 0) {
        anchorElement = $("#menu-items");
        insert = false;
    } else {
        var searchLevel = parseInt(root.attr("data-root"));
        if (action === "additem" && searchLevel === 0) {
            anchorElement = $("#menu-items");
            insert = false;
        } else {
            let guid = root.attr("id");
            searchLevel = searchLevel + 1;
            let siblings = root.siblings("[data-root='" + (searchLevel) + "']").filter("[data-parent='" + guid + "']");
            if (action === "additem" && siblings.length === 0) {
                var parentGuid = root.attr("data-parent");
                searchLevel = parseInt(root.attr("data-root"));
                siblings = root.siblings("[data-root='" + (searchLevel) + "']")
                    .filter("[data-parent='" + parentGuid + "']");
            }
            if (siblings.length > 0) {
                while (siblings.length !== 0) {
                    lastChild = siblings.last();
                    guid = lastChild.attr("id");
                    searchLevel = parseInt(lastChild.attr("data-root")) + 1;
                    siblings = lastChild.siblings("[data-root='" + (searchLevel) + "']")
                        .filter("[data-parent='" + guid + "']");
                };
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
    let level = parseInt(root.attr("data-root")) + 1;
    let siblings = root.siblings("[data-root='" + level + "']").filter("[data-parent='" + guid + "']");
    let result = getAllItemChildren(siblings);
    if (includeParent === true) {
        result.push(root);
    }
    return result;
}

function getAllChildren(row, includeParent) {
    let guid = row.attr("id");
    var result = [];
    if (includeParent) {
        result.push(row[0]);
    }

    var children = $(`[data-parent='${guid}']`);
    $.each(children,
        function () {
            var child = $(this);
            result.push(child[0]);
            var grandChildren = getAllChildren(child, false);
            $.each(grandChildren,
                function () {
                    var grandChild = $(this);
                    result.push(grandChild[0]);
                });
        });

    return result;
}

function getAllItemChildren(s) {
    var elements = [];
    if (s.length === 0) {
        return elements;
    }
    elements.push(...s);
    s.each(function (index, element) {
        let el = $(element);
        let sibGuid = el.attr("id");
        let sibLevel = parseInt(el.attr("data-root")) + 1;
        let sibs = el.siblings("[data-root='" + sibLevel + "']").filter("[data-parent='" + sibGuid + "']");
        var elems = getAllItemChildren(sibs);
        elements.push(...elems);
    });
    return elements;
};

// Update the errors on the modal
function updateModalErrors(errors) {
    $(".modal span[data-valmsg-for]").text("");
    $(".modal #modalErrors").text("");
    $(".modal .alert-danger").addClass("d-none");
    var modalErrors = false;
    $.each(errors,
        function () {
            var key = this.Key.split(".")[1];
            if (key === undefined || key === null || key === "") {
                key = this.Key;
            }
            var field = $(".modal span[data-valmsg-for='" + key + "']");
            if (field.length !== 1) {
                modalErrors = true;
                $(".modal #modalErrors").append("<li>" + this.Value + "</li>");
            } else {
                field.text(this.Value);
            }
        });

    if (modalErrors) {
        $(".modal .alert-danger").removeClass("d-none");
    }
}

// Create a modal for adding an entity
function createMenuItemModal(url, data, callback) {
    var ajaxObject = { async: true };
    if (data === null) {
        ajaxObject.type = "GET";
    } else {
        data.WebsiteID = $("#WebsiteID").val();
        ajaxObject.type = "POST";
        ajaxObject.data = data;
    }

    becosoft.ajax(url, ajaxObject, function (result) {
        var eventListenersFunction = function () {
            $("#addTranslation").on("click", loadTranslationRow);
            var classInput = $("#Classes")[0];
            let picker = new TP(classInput,
                {
                    escape: [',', ' '],
                    join: ',',
                    x: true
                });

            $("#add-image-button").on("click", function(e) {
                e.preventDefault();
                $("#menuItemImageUpload").trigger("click");
            });

            $("#TargetsToolTip").tooltip({ html: true, boundary: "window" });

            $("#menuItemImageUpload").on("change", function (e) {
                var imageUploader = e.currentTarget;

                if (imageUploader.files === undefined || imageUploader.files === null) {
                    return;
                }

                var file = imageUploader.files[0];
                $("#ImageName").val(file.name);
                $("#menuItemAddButton").attr("disabled", true);
                ConvertFileUploadToBase64(file).then(function (base64) {
                    $("#Base64Image").val(base64);
                    $("#menuItemExampleImage").attr("src", base64);
                    $("#menuItemExampleImage").removeClass("d-none");
                    $("#menuItemAddButton").attr("disabled", false);

                    $("#DeleteImage").val(false);
                    $("#delete-image-button").prop("disabled", false);
                    //if (base64.length > 0) {
                    //    $("#undo-delete-image-button").prop("disabled", false);
                    //}
                });
            });
            
            $("#menuItemAddButton").on("click", function (e) {
                e.preventDefault();
                var form = $("#menuItemAddForm");
                var formData = form.serialize();
                becosoft.ajax(form.attr("action"), {
                    type: "POST",
                    data: formData
                }, function (postResult) {
                    if (postResult.Result) {
                        closeCurrentModal();
                        callback(postResult.ResultView);
                    } else {
                        updateModalErrors(postResult.Errors);
                    }
                });
            });

            $("#delete-image-button").on("click", function (e) {
                e.preventDefault();

                var upload = $("#menuItemImageUpload");
                var preview = $("#menuItemExampleImage");

                upload.val("");
                preview.attr("src", "");
                preview.addClass("d-none");
                $("#DeleteImage").val(true);
                $("#delete-image-button").prop("disabled", true);

                //var oldSrc = $("#Base64Image").val();
                //if (oldSrc.length > 0) {
                //    $("#undo-delete-image-button").prop("disabled", false);
                //}
            });

            //$("#undo-delete-image-button").on("click", function (e) {
            //    e.preventDefault();

            //    var upload = $("#menuItemImageUpload");
            //    var preview = $("#menuItemExampleImage");
            //    var oldSrc = $("#Base64Image").val();

            //    upload.val("");
            //    $("#undo-delete-image-button").prop("disabled", true);
            //    if (oldSrc.length > 0) {
            //        preview.attr("src", oldSrc);
            //        preview.removeClass("d-none");
            //        $("#delete-image-button").prop("disabled", false);
            //    }

            //    $("#DeleteImage").val(false);
            //});

            $("#modal-close-button").on("click", function (e) {
                e.preventDefault();
                closeCurrentModal();
            });
        };

        openModal("#modal-container", result, false, eventListenersFunction);
    });
}

function createDisplayModal(url, data) {
    becosoft.ajax(url, { async: true, data: data, type: "POST" }, function (result) {
        var eventListenersFunction = function () {
            $("#modal-close-button").on("click", function (e) {
                e.preventDefault();
                closeCurrentModal();
            });
        };

        openModal("#modal-container", result, false, eventListenersFunction);
    });
}

function parseMenuItem(row) {
    let el = $(row);
    let id = el.attr("id");

    let menuItem = {
        Level: parseInt(el.attr("data-root")) || 0,
        TableDefiningID: parseInt($(`[name$='${id}.TableDefiningID']`).val()) || 0,
        WebsiteID: parseInt($(`[name$='${id}.WebsiteID']`).val()) || 0,
        ID: parseInt($(`[name$='${id}.ID']`).val()) || 0,
        SelectedContentPageID: parseInt($(`[name$='${id}.SelectedContentPageID']`).val()) || 0,
        ParentID: parseInt($(`[name$='${id}.ParentID']`).val()) || 0,
        Type: $(`[name$='${id}.Type']`).val(),
        Target: $(`[name$='${id}.Target']`).val(),
        Position: parseInt($(`[name$='${id}.Position']`).val()) || 0,
        Classes: $(`[name$='${id}.Classes']`).val(),
        IsVisible: $(`[name$='${id}.IsVisible']`).val(),
        ImageName: $(`[name$='${id}.ImageName']`).val(),
        Base64Image: $(`[name$='${id}.Base64Image']`).val(),
        DeleteImage: $(`[name$='${id}.DeleteImage']`).val(),
        TemporaryParentItemID: el.attr("data-parent"),
        TemporaryItemID: id,
        Translations: []
    };

    console.log($(`[name$='${id}.Position']`));

    let translations = el.find(".menu-item-translation-row");
    $.each(translations,
        function (j, translation) {
            let tr = $(translation);

            let menuItemTranslation = {
                TableDefiningID: menuItem.TableDefiningID,
                WebsiteMenuItemID: menuItem.WebsiteMenuItemID,
                LocalizeID: tr.find("[name$='.LocalizeID']").val() || 0,
                LanguageID: tr.find("[name$='.LanguageID']").val() || 0,
                Name: tr.find("[name$='.Name']").val() || "",
                ToolTip: tr.find("[name$='.ToolTip']").val() || "",
                Url: tr.find("[name$='.Url']").val() || ""
            };

            menuItem.Translations.push(menuItemTranslation);
        });

    return menuItem;
}

function parseTranslation(row, parentID) {
    let $row = $(row);
    return {
        ParentID: parentID,
        LocalizeID: $row.find("[name$='.LocalizeID']").val() || 0,
        LanguageID: $row.find("[name$='.LanguageID']").val() || 0,
        Description: $row.find("[name$='.Description']").val() || "",
    };
}

function setPositions() {
    var levels = [];
    $("[data-root]").each(function () {
        var el = $(this);
        var level = el.attr("data-root");
        if (!levels.includes(level)) {
            levels.push(level);
        }
    });

    $.each(levels, function () {
        var level = this;
        setPositionsForLevel(level);
    });
}

function setPositionsForLevel(level) {
    var levelRows = $("[data-root='" + level + "']").toArray();
    var groupedRows = groupByParent(levelRows);

    groupedRows.forEach(function (rows) {
        $.each(rows,
            function (i) {
                var position = i + 1;
                var row = $(this);
                row.attr("data-position", position);
                var input = row.find(`[name$='$.Position']`);
                input.val(position);
                var input = row.find("input[type='number']");
                input.val(position);
                input.attr("data-previous", position);
            });
    });
}

function groupByParent(list) {
    const map = new Map();
    list.forEach((item) => {
        var jqItem = $(item);
        const key = jqItem.attr("data-parent");
        const collection = map.get(key);
        if (!collection) {
            map.set(key, [item]);
        } else {
            collection.push(item);
        }
    });
    return map;
}