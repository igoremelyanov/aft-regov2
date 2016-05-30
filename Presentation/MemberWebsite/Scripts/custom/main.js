var lng = $.cookie("CultureCode") || "en-US";

var i18NOptions = {
    useLocalStorage: true | false,
    localStorageExpirationTime: 86400000, // in ms, default 1 week
    detectFromHeaders: false,
    lng: lng,
    fallbackLng: "en-US",
    load: "current",
    ns: "app",
    resGetPath: "/scripts/custom/locales/__lng__/__ns__.json",
    useCookie: false,
    getAsync: false
};

i18n.init(i18NOptions, function () {
    $('#main-container').i18n();
});

$(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
    switch (jqXHR.status) {
        case 500:
            console.log(jqXHR.responseText);
            if (jqXHR.responseJSON.unauthorized === false)
            //location.replace("/Error/ServerError.html");
                popupAlert('Server Error', 'Please contact an administrator.');
            break;
        case 404:
            console.log(jqXHR.responseText);
            location.replace("/Error/PageNotFound.html");
            break;
        case 403:
            console.log(jqXHR.responseText);
            location.replace("/Error/ForbiddenAccess.html");
            break;
        case 408:
            return location.reload();
    }
});