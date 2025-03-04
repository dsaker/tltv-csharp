// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function() {

    // Use event delegation to handle dynamically loaded .voice_info elements
    $(document).on("click", ".voice_info", function () {
        var $title = $(this).find(".title");
        if (!$title.length) {
            $(this).append('<span class="title">' + $(this).attr("title") + '</span>');
        } else {
            $title.remove();
        }
    });
});