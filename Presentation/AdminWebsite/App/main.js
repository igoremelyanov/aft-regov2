requirejs.config({
    paths: {
        text: "../Scripts/text",
        durandal: "../Scripts/durandal",
        plugins: "../Scripts/durandal/plugins",
        transitions: "../Scripts/durandal/transitions",
        nav: "layout/shell/nav",
        i18next: "../Scripts/i18next.amd.withJQuery-1.7.3",
        komapping: "../Scripts/knockout.mapping-latest",
        moment: "../Scripts/moment",
        dateTimePicker: "../Scripts/bootstrap-datetimepicker.min",
        datePicker: "../Scripts/bootstrap-datepicker.min",
        dateBinders: "bonus/dateBinders",
        wizard: "../Scripts/jquery.bootstrap.wizard.min",
        toastr: "../Scripts/toastr",
        shell: "layout/shell/shell",
        colorPicker: "../Scripts/bootstrap-colorpicker",
        config: "config",
        store: "../Scripts/store.min",
        jqueryOAuth: "../Scripts/jquery.oauth",
        vmGrid: "base/base-grid-viewmodel"
    }
});

$(document).ajaxError(function(event, jqXHR, ajaxSettings, thrownError) {
    switch (jqXHR.status) {
        case 500:
        console.log(jqXHR.responseText);
        var response = jqXHR.responseJSON;
        if ((response != null) && (response.error_message != null)) {
            return toastr.error(response.error_message + " " + response.Message);
        }
        break;
    case 408:
        return location.reload();
    }
});

define("jquery", function() { return jQuery; });
define("knockout", ko);

define(["durandal/system", "durandal/app", "durandal/viewLocator", "durandal/binder", "i18next", "config", "store", "jqueryOAuth"],
    function (system, app, viewLocator, binder, i18n, config, store, jqOAuth) {

    //>>excludeStart("build", true);
    system.debug(false); // if debug is true durandal's system.js logError fails on jquery errors
    //>>excludeEnd("build");

    app.title = "REGO V2";

    var defaultCulture = "en-CA";
    var lng = window.navigator.userLanguage || window.navigator.language || defaultCulture;

    var i18NOptions = {
        detectFromHeaders: false,
        lng: lng,
        fallbackLng: defaultCulture,
        load: "current",
        ns: "app",
        resGetPath: "app/locales/__lng__/__ns__.json",
        useCookie: false
    };

    app.configurePlugins({
        router: false,
        dialog: true,
        widget: {
            kinds: [
                "actionButtons",
                "gridHeader",
                "multiSelect",
                "timezones",
            ]
        }
    }, "plugins");

    $("<div>", { id: "initial-loader" }).appendTo("body");
    app.start().then(function () {
        i18n.init(i18NOptions, function () {
            binder.binding = function(obj, view) {
                $(view).i18n();
            };

            //Replace 'viewmodels' in the moduleId with 'views' to locate the view.
            //Look for partial views in a 'views' folder in the root.
            //viewLocator.useConvention();
            //router.setDefaultRoute(router.config.routes.home);
        

            //Show the app by setting the root view model for our application with a transition.
            app.setRoot("shell");
        });
    });

    if (typeof window.store == "undefined") {
        window.store = store;
    }

    var auth = new jqOAuth({
        events: {
            login: function () { },
            logout: function () { },
            tokenExpiration: function () {
                return $.ajax({
                    url: config.adminApi("token"),
                    type: "post",
                    async: false,
                    data: { grant_type: "refresh_token", refresh_token: refreshToken },
                    success: function (response) {
                        auth.setToken(response.access_token, response.refresh_token);
                    },
                    error: function () {
                        logout();
                    }
                });
            }
        }
    });

    auth.login(token, refreshToken);
    window.auth = auth;

    window.logout = function () {
        $.ajax({
            url: "/Account/Logout",
            async: false,
            success: function (response) {
                auth.logout();
                window.location.href = response;
            }
        });
    };
});