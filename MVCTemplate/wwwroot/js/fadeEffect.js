$(document).ready(function () {
    // Fade in the main content
    $(".app-main__inner").animate({ opacity: 1 }, 100, function () {
        // Wait before fading out the logo overlay
        setTimeout(function () {
            $("#logoOverlay").fadeOut(500, function () {
                $(this).remove(); // Remove the overlay after fade out
            });
        }, 500);
    });
});
