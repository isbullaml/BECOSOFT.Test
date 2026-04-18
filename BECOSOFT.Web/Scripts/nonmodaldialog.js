const menuWidth = 192;
const titleBarHeight = 44;
const fixedTabsBarHeight = 36;
const mobileMenu = window.matchMedia("(max-width: 1024px )");
var justSwitchedToMobileMenu = false;

$(window).resize(function () {
    updateNonmodalDialogPosition();
});

/**
 * Updates the dialog's position after a change to the menu.
 * @param {boolean} menuOpen - Whether the menu is expanded or collapsed.
 */
function updateNonmodalDialogPositionAfterMenuChange(menuOpen) {
    $(".bcs-nonmodal-dialog-left").each(function () {
        var leftString = $(this).css("left");
        var left = leftString.substring(0, leftString.length - 2);
        if (menuOpen) {
            $(this).css("left", `${parseInt(left, 10) + menuWidth}px`);
        } else {
            $(this).css("left", `${parseInt(left, 10) - menuWidth}px`);
        }
    });
}

/**
 * Updates the dialog's position.
 */
function updateNonmodalDialogPosition() {
    if (mobileMenu.matches) {
        if (!$(".bcs-main").hasClass("bcs-collapsed-sidebar")) {
            if (justSwitchedToMobileMenu === false) {
                updateNonmodalDialogPositionAfterMenuChange(false);
            }
            justSwitchedToMobileMenu = true;
        }
    } else {
        if (justSwitchedToMobileMenu === true) {
            updateNonmodalDialogPositionAfterMenuChange(true);
        }
        justSwitchedToMobileMenu = false;
    }
}

/**
 * Sets up a non-modal dialog.
 * @param {string} obj - The HTML element to populate.
 * @param {{top: number, right: number, bottom: number, left: number, width: number, height: number, fullHeight: boolean, pageHasFixedTabsBar: boolean, header: string, transparent: boolean}} options - Additional options.
 */
function initNonmodalDialog(obj, options = {}) {

    // set up
    const defaults = {
        top: "",
        right: 0,
        bottom: 0,
        left: "",
        width: 500,
        height: "auto",
        fullHeight: false,
        pageHasFixedTabsBar: false,
        header: "&nbsp;",
        headerColor: "",
        transparent: true,
        hasTransparencyToggle: true,
        hasLocator: true,
        closeable: false
    }
    options = { ...defaults, ...options };
    const targetElem = obj;
    $("#nonmodaldialogcontainer").append(targetElem);
    const targetElemContent = targetElem.html();
    var realTop = titleBarHeight + (options.pageHasFixedTabsBar ? fixedTabsBarHeight : 0);
    if (options.top !== "" && options.bottom !== "") options.bottom = ""; // having both bottom and top set results in improperly behaving "fake full height"
    if (typeof options.top === 'number' && options.top === 0) {
        options.top = realTop;
    } else {
        const parsedTop = (parseInt(options.top, 10)) || options.top;
        options.top = parsedTop + realTop;
    }
    if (typeof options.right === 'number' && options.right === 0) {
        options.right = 0;
    } else {
        const parsedRight = (parseInt(options.right, 10)) || options.right;
        options.right = parsedRight;
    }
    if (typeof options.bottom === 'number' && options.bottom === 0) {
        options.bottom = 0;
    } else {
        const parsedBottom = (parseInt(options.bottom, 10)) || options.bottom;
        options.bottom = parsedBottom;
    }
    if (typeof options.left === 'number' && options.left === 0) {
        options.left = 0;
    } else {
        const parsedLeft = (parseInt(options.left, 10)) || options.left;
        options.left = parsedLeft;
    }
    const parsedHeight = parseInt(options.height, 10) || options.height;
    options.height = parsedHeight;
    const parsedWidth = parseInt(options.width, 10) || options.width;
    options.width = parsedWidth;
    if (!options.header || options.header.trim() === "") options.header = "&nbsp;";
    var headerColorClass = "";
    if (options.headerColor === "red") headerColorClass = "bcs-card-header-bgcolor-red";
    if (options.headerColor === "indigo") headerColorClass = "bcs-card-header-bgcolor-indigo";

    // build
    targetElem.addClass("bcs-nonmodal-dialog");
    if (Number.isInteger(options.left)) targetElem.addClass("bcs-nonmodal-dialog-left");
    const identifier = "dialog_" + getRandomInt(0, 100000);
    const cardElem = $(`<div class="card"></div>`);
    const cardHeaderElem = $(`
    <div class="card-header bcs-card-header ${headerColorClass}">
        <p class="card-title bcs-card-title-with-buttons">
            <a class="accordion-toggle" data-toggle="collapse" href="#${identifier}">
                ${options.header}
            </a>
        </p>
    </div>
    `);
    const cardHeaderButtonLocator = $(`<button class="btn btn-sm btn-icon"><i class="fa fa-crosshairs"></i></button>`);
    const cardHeaderButtonTransparencyToggle = $(`<button class="btn btn-sm btn-icon"><i class="fa fa-eye-slash"></i></button>`);
    const cardHeaderButtonCloseButton = $(`<button class="btn btn-sm btn-icon"><i class="fal fa-xmark"></i></button>`);
    const cardTitleElem = cardHeaderElem.find(".card-title");
    if (options.hasLocator) cardTitleElem.prepend(cardHeaderButtonLocator);
    if (options.hasTransparencyToggle) cardTitleElem.append(cardHeaderButtonTransparencyToggle);
    if (options.closeable) cardTitleElem.append(cardHeaderButtonCloseButton);
    const cardBodyElem = $(`<div class="collapse show" id="${identifier}" style="overflow-y: auto"><div class="bcs-nonmodal-dialog-card-body card-body"></div></div>`);

    // style
    cardElem.addClass("bcs-glow");
    if (Number.isInteger(options.top)) targetElem.css("top", `${options.top}px`);
    if (Number.isInteger(options.right)) targetElem.css("right", `${options.right}px`);
    if (Number.isInteger(options.bottom)) targetElem.css("bottom", `${options.bottom}px`);
    if (Number.isInteger(options.left)) targetElem.css("left", `${options.left + menuWidth}px`);
    targetElem.css({
        "position": "fixed",
        "z-index": 100000,
        "width": `${options.width}px`,
        "height": `${options.height}px`,
    });

    // full height considerations
    if (options.fullHeight) {

        // adjust style
        targetElem.css({
            "top": `${realTop}px`,
            "bottom": "0px",
            "height": "0px"
        });
        cardElem.css("height", `calc(100vh - ${realTop}px)`);
        cardBodyElem.css("height", "100%");
        cardBodyElem.find(".bcs-nonmodal-dialog-card-body").css("height", "100%");

        // header collapsibility
        var collapsed = false;
        cardHeaderElem.find(".accordion-toggle").on("click", function () {
            cardBodyElem.addClass("bcs-opacity-100");
            cardBodyElem.toggleClass("bcs-opacity-0");
            if (collapsed) {
                cardElem.css("height", `calc(100vh - ${realTop}px)`);
                collapsed = false;
            } else {
                cardElem.css("height", "auto")
                collapsed = true;
            }
        });

        // explicitely set height to prevent Bootstrap's collapse from removing it when expanding again
        cardBodyElem.on("shown.bs.collapse", function () {
            cardBodyElem.css("height", "100%");
        });

    } else {

        // required for scrollability and proper post-collapse anchoring when not fullHeight
        cardElem.css("height", "100%");
        cardBodyElem.on("shown.bs.collapse", function () {
            targetElem.css("height", `${options.height}px`);
        });
        cardBodyElem.on("hidden.bs.collapse", function () {
            targetElem.css("height", "auto");
        });
    }

    // compose
    $(targetElem).empty();
    cardBodyElem.children(".bcs-nonmodal-dialog-card-body").append(targetElemContent);
    cardElem.append(cardHeaderElem, cardBodyElem);
    targetElem.prepend(cardElem);
    var innerTargetElem = $(targetElem).find(".bcs-nonmodal-dialog-card-body");
    $(targetElem).hide();

    // add transparency toggle
    var transparencyToggledOn = true;
    cardHeaderButtonTransparencyToggle.on("click", function () {
        var icon = cardHeaderButtonTransparencyToggle.find("i");
        transparencyToggledOn = !transparencyToggledOn;
        icon.toggleClass("fa-eye");
        icon.toggleClass("fa-eye-slash");
    });

    // add transparency
    var elem = $(innerTargetElem);
    $(elem)
        .on("mouseenter", function () {
            if (transparencyToggledOn) {
                $(targetElem).stop().animate({ opacity: 0.1 }, 150);
            }
        })
        .on("mouseleave", function () {
            $(targetElem).stop().animate({ opacity: 1 }, 150);
        });
    if (!options.transparent) cardHeaderButtonTransparencyToggle.trigger('click');

    // define functions
    var nonModalDialogObject = {
        show() {
            $(targetElem).show(); return this;
        },
        hide() {
            $(targetElem).hide(); return this;
        },
        expand() {
            $(targetElem).find("a.accordion-toggle.collapsed").click(); return this;
        },
        collapse() {
            $(targetElem).find("a.accordion-toggle:not(.collapsed)").click(); return this;
        },
        getBody() {
            return $(targetElem).find(".card-body");
        },
        empty() {
            this.getBody().html(""); return this;
        },
        fill(contents) {
            this.getBody().html(contents); return this;
        },
        setInitiator(element) {
            this.initiator = element; return this;
        },
        getInitiator() {
            return this.initiator;
        },
        setHeader(newHeader) {
            $(targetElem).find(".card-title > a").html(newHeader); return this;
        },
        renew(contents) {
            this.empty().fill(contents).show().expand(); return this;
        }
    };

    // add locator/close button behaviour
    cardHeaderButtonLocator.on("click", function () {
        nonModalDialogObject.getInitiator().scrollIntoView({ behavior: "smooth" });
        window.scrollBy(0, -100);
    });
    cardHeaderButtonCloseButton.on("click", function () {
        nonModalDialogObject.hide();
    });

    // finish
    updateNonmodalDialogPosition();
    return nonModalDialogObject;
}