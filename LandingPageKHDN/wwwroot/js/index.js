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
        revealOnScroll('.form-register');
    }

    window.addEventListener('scroll', onScroll);
    onScroll();
});

document.querySelector('form').addEventListener('submit', function (e) {
    var response = grecaptcha.getResponse();
document.getElementById('recaptchaToken').value = response;
// Nếu chưa tick reCAPTCHA thì không cho submit
if (!response) {
    e.preventDefault();
alert('Vui lòng xác thực reCAPTCHA!');
    }
});

