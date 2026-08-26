window.spc = {
    loadInstructionEditor: function () {
        if (window.spcInstructionEditor) {
            return Promise.resolve();
        }

        if (window.__spcInstructionEditorLoading) {
            return window.__spcInstructionEditorLoading;
        }

        window.__spcInstructionEditorLoading = new Promise(function (resolve, reject) {
            var script = document.createElement("script");
            script.src = "js/instruction-editor/instruction-editor.bundle.js";
            script.onload = function () {
                if (window.spcInstructionEditor && typeof window.spcInstructionEditor.init === "function") {
                    resolve();
                    return;
                }

                reject(new Error("Instruction editor bundle loaded but API is missing"));
            };
            script.onerror = function () {
                reject(new Error("Failed to load instruction editor bundle"));
            };
            document.head.appendChild(script);
        });

        return window.__spcInstructionEditorLoading;
    },
    setBeforeUnload: function (enabled) {
        if (enabled) {
            window.onbeforeunload = function () {
                return "";
            };
        } else {
            window.onbeforeunload = null;
        }
    },
    preventComboboxNav: function (event) {
        if (!event.target || event.target.getAttribute("role") !== "combobox") {
            return;
        }

        if (event.target.getAttribute("aria-expanded") !== "true") {
            return;
        }

        if (event.key === "ArrowDown" || event.key === "ArrowUp" || event.key === "Enter" || event.key === "Escape") {
            event.preventDefault();
        }
    }
};

document.addEventListener("keydown", window.spc.preventComboboxNav, true);
