
$(document).on("click", "[data-custom-number-input] [data-custom-number-button-type]", function (e) {
    e.preventDefault();

    const clickedElem = $(this);
    var fieldName = clickedElem.attr('data-custom-number-name');
    var type = clickedElem.attr('data-custom-number-button-type');

    const containerElem = clickedElem.closest('div[data-custom-number-input]');
    const inputElem = containerElem.find("input[data-custom-number-name='" + fieldName + "']");
    var currentVal = parseInt(inputElem.val(), 10) || 0;

    if (type === 'minus') {
        var minValue = parseInt(inputElem.attr('min'), 10) || 0;
        if (currentVal > minValue) {
            inputElem.val(currentVal - 1).trigger("change");
            inputElem.trigger("input");
        }
        if (parseInt(inputElem.val(), 10) === minValue) {
            clickedElem.attr('disabled', true);
        }

    } else if (type === 'plus') {
        var maxValue = parseInt(inputElem.attr('max'), 10) || Number.MAX_SAFE_INTEGER;
        if (currentVal < maxValue) {
            inputElem.val(currentVal + 1).trigger("change");
            inputElem.trigger("input");
        }
        if (parseInt(inputElem.val(), 10) === maxValue) {
            clickedElem.attr('disabled', true);
        }
    }
});
$(document).on("focusin", "[data-custom-number-input] input", function () {
    $(this).data('oldValue', $(this).val());
});
$(document).on("input", "[data-custom-number-input] input", function () {
    var minValue = parseInt($(this).attr('min'), 10) || 0;
    var maxValue = parseInt($(this).attr('max'), 10) || Number.MAX_SAFE_INTEGER;
    var valueCurrent = parseInt($(this).val(), 10);
    if (isNaN(valueCurrent)) {
        return;
    } else {
        var fieldName = $(this).attr('data-custom-number-name');
        if (valueCurrent >= minValue) {
            $("[data-custom-number-button-type='minus'][data-custom-number-name='" + fieldName + "']").removeAttr('disabled');
        } else {
            $(this).val(minValue);
        }
        if (valueCurrent <= maxValue) {
            $("[data-custom-number-button-type='plus'][data-custom-number-name='" + fieldName + "']").removeAttr('disabled');
        } else {
            $(this).val(maxValue);
        }
        if (valueCurrent >= minValue && valueCurrent <= maxValue) {
            $(this).val(valueCurrent);
        }
    }
});
$(document).on("keydown", "[data-custom-number-input] input", function (e) {
    // Allow: backspace, delete, tab, escape, enter and .
    if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 190]) !== -1 ||
        // Allow: Ctrl+A
        (e.keyCode === 65 && e.ctrlKey === true) ||
        // Allow: home, end, left, right
        (e.keyCode >= 35 && e.keyCode <= 39)) {
        // let it happen, don't do anything
        return;
    }
    // Ensure that it is a number and stop the keypress
    if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
        e.preventDefault();
    }
});