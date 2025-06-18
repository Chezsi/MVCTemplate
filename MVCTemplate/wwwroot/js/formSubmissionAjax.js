$(document).ready(function () {
    // Initialize Ladda buttons
    Ladda.bind('.ladda-button');

    // ----- CREATE FORM -----
    const createAddFormBtn = $("#createAddFormSubmitBtn").get(0);
    const createAddFormLadda = Ladda.create(createAddFormBtn);
    $('#createForm').submit(async function (event) {
        event.preventDefault();
        var formData = new FormData(this);
        clearErrors(event.currentTarget);
        try {
            const response = await $.ajax({
                url: $(this).attr('action'),
                type: $(this).attr('method'),
                data: formData,
                contentType: false,
                processData: false,
            });
            // Show Alert
            toastr.success(response.message);
            // Refresh Data Table
            dataTable.ajax.reload();
            // Reset Form
            $('#createForm')[0].reset();
            // Hide Modal
            $('#createModal').modal('hide');
        } catch (error) {
            const formErrors = error?.responseJSON?.errors ?? {};
            showErrors(formErrors, event.currentTarget);
            toastr.error(error?.responseJSON?.message);
        } finally {
            createAddFormLadda.stop();
        }
    });

    // ----- UPDATE FORM -----
    const createEditFormBtn = $("#createEditFormSubmitBtn").get(0);
    const createEditFormLadda = Ladda.create(createEditFormBtn);
    $('#updateForm').submit(async function (event) {
        event.preventDefault();
        var formData = new FormData(this);
        clearErrors(event.currentTarget);
        try {
            const response = await $.ajax({
                url: $(this).attr('action'),
                type: $(this).attr('method'),
                data: formData,
                contentType: false,
                processData: false,
            });
            // Show Alert
            toastr.success(response.message);
            // Refresh Data Table
            dataTable.ajax.reload();
            // Reset Form
            $('#updateForm')[0].reset();
            // Hide Modal
            $('#updateModal').modal('hide');
        } catch (error) {
            const formErrors = error?.responseJSON?.errors ?? {};
            showErrors(formErrors, event.currentTarget);
            toastr.error(error?.responseJSON?.message);
        } finally {
            createEditFormLadda.stop();
        }
    });

    // ----- IMPORT FORM -----
    const importFileFormBtn = $("#importFileSubmitBtn").get(0);
    const importFileFormLadda = Ladda.create(importFileFormBtn);
    $('#importForm').submit(async function (event) {
        event.preventDefault();
        var formData = new FormData(this);
        // No clearErrors or showErrors here — import form unchanged
        try {
            const response = await $.ajax({
                url: $(this).attr('action'),
                type: $(this).attr('method'),
                data: formData,
                contentType: false,
                processData: false,
            });
            // Show Alert
            toastr.success(response.message);
            // Refresh Data Table
            dataTable.ajax.reload();
            // Reset Form
            $('#importForm')[0].reset();
            // Hide Modal
            $('#importModal').modal('hide');
        } catch (error) {
            const formErrors = error?.responseJSON?.errors ?? {};
            // No showErrors call here
            toastr.error(error?.responseJSON?.message);
        } finally {
            importFileFormLadda.stop();
        }
    });

    // ----- Helper functions -----
    function clearErrors(form) {
        $(form).find('.form-control').removeClass('input-validation-error');
        $(form).find('span.text-danger').text('');
    }

    function showErrors(errors, form) {
        for (const key in errors) {
            const input = $(form).find(`[name="${key}"]`);
            input.addClass('input-validation-error');
            const validationSpan = input.siblings(`span[data-valmsg-for="${key}"]`);
            validationSpan.text(errors[key][0]);
        }
    }
});
