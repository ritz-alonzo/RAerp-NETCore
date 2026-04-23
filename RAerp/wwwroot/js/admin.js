// will add javascript functions here
// Search functionality
function SearchFunction(formSearchId, urlAction, partialViewDivId) {

    var data = $(`#${formSearchId}`).find('input, select, textarea').serialize();
    $.ajax({
        type: 'GET',
        url: urlAction,
        data: data
    })
        .done((data) => {
            console.log("Search Success");
            $(`#${partialViewDivId}`).html(data);
        })
        .fail((data) => {
            console.log("Search Failed");
        });
}

// Configuration Ajax POST functionality
function ConfigurationPost(formAjaxId, urlAction) {
    console.log('starting post configure');
    var data = $(`#${formAjaxId}`).find('input, select, textarea').serialize();

    console.log('checking data...');
    console.log(data);

    $.ajax({
        type: 'POST',
        url: urlAction,
        data: data
    })
        .done((data) => {
            console.log("Successfully saved configuration");
            // close modal
            /*$('#pluginConfigurationModal').hide();*/
            $('#pluginConfigurationModal').modal('hide');
            $('.modal-backdrop').addClass("modal").removeClass("modal-backdrop fade show");
            $('.modal-open').attr("style", "");
            $('.modal-open').removeClass("modal-open");
            toastr.success("Successfully saved settings");
        })
        .fail((data) => {
            console.log("Failed to save configuration");
            toastr.error(data.responseJSON.error);
        });
}

// Datatable creator functionality
function generateDataTable(tableId) {
    $(`#${tableId}`).DataTable({
        pageLength: 10,
        responsive: true,
        dom: 't<"row mt-4"<"col-md-9"i><"col-md-3"p>>'
    });
}

// Formats Status
function formatStatus(tableId) {
    $.each($(`#${tableId}`).find('tbody tr td'), (index, item) => {
        if (item !== undefined) {
            if (item.innerText != null || item.innerText != '') {
                var tag = item.innerText;
                if (item.innerText == "Active") {
                    tag = `<span class="badge badge-pill badge-success fs-6">Active</span>`;
                    $(item).html(tag);
                }
                else if (item.innerText == "Inactive") {
                    tag = `<span class="badge badge-pill badge-danger fs-6"><b>Inactive</b></span>`;
                    $(item).html(tag);
                }
            }
        }
    });
}

// notification functions for Controllers
function initializeControllerNotification(notifFunc, message) {

    if (notifFunc == null || notifFunc == "") {
        // do nothing
        return;
    }
    else if (notifFunc == "InfoNotif") {
        toastr.info(message);
    }
    else if (notifFunc == "SuccessNotif") {
        toastr.success(message);
    }
    else if (notifFunc == "WarningNotif") {
        toastr.warning(message);
    }
    else if (notifFunc == "ErrorNotif") {
        toastr.error(message);
    }
}

// Create Child EntityType Ajax POST functionality
function CreateChildEntityPost(formAjaxId, urlAction) {
    console.log('starting post creation');
    var data = $(`#${formAjaxId}`).find('input, select, textarea').serialize();
    var formSearchId = 'childEntityTypes_search';
    var url = '/EntityTypes/ChildEntityTypeListSearch'
    var partialViewId = 'childEntityTypes';
    $.ajax({
        type: 'POST',
        url: urlAction,
        data: data
    })
        .done((data) => {
            console.log("Successfully created child entity");
            /*$('#childEntityCreationModal').hide();*/
            $('#childEntityCreationModal').modal('hide');
            $('.modal-backdrop').addClass("modal").removeClass("modal-backdrop fade show");
            $('.modal-open').attr("style", "");
            $('.modal-open').removeClass("modal-open");
            toastr.success("Successfully created child entity");
            //$(`#${partialViewId}`).html("");
            //SearchFunction(formSearchId, url, partialViewId);
            setTimeout(() => {
                location.reload();
            }, 500)
        })
        .fail((data) => {
            console.log("Failed to save child entity");
            /*$('#childEntityCreationModal').hide();*/
            $('#childEntityCreationModal').modal('hide');
            $('.modal-backdrop').addClass("modal").removeClass("modal-backdrop fade show");
            $('.modal-open').attr("style", "");
            $('.modal-open').removeClass("modal-open");
            toastr.error(data.responseJSON.error);
            $(`#${partialViewId}`).html("");
            SearchFunction(formSearchId, url, partialViewId);
        });
}

// Form Item Selector methods
function FormItemsReload(urlAction, partialViewDivId) {
    var data = $('form').find('input, select, textarea').serialize();
    $.ajax({
        type: 'GET',
        url: urlAction,
        data: data
    })
        .done((data) => {
            console.log("Search Success");
            $(`#${partialViewDivId}`).html(data);
        })
        .fail((data) => {
            console.log("Search Failed");
        });
}

function SelectorInsertItemPost(formAjaxId, postUrl, itemsTabUrl, itemsPartialViewId) {
    console.log('starting form item insert');
    var catalogIds = [];
    var selectedItems = $(`#${formAjaxId}`).find('tbody tr td input[type="checkbox"]:checked');
    $(selectedItems).each((index, item) => {
        var idTableData = $(item).parent('td').siblings('td')[0];
        var catalogId = $(idTableData).find('input[id="Id"]').val();
        if (catalogId != null || catalogId != undefined) {
            catalogIds.push(catalogId);
        }
    });

    var formId = $('form').find('input[id="Id"]')[0].value;
    if (formId == null || formId == undefined) {
        alert('Failed to add item');
        return;
    }
     $.ajax({
        type: 'POST',
        url: postUrl,
        traditional: true,
        data: {
            formId: formId,
            catalogIds: catalogIds
        }
     })
        .done((data) => {
            console.log("Successfully added items");
            $('#formItemModal').hide();
            /*$('#formItemModal').modal('hide');*/
            $('.modal-backdrop').addClass("modal").removeClass("modal-backdrop fade show");
            $('.modal-open').attr("style", "");
            $('.modal-open').removeClass("modal-open");
            toastr.success("Successfully added items");
            $(`#${itemsPartialViewId}`).html("");
            FormItemsReload(itemsTabUrl, itemsPartialViewId);
        })
        .fail((data) => {
            console.log("Failed to add items");
            $('#formItemModal').hide();
            /*$('#formItemModal').modal('hide');*/
            $('.modal-backdrop').addClass("modal").removeClass("modal-backdrop fade show");
            $('.modal-open').attr("style", "");
            $('.modal-open').removeClass("modal-open");
            toastr.error(data.responseJSON.error);
            $(`#${itemsPartialViewId}`).html("");
            FormItemsReload(itemsTabUrl, itemsPartialViewId);
        });
}


$('tbody tr').on('dblclick', (e) => {
    console.log('table body table row double click');
    e.stopImmediatePropagation();
    var selectedTableRow = $(e.currentTarget);

    if (selectedTableRow == null || selectedTableRow == undefined) {
        return;
    }
    // need to add flag if row is in edit or view
    var tableRowActionFlag = $(selectedTableRow).attr('data-row-action');
    if (tableRowActionFlag != null || tableRowActionFlag != null || tableRowActionFlag != undefined) {
        console.log('Table row is in edit state');
        return;
    }
    $(selectedTableRow).attr('data-row-action', 'Edit');

    var embeddedEditBtn = selectedTableRow.find('.embedded-table-edit-btn');
    if (embeddedEditBtn == null || embeddedEditBtn == undefined) {
        return;
    }
    var embeddedSaveBtn = selectedTableRow.find('.embedded-table-save-btn');
    if (embeddedSaveBtn == null || embeddedSaveBtn == undefined) {
        return;
    }
    var embeddedDeleteBtn = selectedTableRow.find('.embedded-table-delete-btn');
    if (embeddedDeleteBtn == null || embeddedDeleteBtn == undefined) {
        return;
    }
    var embeddedCancelBtn = selectedTableRow.find('.embedded-table-cancel-btn');
    if (embeddedCancelBtn == null || embeddedCancelBtn == undefined) {
        return;
    }
    $(embeddedEditBtn).hide();
    $(embeddedSaveBtn).show();
    $(embeddedDeleteBtn).hide();
    $(embeddedCancelBtn).show();

    var editableTableDatas = selectedTableRow.find('td[data-editable="True"]');
    console.log(editableTableDatas);
    $(editableTableDatas).each((index, item) => {
        var itemOriginalValue = $(item).html();
        console.log(itemOriginalValue);
        var itemElementType = $(item).attr('data-element-type');
        var itemElementHeaderName = $(item).attr('data-element-header');
        $(item).html(`<input type="${itemElementType}" name="${itemElementHeaderName}" id="${itemElementHeaderName}" value="${itemOriginalValue}">`);
    });
});

$('.embedded-table-edit-btn').on('click', (e) => {
    console.log('edit btn click');
    e.stopImmediatePropagation();
    var embeddedEditBtn = $(e.currentTarget);
    if (embeddedEditBtn == null || embeddedEditBtn == undefined) {
        return;
    }
    var itemRow = $(embeddedEditBtn).parent('td').parent('tr');
    if (itemRow == undefined || itemRow.length == 0) {
        console.log('Failed to get item row');
        return;
    }
    // need to add flag if row is in edit or view
    var tableRowActionFlag = $(itemRow).attr('data-row-action');
    if (tableRowActionFlag != null || tableRowActionFlag != null || tableRowActionFlag != undefined) {
        console.log('Table row is in edit state');
        return;
    }
    $(itemRow).attr('data-row-action', 'Edit');

    var embeddedSaveBtn = embeddedEditBtn.siblings('.embedded-table-save-btn');
    console.log(embeddedSaveBtn);
    if (embeddedSaveBtn == null || embeddedSaveBtn == undefined) {
        return;
    }
    var embeddedDeleteBtn = embeddedEditBtn.siblings('.embedded-table-delete-btn');
    console.log(embeddedDeleteBtn);
    if (embeddedDeleteBtn == null || embeddedDeleteBtn == undefined) {
        return;
    }
    var embeddedCancelBtn = embeddedEditBtn.siblings('.embedded-table-cancel-btn');
    console.log(embeddedCancelBtn);
    if (embeddedCancelBtn == null || embeddedCancelBtn == undefined) {
        return;
    }
    $(embeddedEditBtn).hide();
    $(embeddedSaveBtn).show();
    $(embeddedDeleteBtn).hide();
    $(embeddedCancelBtn).show();

    var editableTableDatas = $(itemRow).find('td[data-editable="True"]');
    console.log(editableTableDatas);
    $(editableTableDatas).each((index, item) => {
        var itemOriginalValue = $(item).html();
        console.log(itemOriginalValue);
        var itemElementType = $(item).attr('data-element-type');
        var itemElementHeaderName = $(item).attr('data-element-header');
        $(item).html(`<input type="${itemElementType}" name="${itemElementHeaderName}" id="${itemElementHeaderName}" value="${itemOriginalValue}">`);
    });
});

$('.embedded-table-save-btn').on('click', (e) => {
    console.log('save btn clicked');
    var embeddedSaveBtn = $(e.currentTarget);
    var itemRow = $(embeddedSaveBtn).parent('td').parent('tr');
    if (itemRow == undefined || itemRow.length == 0) {
        console.log('Failed to get item row');
        return;
    }
    // check if null 
    var itemId = $(itemRow).find('td input[id="Id"]')[0];
    if (itemId == undefined || itemId.length == 0) {
        console.log('Id is not yet created');
    }
    var editUrl = $(embeddedSaveBtn).attr('data-edit-url');
    if (editUrl == null || editUrl == '' || editUrl == undefined) {
        console.log('Edit url is not defined');
        return;
    }
    var viewUrl = $(embeddedSaveBtn).attr('data-partial-view-url');
    if (viewUrl == null || viewUrl == '' || viewUrl == undefined) {
        console.log('View url is not defined');
        return;
    }
    var partialViewId = $(embeddedSaveBtn).attr('data-partial-view-id');
    if (partialViewId == null || partialViewId == '' || partialViewId == undefined) {
        console.log('Partial view id is not defined');
        return;
    }
    var formItem = $(itemRow).find('td, input');
    if (formItem.length == 0 || formItem == null || formItem == undefined) {
        console.log('Form doesnt have any data');
        return;
    }
    var formItemData = $(formItem).serialize();
    $.ajax({
        type: 'POST',
        url: editUrl,
        data: formItemData
    })
        .done((data) => {
            console.log("Successfully saved items");
            toastr.success("Successfully saved items");
            // format total decimals
            $.each(data, (key, value) => {
                var formElementId = key.at(0).toUpperCase() + key.slice(1);
                var formElement = $('form').find(`input[id='${formElementId}']`);
                console.log(formElement);
                if (formElement != null || formElement != undefined || formElement.length == 1) {
                    var formattedValue = parseFloat(value).toFixed(2);
                    $(formElement).val(formattedValue);
                }
            });

            FormItemsReload(viewUrl, partialViewId);
        })
        .fail((data) => {
            console.log("Failed to edit item");
            toastr.error(data.responseJSON.error);
        });

});

$('.embedded-table-cancel-btn').on('click', (e) => {
    console.log('cancel click');
    e.stopImmediatePropagation();
    var embeddedCancelBtn = $(e.currentTarget);
    var embeddedEditBtn = embeddedCancelBtn.siblings('.embedded-table-edit-btn');
    if (embeddedEditBtn == null || embeddedEditBtn == undefined) {
        return;
    }
    var embeddedDeleteBtn = embeddedCancelBtn.siblings('.embedded-table-delete-btn');
    if (embeddedDeleteBtn == null || embeddedDeleteBtn == undefined) {
        return;
    }
    var embeddedSaveBtn = embeddedCancelBtn.siblings('.embedded-table-save-btn');
    if (embeddedSaveBtn == null || embeddedSaveBtn == undefined) {
        return;
    }
    var editableTableDatas = embeddedCancelBtn.parents('tr').find('td[data-editable="True"]');
    $(editableTableDatas).each((index, item) => {
        var originalValue = $(item).attr('data-element-originalvalue');
        if (originalValue == null || originalValue == '' || originalValue == undefined) {
            return;
        }
        $(item).html(originalValue);
    });

    var itemRow = $(embeddedCancelBtn).parent('td').parent('tr');
    if (itemRow == null || itemRow == undefined) {
        console.log('Item row not defined');
        return;
    }
    var itemRowDataAction = $(embeddedCancelBtn).attr('data-row-action');
    if (itemRowDataAction != null || itemRowDataAction != '' || itemRowDataAction != null) {
        console.log('Removing item row data action')
        $(itemRow).removeAttr('data-row-action');
    }

    $(embeddedEditBtn).show();
    $(embeddedDeleteBtn).show();
    $(embeddedCancelBtn).hide();
    $(embeddedSaveBtn).hide();

});

// Form Item Selector methods END

// Label and Input Control animation
// $('.custom-input-set input.form-control').on('focus', (e) => {
//     var readonlyActive = $(e.target).attr('readonly');
//     if (readonlyActive != null || readonlyActive != undefined)
//         return;
//     $(':focus').siblings('label.form-label').css('top', '0.7rem');
//     $(':focus').siblings('label.form-label').css('margin', '0 0 0 7px');
// });

// $('.custom-input-set input.form-control').on('focusout', () => {
//     $('.custom-input-set label.form-label').css('top', '');
//     $('.custom-input-set label.form-label').css('margin', '');
// });

// // Label and Select Control animation
// $('.custom-input-set select.select2').on('focus', (e) => {
//     var readonlyActive = $(e.target).attr('readonly');
//     if (readonlyActive != null || readonlyActive != undefined)
//         return;
//     $(':focus').siblings('label.form-label').css('top', '0.7rem');
//     $(':focus').siblings('label.form-label').css('margin', '0 0 0 7px');
// });

// $('.custom-input-set select.select2').on('focusout', () => {
//     $('.custom-input-set label.form-label').css('top', '');
//     $('.custom-input-set label.form-label').css('margin', '');
// });

// // Label and TextArea Control animation
// $('.custom-input-set textarea.form-control').on('focus', () => {
//     var readonlyActive = $(e.target).attr('readonly');
//     if (readonlyActive != null || readonlyActive != undefined)
//         return;
//     $(':focus').siblings('label.form-label').css('top', '0.7rem');
//     $(':focus').siblings('label.form-label').css('margin', '0 0 0 7px');
// });

// $('.custom-input-set textarea.form-control').on('focusout', () => {
//     $('.custom-input-set label.form-label').css('top', '');
//     $('.custom-input-set label.form-label').css('margin', '');
// });
// END

//$('.login-form-group input.login-form-control').on('focus', (e) => {
//    var currentElement = $(e.currentTarget);
//    if (currentElement.val() != '') {

//        var labelElement = currentElement.siblings('label');
//        if (labelElement.length > 0) {
//            var idAttr = currentElement.attr('id');
//            if (idAttr == 'Username') {
//                labelElement.css('top', '0px');
//                console.log('adjusting top on focus username');
//            }
//            else {
//                labelElement.css('top', '65px');
//                console.log('adjusting top on focus password');
//            }
//        }
//    };
//    /*$(':focus').siblings('label.login-form-label').css('color', 'rgb(13, 211, 213)');*/
//});

//$('.login-form-group input.login-form-control').on('focusout', (e) => {
//    var currentElement = $(e.currentTarget);
//    if (currentElement.val() != '') {
//        var labelElement = currentElement.siblings('label');
//        if (labelElement.length > 0) {
//            labelElement.css('padding-top', '10px');
//            console.log('adjusting padding on focus out');
//        }
//    };
//    /*$('.login-form-group label.login-form-label').css('color', 'blue');*/
//});

// Date Formatter for Forms

/* Field on load Format */

$(window).on('load', (e) => {
    var createdOnElements = $('.custom-input-set input[type="datetime"]');
    if (createdOnElements.length > 0) {
        $(createdOnElements).each((index, item) => {
            if (item.value != '') {
                item.value = moment(item.value, 'YYYY-MM-DD hh:mm:ss A').format('YYYY-MM-DD hh:mm:ss A');
            }
        });
    }

    // Datetime local Flatpicker v4
    var datetimeLocalInputTypes = $('.custom-input-set input[type="datetime-local"]');
    if (datetimeLocalInputTypes.length > 0) {
        $(datetimeLocalInputTypes).each((index, item) => {
            var isEdittable = false;
            if ($(item).attr('readonly') == null || $(item).attr('readonly') == undefined) {
                isEdittable = true;
            }
            $(item).flatpickr({
                time_24hr: false,
                enableTime: true,
                enableSeconds: false,
                dateFormat: "Y-m-d h:i K",
                allowInput: isEdittable,
                clickOpens: isEdittable
            });
        });
    }

    // Number format - to add here
    var numberElements = $('.custom-input-set input[type="number"]');
    if (numberElements.length > 0) {
        $(numberElements).each((index, item) => {
            var currentItemValue = $(item).val();
            if (currentItemValue != null || currentItemValue != undefined) {
                var formattedValue = parseFloat(currentItemValue).toFixed(2); // 2 decimal places
                $(item).val(formattedValue);
            }
        });
    }
});

// refresh grid
//function refreshGrid() {
//    console.log('refreshing grid');
//    var selectedButton = $(this.currentTarget);
//    var formSearchId = selectedButton.attr('data-grid-searchId');
//    if (formSearchId == '' || formSearchId == "") {
//        alert('Failed to refresh grid');
//    }
//    var partialViewId = selectedButton.attr('data-grid-partialViewId');
//    if (partialViewId == '' || partialViewId == "") {
//        alert('Failed to refresh grid');
//    }
//    var gridAction = selectedButton.attr('data-grid-url-action');
//    console.log('this is the grid action ' + gridAction);
//    if (gridAction == '' || gridAction == "") {
//        alert('Failed to refresh grid');
//    }

//    $('#loader').css("display", "flex");
//    console.log('showing loader');
//    setTimeout(function () {
//        var selectedButton = $(this.currentTarget);
//        var formSearchId = selectedButton.attr('data-grid-searchId');
//        if (formSearchId == '' || formSearchId == "") {
//            alert('Failed to refresh grid');
//        }
//        var partialViewId = selectedButton.attr('data-grid-partialViewId');
//        if (partialViewId == '' || partialViewId == "") {
//            alert('Failed to refresh grid');
//        }
//        var gridAction = selectedButton.attr('data-grid-url-action');
//        if (gridAction == '' || gridAction == "") {
//            alert('Failed to refresh grid');
//        }
//        SearchFunction(formSearchId, gridAction, partialViewId);

//        console.log('hide loader');
//        $('#loader').css("display", "none");
//    }, 1000);
//}




