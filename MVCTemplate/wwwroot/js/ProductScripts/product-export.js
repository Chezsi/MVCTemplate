
/*document.querySelector("#button-excel").addEventListener("click", async function () {
    var table = $('#Products').DataTable(); // ??change to account for the name in ApplicationDbContext
    var searchValue = table.search();
    var dataToExport;

    if (searchValue) {
        dataToExport = table.rows({ search: 'applied' }).data().toArray();
    } else {
        let response = await fetch('/Admin/Product/GetAllProducts'); //change to account for url naming
        let result = await response.json();
        dataToExport = result.data;
    }

    dataToExport.sort((a, b) => a.name.localeCompare(b.name)); // ^ change to account for name of data (Model)

    let tempTable = document.createElement('table');
    let thead = document.createElement('thead');
    thead.innerHTML = '<tr><th>ID</th> <th>Name</th> <th>Description</th> <th>Quantity</th> </tr>'; 
    tempTable.appendChild(thead);

    let tbody = document.createElement('tbody');
    dataToExport.forEach(row => {
        let tr = document.createElement('tr');
        tr.innerHTML = `<td>${row.id}</td> <td>${row.name}</td> <td>${row.description}</td> <td>${row.quantity}</td>`;
        tbody.appendChild(tr); // ^ change to account for name of data (Model) and number of column 
    });
    tempTable.appendChild(tbody);

    TableToExcel.convert(tempTable, { name: "Product.xlsx" });
});*/

document.querySelector("#button-excel").addEventListener("click", async function () {
    var table = $('#Products').DataTable();
    var searchValue = table.search();
    var dataToExport;

    if (searchValue) {
        dataToExport = table.rows({ search: 'applied' }).data().toArray();
    } else {
        let response = await fetch('/Admin/Product/GetAllProducts');
        let result = await response.json();
        dataToExport = result.data;
    }

    dataToExport.sort((a, b) => a.name.localeCompare(b.name));

    // Format date for "Generated at"
    function formatDateTimeWordMDY(dateValue) {
        if (!dateValue) return "";
        const d = new Date(dateValue);
        if (isNaN(d)) return dateValue;
        return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) + ", " +
            d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
    }

    const now = new Date();
    const datetimeString = formatDateTimeWordMDY(now);

    // Worksheet data
    const ws_data = [
        ["Product Data"],
        [`Generated at: ${datetimeString}`],
        ["ID", "Name", "Description", "Quantity"],
        ...dataToExport.map(row => [
            row.id,
            row.name,
            row.description,
            row.quantity
        ])
    ];

    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.aoa_to_sheet(ws_data);

    const lastRow = ws_data.length;
    ws['!autofilter'] = { ref: `A3:D${lastRow}` };

    // Merge the top rows
    ws['!merges'] = [
        { s: { r: 0, c: 0 }, e: { r: 0, c: 3 } },  // A1:D1
        { s: { r: 1, c: 0 }, e: { r: 1, c: 3 } }   // A2:D2
    ];

    ws['A1'].s = {
        alignment: {
            horizontal: "center"
        },
        font: {
            bold: true,
            sz: 14
        }
    };

    ws['A2'].s = {
        alignment: {
            horizontal: "center"
        },
        font: {
            italic: true
        }
    };

    // Dynamically calculate column widths
    const padding = 2;
    const getMaxLength = (index) => {
        return ws_data.reduce((max, row, i) => {
            if (i < 3) return max;
            const val = row[index];
            return Math.max(max, val ? val.toString().length : 0);
        }, 0);
    };

    ws['!cols'] = [
        { wch: 10 },                         // ID
        { wch: getMaxLength(1) + padding },  // Name
        { wch: getMaxLength(2) + padding },  // Description
        { wch: 10 }                          // Quantity
    ];

    XLSX.utils.book_append_sheet(wb, ws, "Products");
    XLSX.writeFile(wb, "Product.xlsx");
});
