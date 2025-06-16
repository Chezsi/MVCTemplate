$(document).ready(function () {
    // Step 1: Fade in the main content
    $(".app-main__inner").animate({ opacity: 1 }, 100, function () {
        // Step 2: Fade out and remove the logo overlay after a short delay
        setTimeout(function () {
            $("#logoOverlay").fadeOut(500, function () {
                $(this).remove(); // Clean up
            });
        }, 500);

        // Step 3: Trigger staggered animations
        const staggeredElements = document.querySelectorAll(".staggered");
        staggeredElements.forEach((el, index) => {
            setTimeout(() => {
                el.classList.add("show");
            }, index * 200); // Adjust delay to control timing
        });
    });
});
