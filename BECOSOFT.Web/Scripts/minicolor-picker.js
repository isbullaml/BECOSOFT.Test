$('.demo, .background, .backgroundContentRow').each(function () {
    $(this).minicolors({
        // animation speed
        animationSpeed: 50,
        // easing function
        animationEasing: 'swing',
        // defers the change event from firing while the user makes a selection
        changeDelay: 0,
        // hue, brightness, saturation, or wheel
        control: 'hue',
        // default color
        defaultValue: $(this).data('backgroundcolor'),
        // hex or rgb
        format: 'rgb',
        // show/hide speed
        showSpeed: 100,
        hideSpeed: 100,
        // is inline mode?
        inline: false,
        // a comma-separated list of keywords that the control should accept (e.g. inherit, transparent, initial).
        keywords: '',
        // uppercase or lowercase
        letterCase: 'lowercase',
        // enables opacity slider
        opacity: true,
        // custom position
        position: 'bottom left',
        // additional theme class
        theme: 'default',
        // an array of colors
        swatches: ['#f01818', '#eeff00ff', '#4118f5', '#23f718', '#ff17fbff', '#000000ff', '#6fa9ebff']
    });
});

function clearColor() {
    $(".demo").val("transparent");
    $('.minicolors-input-swatch .minicolors-swatch-color').css('backgroundColor', "transparent");
    $('.minicolors-grid.minicolors-sprite').css('backgroundColor', "transparent");
    $('.minicolors-picker').css('top', "0px");
    $('.minicolors-picker').css('left', "0px");
    $(".demo").minicolors({
        swatches: ['#f01818', '#eeff00ff', '#4118f5', '#23f718', '#ff17fbff', '#000000ff', '#6fa9ebff']
    });

    sessionStorage.setItem('ContentChangedID', $(this).attr("id"));
    sessionStorage.setItem('ContentPanelChanged', true);
}

function clearBackgroundColor() {
    $(".background").val("transparent");
    $('.minicolors-input-swatch .minicolors-swatch-color').css('backgroundColor', "transparent");
    $('.minicolors-grid.minicolors-sprite').css('backgroundColor', "transparent");
    $('.minicolors-picker').css('top', "0px");
    $('.minicolors-picker').css('left', "0px");
    $(".background").minicolors({
        swatches: ['#f01818', '#eeff00ff', '#4118f5', '#23f718', '#ff17fbff', '#000000ff', '#6fa9ebff']
    });

    sessionStorage.setItem('ContentChangedID', $(this).attr("id"));
    sessionStorage.setItem('ContentPanelChanged', true);
}


function clearBackgroundColorRow() {
    $(".backgroundContentRow").val("transparent");
    $('.minicolors-input-swatch .minicolors-swatch-color').css('backgroundColor', "transparent");
    $('.minicolors-grid.minicolors-sprite').css('backgroundColor', "transparent");
    $('.minicolors-picker').css('top', "0px");
    $('.minicolors-picker').css('left', "0px");
    $(".background").minicolors({
        swatches: ['#f01818', '#eeff00ff', '#4118f5', '#23f718', '#ff17fbff', '#000000ff', '#6fa9ebff']
    });

    sessionStorage.setItem('ContentChangedID', $(this).attr("id"));
    sessionStorage.setItem('ContentPanelChanged', true);
}