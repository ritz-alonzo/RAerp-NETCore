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
        $('#pluginConfigurationModal').modal("hide");
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
            $('#childEntityCreationModal').hide();
            $('.modal-backdrop').addClass("modal").removeClass("modal-backdrop fade show");
            $('.modal-open').attr("style", "");
            $('.modal-open').removeClass("modal-open");
            toastr.success("Successfully created child entity");
            //$(`#${partialViewId}`).html("");
            //SearchFunction(formSearchId, url, partialViewId);
            setTimeout(() => {
                location.reload();
            }, 1500)
        })
        .fail((data) => {
            console.log("Failed to save child entity");
            $('#childEntityCreationModal').hide();
            $('.modal-backdrop').addClass("modal").removeClass("modal-backdrop fade show");
            $('.modal-open').attr("style", "");
            $('.modal-open').removeClass("modal-open");
            toastr.error(data.responseJSON.error);
            $(`#${partialViewId}`).html("");
            SearchFunction(formSearchId, url, partialViewId);
        });
}

// Label and Input Control animation
$('.custom-input-set input.form-control').on('focus', (e) => {
    var readonlyActive = $(e.target).attr('readonly');
    if (readonlyActive != null || readonlyActive != undefined)
        return;
    $(':focus').siblings('label.form-label').css('top', '0.7rem');
    $(':focus').siblings('label.form-label').css('margin', '0 0 0 7px');
});

$('.custom-input-set input.form-control').on('focusout', () => {
    $('.custom-input-set label.form-label').css('top', '');
    $('.custom-input-set label.form-label').css('margin', '');
});

// Label and Select Control animation
$('.custom-input-set select.select2').on('focus', (e) => {
    var readonlyActive = $(e.target).attr('readonly');
    if (readonlyActive != null || readonlyActive != undefined)
        return;
    $(':focus').siblings('label.form-label').css('top', '0.7rem');
    $(':focus').siblings('label.form-label').css('margin', '0 0 0 7px');
});

$('.custom-input-set select.select2').on('focusout', () => {
    $('.custom-input-set label.form-label').css('top', '');
    $('.custom-input-set label.form-label').css('margin', '');
});

// Label and TextArea Control animation
$('.custom-input-set textarea.form-control').on('focus', () => {
    var readonlyActive = $(e.target).attr('readonly');
    if (readonlyActive != null || readonlyActive != undefined)
        return;
    $(':focus').siblings('label.form-label').css('top', '0.7rem');
    $(':focus').siblings('label.form-label').css('margin', '0 0 0 7px');
});

$('.custom-input-set textarea.form-control').on('focusout', () => {
    $('.custom-input-set label.form-label').css('top', '');
    $('.custom-input-set label.form-label').css('margin', '');
});
// END

$('.login-form-group input.login-form-control').on('focus', (e) => {
    var currentElement = $(e.currentTarget);
    if (currentElement.val() != '') {
        
        var labelElement = currentElement.siblings('label');
        if (labelElement.length > 0) {
            var idAttr = currentElement.attr('id');
            if (idAttr == 'Username') {
                labelElement.css('top', '0px');
                console.log('adjusting top on focus username');
            }
            else {
                labelElement.css('top', '65px');
                console.log('adjusting top on focus password');
            }
        }
    };
    /*$(':focus').siblings('label.login-form-label').css('color', 'rgb(13, 211, 213)');*/
});

$('.login-form-group input.login-form-control').on('focusout', (e) => {
    var currentElement = $(e.currentTarget);
    if (currentElement.val() != '') {
        var labelElement = currentElement.siblings('label');
        if (labelElement.length > 0) {
            labelElement.css('padding-top', '10px');
            console.log('adjusting padding on focus out');
        }
    };
    /*$('.login-form-group label.login-form-label').css('color', 'blue');*/
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




