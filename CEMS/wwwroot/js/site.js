// CEMS - client-side behaviour (progressive enhancement; the server validates everything again)
document.addEventListener('DOMContentLoaded', function () {

    // 1. Success messages disappear by themselves after 5 seconds
    document.querySelectorAll('.alert-success.alert-dismissible').forEach(function (el) {
        setTimeout(function () {
            if (window.bootstrap) bootstrap.Alert.getOrCreateInstance(el).close();
        }, 5000);
    });

    // 2. Confirmation dialog for forms marked with data-confirm="message"
    document.addEventListener('submit', function (e) {
        var message = e.target && e.target.dataset ? e.target.dataset.confirm : null;
        if (message && !window.confirm(message)) e.preventDefault();
    });

    // 3. File upload check: type + size (data-max-mb) before the form is sent
    var allowed = ['pdf', 'doc', 'docx', 'png', 'jpg', 'jpeg'];
    document.querySelectorAll('input[type=file][data-max-mb]').forEach(function (input) {
        input.addEventListener('change', function () {
            var maxBytes = parseFloat(input.dataset.maxMb) * 1024 * 1024;
            for (var i = 0; i < input.files.length; i++) {
                var f = input.files[i];
                var ext = f.name.split('.').pop().toLowerCase();
                if (allowed.indexOf(ext) === -1 || f.size > maxBytes) {
                    alert('"' + f.name + '" cannot be uploaded. Use PDF, Word or image files up to ' + input.dataset.maxMb + ' MB.');
                    input.value = '';
                    return;
                }
            }
        });
    });

    // 4. "Confirm password" fields (data-match="OtherFieldName") must equal the other field
    document.querySelectorAll('input[data-match]').forEach(function (confirmBox) {
        var other = confirmBox.form.querySelector('[name="' + confirmBox.dataset.match + '"]');
        if (!other) return;
        function check() {
            var mismatch = confirmBox.value !== '' && confirmBox.value !== other.value;
            confirmBox.classList.toggle('is-mismatch', mismatch);
            confirmBox.setCustomValidity(mismatch ? 'Passwords do not match' : '');
        }
        confirmBox.addEventListener('input', check);
        other.addEventListener('input', check);
    });
});
