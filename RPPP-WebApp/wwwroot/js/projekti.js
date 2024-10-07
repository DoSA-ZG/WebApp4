$(document).ready(function () {
    $(document).on('click', '.deleterow', function (event) {
        event.preventDefault();
        var tr = $(this).closest("tr");
        tr.remove();
    });

    $(".form-control").on('keydown', function (event) {
        if (event.which === 13) {
            event.preventDefault();
        }
    });

});