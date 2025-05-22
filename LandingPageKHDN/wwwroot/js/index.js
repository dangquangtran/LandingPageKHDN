document.addEventListener("DOMContentLoaded", function () {
    function revealOnScroll(selector) {
        var els = document.querySelectorAll(selector);
        var windowHeight = (window.innerHeight || document.documentElement.clientHeight);
        els.forEach(function (el) {
            var rect = el.getBoundingClientRect();
            if (rect.top <= windowHeight - 100) {
                el.classList.add('show');
            }
        });
    }

    function onScroll() {
        revealOnScroll('.overview');
        revealOnScroll('.scroll-animate');
    }

    window.addEventListener('scroll', onScroll);
    onScroll();
});
