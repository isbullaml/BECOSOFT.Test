const mobileScreen = window.matchMedia("(max-width: 1024px )");

$(document).ready(function () {
    $(".bcs-sidebar-dropdown-alwaysopen").addClass("bcs-sidebar-toggle-open");
});

// sidebar dropdown open/close
$(document).on("click", ".bcs-sidebar-dropdown-toggle", function () {
    const element = $(this);

    // skip nested dropdowns
    if (element.parent().hasClass("bcs-sidebar-dropdown-menu") || element.parent().hasClass("bcs-sidebar-level3")) {
        element.toggleClass("bcs-sidebar-toggle-open");
        element.siblings().removeClass("bcs-sidebar-toggle-open");
        return
    }

    // close other dropdowns
    element.closest(".bcs-sidebar-dropdown")
        .toggleClass("bcs-sidebar-toggle-open")
        .find(".bcs-sidebar-dropdown")
        .removeClass("bcs-sidebar-toggle-open");
    element.parent()
        .siblings()
        .removeClass("bcs-sidebar-toggle-open");
});

// mobile menu
$(document).on("click", ".bcs-menu-toggle", function () {
    toggleMenu();
});

$(document).on("click", ".bcs-tablet-menu-backdrop", function () {
    toggleMenu();
});

var toggleMenu = function () {
    if (mobileScreen.matches) {
        $(".bcs-sidebar").toggleClass("bcs-mobile-show");
        $(".bcs-brand").toggleClass("bcs-mobile-show");
        $(".bcs-tablet-menu-backdrop").toggleClass("d-none");
    } else {
        $(".bcs-main").toggleClass("bcs-collapsed-sidebar");
        $(".bcs-brand").toggleClass("bcs-brand-smaller");
        $(".bcs-brand-logo-big").toggleClass("bcs-brand-logo-show");
        $(".bcs-brand-logo-small").toggleClass("bcs-brand-logo-show");
        $(".bcs-titlebar").toggleClass("bcs-titlebar-smaller");
        updateNonmodalDialogPositionAfterMenuChange(!$(".bcs-main").hasClass("bcs-collapsed-sidebar"));
    }
};

var hideMenu = function () {
    if (!$(".bcs-brand").hasClass("bcs-brand-smaller")) {
        toggleMenu();
    }
};