
function roleChanged($selectElement) {
    let selectedOption = $selectElement.find(':selected');
    let setSalesmanPerUser = selectedOption.data('set-salesman-per-user');
    let $salesmanIdElement = $('#SalesmanId');

    toggleDisabledAttr($salesmanIdElement, !setSalesmanPerUser);

    $salesmanIdElement.val(selectedOption.data('default-salesmanid'));
}

function toggleDisabledAttr($element, add) {
    if (add === undefined) {
        add = !$element.hasAttr('disabled');
    }

    if (add) {
        $element.attr('disabled', 'disabled');
    }
    else {
        $element.removeAttr('disabled');
    }
}


function postFilter($currentform) {
    let $parent = $currentform.closest('[data-id=usercontact-contact-overview]');
    $('[data-type="usercontact-process-all"]', $parent).hide();

    let assignedContactIds = getAssignedContactIds();

    $('[name="Filter.ContactIDsJson"]', $currentform).remove();
    $currentform.append('<input type="hidden" name="Filter.ContactIDsJson" value="[' + assignedContactIds.toString() + ']" />');

    let formData = $currentform.serialize();

    loadingIcon.ajax('#' + $currentform.data('result-id'),
        $currentform.attr("action"),
        {
            type: $currentform.attr("method"),
            data: formData
        },
        function (result) {
            var $resultElem = $('#' + $currentform.data('result-id'));

            $resultElem.html("");
            $resultElem.html(result);
            //var sortField = $("#PaginationBlock_SortField", $currentform).val();
            //$("#Filter_PageInfo_SortField", $currentform).val(sortField);

            //initializeSettings();
            //bindEvents($currentform);
            //bindPageEvents($currentform);


            let $parent = $($resultElem).closest('[data-id=usercontact-contact-overview]');

            if ($('[data-id="usercontact-result"] tbody tr:not(.heading)', $parent).length > 0) {
                $('[data-type="usercontact-process-all"]', $parent).show();
            }
            bindResultEvents();
            observerAssignedContacts();
        }
    );
}


var $mainform = $('#WebsiteUserEdit');
function reIndexWebsiteUserContacts() {
    let websiteUserContactsIdentifier = '[data-type=website-user-contact-item]:has(*)';
    let websiteuserContacts = $mainform.find(websiteUserContactsIdentifier).get();
    var element = null;
    for (let index = 0, length = websiteuserContacts.length; index < length; index++) {
        element = websiteuserContacts[index];
        let fields = $("[id^='WebsiteUserContacts_']", element).get();
        var field = null;
        for (let j = 0, fieldsLength = fields.length; j < fieldsLength; j++) {
            field = fields[j];
            var id = field.getAttribute("id");
            var split = id.split("_");
            split[1] = index + "";
            field.setAttribute("id", split.join('_'));

            var name = field.getAttribute("name");
            split = name.split("[");
            var secondPart = split[1];
            var split2 = secondPart.split("]");
            split2[0] = index;
            split[1] = split2.join("]");
            name = split.join("[");
            field.setAttribute("name", name);

            if (id.endsWith('__HtmlFieldPrefix')) {
                split = name.split(".");
                split.pop();
                var prefix = split.join(".");
                field.setAttribute('value', prefix);
            }
        }
    }
}

function bindResultEvents() {
    $("[data-id=usercontact-contact-overview] [data-id=paginationForm] a[type=submit]").off("click").on("click", function (e) {
        let $parent = $(this).closest('[data-id=usercontact-contact-overview]');
        let $result = $('[data-id=usercontact-result]', $parent);

        $(this).attr("data-clicked", true);
        $("[data-id=paginationForm]", $result).submit();
        return false;
    });

    $("[data-id=usercontact-result] [data-id=paginationForm]").off("submit").on("submit", function (e) {
        e.preventDefault();

        let $parent = $(this).closest('[data-id=usercontact-contact-overview]');
        let $currentform = $('form[id$=-filterForm]', $parent);

        var btn = $(this).closest_descendent("a[data-clicked='true']");
        var value = btn.attr("value");
        var selectedPage = parseInt(value);
        $('[data-id=Filter_PageInfo_Page]', $parent).val(selectedPage);
        var pageSize = $('[data-id=PaginationBlock_PageSize]', $parent).val();
        //$('[data-id=Filter_PageInfo_PageSize]', $parent).val(pageSize);
        postFilter($currentform);
    });
}

function setAssignedContacts() {
    let jsonObj = [];

    // Get all contact ids, by data-contact-id attribute

    $("table[data-assigned-contacts] .index-row[data-contact-id]").each(function () {
        let item = {}
        item.contactId = $(this).data('contact-id');
        item.ContactDeliveryAddressIDsString = "";
        var selectedAddresses = $(this).find("select[data-id='delivery-addresses']").val();
        item.ContactDeliveryAddressIDsString = selectedAddresses.join(",");

        jsonObj.push(item);
    });

    let result = JSON.stringify(jsonObj);
    $("#newContactsJson").val(result);
}

function addNewContactIds(contactId) {
    //add new field
    $($mainform).append('<input type="hidden" name="NewContactIds" value="' + contactId + '" />');
}
function removeContactIds(contactIds) {
    //Remove existing fields
    $.each(contactIds, function (index, contactId) {
        let existingFields = $('[id^=WebsiteUserContacts_][id$=_ContactID][value=' + contactId + ']');
        let parent = existingFields.closest('[data-type=website-user-contact-item]').remove();

        //remove new fields
        $('[name=NewContactIds][value="' + contactId + '"]', $mainform).remove();
    });

    reIndexWebsiteUserContacts();
}
function unAssignContacts(contactIds) {
    //remove existing fields
    $('[id^=WebsiteUserContacts_][id$=_ContactID]').map(function (index, elem) {
        let currentContactId = parseInt($(elem).val());
        let toRemove = $.inArray(currentContactId, contactIds);
        if (toRemove !== -1) {
            $(this).closest('[data-type=website-user-contact-item]').remove();
        }
    });

    //remove new fields
    $('[name=NewContactIds]', $mainform).map(function (index, elem) {
        let currentContactId = parseInt($(elem).val());
        let toRemove = $.inArray(currentContactId, contactIds);
        if (toRemove !== -1) {
            $(this).remove();
        }
    });

    reIndexWebsiteUserContacts();
}

function getAssignedContactIds() {
    let assignedContactIds = $('[id^=WebsiteUserContacts_][id$=_ContactID]').map(function (idx, ele) {
        return $(ele).val();
    }).get();

    $('[name=NewContactIds]').map(function (idx, ele) {
        let value = $(ele).val();
        assignedContactIds.push(value);
    });

    return assignedContactIds;
}

function updateRow(row, assign) {
    var newRow = $(row).clone();
    var contactId = newRow.data("contact-id");
    var contactIds = [contactId];
    removeContactIds(contactIds);

    //remove row from table
    row.remove();

    if (assign) {
        addNewContactIds(contactId);

        //Show columns
        newRow.find("[data-unassign-column]").removeClass("d-none");
        newRow.find("[data-delivery-addresses-column]").removeClass("d-none");
        //Hide column
        newRow.find("[data-assign-column]").addClass("d-none");

        var selectlist = newRow.find("select[data-id='delivery-addresses']")

        initMultiSelect(selectlist, { /*disabled: true, */collapsible: true, charLength: 50, refresh: false });


        //add row to table
        newRow.insertAfter($("#assigned-result tbody tr.heading"));
    }
    else {
        //Hide column
        newRow.find("[data-unassign-column]").addClass("d-none");
        newRow.find("[data-delivery-addresses-column]").addClass("d-none");
        //Show column
        newRow.find("[data-assign-column]").removeClass("d-none");


        //add row to table
        newRow.insertAfter($("#unassigned-result tbody tr.heading"));
    }

    bindResultEvents();
    ////add click handler
    //newRow.on("click",
    //    function () {
    //        updateRow($(this), !assign);
    //    });
}

function observerAssignedContacts() {
    // Observe each table row
    $('table[data-assigned-contacts] tr').each(function () {
        observer.observe(this);
    });
};

function addAllContacts(importedContactIDs) {

    var url = $("[data-id=usercontact-add-all]").data('url');
    let assignedContactIds = getAssignedContactIds();

    let $currentform = $('#unassigned-filterForm');
    $('[name="Filter.ContactIDsJson"]', $currentform).remove();
    $currentform.append('<input type="hidden" name="Filter.ContactIDsJson" value="[' + assignedContactIds.toString() + ']" />');
    if (importedContactIDs !== undefined) {
        $currentform.append('<input type="hidden" name="Filter.ImportedContactIDsJson" value="[' + importedContactIDs.toString() + ']" />');
    }

    loadingIcon.ajax('#' + $('[data-id=usercontact-contact-overview-parent]').attr('id'), url,
        {
            type: 'POST',
            data: $currentform.serialize(),
        },
        function (result) {
            $(result).each(function (index, contactId) {
                addNewContactIds(contactId);
            });

            $('#unassigned-filterForm').submit();
            $('#assigned-filterForm').submit();
        }
    );
}

function removeAllContacts($elem, callback) {
    let assignedContactIds = getAssignedContactIds();

    let $currentform = $('#assigned-filterForm');
    $('[name="Filter.ContactIDsJson"]', $currentform).remove();
    $currentform.append('<input type="hidden" name="Filter.ContactIDsJson" value="[' + assignedContactIds.toString() + ']" />');

    loadingIcon.ajax('#' + $('[data-id=usercontact-contact-overview-parent]').attr('id'),
        $elem.data('url'),
        {
            type: 'POST',
            data: $currentform.serialize(),
        },
        function (result) {
            removeContactIds(result);

            if (typeof callback === "function") {
                callback();
            }

            $('#assigned-filterForm').submit();
        }
    );
}

function onIntersection(entries, observer) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            // Row is in the viewport, initialize the multiselect
            var selectList = $(entry.target).find("td[data-delivery-addresses-column]").find("select");
            initMultiSelect($(selectList), { /*disabled: true, */collapsible: true, charLength: 50, refresh: false });
            // Stop observing the row after initialization
            observer.unobserve(entry.target);
        }
    });
}

// Create an Intersection Observer
const observer = new IntersectionObserver(onIntersection, {
    root: null, // Use the viewport as the container
    rootMargin: '0px',
    threshold: 0.1 // Trigger when 10% of the row is visible
});

$(document).ready(function () {

    $("#WebsiteUserEdit").on("submit", function (e) {
        e.preventDefault();
        setAssignedContacts();
        this.submit();
    });

    roleChanged($('#RoleId'));

    $("#unassigned-filterForm, #assigned-filterForm").on('submit', function (e) {
        e.preventDefault();
        postFilter($(this));
    });

    $('[data-id=usercontact-add-all]').on('click', function () { addAllContacts(); });
    $('[data-id=usercontact-remove-all]').on('click', function () { removeAllContacts($(this)); });

    $(function () {
        bindResultEvents();
    });

    observerAssignedContacts();

});
function assignContact(btn) {
    let row = $(btn).closest('tr');

    let assign = $(row).closest('[data-id="usercontact-result"]').attr('id') === 'unassigned-result';
    updateRow($(row), assign);
};

$(document).ready(function () {
    $('#contactExcelUpload').on('change', function (event) {
        $("#contactImportModalInvalidExcel").css("display", "none");
        const file = event.target.files[0];
        const websiteID = $(this).data('website-id');

        if (file) {
            const formData = new FormData();
            formData.append('excelFile', file);

            loadingIcon.ajaxWithLoadingScreen(`/WebsiteUser/${websiteID}/UploadExcel`,
                {
                    type: "POST",
                    data: formData,
                    contentType: false,
                    processData: false,
                    success: function (result) {
                        $("#contactImportModalTable").html(result);
                    },
                    error: function () {
                        $("#contactImportModalInvalidExcel").css("display", "block");
                        $("#contactExcelUpload").val("");
                    }
                }
            );
        }
    });

    $("#contactImportModalSaveButton").on("click", function (e) {
        e.preventDefault();
        e.stopPropagation();
        const contactIDs = [];

        $(".contactTableRow").each(function () {
            const contactID = $(this).data("contact-id");
            if (contactID) {
                contactIDs.push(contactID);
            }
        });

        removeAllContacts($('[data-id=usercontact-remove-all]'), function () {
            if (contactIDs.length > 0) {
                $.each(contactIDs, function (index, contactID) {
                    addNewContactIds(contactID);
                });
            }

            $("#contactImportModal").modal("hide");
        });
    })
});