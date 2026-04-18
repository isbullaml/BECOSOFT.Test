
function bindConditionFilterEvents(document) {
    $(document).on("click",
        "button[data-add], button[data-delete]",
        function (e) {
            e.preventDefault();
            let elem = $(this);
            const addDataValue = elem.data("add");
            const isAdd = addDataValue !== undefined;
            const isPreselectionCondition = elem.closest($("[data-id='preselection-condition-filter-container'")).length > 0;
            let container = elem.closest("[data-id='container']");
            if (isAdd) {
                switch (elem.data("add")) {
                    case "rule":
                        {
                            const group = elem.closest(".rules-group-container");
                            const rulesList = group.find(".rules-list:first");
                            const ruleTemplate = container.find("[data-id='rule-container-template']").children(":first");
                            const rule = ruleTemplate.clone();
                            addEntitySelectToRule(rule, isPreselectionCondition, container);
                            rulesList.append(rule);
                            rule.find(".rule-entity-container").find("select:first").change();
                            break;
                        }
                    case "group":
                        {
                            const rulesList = elem.closest(".rules-group-container").find(".rules-list:first");
                            const groupTemplate = container.find("[data-id='rule-group-container-template']").children(":first");
                            const group = groupTemplate.clone();
                            rulesList.append(group);

                            break;
                        }
                }
            } else {
                switch (elem.data("delete")) {
                    case "rule":
                        elem.closest(".rule-container").remove();
                        break;
                    case "group":
                        elem.closest(".rules-group-container").remove();
                        break;
                }
            }
            validateData(container);

        });

    $(document).on("focus",
        "div.rule-value-operator > [data-type]",
        function (e) {
            e.preventDefault();
            let elem = $(this);
            let container = elem.closest("[data-id='container']");
            clearErrors(elem, container);
        });

    $(document).on("click",
        "input[data-condition]",
        function (e) {
            e.preventDefault();
            let elem = $(this);
            handleConditionClick(elem);
        });

    $(document).on("click",
        "input[data-boolean-option]",
        function (e) {
            e.preventDefault();
            let elem = $(this);
            handleBooleanClick(elem);
            let container = elem.closest("[data-id='container']");
            validateData(container);
        });

    $(document).on("click",
        "input[data-exists-option]",
        function (e) {
            e.preventDefault();
            let elem = $(this);
            handleExistsClick(elem);
            let container = elem.closest("[data-id='container']");
            validateData(container);
        });

    $(document).on("click",
        "button[data-selection]",
        function (e) {
            e.preventDefault();
            let elem = $(this);
            const selectionMode = elem.data("selection");
            const select = elem.parent().find("select:not([data-type='multiselect'])");
            const multiselect = elem.parent().find("select[data-type='multiselect']");
            const vals = [];
            if (selectionMode !== "multiple") {
                const selValues = multiselect.val();
                const val = $.isArray(selValues) ? selValues[0] : selValues;
                if ($.isArray(selValues)) {
                    $.each(multiselect.val(),
                        function () {
                            const tempValue = parseInt(this, 10) || 0;
                            vals.push(tempValue);
                        }
                    );
                } else {
                    if (val !== "") {
                        const tempValue = parseInt(val, 10) || 0;
                        vals.push(tempValue);
                    }
                }
            } else {
                const selValues = select.val();
                const val = $.isArray(selValues) ? selValues[0] : selValues;
                if (val !== "") {
                    const tempValue = parseInt(val, 10) || 0;
                    vals.push(tempValue);
                }
            }

            setSelectionButtons(elem);
            if (selectionMode !== "multiple") {
                select.val(vals).change();
                initMultiSelect(select, { collapsible: false, refresh: true });
            } else {
                multiselect.val(vals).change();
                initMultiSelect(multiselect, { collapsible: false, refresh: true });
            }
        });

    $(document).on("change", "select[data-entity-select], select[data-entity], select[data-possible-property], select[data-type], input[data-checkbox]", function (e) {
        e.preventDefault();
         handleChanges($(this));
    });
    $(document).on("input", "input[data-type]", function (e) {
        e.preventDefault();
         handleChanges($(this));
    });
}

function processData(jsonData, container) {
    if (container === undefined || container === null || container.length === 0) {
        container = $(document);
    }

    prepareEntityOptions(jsonData.filterOptions, false, container);
    preparePreselectionOptions(jsonData.filterOptions, container);
    prepareFilterOptions(jsonData.filterOptions, container);
    prepareTypeOperators(jsonData.typeOperators, container);
    if (jsonData.preselectionFilterOption !== undefined) {
        const preselectionFilterOptions = [jsonData.preselectionFilterOption];
        prepareEntityOptions(preselectionFilterOptions, true, container);
        preparePreselectionOptions(preselectionFilterOptions, container);
        prepareFilterOptions(preselectionFilterOptions, container);
        prepareTypeOperators(preselectionFilterOptions, container);
        container.find("[data-id='preselection-container-title']").find("[data-preselection-title]").text(jsonData.preselectionFilterOption.friendlyName);
    } else {
        container.find("[data-id='preselect-option-container']").addClass("d-none");
    }
    if (jsonData.preselectionCondition !== undefined) {
        loadPreselectionConditionData(jsonData, container);
    }
    jsonData.preselectionCondition = undefined;
    if (jsonData.current !== undefined) {
        loadData(jsonData, container);
    }
    jsonData.condition = undefined;
    storeData(jsonData, container);
}

function getData(container) {
    if (container === undefined || container === null || container.length === 0) {
        container = $(document);
    }

    const preselectionConditionContainer = container.find("[data-id='preselection-condition-filter-container']");
    let preselectionConditions = undefined;
    if (!preselectionConditionContainer.hasClass("d-none") && preselectionConditionContainer.data("json") !== undefined) {
        const preselectionConditionRoot = preselectionConditionContainer.find("div.rules-group-container:first");
        preselectionConditions = parseRuleList(preselectionConditionRoot, false, true);
    }

    const filterContainer = container.find("[data-id='filter-container']");
    const rootGroup = filterContainer.find("div.rules-group-container:first");
    const conditions = parseRuleList(rootGroup, false, false);
    const result = {
        preselectionCondition: preselectionConditions,
        condition: conditions
    };
    return result;
}

function validateData(container) {
    clearErrors(null, container);
    const preselectionConditionContainer = container.find("[data-id='preselection-condition-filter-container']");
    if (!preselectionConditionContainer.hasClass("d-none") && preselectionConditionContainer.data("json") !== undefined) {
        const preselectionConditionRoot = preselectionConditionContainer.find("div.rules-group-container:first");
        parseRuleList(preselectionConditionRoot, true, true);
    }

    const filterContainer = container.find("[data-id='filter-container']");
    const rootGroup = filterContainer.find("div.rules-group-container:first");
    parseRuleList(rootGroup, true, false);
    return container.find(".has-error").not(".d-none").length === 0;
}

function storeData(data, container) {
    var json = {
        filterOptions: data.filterOptions,
        typeOperators: data.typeOperators
    };

    const filterContainer = container.find("[data-id='filter-container']");
    filterContainer.data("json", JSON.stringify(json));
    if (data.preselectionFilterOption !== undefined && data.preselectionFilterOption !== null) {
        json = {
            filterOptions: [data.preselectionFilterOption],
            typeOperators: data.typeOperators
        };
        const preselectionConditionContainer = container.find("[data-id='preselection-condition-filter-container']");
        preselectionConditionContainer.data("json", JSON.stringify(json));
    }
}

function retrieveData(isPreselectionCondition, container) {
    const selector = isPreselectionCondition ? "[data-id='preselection-condition-filter-container']" : "[data-id='filter-container']";

    if (container.find(selector).length === 0)
        return;

    const raw = container.find(selector).data("json");
    return JSON.parse(raw);
}

function parseRuleList(group, withValidation, isPreselectionCondition) {
    var logicalGrouping = group.find("div[data-conditions]:first").find('input:checked').val();
    if (logicalGrouping === undefined) {
        logicalGrouping = group.find("div[data-conditions]:first").find("input[checked='checked']").val();
        if (logicalGrouping === undefined) {
            const toChange = group.find("div[data-conditions]").find(`label.active`).find("input[data-condition]");
            toChange.prop("checked", true);
            logicalGrouping = toChange.val();
        }
    }
    if (logicalGrouping === undefined) {
        logicalGrouping = "and";
    }
    var newData = {
        logicalGrouping: logicalGrouping,
        conditions: []
    };
    const rulesList = group.find("div.rules-list:first");
    const rules = rulesList.children();
    rules.each(function (index, r) {
        const rule = $(r);
        if (rule.hasClass("rules-group-container")) {
            const parsedGroup = parseRuleList(rule, withValidation, isPreselectionCondition);
            newData.conditions.push(parsedGroup);
        } else {
            const condition = parseCondition(rule, withValidation, isPreselectionCondition);
            newData.conditions.push(condition);
        }
    });
    if (withValidation && rules.length === 0) {
        const errorContainer = group.find("div.error-container");
        errorContainer.addClass("has-error");
        errorContainer.removeClass("d-none");
        errorContainer.append("<div><span>No rules present</span></div>");
    }
    return newData;
}

function parseCondition(rule, withValidation) {
    const condition = {
        entity: rule.find("div.rule-entity-container:first").find("select:first").val(),
        preselectedValue: rule.find("div.rule-preselection-container:first").find("select:first").val(),
        property: rule.find("div.rule-filter-container:first").find("select:first").val(),
        operator: rule.find("div.rule-operator-container:first").find("select:first").val()
    };
    const input = rule.find("div.rule-value-container:first").find("[data-type]");

    var showError = false;
    switch (input.data("type")) {
        case "boolean":
        case "exists":
            var checkElem = input.find("input:checked");
            if (checkElem.length === 0) {
                checkElem = input.find("label.active").find("input:first");
            }
            condition.value = checkElem.attr('value');
            if (withValidation && (condition.value === undefined || condition.value === null)) {
                showError = true;
            }
            break;
        case "stringstrippedfromhtmllength":
        case "integer":
            {
                const tempVal = input.val();
                condition.value = parseInt(tempVal, 10);
                if (withValidation && (tempVal === undefined || tempVal === "" || condition.value === Number.NaN || condition.value === Number.POSITIVE_INFINITY || condition.value === Number.NEGATIVE_INFINITY)) {
                    showError = true;
                }
                break;
            }
        case "decimal":
            {
                const tempVal = input.val();
                condition.value = Number(tempVal);

                if (withValidation && (tempVal === undefined || tempVal === "" || condition.value === Number.NaN || condition.value === Number.POSITIVE_INFINITY || condition.value === Number.NEGATIVE_INFINITY)) {
                    showError = true;
                }
                break;
            }
        case "date":
            {
                const isRelative = input.find('input[type="radio"]:checked').val() === "true";
                let dateValue = {
                    isRelative: isRelative
                };
                if (isRelative) {
                    let relativeDateContainer = input.find("div[data-relative-date]");
                    dateValue.relativeDateConstantValue = parseInt(relativeDateContainer.find("select[data-relative-value]").val(), 10) || 0;
                    dateValue.offsetTypeValue = parseInt(relativeDateContainer.find("select[data-offset-type-value]").val(), 10) || 0;
                    const tempOffsetValue = relativeDateContainer.find("input[data-offset-value]").val();
                    dateValue.offsetValue = parseInt(tempOffsetValue, 10) || 0;
                    if (withValidation && dateValue.relativeDateConstantValue === 0) {
                        showError = true;
                    }
                } else {
                    const tempVal = input.find("div[data-date-time-value]").find("input")[0].value;
                    dateValue.value = tempVal;
                    if (withValidation && (!tempVal || tempVal === "")) {
                        showError = true;
                    }
                }
                //const tempVal = input[0].value;
                //condition.value = tempVal;
                condition.value = dateValue;
                break;
            }
        case "multiselect":
            {
                const checked = input.find("option:selected");
                const tempValues = [];
                $.each(checked,
                    function (i, v) {
                        tempValues.push({ value: parseInt($(v).val(), 10) || 0 });
                    });
                condition.values = tempValues;
                if (withValidation && (condition.values.length === 0)) {
                    showError = true;
                }
                break;
            }
        case "select":
            {
                const selValues = input.val();
                const tempValue = $.isArray(selValues) ? selValues[0] : selValues;
                condition.value = parseInt(tempValue, 10);
                if (withValidation && (!tempValue)) {
                    showError = true;
                }
                break;
            }
        case "linkedselect":
            {
                const selects = input.find("select:not([data-type='multiselect'])");
                const temp = [];
                $.each(selects,
                    function (index, s) {
                        const sel = $(s);
                        const multiSel = sel.parent().parent().find("select[data-type='multiselect']");
                        const isSingle = sel.parent().is(":visible");
                        const isMulti = multiSel.parent().is(":visible");
                        const prop = sel.attr("data-possible-property");
                        const value = {
                            property: prop
                        };

                        if (isSingle) {
                            const selValues = sel.val();
                            const val = $.isArray(selValues) ? selValues[0] : selValues;
                            if (val === "") {
                                return false;
                            }

                            temp.push(value);
                            const tempValue = parseInt(val, 10) || 0;
                            value.value = tempValue;
                            return tempValue !== 0;
                        } else if (isMulti) {
                            const checked = multiSel.val();
                            const tempValues = [];
                            $.each(checked,
                                function(i, v) {
                                    tempValues.push({ value: parseInt(v, 10) || 0 });
                                });

                            temp.push(value);
                            value.values = tempValues;
                            return false;
                        } else {
                            return false;
                        }
                    });
                condition.values = temp;
                break;
            }
        case "search":
            {
                const selValues = input.find("input[type='hidden']").val().split(",");
                const tempValues = [];
                $.each(selValues,
                    function (i, v) {
                        tempValues.push({ value: parseInt(v, 10) || 0 });
                    });
                condition.values = tempValues;
                if (withValidation && (condition.values.length === 0)) {
                    showError = true;
                }
                break;
            }
        default:
            condition.value = input.val();
            break;
    }
    if (showError) {
        const errorContainer = rule.find("div.error-container");
        errorContainer.addClass("has-error");
        errorContainer.removeClass(" d-none");
    }
    return condition;
}

function loadData(jsonData, container) {
    const current = jsonData.current;
    handleGroup(current, undefined, jsonData.filterOptions, jsonData.typeOperators, false, container);
    const filterContainer = container.find("[data-id='filter-container']");
    filterContainer.find('button[data-delete="group"]:first').remove();
}

function loadPreselectionConditionData(jsonData, container) {
    const current = jsonData.preselectionCondition;
    handleGroup(current, undefined, [jsonData.preselectionFilterOption], jsonData.typeOperators, true, container);
    const preselectionConditionContainer = container.find("[data-id='preselection-condition-filter-container']");
    preselectionConditionContainer.find('button[data-delete="group"]:first').remove();
}

function handleGroup(condition, parentGroup, filterOptions, typeOperators, isPreselectionCondition, container) {
    const groupTemplate = container.find("[data-id='rule-group-container-template']").children(":first");
    const group = groupTemplate.clone();

    const selector = isPreselectionCondition ? "[data-id='preselection-condition-filter-container']" : "[data-id='filter-container']";
    const filterContainer = container.find(selector);
    const rulesList = (parentGroup === undefined || parentGroup === null) ? filterContainer : parentGroup.find("div.rules-list").first();
    rulesList.append(group);
    const condRadio = group.find("div.group-conditions").find(`input[value=${condition.logicalGrouping}]`);
    handleConditionClick(condRadio);
    $.each(condition.conditions,
        function (index, condition) {
            if (condition.logicalGrouping === undefined) {
                handleRule(condition, group, filterOptions, typeOperators, isPreselectionCondition, container);
            } else {
                handleGroup(condition, group, filterOptions, typeOperators, isPreselectionCondition, container);
            }
        });
}

function addEntitySelectToRule(rule, isPreselectionCondition, container) {
    const selector = isPreselectionCondition ? "[data-id='preselection-condition-entity-option-container']" : "[data-id='entity-option-container']";
    const filterOptionContainer = container.find(selector);
    const entityOptions = filterOptionContainer.find(`select[data-entity-select="1"]`).first().clone();
    rule.find("div.rule-entity-container").append(entityOptions);
    return entityOptions;
}

function handleRule(condition, group, filterOptions, typeOperators, isPreselectionCondition, container) {
    const ruleTemplate = container.find("[data-id='rule-container-template']").children(":first");
    let rulesList = group.find("div.rules-list").first();

    let entity = filterOptions.find(x => x.entity === condition.entity);

    var rule = ruleTemplate.clone();
    let entityOptions = addEntitySelectToRule(rule, isPreselectionCondition, container);
    entityOptions.val(condition.entity);
    let preselectionValue;
    if (entity.preselectionValues !== undefined) {
        let preselectOptions = container.find("[data-id='preselect-option-container']").find(`select[data-entity="${condition.entity}"]`).first().clone();
        let preSelectionContainer = rule.find("div.rule-preselection-container");
        preSelectionContainer.append(preselectOptions);
        preSelectionContainer.removeClass("d-none");

        preselectOptions.val(condition.preselectedValue);
        preselectionValue = condition.preselectedValue;
    } else {
        let preSelectionContainer = rule.find("div.rule-preselection-container");
        preSelectionContainer.addClass("d-none");
        preselectionValue = undefined;
    }
    let propertyOptions = container.find("[data-id='filter-option-container']").find(`select[data-entity="${condition.entity}"]`).first().clone();
    rule.find("div.rule-filter-container").append(propertyOptions);
    propertyOptions.val(condition.property);
    let field = entity.properties.find(x => x.property === condition.property);
    if (field === undefined || field === null) {
        console.log("Cannot find field " + condition.property);
        return;
    }
    let typeOperator = typeOperators.find(x => x.dataType === field.dataType);
    let operatorOptions = container.find("[data-id='type-option-container']").find(`select[data-type=${field.dataType}]`).first().clone();
    const operatorContainer = rule.find("div.rule-operator-container");
    operatorContainer.append(operatorOptions);
    const operSelect = operatorContainer.find("select:first").val(condition.operator);
    const value = createValueInput(field, condition, typeOperator, entity, preselectionValue, container);

    rule.find("div.rule-value-container").append(value);
    handleMultiSelectInit(value, field);

    rulesList.append(rule);

}

function createValueInput(field, condition, typeOperator, entity, preselectedValue, container) {
    var value = undefined;
    var templateContainer = container.find("[data-id='value-option-template-container']");
    switch (field.dataType) {
        case "integer":
            value = templateContainer.find('[data-type="integer"]').clone();
            value.val(condition.value);
            break;
        case "string":
            value = templateContainer.find('[data-type="text"]').clone();
            value.val(condition.value);
            break;
        case "exists":
            {
                value = templateContainer.find('[data-type="exists"]').clone();
                $.each(typeOperator.dataTypeValues,
                    function (index, option) {
                        let name = option.friendlyName;
                        if (name === undefined || name === null || name === "") {
                            name = option.value;
                        }
                        value.append(`<label class="btn btn-sm btn-light"><input type="radio" value="${option.value}" data-exists-option>${name}</label> `);
                    });
                const tempVal = condition.value && condition.value.length !== 0 ? condition.value : typeOperator.dataTypeValues[0].value;
                handleExistsClick(value.find(`input[value="${tempVal}"]`));
                break;
            }
        case "boolean":
            {
                value = templateContainer.find('[data-type="boolean"]').clone();
                $.each(typeOperator.dataTypeValues,
                    function (index, option) {
                        let name = option.friendlyName;
                        if (name === undefined || name === null || name === "") {
                            name = option.value;
                        }
                        value.append(`<label class="btn btn-sm btn-light"><input type="radio" value="${option.value}" data-boolean-option>${name}</label> `);
                    });
                const tempVal = condition.value && condition.value.length !== 0 ? condition.value : typeOperator.dataTypeValues[0].value;
                handleBooleanClick(value.find(`input[value="${tempVal}"]`));
                break;
            }
        case "date":
            {
                let dateValue = condition.value;
                if (dateValue === undefined || dateValue === null || dateValue === "") {
                    dateValue = {
                        isRelative: true,
                        relativeDateConstantValue: typeOperator.relativeDateValues[0].key,
                        offsetTypeValue: typeOperator.offsetTypeValues[0].key,
                        offsetValue: 0
                    };
                }

                value = templateContainer.find('[data-type="date"]').clone();
                let relativeDateContainer = value.find("div[data-relative-date]");
                let relativeConstantSelect = relativeDateContainer.find("select[data-relative-value]");
                $.each(typeOperator.relativeDateValues,
                    function (index, option) {
                        relativeConstantSelect.append(`<option value="${option.key}">${option.value}</option> `);
                    });
                let offsetTypeSelect = relativeDateContainer.find("select[data-offset-type-value]");
                $.each(typeOperator.offsetTypeValues,
                    function (index, option) {
                        offsetTypeSelect.append(`<option value="${option.key}">${option.value}</option> `);
                    });
                value.on("change click",
                    'input[type="radio"]',
                    function (e) {
                        const elem = $(this);
                        const isChecked = elem.is(":checked");
                        const isRelative = elem.val() === "true";
                        const parent = elem.closest('div[data-type="date"]');
                        const radios = parent.find('input[type="radio"]');
                        if (isChecked) {
                            radios.not(elem).prop("checked", false);
                        }
                        const relativeContainer = parent.find("div[data-relative-container]");
                        const dateContainer = parent.find("div[data-date-container]");
                        const dateInputs = dateContainer.find('input:not([type="radio"])');
                        const relativeInputs = relativeContainer.find('input:not([type="radio"])');
                        const relativeSelects = relativeContainer.find("select");
                        if (isRelative) {
                            dateInputs.attr("disabled", "disabled");
                            relativeInputs.removeAttr("disabled");
                            relativeSelects.removeAttr("disabled");
                        } else {
                            dateInputs.removeAttr("disabled");
                            relativeInputs.attr("disabled", "disabled");
                            relativeSelects.attr("disabled", "disabled");
                        }
                    });
                let option = value.find(`input[type="radio"][value="${dateValue.isRelative}"]`);
                option.prop("checked", true);
                option[0].click();
                if (!dateValue.isRelative) {
                    value.find("div[data-date-time-value]").find("input").val(dateValue.value);
                } else {
                    relativeConstantSelect.val(dateValue.relativeDateConstantValue);
                    offsetTypeSelect.val(dateValue.offsetTypeValue);
                    let offsetValueInput = relativeDateContainer.find("input[data-offset-value]");
                    offsetValueInput.val(dateValue.offsetValue);
                }
                break;
            }
        case "decimal":
            value = templateContainer.find('[data-type="decimal"]').clone();
            value.val(condition.value);
            break;
        case "multiselect":
        case "select":
            {
                value = templateContainer.find('[data-type="multiselect"]').clone();
                value.attr("name", entity.entity + '_' + field.property);
                value.append('<option>-</option>');

                let valsToSet = [];
                if (condition.values && condition.values.length > 0) {
                    valsToSet = condition.values.map(v => v.value);
                } else {
                    valsToSet.push(condition.value);
                }
                let possibleValues = undefined;
                if (entity.preselectionValues && entity.preselectionValues.length !== 0) {
                    const preselectionValue = entity.preselectionValues.find(pv => preselectedValue === undefined || pv.id === preselectedValue);
                    const pr = preselectionValue.properties.find(p => p.property === field.property);
                    if (pr !== undefined && pr !== null) {
                        possibleValues = pr.possibleValues;
                    }
                }
                if (possibleValues === undefined) {
                    possibleValues = field.possibleValues;
                }

                $.each(possibleValues,
                    function (index, option) {
                        const isChecked = valsToSet.includes(option.id);
                        value.append(`<option value='${option.id}' ${isChecked ? "selected" : ""}>
                                ${option.value}
                            </option>
                        `);
                    });
                break;
            }
        case "linkedselect":
            {
                value = templateContainer.find('[data-type="linkedselect"]').clone();
                let property;
                let possibleProperties;
                if (entity.preselectionValues && entity.preselectionValues.length !== 0) {
                    const preselectionValue = entity.preselectionValues.find(pv => preselectedValue === undefined || pv.id === preselectedValue);
                    const pr = preselectionValue.properties.find(p => p.property === field.property);
                    possibleProperties = pr.possibleProperties;
                    property = pr.possibleValues[0];
                } else {
                    possibleProperties = field.possibleProperties;
                    property = field.possibleValues[0];
                }

                var singleText = templateContainer.find("span[data-single-title]").text();
                var multipleText = templateContainer.find("span[data-multiple-title]").text();
                $.each(possibleProperties,
                    function (index, possibleProperty) {
                        let name = possibleProperty.friendlyName;
                        if (name === undefined || name === null || name === "") {
                            name = possibleProperty.property;
                        }

                        let margin = index === 0 ? "" : "ml-2";
                        const propContainer = $(`
<div class='form-group float-left ${margin}' data-possible-property='${possibleProperty.property}'>
    <small>${name}</small>
    <div class="d-none bcs-list-group"><select data-possible-property='${possibleProperty.property}' name='${entity.entity}_${field.property}_single' class='form-control form-control-sm'></select></div>
    <div class="d-none bcs-list-group"><select data-type="multiselect" data-possible-property='${possibleProperty.property}' name='${entity.entity}_${field.property}_multiple' class='form-control form-control-sm' multiple></select></div>
    <span class='form-control'>-</span>
    <button class='btn btn-sm btn-outline d-none m-1 bcs-button-toggle-multiple-single' data-selection='single'>${singleText}</button>
    <button class='btn btn-sm btn-outline d-none m-1 bcs-button-toggle-multiple-single' data-selection='multiple'>${multipleText}</button>
</div>`);
                        value.append(propContainer);
                    });
                $.each(possibleProperties,
                    function (index, possibleProperty) {
                        const ppContainer = value.find(`div[data-possible-property="${possibleProperty.property}"]`);
                        const ppSelect = ppContainer.find("select:not([data-type='multiselect'])");
                        const ppMultiSelect = ppContainer.find("select[data-type='multiselect']");
                        const ppSpan = ppContainer.find("span");
                        const propIsUndefined = (property === null || property === undefined);
                        if ((propIsUndefined || (!propIsUndefined && (property.values === undefined) || property.property !== possibleProperty.property))) {
                            ppContainer.addClass("d-none");
                            return true;
                        }
                        ppSelect.find("option").remove();
                        ppMultiSelect.find("option").remove();
                        ppSelect.append(`<option value="">-</option> `);
                        ppMultiSelect.append(`<option value="">-</option> `);
                        $.each(property.values,
                            function (index, pv) {
                                ppSelect.append(`<option value="${pv.id}">${pv.value}</option> `);
                                ppMultiSelect.append(`<option value="${pv.id}">${pv.value}</option>`);
                            });
                        const condValue = condition.values.find(cv => cv.property === possibleProperty.property);
                        ppMultiSelect.parent().removeClass("d-none");
                        ppSpan.addClass("d-none");
                        if (condValue === undefined || condValue === null) {
                            ppMultiSelect.parent().addClass("d-none");
                            ppSelect.parent().removeClass("d-none");
                            ppSelect.parent().siblings('button[data-selection="multiple"]').removeClass("d-none");
                            if (index === 0) {
                                ppSelect.val(0);
                                property = undefined;
                            }
                            return true;
                        }
                        if (condValue.value !== undefined && condValue.value !== null) {
                            ppMultiSelect.parent().addClass("d-none");
                            ppSelect.parent().removeClass("d-none");
                            ppSelect.parent().siblings('button[data-selection="multiple"]').removeClass("d-none");
                            ppSelect.val(condValue.value);
                            property = property.values.find(f => f.id === condValue.value).linked;
                        } else {
                            ppMultiSelect.parent().removeClass("d-none");
                            ppSelect.parent().addClass("d-none");
                            ppSelect.parent().siblings('button[data-selection="single"]').removeClass("d-none");
                            let vals = [];
                            $.each(condValue.values,
                                function (index, pv) {
                                    vals.push(pv.value);
                                });
                            ppMultiSelect.val(vals);
                            property = undefined;
                        }
                        return true;
                    });
                break;
            }
        case "stringstrippedfromhtmllength":
            value = templateContainer.find('[data-type="stringstrippedfromhtmllength"]').clone();
            value.val(condition.value);
            break;
        case "stringstrippedfromhtml":
            value = templateContainer.find('[data-type="stringstrippedfromhtml"]').clone();
            value.val(condition.value);
            break;
        case "search":
            {
                value = templateContainer.find('[data-type="search"]').clone();
                const input = value.find("input[type='hidden']");
                const valsToSet = condition.values.map(v => v.value);
                input.val(valsToSet);
                
                const selectedText = templateContainer.find("span[data-selected-title]").text();
                const labelInput = value.find("input[type='text']");
                labelInput.val(valsToSet.length + " " + selectedText);

                let searchButton = value.find("[data-action]");
                searchButton.on("click", function (e) {
                    window[field.searchAction](e);

                    labelInput.val(valsToSet.length + " " + selectedText);
                });
                break;
            }
    }
    return value;
}

function prepareTypeOperators(typeOperators, container) {
    const typeOptionContainer = container.find("[data-id='type-option-container']");
    $.each(typeOperators,
        function (index, operator) {
            const select = $(`<select class="form-control form-control-sm" data-type="${operator.dataType}"></select>`);
            $.each(operator.operators,
                function (index, operator) {
                    const option = $(`<option value=${operator.code}>${operator.value}</option>`);
                    select.append(option);
                });
            typeOptionContainer.append(select);
        });
}

function prepareFilterOptions(filterOptions, container) {
    const filterOptionContainer = container.find("[data-id='filter-option-container']");
    $.each(filterOptions,
        function (index, option) {
            const select = $(`<select class="form-control form-control-sm" data-entity="${option.entity}"></select>`);
            $.each(option.properties,
                function (index, field) {
                    let name = field.friendlyName;
                    if (name === undefined || name === null || name === "") {
                        name = field.property;
                    }
                    const option = $(`<option value="${field.property}">${(name)}</option>`);
                    select.append(option);
                });
            filterOptionContainer.append(select);
        });
}

function prepareEntityOptions(filterOptions, isPreselectionCondition, container) {
    const selector = isPreselectionCondition ? "[data-id='preselection-condition-entity-option-container']" : "[data-id='entity-option-container']";
    const filterOptionContainer = container.find(selector);
    const select = $(`<select class="form-control form-control-sm" data-entity-select="1"></select>`);
    $.each(filterOptions,
        function (index, filterOption) {
            let name = filterOption.friendlyName;
            if (name === undefined || name === null || name === "") {
                name = filterOption.entity;
            }
            const option = $(`<option value="${filterOption.entity}">${name}</option>`);
            select.append(option);
            preparePreselectionOptions(filterOption, container);
        });
    filterOptionContainer.append(select);
}

function preparePreselectionOptions(filterOption, container) {
    if (filterOption.preselectionValues === undefined) {
        return;
    }
    const preselectOptionContainer = container.find("[data-id='preselect-option-container']");
    const select = $(`<select class="form-control form-control-sm" data-entity="${filterOption.entity}"></select>`);
    $.each(filterOption.preselectionValues,
        function (index, preselectionOption) {
            const option = $(`<option value="${preselectionOption.id}">${preselectionOption.value}</option>`);
            select.append(option);
        });
    preselectOptionContainer.append(select);
}

function clearErrors(elem, container) {
    const errorContainer = elem ? elem.siblings("div.error-container") : container.find("div.error-container");
    errorContainer.children("div").remove();
    errorContainer.removeClass("has-error");
    errorContainer.addClass(" d-none");
}

function handleBooleanClick(elem) {
    handleRadioClick(elem, 'div[data-type="boolean"]', "input[data-boolean-option]");
}

function handleExistsClick(elem) {
    handleRadioClick(elem, 'div[data-type="exists"]', "input[data-exists-option]");
}

function handleConditionClick(elem) {
    handleRadioClick(elem, "div[data-conditions]", "input[data-condition]");
}

function handleRadioClick(elem, parentDiv, radioDataAttr) {
    const parent = elem.closest(parentDiv);
    const radios = parent.find(radioDataAttr);
    radios.removeAttr("checked");
    radios.parent().removeClass("active");
    elem.attr("checked", "checked");
    elem.parent().toggleClass("active");
}

function handleChanges(elem) {
    let container = elem.closest("[data-id='container']");
    if (container === undefined || container === null || container.length === 0) {
        container = $(document);
    }
    const ruleContainer = elem.closest(".rule-container");
    const parents = ruleContainer.find("[data-rule]");
    const parentsPerType = $.map(parents,
        p => {
            const pa = $(p);
            const x = {
                type: pa.attr("data-rule"),
                value: pa
            };
            return x;
        });
    const parent = elem.closest("[data-rule]");
    clearErrors(parent);
    const type = parent.attr("data-rule");
    const isPreselectionCondition = elem.closest($("[data-id='preselection-condition-filter-container']")).length > 0;
    const data = retrieveData(isPreselectionCondition, container);
    switch (type) {
        case "entity":
            {
                const entityVal = elem.val();
                const options = data.filterOptions.find(x => x.entity === entityVal);
                const preselectionParent = $(parentsPerType.find(p => p.type === "preselection"))[0];
                const preselectionParentValue = $(preselectionParent.value);
                preselectionParentValue.children().remove();
                if (options.preselectionValues !== undefined) {
                    const preselectOptions = container.find("[data-id='preselect-option-container']").find(`select[data-entity="${options.entity}"]`).first().clone();
                    preselectionParentValue.append(preselectOptions);
                    preselectionParentValue.removeClass("d-none");
                    preselectOptions.val(options.preselectionValues[0].id).change();
                } else {
                    preselectionParentValue.addClass("d-none");
                }
                const field = options.properties.find(f => f !== undefined);
                const propertyOptions = container.find("[data-id='filter-option-container']").find(`select[data-entity="${options.entity}"]`).first().clone();
                const filterParent = parentsPerType.find(p => p.type === "filter").value;
                filterParent.children().remove();
                filterParent.append(propertyOptions);
                propertyOptions.val(field.property).change();
            }
            break;
        case "preselection":
            {
                const entityParent = parentsPerType.find(p => p.type === "entity").value;
                const entityVal = entityParent.find("select:first").val();
                const options = data.filterOptions.find(x => x.entity === entityVal);
                if (options.preselectionValues !== undefined) {
                    const filterParent = parentsPerType.find(p => p.type === "filter").value;
                    filterParent.find(`select[data-entity="${options.entity}"]`).change();
                }
            }
            break;
        case "filter":
            {
                const entityParent = parentsPerType.find(p => p.type === "entity").value;
                const entityVal = entityParent.find("select:first").val();
                const options = data.filterOptions.find(x => x.entity === entityVal);
                const field = options.properties.find(f => f.property === elem.val());
                const typeOperator = data.typeOperators.find(x => x.dataType === field.dataType);
                const operatorOptions = container.find("[data-id='type-option-container']").find(`select[data-type=${field.dataType}]`).first().clone();
                const operatorParent = parentsPerType.find(p => p.type === "operator").value;
                operatorParent.children().remove();
                operatorParent.append(operatorOptions);
                operatorOptions.val(typeOperator.operators[0].code).change();
                let preselectionValue;
                if (options.preselectionValues !== undefined) {
                    let preselectedValueElem = ruleContainer.find("[data-rule='preselection']").find(`select[data-entity="${options.entity}"]`);
                    preselectionValue = parseInt(preselectedValueElem.val(), 10);
                } else {
                    preselectionValue = undefined;
                }

                const condition = {
                    value: "",
                    values: []
                };
                const value = createValueInput(field, condition, typeOperator, options, preselectionValue, container);

                const valueParent = parentsPerType.find(p => p.type === "value").value;
                valueParent.children().remove();
                valueParent.append(value);
                handleMultiSelectInit(value, field);
            }
            break;
        case "operator":
            // nothing required currently
            break;
        case "value":
            if (elem.data("possible-property") !== undefined) {
                handleLinkedSelect(elem, data, parentsPerType);
            }
            validateData(container);
            break;
    }
}

function handleMultiSelectInit(value, field, refresh = false) {
    if (field.dataType === "select" || field.dataType === "multiselect") {
        initMultiSelect(value, { collapsible: true, refresh: refresh });
        const valueParent = value.parent();
        //valueParent.find('.list-group').removeClass('list-group'); // commented this line below because we use those list-group elements for style, no idea why they had to be removed in the first place
        valueParent.css("max-width", "400px");
        valueParent.css("width", "400px");
        valueParent.addClass("bcs-list-group"); // necessary for style
    } else if (field.dataType === "linkedselect") {
        $.each(value.find('select'),
            function() {
                const el = $(this);
                initMultiSelect(el, { collapsible: false, refresh: refresh });
            });
    }
}

function handleLinkedSelect(elem, data, parentsPerType) {
    const parentPossibleProperty = elem.closest("div[data-possible-property]");
    const possiblePropertyContainer = parentPossibleProperty.parent();
    const entityParent = parentsPerType.find(p => p.type === "entity").value;
    const entityVal = entityParent.find("select:first").val();
    const preselectionParent = parentsPerType.find(p => p.type === "preselection").value;
    const preselectedValue = parseInt(preselectionParent.find("select:first").val(), 10);
    const entity = data.filterOptions.find(x => x.entity === entityVal);
    const filterParent = parentsPerType.find(p => p.type === "filter").value;
    const filterVal = filterParent.find("select:first").val();
    const field = entity.properties.find(f => f.property === filterVal);
    let property;
    let possibleProperties;
    if (entity.preselectionValues && entity.preselectionValues.length !== 0) {
        const preselectionValue = entity.preselectionValues.find(pv => preselectedValue === undefined || pv.id === preselectedValue);
        const pr = preselectionValue.properties.find(p => p.property === field.property);
        possibleProperties = pr.possibleProperties;
        property = pr.possibleValues[0];
    } else {
        possibleProperties = field.possibleProperties;
        property = field.possibleValues[0];
    }

    var indexOfCurrentPossibleProperty = undefined;
    $.each(possibleProperties,
        function (index, possibleProperty) {
            const ppContainer = possiblePropertyContainer.find(`div[data-possible-property="${possibleProperty.property}"]:not([data-type="multiselect"])`);
            if (ppContainer.is(parentPossibleProperty)) {
                indexOfCurrentPossibleProperty = index;
            }
        });
    $.each(possibleProperties,
        function (index, possibleProperty) {
            const ppContainer = possiblePropertyContainer.find(`div[data-possible-property="${possibleProperty.property}"]:not([data-type="multiselect"])`);
            const ppSelect = ppContainer.find("select:not([data-type='multiselect'])");
            const ppMultiSelect = ppContainer.find("select[data-type='multiselect']");
            if (index < indexOfCurrentPossibleProperty) {
                ppContainer.removeClass("d-none");
                const value = ppSelect.val();
                let propVal = property.values.find(p => p.id === (parseInt(value, 10) || 0));
                if (propVal === undefined || propVal === null) {
                    property = undefined;
                } else {
                    property = propVal.linked;
                }
            } else if (index === indexOfCurrentPossibleProperty) {
                ppContainer.removeClass("d-none");
                let isSingle = ppSelect.parent().is(":visible");
                if (isSingle) {
                    const value = ppSelect.val();
                    let propVal = property.values.find(p => p.id === (parseInt(value, 10) || 0));
                    if (propVal === undefined || propVal === null) {
                        property = undefined;
                    } else {
                        property = propVal.linked;
                    }
                } else {
                    property = undefined;
                }
            } else if (index === indexOfCurrentPossibleProperty + 1) {
                if (property === undefined) {
                    ppContainer.addClass("d-none");
                } else {
                    ppContainer.removeClass("d-none");
                    ppSelect.find("option").remove();
                    ppMultiSelect.find("option").remove();
                    ppSelect.parent().removeClass("d-none");
                    const ppSpan = ppContainer.find("span");
                    ppSpan.addClass("d-none");
                    ppSelect.append(`<option value="">-</option> `);
                    ppMultiSelect.append(`<option value="">-</option>`);
                    $.each(property.values,
                        function (i, pv) {
                            ppSelect.append(`<option value="${pv.id}">${pv.value}</option> `);
                            ppMultiSelect.append(`<option value="${pv.id}">${pv.value}</option> `);
                        });
                    const btn = ppContainer.find('button[data-selection="single"]');
                    initMultiSelect(ppSelect, { collapsible: false, refresh: true });
                    initMultiSelect(ppMultiSelect, { collapsible: false, refresh: true });
                    setSelectionButtons(btn);
                }
            } else {
                const previousPossibleProperty = possiblePropertyContainer.find(`div[data-possible-property="${possibleProperties[index - 1].property}"]`);
                const previousSelect = previousPossibleProperty.find("select:not([data-type='multiselect'])");
                if (previousSelect !== undefined || previousPossibleProperty.hasClass("d-none")) {
                    ppContainer.addClass("d-none");
                }
            }
        });
}

function setSelectionButtons(elem) {
    elem.addClass("d-none");
    elem.siblings("button[data-selection]").removeClass("d-none");
    const selectionMode = elem.data("selection");
    const select = elem.parent().find("select:not([data-type='multiselect'])");
    const multiselect = elem.parent().find("select[data-type='multiselect']");
    const nextPossibleProperty = elem.closest("div[data-possible-property]").next();
    const parentPossibleProp = elem.closest("div[data-possible-property]");
    const siblings = parentPossibleProp.parent().find("div[data-possible-property]");
    $(siblings.get().reverse()).each(function (index, pp) {
        const possibleProperty = $(pp);
        if (possibleProperty.is(parentPossibleProp)) {
            return false;
        }
        possibleProperty.find("select:not([data-type='multiselect'])").val([]);
        possibleProperty.addClass("d-none");
        const ppSelect = possibleProperty.find("select:not([data-type='multiselect'])");
        const ppMultiSelect = possibleProperty.find("select[data-type='multiselect']");
        const ppSpan = possibleProperty.find("span");
        ppSelect.parent().addClass("d-none");
        ppMultiSelect.parent().addClass("d-none");
        ppSpan.removeClass("d-none");
        return true;
    });
    if (selectionMode !== "multiple") {
        select.parent().removeClass("d-none");
        multiselect.parent().addClass("d-none");
        if (nextPossibleProperty) {
            const ppSelect = nextPossibleProperty.find("select:not([data-type='multiselect'])");
            const ppMultiSelect = nextPossibleProperty.find("select[data-type='multiselect']");
            if (ppSelect.find("option").length > 0) {
                nextPossibleProperty.removeClass("d-none");
                const ppSpan = nextPossibleProperty.find("span");
                ppSelect.parent().removeClass("d-none");
                ppMultiSelect.parent().addClass("d-none");
                ppSpan.addClass("d-none");
            } else {
                nextPossibleProperty.find("select:not([data-type='multiselect'])").val([]);
                nextPossibleProperty.addClass("d-none");
                const ppSelect = nextPossibleProperty.find("select:not([data-type='multiselect'])");
                const ppSpan = nextPossibleProperty.find("span");
                ppSelect.parent().addClass("d-none");
                ppMultiSelect.parent().addClass("d-none");
                ppSpan.removeClass("d-none");
            }
        }
    } else {
        select.parent().addClass("d-none");
        multiselect.parent().removeClass("d-none");
        if (nextPossibleProperty) {
            nextPossibleProperty.find("select:not([data-type='multiselect'])").val([]);
            nextPossibleProperty.addClass("d-none");
            const ppSelect = nextPossibleProperty.find("select:not([data-type='multiselect'])");
            const ppSpan = nextPossibleProperty.find("span");
            ppSelect.parent().addClass("d-none");
            ppSpan.removeClass("d-none");
        }
    }
}