$(document).ready(function () {
    // Initialize event listeners
    initializeEventListeners();
});

function initializeEventListeners() {
    $('div[data-articlepurchase]').on('click', '[id=addSupplier]', addSupplier);
    $('div[data-articlepurchase]').on('click', '[id=addWarehouse]', addWarehouse);

    $('#supplierTable tbody').on('change', 'input[type="checkbox"].defaultSupplierCheckbox', handleDefaultDeliveryChange);
    $('#supplierTable tbody').on('change', '[id^="defaultDelivery_"]', handleOtherDefaultDeliveryChange);

    $('#supplierTable').on('change', 'input[type=checkbox][name^="ArticleWarehouseDeliveries"]', handleCheckboxValueChange);
    $('#articleStockCheckTable').on('change', 'input[type=checkbox][name^="ArticleWarehouses"]', handleWarehouseCheckboxValueChange);

    $('#articleStockCheckTable').on('change', 'select[id^="warehouse_"], select[id^="purchasePeriod_"], input[id^="assortment_"]', handleArticleStockCheckChange);

    $('#articleStockCheckTable').on('click', '#btnWarehouseRemove', removeWarehouseRow); 
    $('#supplierTable').on('click', '#btnSupplierRemove', removeSupplierRow);

    $("#chkDeliveryTime").on('click', function () {
        handleDropDown(this, '#Purchase_DeliveryPeriodID');
    });
    $("#chkTransportMethod").on('click', function () {
        handleDropDown(this, '#Purchase_WaysToShipID');
    });

    $('#Purchase_DeliveryPeriodID').css('text-align', 'left');
    $('#Purchase_WaysToShipID').css('text-align', 'left');
}


function handleDropDown(checkbox, dropdownSelector) {
    if ($(checkbox).is(':checked')) {
        $(dropdownSelector).prop('disabled', false);
    } else {
        $(dropdownSelector).val('');  
        $(dropdownSelector).prop('disabled', true);
    }

    if ($('#Purchase_DeliveryPeriodID').val() === '0' && checkbox === '#chkDeliveryTime') {
        $(checkbox).prop('checked', false);
        $(dropdownSelector).prop('disabled', true); 
    }

    if ($('#Purchase_WaysToShipID').val() === '0' && checkbox === '#chkTransportMethod') {
        $(checkbox).prop('checked', false); 
        $(dropdownSelector).prop('disabled', true); 
    }
}

function removeWarehouseRow() {
    const row = $(this).closest('tr');
    row.remove();
    $('#articleStockCheckTable tbody tr').each(function (index) {
        $(this).attr('row-index', index);
        $(this).find('select, input').each(function () {
            const id = $(this).attr('id');
            const name = $(this).attr('name');
            if (id) {
                const newId = id.replace(/_\d+$/, `_${index}`);
                $(this).attr('id', newId);
            }
            if (name) {
                const newName = name.replace(/\[\d+\]/, `[${index}]`);
                $(this).attr('name', newName);
            }
        });
    });
}

// Handle change event for article stock check inputs and selects
function handleArticleStockCheckChange() {
    var currentRow = $(this).closest('tr');
    var currentRowData = {
        index: currentRow.attr('row-index'),
        warehouseID: currentRow.find('select[id^="warehouse_"]').val(),
        purchasePeriod: currentRow.find('select[id^="purchasePeriod_"]').val(),
        assortment: currentRow.find('input[id^="assortment_"]').is(':checked')
    };
    if (checkForDuplicates(currentRowData)) {
        showDynamicModal(messages.duplicateWarehouseCombination);
        $('#addWarehouse').prop('disabled', true);
    } else {
        $('#addWarehouse').prop('disabled', false);
    }

}

function checkForDuplicates(currentRowData) {
    //debugger;
    // Find matching rows based on the specified criteria
    var matchingData = $('#articleStockCheckTable tbody tr').filter(function () {
        //debugger;
        var rowIndex = $(this).attr('row-index');

        // Skip the row if it's the same as the current row being added
        if (currentRowData.index !== undefined && currentRowData.index.toString() === rowIndex) {
            //debugger;
            return false;
        }

        // Check if the current row's data matches the criteria
        return (
            $(this).find('select[id^="warehouse_"]').val() === currentRowData.warehouseID &&
            $(this).find('select[id^="purchasePeriod_"]').val() === currentRowData.purchasePeriod &&
            $(this).find('input[id^="assortment_"]').is(':checked') === currentRowData.assortment
        );
    })
        .map(function () {
            //debugger;
            // Get the text or values of each matching row's cells for debugging or further use
            return $(this).find('td').map(function () {
                //debugger;
                return $(this).text();
            }).get();
        }).get();

    // Return true if there's any matching data, indicating a duplicate
    return matchingData.length > 0;
}


// Add Warehouse Functionality
function addWarehouse(e) {
    e.preventDefault();

    const rowCount = $('#articleStockCheckTable tbody tr').length;
    const currentRowData = getCurrentRowData(rowCount);

    if (checkForDuplicates(currentRowData)) {
        showDynamicModal(messages.duplicateWarehouseWarning);
        return;
    }

    const newRowHtml = createNewWarehouseRowHtml(rowCount);
    $('#articleStockCheckTable tbody').append(newRowHtml);
    $('#addWarehouse').prop('disabled', false);
}

// Function to handle checkbox value change in warehouse table
function handleWarehouseCheckboxValueChange() {
    if ($(this).is(':checked')) {
        $(this).val('true');
    } else {
        $(this).val('false');
    }
}

// Function to handle checkbox value change
function handleCheckboxValueChange() {
    if ($(this).is(':checked')) {
        $(this).val('true');
    } else {
        $(this).val('false');
    }
}

// Remove Supplier Row Functionality
function removeSupplierRow() {
    const row = $(this).closest('tr');
    row.remove();
    $('#supplierTable tbody tr').each(function (index) {
        $(this).attr('row-index', index);
        $(this).find('select, input').each(function () {
            const id = $(this).attr('id');
            const name = $(this).attr('name');
            if (id) {
                const newId = id.replace(/_\d+$/, `_${index}`);
                $(this).attr('id', newId);
            }
            if (name) {
                const newName = name.replace(/\[\d+\]/, `[${index}]`);
                $(this).attr('name', newName);
            }
        });
    });
}

function handleOtherDefaultDeliveryChange() {
    var existingDefaults = $('#supplierTable tbody input[type="checkbox"][id^="defaultDelivery_"]:checked');

    if (this.checked && existingDefaults.length > 1) {
        showDynamicModal(messages.multipleDefaultWarehouse);
        $(this).prop('checked', false);
    }
}

// Function to handle default delivery checkbox change
function handleDefaultDeliveryChange() {
    var existingDefaults = $('#supplierTable tbody input[type="checkbox"].defaultSupplierCheckbox:checked');

    // Check if this checkbox is checked and if more than one is checked
    if (this.checked && existingDefaults.length > 1) {
        showDynamicModal(messages.multipleDefaultSupplier);
        $(this).prop('checked', false); // Uncheck this one
    }
}

// Add Supplier Functionality
function addSupplier(e) {
    e.preventDefault();

    const rowCount = $('#supplierTable tbody tr').length;
    const isDefaultChecked = rowCount === 0 ? 'checked' : '';
    const isDefaultValue = rowCount === 0 ? 'true' : 'false';

    const newRowHtml = createNewSupplierRowHtml(rowCount, isDefaultChecked, isDefaultValue);
    $('#supplierTable tbody').append(newRowHtml);
}

function createNewSupplierRowHtml(rowCount, isDefaultChecked, isDefaultValue) {
    const suppliersList = $('#articleSuppliers').html();
    const warehousesList = $('#articleWarehouses').html();

    return `
        <tr row-index=${rowCount}>
            <td style="width: 15%;">
                <select class='form-control form-control-sm small-select' name="ArticleWarehouseDeliveries[${rowCount}].ContactID">
                    ${suppliersList}
                </select>
            </td>
            <td style="width: 10%;">
                <input type='text' class='form-control form-control-sm small-input' placeholder="Order No." name="ArticleWarehouseDeliveries[${rowCount}].OrderNumber" />
            </td>
            <td style="width: 10%;">
                <input type='text' class='form-control form-control-sm small-input' placeholder="Amount" name="ArticleWarehouseDeliveries[${rowCount}].OrderAmount" />
            </td>
            <td style="width: 15%;">
                <select class='form-control form-control-sm small-select' name="ArticleWarehouseDeliveries[${rowCount}].WarehouseID">
                    ${warehousesList}
                </select>
            </td>
            <td style="width: 5%;">
                <div class="d-flex justify-content-center align-items-center form-check" style="height: 100%;margin-top:10px">
                    <input type='checkbox' class='form-check-input' name="ArticleWarehouseDeliveries[${rowCount}].IsActive" value="false" />
                </div>
            </td>
            <td style="width: 5%;">
                <div class="d-flex justify-content-center align-items-center" style="height: 100%;margin-top:10px">
                    <input type='checkbox' class='form-check-input defaultSupplierCheckbox' id='defaultDelivery_${rowCount}' name="ArticleWarehouseDeliveries[${rowCount}].IsDefaultSupplier" ${isDefaultChecked} value='${isDefaultValue}' />
                </div>
            </td>
            <td style="width: 5%;">
                <div class="d-flex justify-content-center align-items-center form-check" style="height: 100%;margin-top:10px">
                    <input type='checkbox' class='form-check-input' name="ArticleWarehouseDeliveries[${rowCount}].IsAvailable" value="false" />
                </div>
            </td>
            <td style="width: 15%;">
                <input type='date' class='form-control form-control-sm small-input' name="ArticleWarehouseDeliveries[${rowCount}].DateAvailable" />
            </td>
            <td style="width: 10%;">
                <button class="btn btn-sm btn-outline-danger" id='btnSupplierRemove' style="text-decoration: none;">Remove</button>
            </td>
        </tr>
    `;
}




function getCurrentRowData(rowCount) {
    const currentRowWarehouseID = $('#warehouse_' + (rowCount - 1)).val();
    const currentRowPurchasePeriod = $('#purchasePeriod_' + (rowCount - 1)).val();
    const currentRowAssortment = $('#assortment_' + (rowCount - 1)).is(':checked');

    return {
        index: rowCount - 1,
        warehouseID: currentRowWarehouseID,
        purchasePeriod: currentRowPurchasePeriod,
        assortment: currentRowAssortment
    };
}


function showDynamicModal(message) {
    $('#dynamicModalForArticlePurchase').appendTo("body");
    $('#dynamicModalForArticlePurchase').modal({ backdrop: 'static', show: true });
    $('#dynamicModalForArticleBody').text(message);

}

function createNewWarehouseRowHtml(rowCount) {
    const warehousesList = $('#articleWarehouses').html();
    const purchasePeriodsList = $('#purchasePeriods').html();

    return `
        <tr row-index=${rowCount}>
            <td style="width: 15%;">
                <select class='form-control form-control-sm small-select' id="warehouse_${rowCount}" name="ArticleWarehouses[${rowCount}].WarehouseID">
                    ${warehousesList}
                </select>
            </td>
            <td style="width: 10%;">
                <input type='text' class='form-control form-control-sm small-input' placeholder="Minimal stock" id="minimalStock_${rowCount}" name="ArticleWarehouses[${rowCount}].MinimumStock"/>
            </td>
            <td style="width: 10%;">
                <input type='text' class='form-control form-control-sm small-input' placeholder="Order amount" id="orderAmount_${rowCount}" name="ArticleWarehouses[${rowCount}].PurchaseAmount"/>
            </td>
            <td style="width: 10%;">
                <input type='text' class='form-control form-control-sm small-input' placeholder="Maximum stock" id="maximumStock_${rowCount}" name="ArticleWarehouses[${rowCount}].MaximumStock" />
            </td>
            <td style="width: 15%;">
                <select class='form-control form-control-sm small-select' id="purchasePeriod_${rowCount}" name="ArticleWarehouses[${rowCount}].PurchasePeriodID">
                    ${purchasePeriodsList}
                </select>
            </td>
            <td style="width: 5%;">
                <div class="d-flex justify-content-center align-items-center form-check" style="height: 100%;margin-top:10px">
                    <input type='checkbox' class='form-check-input form-check' id="assortment_${rowCount}" name="ArticleWarehouses[${rowCount}].Assortment" value="false" />                                                  
                </div>
            </td>
            <td style="width: 10%;">
                <button class="btn btn-sm btn-outline-danger" id='btnWarehouseRemove' style="text-decoration: none;">Remove</button>
            </td>
        </tr>
    `;
}
