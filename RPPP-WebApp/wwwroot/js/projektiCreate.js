var documentIndex = 0;

function addDocument(event) {
    event.preventDefault();
    var tableBody = document.getElementById("documentsSection");

    var newRow = tableBody.insertRow();
    newRow.id = 'documentRow_' + documentIndex;

    console.log(documentIndex);

    newRow.innerHTML = `
        <td><input type="text" name="Dokumenti[${documentIndex}].NazivDok" placeholder="Naziv dokumenta..." class="form-control" /></td>
        <td>
            <select name="Dokumenti[${documentIndex}].VrstaDokumentaId" class="form-control">
                <option disabled selected value="">Odaberite vrstu dokumenta</option>
                ${generateDropdownOptions(vrsteDokJson)}
            </select>
        </td>
        <td>
            <select name="Dokumenti[${documentIndex}].StatusDokumentaId" class="form-control">
                <option disabled selected value="">Odaberite status dokumenta</option>
                ${generateDropdownOptions(statusiDokJson)}
            </select>
        </td>
        <td><input type="text" name="Dokumenti[${documentIndex}].EkstenzijaDokumenta" class="form-control" />
        <td><input type="text" name="Dokumenti[${documentIndex}].VrPrijenos" class="form-control" />
        <td><input type="text" name="Dokumenti[${documentIndex}].DatumZadIzmj" class="form-control" />
        <td><input type="file" name="Dokumenti[${documentIndex}].DatotekaFile" class="form-control" />
        <td><button class="btn btn-sm btn-danger" onclick="removeDocument(${documentIndex})"><i class="fa fa-minus"></i></button></td>
    `;

    documentIndex++;
}

    function removeDocument(index) {
        var rowToRemove = document.getElementById('documentRow_' + index);
        rowToRemove.parentNode.removeChild(rowToRemove);
    }

document.addEventListener('change', function (e) {
    if (e.target && e.target.type === "file") {
        var fileRow = e.target.closest("tr");

        var fileInput = fileRow.querySelector('input[type="file"]');
        var fileNameInput = fileRow.querySelector('input[name^="Dokumenti"][name$="NazivDok"]');
        var fileExtensionInput = fileRow.querySelector('input[name^="Dokumenti"][name$="EkstenzijaDokumenta"]');
        var vrPrijenosInput = fileRow.querySelector('input[name^="Dokumenti"][name$="VrPrijenos"]');
        var datumZadIzmjInput = fileRow.querySelector('input[name^="Dokumenti"][name$="DatumZadIzmj"]');

        if (fileInput.files.length > 0) {
            var fileName = fileInput.files[0].name;
            fileNameInput.value = fileName.split('.').slice(0, -1).join('.');
            fileExtensionInput.value = '.' + getFileExtension(fileName);
            vrPrijenosInput.value = getCurrentDateTime();
            datumZadIzmjInput.value = getCurrentDateTime();
        }
    }
});

function getFileExtension(fileName) {
    return fileName.split('.').pop();
}

function getCurrentDateTime() {
    var now = new Date();
    return now.toISOString().slice(0, 19).replace('T', ' ');
}
