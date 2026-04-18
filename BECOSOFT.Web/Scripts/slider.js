
function initSlider(obj, refObj, options = {}) {

    // set up
    const defaults = {
        min: 0,
        max: 100,
        disabled: false
    }
    options = { ...defaults, ...options };
    const targetElem = obj;
    const refElem = refObj;
    var value = value = refObj.attr("value");
    if (typeof options.min === 'number' && options.min === 0) {
        options.min = 0;
    } else {
        const parsedMin = (parseInt(options.min, 10)) || options.min;
        options.min = parsedMin;
    }
    if (typeof options.max === 'number' && options.max === 0) {
        options.max = 0;
    } else {
        const parsedMax = (parseInt(options.max, 10)) || options.max;
        options.max = parsedMax;
    }

    // build
    targetElem.addClass("bcs-slider");
    targetElem.slider({
        range: "max",
        min: options.min,
        max: options.max,
        value: value,
        disabled: options.disabled,
        slide: function (event, ui) {
            refElem.val(ui.value);
        }
    });
    refElem.on("input", function () {
        targetElem.slider("value", $(this).val());
    });
};