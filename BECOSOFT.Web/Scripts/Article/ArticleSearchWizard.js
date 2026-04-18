function searchWizard() {
    var fileInfoID = 0;
    var currentErrorIndex = 0;
    var currentBlankIndex = 0;
    var timeOutID = 0;

    function initStep1() {
        var matchingTemplateValue = $('#NewArticleSearchViewModel_MatchingTemplates option:selected').text();
        $("#matchingTypeInfo").html(matchingTemplateValue);
        updateStepInfo("searchstep1");
    }

    initStep1();

    function updateStepInfo(currentStep) {
        switch (currentStep) {
            case "searchstep1":
                $("#stepInfoTitle").html(step1Title);
                $("#currentStepInfo").html(step1Info);
                break;
            case "searchstep2":
                $("#stepInfoTitle").html(step2Title);
                $("#currentStepInfo").html(step2Info);
                break;
            case "searchstep3":
                $("#stepInfoTitle").html(step3Title);
                $("#currentStepInfo").html(step3Info);
                break;
            case "searchstep4":
                $("#stepInfoTitle").html(step4Title);
                $("#currentStepInfo").html(step4Info);
                break;
            default:
        }
    }

    $(async function () {
        await searchUpdateInfo();
        $("#modal-content[data-cascading][data-cascading-initial-value]:not([data-initial-value='0'])").trigger("change");

        const jsonContainers = $("[data-search-step-container='5'] div[data-json-container]");
        $(jsonContainers).each(function () {
            const container = $(this);
            const json = container.data("json-container");
            const parentContainer = container.find("[data-id='container']");
            processData(json, parentContainer);
        });
    });

    //Next step
    $("#modal-content").on("click", "div[data-search-step-container] button[data-action='searchnext']", async function (e) {
            e.preventDefault();
            const currentStepDiv = $(this).closest("div[data-search-step-container]");
            const currentStep = currentStepDiv.data("search-step-container");

            const validationResult = await validateSearchStep(currentStepDiv);
            if (!validationResult) {
                //Previous step is invalid
                return;
            }

            const nextStep = currentStep + 1;

            const nextStepDiv = $(`#modal-content div[data-search-step-container='${nextStep}']`);
            if (!nextStepDiv) {
                //There is no next step
                return;
            }

            const currentStepProgress = $(`#modal-content div[data-search-step='${currentStep}']`);
            $(currentStepProgress).removeClass("is-active");
            $(currentStepProgress).addClass("is-complete");
            currentStepDiv.addClass("d-none");

            const nextStepProgress = $(`#modal-content div[data-search-step='${nextStep}']`);
            $(nextStepProgress).addClass("is-active");

            if ($(nextStepProgress).attr('id') === 'searchstep2') {
                var matchingTemplateID = $('#NewArticleSearchViewModel_MatchingTemplates').val();
                var url = urls.getMatching + `?importFileID=${fileInfoID}&importTemplateID=${matchingTemplateID}`;
                becosoft.ajax(url,
                    { type: "GET", async: false },
                    function (result) {
                        addMatchingView(result);
                    });

                var allSelects = $("#matchColumnsStep select");
                var blanksCount = 0;
                allSelects.each(function () {
                    if ($(this).val() !== "0") {
                        $(this).closest('.row').find('input:checkbox').prop('checked', true);
                        $(this).closest('.row').find('input:checkbox').trigger("change");
                    }
                });

                $("#doublesCount").removeClass("d-none");
                $("#blanksCount").removeClass("d-none");
            }

            if ($(nextStepProgress).attr('id') === 'searchstep3') {
                $("#doublesCount").addClass("d-none");
                $("#blanksCount").addClass("d-none");
                $("#matchingPropertiesInfo").removeClass("d-none");
            }

            if ($(nextStepProgress).attr('id') === 'searchstep4') {
                var form = $("#validationForm");
                var url = form.attr("action");
                var formData = form.serializeArray();
                loadingIcon.callback("#searchResultStep",
                    function () {
                        becosoft.ajax(url,
                            {
                                type: "POST",
                                data: formData,
                                success: function (result) {
                                    $("#searchResultStep").html(result);
                                }
                            });
                    });
            }

            updateStepInfo($(nextStepProgress).attr('id'));

            nextStepDiv.removeClass("d-none");

            await searchUpdateInfo();
        });

    //Previous step
    $("#modal-content").on("click", "div[data-search-step-container] button[data-action='searchprevious']", async function (e) {
            e.preventDefault();

            const currentStepDiv = $(this).closest("div[data-search-step-container]");
            const currentStep = currentStepDiv.data("search-step-container");
            const previousStep = currentStep - 1;

            const previousStepDiv = $(`#modal-content div[data-search-step-container='${previousStep}']`);
            if (!previousStepDiv) {
                //There is no previous step
                return;
            }

            const currentStepProgress = $(`#modal-content div[data-search-step='${currentStep}']`);
            $(currentStepProgress).removeClass("is-active");
            currentStepDiv.addClass("d-none");

            const previousStepProgress = $(`#modal-content div[data-search-step='${previousStep}']`);
            $(previousStepProgress).addClass("is-active");
            $(previousStepProgress).removeClass("is-complete");

            if ($(previousStepProgress).attr('id') === 'searchstep2') {
                $("#doublesCount").removeClass("d-none");
                $("#blanksCount").removeClass("d-none");
                $("#matchingPropertiesInfo").addClass("d-none");
                $("#matchingOption1Info").addClass("d-none");
                $("#matchingExtraOptionsInfo").addClass("d-none");
            }

            if ($(previousStepProgress).attr('id') === 'searchstep1') {
                $("#doublesCount").addClass("d-none");
                $("#blanksCount").addClass("d-none");
                $("#ArticleSearchContainerViewModel_FileLines").val("0");
            }

            updateStepInfo($(previousStepProgress).attr('id'));
            previousStepDiv.removeClass("d-none");

            await searchUpdateInfo();
        });

    //Save
    $("#modal-content").on("click", "div[data-search-step-container] button[data-action='searchsave']", async function (e) {
            e.preventDefault();
            const button = $(this);
            const currentStepDiv = button.closest("div[data-search-step-container]");

            const validationResult = await validateSearchStep(currentStepDiv);
            if (!validationResult) {
                //Previous step is invalid
                return;
            }

            //Disable all previous/next steps
            $("#modal-content div[data-search-step-container] button[data-action]").attr("disabled", true);
            await searchUpdateInfo();

            var form = $("#validationForm");
            var formData = form.serializeArray();
            becosoft.ajax(urls.saveArticlesAsTag,
                {
                    type: "POST",
                    data: formData,
                    success: function (result) {
                        if (result.Result === "success") {
                            var formattedResource = addedTagName.replace('{0}', result.TagName);                            

                            const jsonContainers = $("[data-step-container='5'] div[data-json-container]");
                            $(jsonContainers).each(function() {
                                const container = $(this);
                                const json = container.data("json-container");                              

                                var tagAlreadyExists = json.filterOptions[0].properties.find(x => x.property === "TagIDs").possibleValues.find(value => value.id === result.TagId);
                                if(!tagAlreadyExists){
                                    json.filterOptions[0].properties.find(x => x.property === "TagIDs").possibleValues.push({id: result.TagId, value: result.TagName });
                                    json.filterOptions[0].properties.find(x => x.property === "TagIDs").possibleValues.sort((a, b) => a.value.localeCompare(b.value));                                   
                                    const parentContainer = container.find("[data-id='container']");
                                    storeData(json, parentContainer);
                                }                                
                            });

                            $("#tagNameForPromo").html(formattedResource);
                        }
                        // set name of tag in info string
                    }
                });

            const validationAlert = $("#page-errors-alert-search");
            validationAlert.addClass("d-none");

            closeCurrentModal();
        });

    async function validateSearchStep(stepContainer) {
        const validationAlert = $("#page-errors-alert-search");
        validationAlert.addClass("d-none");

        const errors = $("#page-errors-search");
        errors.html("");

        const toClear = stepContainer.find(".custom-invalid");
        toClear.removeClass("custom-invalid");

        const mandatoryFields = stepContainer.find("[data-property][data-mandatory]");
        let validationResult = true;
        for (let i = 0; i < mandatoryFields.length; i++) {
            const field = $(mandatoryFields[i]);
            const val = field.val();

            const missesValue = val.length === 0;
            const parsedValue = parseInt(val);
            const hasInvalidValue = isNumeric(val) && (isNaN(parsedValue) || parsedValue === 0);
            if (missesValue || hasInvalidValue) {
                validationResult = false;
                const fieldName = getPropertyName(field);
                const validationMessage = _mandatoryMessage.replace("{0}", fieldName);
                addValidationError(validationMessage);
                field.addClass("custom-invalid");
                continue;
            }
        }

        const validationStep = parseInt(stepContainer.data("search-step-container"));
        let stepValidationResult = true;
        switch (validationStep) {
            case 1:
                stepValidationResult = validateSearchStep1(stepContainer);
                break;
            case 2:
                stepValidationResult = await validateSearchStep2(stepContainer);
                break;
            case 3:
                stepValidationResult = await validateSearchStep3(stepContainer);
                break;
            case 4:
                stepValidationResult = validateSearchStep4(stepContainer);
                break;
        }

        await searchUpdateInfo();
        var isValid = validationResult && stepValidationResult;
        var hasErrors = !isValid;

        if (hasErrors) {
            const firstError = stepContainer.find(".custom-invalid");
            if (firstError) {
                firstError.focus();
            }
            validationAlert.removeClass("d-none");
        }

        return isValid;
    }

    function validateSearchStep1(stepContainer) {
        let validationResult = true;

        if (fileInfoID === 0) {
            const selector = stepContainer.find("#importableFiles");
            selector.addClass("custom-invalid");
            validationResult = false;

            const validationMessage = noFileSelected;
            addValidationError(validationMessage);
        }

        return validationResult;
    }

    function validateSearchStep2(stepContainer) {
        let validationResult = true;

        var doublesCount = checkForDoubles();
        if (doublesCount !== 0) {
            const selector = stepContainer.find("#matchColumnsStep");
            selector.addClass("custom-invalid");
            validationResult = false;

            const validationMessage = matchingDoubles;
            addValidationError(validationMessage);
            return validationResult;
        }

        var selectedValues = [];

        $("#headers li").not(".disabled").each(function (i, e) {
            var element = $(e);
            if (element.hasClass("list-group-item-danger") || element.hasClass("list-group-item-warning")) { return; }
            var isChecked = element.find("input[type='checkbox']").prop("checked");
            if (!isChecked) { return; }
            var selectedValue = element.data('key');
            selectedValues.push(selectedValue);
        });

        becosoft.ajax(urls.validateSelectedProperties,
            { type: "POST", data: { selectedProperties: selectedValues }, async: false },
            function (result) {
                if (!result.Success) {
                    const selector = stepContainer.find("#matchColumnsStep");
                    selector.addClass("custom-invalid");
                    validationResult = false;
                    addValidationError(result.Message);
                }
            });

        return validationResult;
    }

    function validateSearchStep3(stepContainer) {
        let validationResult = true;

        var data = $("#ArticleSearchContainerViewModel_SelectedMatchingMappings").val();
        becosoft.ajax(urls.validateMatching,
            { type: "POST", data: { selectedMatchingProperties: data }, async: false }, function (result) {
                if (!result.Success) {
                    const selector = stepContainer.find("#ms-ArticleSearchContainerViewModel_SelectedMatchingMappings");
                    selector.addClass("custom-invalid");
                    validationResult = false;
                    addValidationError(result.Message);
                }
            });
        return validationResult;
    }

    function validateSearchStep4(stepContainer) {
        var tagName = $("#ArticleSearchWizardResultViewModel_TagName").val();
        if (!tagName) {
            addValidationError(tagNameRequired);
            return false;
        }
        return true;
    }

    function addValidationError(errorMessage) {
        $("#page-errors-search").append(`<li>${errorMessage}</li>`);
    }

    function getPropertyName(input, container) {
        const propertyName = input.data("property");
        if (container === undefined || container === null || container.length === 0) {
            container = $("#main-content");
        }

        // ReSharper disable once QualifiedExpressionMaybeNull
        let fieldName = container.find(`[data-label='${propertyName}']`).text();
        if (fieldName.length === 0) {
            //Fallback
            fieldName = propertyName;
        }

        return fieldName;
    }

    async function searchUpdateInfo() {
        if ($("#ArticleSearchContainerViewModel_FileLines").length) {

            $("#rowCountInfo").html(`#${generalRows} ${$("#ArticleSearchContainerViewModel_FileLines").val()}`);
        }
        var matchingTemplateValue = $('#NewArticleSearchViewModel_MatchingTemplates option:selected').text();
        $("#matchingTypeInfo").html(matchingTemplateValue);

        var matchingProperties = [];
        $("#ArticleSearchContainerViewModel_SelectedMatchingMappings option:selected").each(function () {
            var $this = $(this);
            if ($this.length) {
                matchingProperties.push($this.text());
            }
        });

        $("#matchingPropertiesInfo").html(matchingProperties.join(", "));
    }

    $("#NewArticleSearchViewModel_MatchingTemplates").on("change", function () {
            var matchingTemplateValue = $('#NewArticleSearchViewModel_MatchingTemplates option:selected').text();
            $("#matchingTypeInfo").html(matchingTemplateValue);
        });

    $("#doublesCount").on("click", function (e) {
        e.preventDefault();
        var errors = document.getElementsByClassName("list-group-item-danger");
        if (errors.length <= currentErrorIndex) {
            currentErrorIndex = 0;
        }
        errors[currentErrorIndex].scrollIntoView({ behavior: "smooth" });
        currentErrorIndex++;
    });

    $("#blanksCount").on("click", function (e) {
        e.preventDefault();
        var blanks = document.getElementsByClassName("list-group-item-warning");
        if (blanks.length <= currentBlankIndex) {
            currentBlankIndex = 0;
        }
        blanks[currentBlankIndex].scrollIntoView({ behavior: "smooth" });
        currentBlankIndex++;
    });

    $("#modal-content").off("change").on("change", ":file", function () {
            var input = $(this);
            if (input.val().length === 0) { return false; }

            var label = input.val().replace(/\\/g, "/").replace(/.*\//, "");
            var extension = label.replace(/^.*\./, "");
            if (extension !== "xlsx") {
                $(this).val("");
                return false;
            }

            $("#fileName").val(label);
            $("#selectedFileNameInfo").html(label);
            $("#selectedFileNameInfo").removeClass("d-none");
            fileInfoID = 0;
            $(".active").removeClass("active");
            $("#uploadForm").trigger("submit");
        });

    $("#uploadForm").off("submit").on("submit",
        function () {
            var form = $(this);
            var formData = new FormData();
            formData.append("file", $("#file").prop('files')[0]);
            formData.append("currentType", articleImportType);
            var url = form.attr("action");

            console.log(formData);
            becosoft.ajax(url, {
                type: "POST",
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
                success: function (result) {
                    fileInfoID = result;
                    getTemplates();
                    getImportableFiles(fileInfoID);
                }
            });
            return false;
        });

    function getImportableFiles(newFileInfoId) {
        $('#uploadForm').trigger("reset");
        fileInfoID = 0;
        $(".importable-file").remove();
        const url = urls.getImportableFiles;
        becosoft.ajax(url, { type: "GET" }, function (result) {
            $("#importableFiles").append(result);
            $('#importableFiles [data-toggle="tooltip"]').tooltip();
            if (newFileInfoId !== 0 && newFileInfoId !== '0') {
                $(`#importableFiles [data-id='${newFileInfoId}']`).trigger("click");
            }
        });
    }

    function getTemplates() {
        const url = urls.getMatchingTemplates;
    }

    $(".delete-link").on("click",
        function (e) {
            e.preventDefault();
            var currentItem = $(this).data("id");
            var url = urls.deleteDuringImport;
            url = url + "?id=" + currentItem;
            becosoft.ajax(url, { type: "GET", async: false }, function (result) {
            });

            getImportableFiles();
        });

    $("#importableFiles").on("click",
        ".importable-file-link",
        function (e) {
            e.preventDefault();
            $('#uploadForm').trigger("reset");
            $(".active").removeClass("active");
            $(this).closest(".importable-file").addClass("active");
            fileInfoID = $(this).data("id");
            $("#selectedFileNameInfo").html($(this).data("filename"));
            $("#selectedFileNameInfo").removeClass("d-none");
            $("#matchingTemplates select, #matchingTemplates button").removeAttr("disabled");
            $("#manualMatchingButton").removeAttr("disabled");
        });

    function addMatchingView(result) {
        $("#matchColumnsStep").html(result);
        $('#matchColumnsStep [data-toggle="tooltip"]').tooltip();
        checkForDoubles();
        $("#matchColumnsStep")[0].scrollIntoView({ behavior: "smooth" });
        $("#matchColumnsStep li:not(.disabled) :checkbox").trigger("change");
        $("#ArticleSearchContainerViewModel_SelectedMatchingMappings").multiSelect({
            selectableHeader: `<input type='text' class='form-control searchField' autocomplete='off' placeholder='${importField}'>`,
            selectionHeader: `<input type='text' class='form-control searchField' autocomplete='off' placeholder='${importField}'>`,
            afterInit: function () {
                var that = this,
                    $selectableSearch = that.$selectableUl.prev(),
                    $selectionSearch = that.$selectionUl.prev(),
                    selectableSearchString = '#' + that.$container.attr('id') + ' .ms-elem-selectable:not(.ms-selected)',
                    selectionSearchString = '#' + that.$container.attr('id') + ' .ms-elem-selection.ms-selected';

                that.qs1 = $selectableSearch.quicksearch(selectableSearchString)
                    .on('keydown', function (e) {
                        if (e.which === 40) {
                            that.$selectableUl.focus();
                            return false;
                        }
                        return true;
                    });

                that.qs2 = $selectionSearch.quicksearch(selectionSearchString)
                    .on('keydown', function (e) {
                        if (e.which === 40) {
                            that.$selectionUl.focus();
                            return false;
                        }
                        return true;
                    });
            },
            afterSelect: function () {
                this.qs1.cache();
                this.qs2.cache();
            },
            afterDeselect: function () {
                this.qs1.cache();
                this.qs2.cache();
            }
        });

        $("#saveTemplateButton").on("click", function (e) {
            e.preventDefault();
            $(this).addClass("d-none");
            $("#templateNameForm").removeClass("d-none");
        });

        $("#confirmTemplateButton").on("click", function (e) {
            e.preventDefault();
            if (checkForDoubles() > 0) {
                return false;
            }

            var form = $("#validationForm");
            var url = urls.importTemplateSave;
            var formData = form.serializeArray();

            becosoft.ajax(url, {
                type: "POST",
                data: formData,
                success: function (result) {
                    if (!isNaN(parseFloat(result)) && isFinite(result)) {
                        $("#ArticleSearchContainerViewModel_TemplateID").val(result);
                        $("#templateNameValidation").text("");
                        $("#templateNameForm").addClass("d-none");
                        $("#saveTemplateButton").removeClass("d-none");
                    } else {
                        $("#templateNameValidation").text(result);
                    }
                }
            });

            return false;
        });

        $("#cancelTemplateButton").on("click", function (e) {
            e.preventDefault();
            $("#templateNameForm").addClass("d-none");
            $("#saveTemplateButton").removeClass("d-none");
            return false;
        });
    }

    function checkForDoubles() {
        resetColors();
        fillBlanks();
        currentErrorIndex = 0;
        var allValues = $("#matchColumnsStep select:first option");
        var doublesCount = 0;
        allValues.each(function () {
            if ($(this).val() === "0") {
                return;
            }
            var selects = getWithValue($(this).val());

            if (selects.length > 1) {
                for (var i = 0; i < selects.length; i++) {
                    doublesCount++;

                    var current = selects[i];
                    $(current).closest("li").addClass("list-group-item-danger");
                }
            }
        });
        if (doublesCount === 0) {
            $("#doublesCount").text("");
        } else {
            $("#doublesCount").text(doublesCount + duplicateColumns);
        }
        return doublesCount;
    }

    function fillBlanks() {
        currentBlankIndex = 0;
        var allSelects = $("#matchColumnsStep select");
        var blanksCount = 0;
        allSelects.each(function () {
            if ($(this).val() === "0") {
                $(this).closest("li").addClass("list-group-item-warning");
                blanksCount++;
            }
        });
        if (blanksCount === 0) {
            $("#blanksCount").text("");
        } else {
            $("#blanksCount").text(blanksCount + emptyColumns);
        }
    }

    function resetColors() {
        var allSelects = $("#matchColumnsStep select");
        allSelects.each(function () {
            $(this).closest("li").removeClass("list-group-item-danger");
            $(this).closest("li").removeClass("list-group-item-warning");
        });
    }

    function getWithValue(selectedValue) {
        var allSelects = $("#matchColumnsStep select");
        var sameValueSelects = [];
        allSelects.each(function () {
            if ($(this).val() === selectedValue) {
                sameValueSelects.push($(this));
            }
        });
        return sameValueSelects;
    }

    $("#matchColumnsStep").on("change",
        "li:not(.disabled) :checkbox",
        function () {
            var checked = $(this).prop("checked");
            var row = $(this).closest("li");
            var key = row.data("key");

            $(`input[name='ArticleSearchContainerViewModel.SelectedMatchedPropertyMappings[${key}]']`).val(checked);
            if (checked) {
                $(`#ArticleSearchContainerViewModel_SelectedMatchingMappings option[value='${key}']`).prop("disabled", false);
            } else {
                $(`#ArticleSearchContainerViewModel_SelectedMatchingMappings option[value='${key}']`).prop("disabled", "disabled");
            }
            $("#ArticleSearchContainerViewModel_SelectedMatchingMappings").multiSelect("refresh");
        });

    $("#matchColumnsStep").on("change",
        "select",
        function () {
            checkForDoubles();
            var exampleElement = $(this).find(":selected")[0];
            var example = $(exampleElement).data("example");
            var newVal = $(this).val();

            var row = $(this).closest("li");
            row.find(".example").html(example);
            row.find("input[type='checkbox']").prop('disabled', newVal === "0");
            row.find("input[type='checkbox']").prop('checked', newVal !== "0").trigger('change');

            var key = row.data("key");
            $(`input[name='ArticleSearchContainerViewModel.MatchedPropertyMappings[${key}]']`).attr("value", newVal.toString());
        });

    $("#matchColumnsStep").on("change",
        "#selectAllCheckboxes",
        function () {
            var checked = $(this).is(":checked");
            $("#matchColumnsStep li:not(.disabled) :checkbox:not(:disabled)").prop("checked", checked);
            $("#matchColumnsStep li:not(.disabled) :checkbox:not(:disabled)").trigger("change");
        });
};