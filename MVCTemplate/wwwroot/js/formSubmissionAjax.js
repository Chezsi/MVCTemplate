$(document).ready(function () {
    // Initialize Ladda buttons
    Ladda.bind('.ladda-button');
    //Initialize Create Button
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
            })
            //Show Alert
            toastr.success(response.message);
            //Refresh Data Table
            dataTable.ajax.reload();
            //Reset Form
            $('#createForm')[0].reset();
            //Hide Modal
            $('#createModal').modal('hide');
        } catch (error) {
            const formErrors = error?.responseJSON?.errors ?? {}
            showErrors(formErrors, event.currentTarget)
            toastr.error(error?.responseJSON?.message);
        } finally {
            createAddFormLadda.stop()
        }
    });
    //Initiliaze Update Button
    const createEditFormBtn = $("#createEditFormSubmitBtn").get(0);
    const createEditFormLadda = Ladda.create(createEditFormBtn);

    // Handle update form submission
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
            //Show Alert
            toastr.success(response.message);
            //Refresh Data Table
            dataTable.ajax.reload(); // to be used to avoid datatable reinitailization 
            //Reset Form
            $('#updateForm')[0].reset();
            //Hide Modal
            $('#updateModal').modal('hide');
        } catch (error) {
            const formErrors = error?.responseJSON?.errors ?? {}
            showErrors(formErrors, event.currentTarget)
            toastr.error(error?.responseJSON?.message);
        } finally {
            createEditFormLadda.stop();
        }
    });

    const importFileFormBtn = $("#importFileSubmitBtn").get(0);
    const importFileFormLadda = Ladda.create(importFileFormBtn);
    $('#importForm').submit(async function (event) {
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
            })
            //Show Alert
            toastr.success(response.message);
            //Refresh Data Table
            dataTable.ajax.reload();
            //Reset Form
            $('#importForm')[0].reset();
            //Hide Modal
            $('#importModal').modal('hide');
        } catch (error) {
            const formErrors = error?.responseJSON?.errors ?? {}
            showErrors(formErrors, event.currentTarget)
            toastr.error(error?.responseJSON?.message);
        } finally {
            importFileFormLadda.stop()
        }
    });


});