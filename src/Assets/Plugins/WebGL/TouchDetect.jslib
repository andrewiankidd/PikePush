mergeInto(LibraryManager.library, {
    JS_IsTouchDevice: function () {
        return ('ontouchstart' in window) ||
               (navigator.maxTouchPoints > 0) ||
               (navigator.msMaxTouchPoints > 0);
    }
});
