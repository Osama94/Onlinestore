//fixed menu

$(function () {
    $('.fixed-menu .fa-gear').on('click', function () {

        $(this).parent('.fixed-menu').toggleClass('is-visible');

        if ($(this).parent('.fixed-menu').hasClass('is-visible')) {
            $(this).parent('.fixed-menu').animate({
                left: 0

            }, 500);

          

        }
        else {
            $(this).parent('.fixed-menu').animate({
                left: '-220px'

            }, 500);

          
        }

    });

});




////backTop
var scrollButton = $('#back-top');
$(window).scroll(function () {

    if ($(this).scrollTop() > 130) {
        scrollButton.show();
    } else {
        scrollButton.hide();
    }
});
scrollButton.on('click', function () {
    $('html,body').animate({ scrollTop: 0 }, 2000);
});


