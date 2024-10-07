$(document).on('click', '.deleterow', function (event) {
    event.preventDefault();
    var tr = $(this).parents("tr");
    tr.remove();
});

$(function () {
    $(".form-control").bind('keydown', function (event) {
        if (event.which === 13) {
            event.preventDefault();
        }
    });

    // $("#artikl-kolicina, #artikl-rabat").bind('keydown', function (event) {
    //     if (event.which === 13) {
    //         event.preventDefault();
    //         dodajArtikl();
    //     }
    // });


    $("#uloga-dodaj").click(function (event) {
        event.preventDefault();
        dodajUlogu();
    });
});

var newId = 1000000;

function dodajUlogu() {

        var datumOd = $("#uloga-datum-od").val();
        var datumDo = $("#uloga-datum-do").val();
        var projektId = $("#uloga-projekt").val();
        var vrstaId = $("#uloga-vrsta-uloge").val();
        
        if(projektId === null){
            alert('Niste odabrali projekt!');
            return;
        }

        if(vrstaId === null){
            alert('Niste odabrali vrstu uloge!');
            return;
        }
        
        if(datumOd === null || datumOd === ''){
            alert('Niste odabrali datum početka!');
            return;
        }

        if(datumDo === null || datumDo === ''){
            alert('Niste odabrali datum kraja!');
            return;
        }
        
        var template = $('#template').html();
        template = template.replace(/--id--/g, newId)
            .replace(/--datum_od--/g, datumOd)
            .replace(/--datum_do--/g, datumDo)
            .replace(/--projekt_id--/g, projektId)
            .replace(/--vrsta_id--/g, vrstaId)
            .replace(/--suradnik_id--/g, suradnikId)
            .replace(/--projekti--/g, generateDropdownOptions(projektiJson, projektId))
            .replace(/--vrste_uloga--/g, generateDropdownOptions(vrsteUlogaJson, vrstaId));
        $(template).find('tr').insertBefore($("#table-uloge").find('tr').last());

        $("#uloga-id").val(newId);
        $("#uloga-datum-od").val('');
        $("#uloga-datum-do").val('');
        $("#uloga-projekt").val('');
        $("#uloga-vrsta-uloge").val('');
        
        newId++;
}