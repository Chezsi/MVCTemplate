document.querySelector("#button-excel-person").addEventListener("click", async function () { //prior to renaming it instead gets the category one
    var table = $('#Persons').DataTable(); // ??change to account for the name in ApplicationDbContext
    var searchValue = table.search();
    var dataToExport;

    if (searchValue) {
        dataToExport = table.rows({ search: 'applied' }).data().toArray();
    } else {
        let response = await fetch('/Admin/Person/GetAllPersons'); //change to account for url naming
        let result = await response.json();
        dataToExport = result.data;
    }

    dataToExport.sort((a, b) => a.name.localeCompare(b.name)); // change to account for name of data (Model)

    let tempTable = document.createElement('table');
    let thead = document.createElement('thead');
    thead.innerHTML = '<tr><th>ID</th> <th>Name</th> <th>Position</th> <th>CategoryId</th></tr>';
    tempTable.appendChild(thead);

    let tbody = document.createElement('tbody');
    dataToExport.forEach(row => {
        let tr = document.createElement('tr');
        tr.innerHTML = `<td>${row.id}</td> <td>${row.name}</td> <td>${row.position}</td> <td>${row.categoryId}</td>`;
        tbody.appendChild(tr); // ^ change to account for name of data (Model) and number of column 
    });
    tempTable.appendChild(tbody);

    TableToExcel.convert(tempTable, { name: "Person.xlsx" });
});

document.querySelector("#button-export-current-contracts").addEventListener("click", async function () {
    try {
        const response = await fetch('/Admin/Person/ExportPersonsWithCurrentContracts');
        const data = await response.json();

        if (!data || data.length === 0) {
            alert('No current contracts found.');
            return;
        }

        function formatDateWordMDY(dateValue) {
            if (!dateValue) return "";
            const d = new Date(dateValue);
            if (isNaN(d)) return dateValue;
            return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
        }

        function formatDateTimeWordMDY(dateValue) {
            if (!dateValue) return "";
            const d = new Date(dateValue);
            if (isNaN(d)) return dateValue;
            return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) + ", " +
                d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
        }

        const now = new Date();
        const generatedAt = formatDateTimeWordMDY(now);

        const ws_data = [
            ["Current Contracts Export"],
            [`Generated at: ${generatedAt}`],
            ["Person ID", "Name", "Position", "Category ID", "Contract ID", "Contract Name", "Description", "Validity"],
            ...data.map(row => [
                row.person_Id,
                row.person_Name,
                row.person_Position,
                row.person_CategoryId,
                row.contract_Id,
                row.contract_Name,
                row.contract_Description,
                formatDateWordMDY(row.contract_Validity)
            ])
        ];

        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.aoa_to_sheet(ws_data);

        // Merge title and generated rows
        ws['!merges'] = [
            { s: { r: 0, c: 0 }, e: { r: 0, c: 7 } },  // A1:H1
            { s: { r: 1, c: 0 }, e: { r: 1, c: 7 } }   // A2:H2
        ];

        // Autofilter
        ws['!autofilter'] = { ref: `A3:H${ws_data.length}` };

        // Column widths
        ws['!cols'] = [
            { wch: 12 }, { wch: 20 }, { wch: 18 }, { wch: 15 },
            { wch: 12 }, { wch: 20 }, { wch: 30 }, { wch: 22 }
        ];

        // Export
        XLSX.utils.book_append_sheet(wb, ws, "CurrentContracts");
        XLSX.writeFile(wb, "CurrentContracts.xlsx");

    } catch (err) {
        console.error("Export error:", err);
        alert("Failed to export current contracts.");
    }
});

