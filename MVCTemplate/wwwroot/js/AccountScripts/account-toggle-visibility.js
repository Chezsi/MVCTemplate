$(document).on('click', '#togglePassword', function () {
    const passwordInput = $('#editPassword');
    const icon = $(this).find('i');

    const type = passwordInput.attr('type') === 'password' ? 'text' : 'password';
    passwordInput.attr('type', type);

    icon.toggleClass('fa-eye fa-eye-slash');
});
