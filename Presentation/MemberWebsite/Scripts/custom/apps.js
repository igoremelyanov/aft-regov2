$(document).ready(function(){

    $('#forgotPassword').on('show.bs.modal', function(){
        $('.modal-password-step:first').show()
            .next().hide();
    });

    $('.ky-panel-styled-radio').change(function () {
        $('.ky-ticket-box').removeClass('is-selected');
        if (this.checked) {
            $(this).closest('.ky-ticket-box').addClass('is-selected');
        }
    });

    if ($("#phone-number").length) {
        // Choice country phone number
        $("#phone-number").intlTelInput({
            allowExtensions: true,
            autoFormat: true,
            autoHideDialCode: false,
            utilsScript: "/Scripts/utils-phonenumber.js",
            numberType: "MOBILE",
            autoPlaceholder: false
        });

        // Select country for phone number
        $("#phone-number").intlTelInput("setCountry", "cn");
    }

    // Remove when user will need to change the country
    $('#register-wrapper .flag-container, #register-wrapper .selected-flag').unbind('click');

    // Beautiful scroll
    $('.site-header .dropdown-menu.messages ul').slimscroll({
        height: '500px'
    });

    // slimscroll
    $('.account .menu.list').slimscroll({
        height: '100%'
    });

    if ($('.claimsbonus, .onlinedeposit').length > 0 && !$('.claimsbonus, .onlinedeposit').hasClass('eligible')) {
        $('.claimsbonus .claim-list').slimscroll({
            height: '644px'
        });

        $('.onlinedeposit .claim-list').slimscroll({
            height: '470px'
        });
    }

    if ($('input[data-target="id_back"]').length) {
        var input_file = $('input[data-target="id_back"]');

        input_file.on('focus', function() {
            console.log('dede');
        });
    }

    if ($('.table.list').length > 0) {
        $('.withdrawalhistory .table.list tbody, .pendingdeposit .table.list tbody,.transactionhistory .table.list tbody, .activebonus .table.list tbody, .bonushistory .table.list tbody, .gamesummary .table.list tbody').pageMe({
            pagerSelector:'#myPager',
            showPrevNext:true,
            hidePageNumbers:false,
            perPage:8
        });
    }

    // slimscroll
    /*$('.bonuslist tbody').slimscroll({
        height: '320px'
    });*/

    // dropdown on hover
    /*$('.dropdown').hover(function() {
      $(this).find('a[data-toggle="dropdown"]').trigger('click');
    }, function() {
      //$(this).find('.dropdown-menu').stop(true, true).delay(200).fadeOut(500);
    });*/

    // hack css for chrome on mac problem of captcha render
    if (navigator.userAgent.indexOf('Mac OS X') != -1 && navigator.userAgent.indexOf('Chrome') != -1) {
        $('.realperson-text').css('letter-spacing', '-3px');
    }

    // Modal game
    $('body').find('.js-game-starter').on('click', function (event) {
        var el = $(this);

        var gameUrl = el.data('game-url');
        var isActive = el.data('is-active');
        var ieCompatibility = el.data('ie-compatibility');

        if (!isActive) {
            popupAlert(i18n.t('app:playGames.notificationTitle'), i18n.t('app:playGames.gameInactive'));
            return;
        }

        if (ua.ie && ieCompatibility && ua.version < ieCompatibility) {
            popupAlert(i18n.t('app:playGames.notificationTitle'), i18n.t('app:playGames.browserNotSupported'));
            return;
        }

        var modal = $('#game-modal');
        modal.find('iframe').attr('src', gameUrl);
        modal.modal();
    });

    $('#game-modal').on('hide.bs.modal', function (event) {
        var modal = $(this);
        modal.find('iframe').attr('src', '');
    });

    $('body').find('.js-show-login').on('click', function (event) {
        $('a[href="#loginForm"]').trigger('click');
    });

    // After reset password
    if (localStorage.getItem("reset") != null) {
        $('a[href="#loginForm"]').trigger('click');
        localStorage.clear();
    }

    if($('.datepicker').length > 0) {
        $('.datepicker').datepicker({
            dateFormat: "M d yy"
        });
    }

    if ($('.account .menu.list').length > 0) {
        /* initiate the plugin */
        $(".account .holder").jPages({
          containerID  : "itemContainer",
          perPage      : 10,
          startPage    : 1,
          startRange   : 1,
          midRange     : 2,
          endRange     : 1
        });
    }

    // on Open login form focus to username input
    $('#loginForm').on('shown.bs.modal', function (e) {
      $('#loginForm .form-control').eq(0).focus();
    });
});

function popupAlert(title, message) {
    $('#alert-modal')
        .find('p.title').text(title).end()
        .find('p.message').text(message).end()
        .modal();
}