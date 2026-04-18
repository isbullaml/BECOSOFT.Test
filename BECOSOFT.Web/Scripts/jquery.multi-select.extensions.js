jQuery.fn.extend({
    multiSelectSearch: function (unselectedResource, selectedResource, keepOrder = false, selectableOptgroup = false) {
        $(this).multiSelect({
            keepOrder: keepOrder,
            selectableOptgroup: selectableOptgroup,
            selectableHeader: "<input type='text' class='form-control form-control-sm searchField mb-2' autocomplete='off' placeholder='" + unselectedResource + "'>",
            selectionHeader: "<input type='text' class='form-control form-control-sm searchField mb-2' autocomplete='off' placeholder='" + selectedResource + "'>",
            afterInit: function () {
                var that = this;
                const $selectableSearch = that.$selectableUl.prev();
                const $selectionSearch = that.$selectionUl.prev();
                const containerID = that.$container.attr("id");
                const selectableSearchString = "#" + containerID + " .ms-elem-selectable:not(.ms-selected)";
                const selectionSearchString = "#" + containerID + " .ms-elem-selection.ms-selected";

                that.qs1 = $selectableSearch.quicksearch(selectableSearchString)
                    .on("keydown", function (e) {
                        if (e.which === 40) {
                            that.$selectableUl.focus();
                            return false;
                        }

                        return true;
                    });

                that.qs2 = $selectionSearch.quicksearch(selectionSearchString)
                    .on("keydown", function (e) {
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
    }
});