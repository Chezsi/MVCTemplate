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

    // ----- Helper Functions -----
    function clearErrors(form) {
        // Only remove error class from fields that have it
        $(form).find('.input-validation-error').removeClass('input-validation-error');

        // Clear all validation messages
        $(form).find('span[data-valmsg-for]').each(function () {
            $(this).text('');
        });
    }

    // comments for debugging
    function showErrors(errors, form) {
        for (const key in errors) {
            const messages = errors[key];
            if (!messages || messages.length === 0) {
                continue; // ⛔ Skip fields with no actual error messages
            }

            const input = $(form).find(`[name="${key}"]`).filter(':input');
            //console.log(`🟨 Matched inputs for '${key}':`, input);

            if (input.length > 0) {
                input.addClass('input-validation-error');

                const validationSpan = $(form).find(`span[data-valmsg-for="${key}"]`);
                //console.log(`🟦 Matched span for '${key}':`, validationSpan);

                validationSpan.text(messages[0]); // Show the first error message
            } else {
                //console.warn(`⚠️ No input found for key: ${key}`);
            }
        }
    }


});
