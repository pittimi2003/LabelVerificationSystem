window.browserStorage = {
    setSessionItem: function (key, value) {
        sessionStorage.setItem(key, value);
    },
    getSessionItem: function (key) {
        return sessionStorage.getItem(key);
    },
    removeSessionItem: function (key) {
        sessionStorage.removeItem(key);
    }
};