window.onload = function () {
    $(".app-main__inner").animate({ opacity: 1 }, 100, function () {
        setTimeout(function () {
            $("#logoOverlay").fadeOut(500, function () {
                $(this).remove();
            });
        }, 500);

        const staggeredElements = document.querySelectorAll(".staggered");
        staggeredElements.forEach((el, index) => {
            setTimeout(() => el.classList.add("show"), index * 200);
        });
    });
};