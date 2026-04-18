function importDocumentDetailWizard() {
    var fileInfoID = 0;
    var currentErrorIndex = 0;
    var currentBlankIndex = 0;
    var timeOutID = 0;

    function initStep1() {
        var matchingTemplateValue = $('#NewImportViewModel_MatchingTemplates option:selected').text();
        $("#matchingTypeInfo").html(matchingTemplateValue);
        updateStepInfo("step1");
    }

    initStep1();

    function updateStepInfo(currentStep) {
        switch (currentStep) {
            case "step1":
                $("#stepInfoTitle").html(step1Title);
                $("#currentStepInfo").html(step1Info);
                break;
            case "step2":
                $("#stepInfoTitle").html(step2Title);
                $("#currentStepInfo").html(step2Info);
                break;
            case "step3":
                $("#stepInfoTitle").html(step3Title);
                $("#currentStepInfo").html(step3Info);
                break;
            case "step4":
                $("#stepInfoTitle").html(step4Title);
                $("#currentStepInfo").html(step4Info);
                break;
            default:
        }
    }

    $(async function () {
        await updateInfo();
        $("[data-cascading][data-cascading-initial-value]:not([data-initial-value='0'])").trigger("change");

        const jsonContainers = $("[data-step-container='5'] div[data-json-container]");
        $(jsonContainers).each(function () {
            const container = $(this);
            const json = container.data("json-container");
            const parentContainer = container.find("[data-id='container']");
            processData(json, parentContainer);
        });
    });

    //Next step
    $("#modal-content").on("click",
        "div[data-step-container] button[data-action='next']",
        async function (e) {
            e.preventDefault();

            const currentStepDiv = $(this).closest("div[data-step-container]");
            const currentStep = currentStepDiv.data("step-container");

            const validationResult = await validateStep(currentStepDiv);
            if (!validationResult) {
                //Previous step is invalid
                return;
            }

            const nextStep = currentStep + 1;

            const nextStepDiv = $(`div[data-step-container='${nextStep}']`);
            if (!nextStepDiv) {
                //There is no next step
                return;
            }

            const currentStepProgress = $(`div[data-step='${currentStep}']`);
            $(currentStepProgress).removeClass("is-active");
            $(currentStepProgress).addClass("is-complete");
            currentStepDiv.addClass("d-none");

            const nextStepProgress = $(`div[data-step='${nextStep}']`);
            $(nextStepProgress).addClass("is-active");

            if ($(nextStepProgress).attr('id') === 'step2') {
                var matchingTemplateID = $('#MatchingTemplates').val();
                const url = urls.getMatching + `?importFileID=${fileInfoID}&importTemplateID=${matchingTemplateID}`;
                becosoft.ajax(url, { type: "GET", async: false }, function (result) {
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

            if ($(nextStepProgress).attr('id') === 'step3') {
                $("#doublesCount").addClass("d-none");
                $("#blanksCount").addClass("d-none");
                $("#matchingPropertiesInfo").removeClass("d-none");
                $("#matchingOption1Info").removeClass("d-none");
                $("#matchingExtraOptionsInfo").removeClass("d-none");
            }

            if ($(nextStepProgress).attr('id') === 'step4') {
                var form = $("#validationForm");
                var url = form.attr("action");
                var formData = form.serializeArray();
                formData.push({ name: 'documentTypeID', value: $('#DocumentTypeID').val() });

                objIndex = formData.findIndex((obj => obj.name === "SelectedMatchingMappings"));
                formData[objIndex].name = "ImportContainerViewModel.SelectedMatchingMappings";

                initiateProgress();
                loadingIcon.callback("#verificationStep", function () {
                    becosoft.ajax(url, {
                        type: "POST",
                        data: formData,
                        success: function (result) {
                            $("#verificationStep").html(result);
                        }
                    });
                });

            }

            updateStepInfo($(nextStepProgress).attr('id'));

            nextStepDiv.removeClass("d-none");

            await updateInfo();
        });

    //Previous step
    $("#modal-content").on("click",
        "div[data-step-container] button[data-action='previous']",
        async function (e) {
            e.preventDefault();

            const currentStepDiv = $(this).closest("div[data-step-container]");
            const currentStep = currentStepDiv.data("step-container");
            const previousStep = currentStep - 1;

            const previousStepDiv = $(`div[data-step-container='${previousStep}']`);
            if (!previousStepDiv) {
                //There is no previous step
                return;
            }

            const currentStepProgress = $(`div[data-step='${currentStep}']`);
            $(currentStepProgress).removeClass("is-active");
            currentStepDiv.addClass("d-none");

            const previousStepProgress = $(`div[data-step='${previousStep}']`);
            $(previousStepProgress).addClass("is-active");
            $(previousStepProgress).removeClass("is-complete");

            if ($(previousStepProgress).attr('id') === 'step3') {
                $("#matchingProgressBarInfo").html("");
                updateProgress(0, "#matchingProgressBar");
                disconnectProgress();
            }

            if ($(previousStepProgress).attr('id') === 'step2') {
                $("#doublesCount").removeClass("d-none");
                $("#blanksCount").removeClass("d-none");
                $("#matchingPropertiesInfo").addClass("d-none");
                $("#matchingOption1Info").addClass("d-none");
                $("#matchingExtraOptionsInfo").addClass("d-none");
            }

            if ($(previousStepProgress).attr('id') === 'step1') {
                $("#doublesCount").addClass("d-none");
                $("#blanksCount").addClass("d-none");
                $("#ImportContainerViewModel_FileLines").val("0");
            }

            updateStepInfo($(previousStepProgress).attr('id'));
            previousStepDiv.removeClass("d-none");

            await updateInfo();
        });

    //Save
    $("#modal-content").on("click",
        "div[data-step-container] button[data-action='save']",
        async function (e) {
            e.preventDefault();
            const button = $(this);
            const currentStepDiv = button.closest("div[data-step-container]");

            const validationResult = await validateStep(currentStepDiv);
            if (!validationResult) {
                //Previous step is invalid
                return;
            }

            //Disable all previous/next steps
            $("div[data-step-container] button[data-action]").attr("disabled", true);
            await updateInfo();

            var form = $("#validationForm");
            var formData = form.serializeArray();
            formData.push({ name: 'documentTypeID', value: $('#DocumentTypeID').val() });
            formData.push({ name: 'vatCodeID', value: $("#VATCodeID").val() });
            formData.push({ name: 'contactID', value: $("#ContactID").val() });
            var position = getLastPosition();
            formData.push({ name: 'index', value: position });
            objIndex = formData.findIndex((obj => obj.name === "SelectedMatchingMappings"));
            formData[objIndex].name = "ImportContainerViewModel.SelectedMatchingMappings";

            loadingIcon.callback("#loadingImportDetails", function () {
                becosoft.ajax(urls.importDocumentDetails, {
                    type: "POST",
                    data: formData,
                    success: function (result) {
                        $("#loadingImportDetails").html("");
                        updateProgress(100, "#importProgressBar");

                        var panel = $("#detailsTable .sortable").eq(position);
                        if (panel === undefined || panel === null || panel.length === 0) {
                            $(result).insertBefore("#detailsTable tfoot");
                        } else {
                            panel.replaceWith(result);
                        }
                        fillTotals();
                        $("#defaultSearchField").val("");
                        var addedBody = $("#detailsTable .sortable").eq(position);
                        addedBody.find("[name$='.Amount']").first().focus();

                        const modal = $('#importExcelModal');
                        modal.modal('hide');
                        //$("#matchingResult .card-body").html(result);
                    }
                });
            });

            const validationAlert = $("#page-errors-alert");
            validationAlert.addClass("d-none");

            closeCurrentModal();
        });

    async function validateStep(stepContainer) {
        const validationAlert = $("#page-errors-alert");
        validationAlert.addClass("d-none");

        const errors = $("#page-errors");
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

        const validationStep = parseInt(stepContainer.data("step-container"));
        let stepValidationResult = true;
        switch (validationStep) {
            case 1:
                stepValidationResult = validateStep1(stepContainer);
                break;
            case 2:
                stepValidationResult = await validateStep2(stepContainer);
                break;
            case 3:
                stepValidationResult = await validateStep3(stepContainer);
                break;
            case 4:
                stepValidationResult = validateStep4(stepContainer);
                break;
        }

        await updateInfo();
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

    function validateStep1(stepContainer) {
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

    function validateStep2(stepContainer) {
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

    function validateStep3(stepContainer) {
        let validationResult = true;

        var data = $("#SelectedMatchingMappings").val();
        becosoft.ajax(urls.validateMatching,
            { type: "POST", data: { selectedMatchingProperties: data }, async: false }, function (result) {
                if (!result.Success) {
                    const selector = stepContainer.find("#ms-SelectedMatchingMappings");
                    selector.addClass("custom-invalid");
                    validationResult = false;
                    addValidationError(result.Message);
                }
            });
        return validationResult;
    }

    function validateStep4(stepContainer) {
        let validationResult = true;

        return validationResult;
    }

    function addValidationError(errorMessage) {
        $("#page-errors").append(`<li>${errorMessage}</li>`);
    }

    function getPropertyName(input, container) {
        const propertyName = input.data("property");
        if (container === undefined || container === null || container.length === 0) {
            container = $("#modal-content");
        }

        // ReSharper disable once QualifiedExpressionMaybeNull
        let fieldName = container.find(`[data-label='${propertyName}']`).text();
        if (fieldName.length === 0) {
            //Fallback
            fieldName = propertyName;
        }

        return fieldName;
    }

    async function updateInfo() {
        if ($("#ImportContainerViewModel_FileLines").length) {

            $("#rowCountInfo").html(`#${generalRows} ${$("#ImportContainerViewModel_FileLines").val()}`);
        }
        var matchingTemplateValue = $('#NewImportViewModel_MatchingTemplates option:selected').text();
        $("#matchingTypeInfo").html(matchingTemplateValue);

        var matchingProperties = [];
        $("#SelectedMatchingMappings option:selected").each(function () {
            var $this = $(this);
            if ($this.length) {
                matchingProperties.push($this.text());
            }
        });

        $("#matchingPropertiesInfo").html(matchingProperties.join(", "));
    }

    $("#NewImportViewModel_MatchingTemplates").on("change",
        function () {
            var matchingTemplateValue = $('#NewImportViewModel_MatchingTemplates option:selected').text();
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

    $(document).off("change").on("change",
        ":file",
        function () {
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
            $(".list-group-item-info").removeClass("list-group-item-info");
            $("#uploadForm").trigger("submit");
        });

    $("#uploadForm").off("submit").on("submit",
        function () {
            var form = $(this);
            var formData = new FormData();
            formData.append("file", $("#file").prop('files')[0]);
            var url = form.attr("action");

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
            $(".list-group-item-info").removeClass("list-group-item-info");
            $(this).closest(".importable-file").addClass("list-group-item-info");
            fileInfoID = $(this).data("id");
            $("#selectedFileNameInfo").html($(this).data("filename"));
            $("#selectedFileNameInfo").removeClass("d-none");
            $("#matchingTemplates select, #matchingTemplates button").removeAttr("disabled");
            $("#manualMatchingButton").removeAttr("disabled");
        });

    function parseFilter(parent) {
        const row = parent.find("[data-property='ConditionFilter']");
        const container = row.find("[data-id='container']");
        const condition = {
            FilterConditionID: parseInt(row.find("[data-property='FilterConditionID']").val()),
            JsonData: null
        };

        if (validateData(container)) {
            const data = getData(container);
            condition.JsonData = JSON.stringify(data);
        }

        return condition;
    }

    if ($('#NewImportViewModel_ImportType').val() === '@ImportType.Article') {
        showExtraArticleOptions($('#ImportContainerViewModel_ExtraImportOptions_Option1ID').find(":selected").val());
    }

    $("#ImportContainerViewModel_ExtraImportOptions_Option1ID").on("change",
        function () {
            var checkedOption = $('#ImportContainerViewModel_ExtraImportOptions_Option1ID').find(":selected").val();
            if ($('#NewImportViewModel_ImportType').val() === '@ImportType.Article') {
                showExtraArticleOptions(checkedOption);
                $("#matchingOption1Info").html($("#ImportContainerViewModel_ExtraImportOptions_Option1ID option:selected").text());
            }
        });

    function showExtraArticleOptions(checkedOption) {
        var showKeepPrices = false;
        var showKeepPercentages = false;
        var showUsePromo = false;
        var showOnlyForSpecificSupplier = false;

        $("#ImportContainerViewModel_ExtraImportOptions_Article_KeepPrices").prop('checked', false);
        $("#ImportContainerViewModel_ExtraImportOptions_Article_KeepPercentages").prop('checked', false);
        $("#ImportContainerViewModel_ExtraImportOptions_Article_UsePromo").prop('checked', $("#ImportContainerViewModel_ExtraImportOptions_Article_UsePromoCheckedByDefault").prop('checked'));
        $("#ImportContainerViewModel_ExtraImportOptions_Article_OnlyForSpecificSupplier").prop('checked', false);

        switch (checkedOption) {
            case "1": // overwrite existing prices
                showKeepPrices = true;
                showKeepPercentages = true;
                showUsePromo = true;
                showOnlyForSpecificSupplier = true;
                break;
            case "2": // Delete existing prices
                showKeepPrices = true;
                showKeepPercentages = true;
                showUsePromo = false;
                showOnlyForSpecificSupplier = true;
                break;
            case "3": // Keep existing and add imported prices
                showKeepPrices = false;
                showKeepPercentages = false;
                showUsePromo = false;
                showOnlyForSpecificSupplier = false;
                break;
            default:
        }

        var showAnyExtraOptions = showKeepPrices || showKeepPercentages || showUsePromo || showOnlyForSpecificSupplier;

        $("#keepPricesContainer").attr('hidden', !showKeepPrices);
        $("#keepPercentagesContainer").attr('hidden', !showKeepPercentages);
        $("#usePromoContainer").attr('hidden', !showUsePromo);
        $("#onlyForSpecificSupplierContainer").attr('hidden', !showOnlyForSpecificSupplier);

        $("#extraOptionsContainer").attr('hidden', !showAnyExtraOptions);
    }

    function addMatchingView(result) {
        $("#matchColumnsStep").html(result);
        $('#matchColumnsStep [data-toggle="tooltip"]').tooltip();
        checkForDoubles();
        $("#matchColumnsStep")[0].scrollIntoView({ behavior: "smooth" });
        $("#matchColumnsStep li:not(.disabled) :checkbox").trigger("change");
        $("#SelectedMatchingMappings").multiSelect({
            selectableHeader: "<input type='text' class='form-control searchField' autocomplete='off' placeholder='${importField}'>",
            selectionHeader: "<input type='text' class='form-control searchField' autocomplete='off' placeholder='${importField}'>",
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
                        $("#ImportContainerViewModel_TemplateID").val(result);
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

            $(`input[name='ImportContainerViewModel.SelectedMatchedPropertyMappings[${key}]']`).val(checked);
            if (checked) {
                $(`#SelectedMatchingMappings option[value='${key}']`).prop("disabled", false);
            } else {
                $(`#SelectedMatchingMappings option[value='${key}']`).prop("disabled", "disabled");
            }
            $("#SelectedMatchingMappings").multiSelect("refresh");
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
            $(`input[name='ImportContainerViewModel.MatchedPropertyMappings[${key}]']`).attr("value", newVal.toString());
        });

    $("#matchColumnsStep").on("change",
        "#selectAllCheckboxes",
        function () {
            var checked = $(this).is(":checked");
            $("#matchColumnsStep li:not(.disabled) :checkbox:not(:disabled)").prop("checked", checked);
            $("#matchColumnsStep li:not(.disabled) :checkbox:not(:disabled)").trigger("change");
        });

    function initiateProgress() {
        var progressNotifier = $.connection.progressHub;
        $("#matchingProgressBar").addClass("progress-bar-striped");
        $("#matchingProgressBar").addClass("progress-bar-animated");
        $("#importProgressBar").addClass("progress-bar-striped");
        $("#importProgressBar").addClass("progress-bar-animated");

        progressNotifier.client.updateValidationProgress = function (prog, message) {
            console.log(prog);
            $("#matchingProgressBarInfo").html(message);
            if (prog === 100) {
                $("#matchingProgressBar").removeClass("progress-bar-striped");
                $("#matchingProgressBar").removeClass("progress-bar-animated");
            }
            updateProgress(prog, "#matchingProgressBar");
        };

        progressNotifier.client.updateImportProgress = function (prog, message) {
            console.log(prog);
            $("#importProgressBarInfo").html(message);
            if (prog === 100) {
                $("#importProgressBar").removeClass("progress-bar-striped");
                $("#importProgressBar").removeClass("progress-bar-animated");
            }
            updateProgress(prog, "#importProgressBar");
        };

        $.connection.hub.start().done(function () {
            progressNotifier.server.start(username);
        });

        $.connection.hub.disconnected(function () {
            timeOutID = setTimeout(function () {
                $.connection.hub.start();
            }, 5000);
        });
    }

    function disconnectProgress() {
        $.connection.hub.stop();
        clearTimeout(timeOutID);
    }

    function updateProgress(prog, progressBar) {
        $(progressBar).attr("aria-valuenow", prog + "%");
        $(progressBar).css("width", prog + "%");
    }
};