window.spc = {
    setBeforeUnload: function (enabled) {
        if (enabled) {
            window.onbeforeunload = function () {
                return "";
            };
        } else {
            window.onbeforeunload = null;
        }
    }
};
