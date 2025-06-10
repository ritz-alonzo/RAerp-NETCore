$(document).ready(() => {
    // GetBarangaysByCityCode
    $('#Address_RegionCode').change((e) => {
        var value = $(e.target).val();
        if (value != null || value != undefined) {
            $.ajax({
                url: window.location.origin + '/Address/GetCitiesByRegionCode',
                method: 'GET',
                data: {
                    regionCode: value
                },
            })
            .done((data) => {
                if (data != null && data != undefined && data.length > 0) {
                    $.each(data, (index, dataValue) => {
                        $('#Address_CityCode').append(`<option value=${dataValue.value}>${dataValue.text}</option>`);
                        $('#Address_CityCode').attr('disabled', false);
                    })
                }
            })
            .fail((data) => {
                console.log('failed to load cities');
            })
        }
    });

    $('#Address_CityCode').change((e) => {
        var value = $(e.target).val();
        console.log('barangay code');
        if (value != null || value != undefined) {
            $.ajax({
                url: window.location.origin + '/Address/GetBarangaysByCityCode',
                method: 'GET',
                data: {
                    cityCode: value
                },
            })
            .done((data) => {
                if (data != null && data != undefined && data.length > 0) {
                    $.each(data, (index, dataValue) => {
                        $('#Address_BarangayCode').append(`<option value=${dataValue.value}>${dataValue.text}</option>`);
                        $('#Address_BarangayCode').attr('disabled', false);
                    })
                }
            })
            .fail((data) => {
                console.log('failed to load barangays');
            })
        }
    });
});