var documentIndex = 0;

function addDocumentEdit(event) {
    event.preventDefault();
    var tableBody = document.querySelector("#table-dokumentis tbody");

    var newIndex = documentIndex;
    var newRow = document.createElement('tr');
    newRow.id = 'documentRow_' + newIndex;

    console.log(newIndex);

    newRow.innerHTML = `
        <td><input type="text" name="NoviDokumenti[${newIndex}].NazivDok" placeholder="Naziv dokumenta..." class="form-control" /></td>
        <td>
            <select name="NoviDokumenti[${newIndex}].VrstaDokumentaId" class="form-control">
                <option disabled selected value="">Odaberite vrstu dokumenta</option>
                ${generateDropdownOptions(vrsteDokJson)}
            </select>
        </td>
        <td>
            <select name="NoviDokumenti[${newIndex}].StatusDokumentaId" class="form-control">
                <option disabled selected value="">Odaberite status dokumenta</option>
                ${generateDropdownOptions(statusiDokJson)}
            </select>
        </td>
        <td><input type="text" name="NoviDokumenti[${newIndex}].EkstenzijaDokumenta" class="form-control" /></td>
        <td><input type="text" name="NoviDokumenti[${newIndex}].VrPrijenos" class="form-control" /></td>
        <td><input type="text" name="NoviDokumenti[${newIndex}].DatumZadIzmj" class="form-control" /></td>
        <td><input type="file" name="NoviDokumenti[${newIndex}].DatotekaFile" class="form-control" /></td>
        <td><button class="btn btn-sm btn-danger" onclick="removeDocument(${newIndex})"><i class="fa fa-minus"></i></button></td>
    `;

    tableBody.appendChild(newRow);
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
        var fileNameInput = fileRow.querySelector('input[name^="NoviDokumenti"][name$="NazivDok"]');
        var fileExtensionInput = fileRow.querySelector('input[name^="NoviDokumenti"][name$="EkstenzijaDokumenta"]');
        var vrPrijenosInput = fileRow.querySelector('input[name^="NoviDokumenti"][name$="VrPrijenos"]');
        var datumZadIzmjInput = fileRow.querySelector('input[name^="NoviDokumenti"][name$="DatumZadIzmj"]');

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
