document.addEventListener("DOMContentLoaded", function () {
    function onScroll() {
        var el = document.querySelector('.overview');
        if (!el) return;
        var rect = el.getBoundingClientRect();
        var windowHeight = (window.innerHeight || document.documentElement.clientHeight);
        if (rect.top <= windowHeight - 100) {
            el.classList.add('show');
            window.removeEventListener('scroll', onScroll);
        }
    }
window.addEventListener('scroll', onScroll);
});
