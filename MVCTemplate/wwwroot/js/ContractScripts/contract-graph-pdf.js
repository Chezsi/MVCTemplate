async function exportAllContractChartsToPDF() {
    const chartTypes = [
        { category: "annual", type: "bar", url: "/Admin/Contract/GetContractsPerYear", title: "Contracts Per Year", xTitle: "Year", yTitle: "Contracts" },
        { category: "annual", type: "line", url: "/Admin/Contract/GetContractsPerYear", title: "Contracts Per Year", xTitle: "Year", yTitle: "Contracts" },
        { category: "annual", type: "pie", url: "/Admin/Contract/GetContractsPerYear", title: "Annual Contracts Distribution" },
        { category: "annual", type: "doughnut", url: "/Admin/Contract/GetContractsPerYear", title: "Annual Contracts Distribution" },
        { category: "monthly", type: "bar", url: "/Admin/Contract/GetContractsPerMonth", title: "Monthly Contracts", xTitle: "Month", yTitle: "Contracts" },
        { category: "monthly", type: "line", url: "/Admin/Contract/GetContractsPerMonth", title: "Monthly Contracts", xTitle: "Month", yTitle: "Contracts" },
        { category: "monthly", type: "pie", url: "/Admin/Contract/GetContractsPerMonth", title: "Contracts Per Month" },
        { category: "monthly", type: "doughnut", url: "/Admin/Contract/GetContractsPerMonth", title: "Contracts Per Month" }
    ];

    const { jsPDF } = window.jspdf || window.jspdf?.jsPDF || window;
    const pdf = new jsPDF({ unit: "pt", format: "a4" });

    const pageWidth = pdf.internal.pageSize.getWidth();
    const pageHeight = pdf.internal.pageSize.getHeight();
    const margin = 20;

    function getFormattedTimestamp() {
        const now = new Date();
        const months = [
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        ];
        const month = months[now.getMonth()];
        const day = String(now.getDate()).padStart(2, "0");
        const year = now.getFullYear();

        let hour = now.getHours();
        const minute = String(now.getMinutes()).padStart(2, "0");
        const ampm = hour >= 12 ? "PM" : "AM";
        hour = hour % 12 || 12;

        return `${month}-${day}-${year} ${hour}:${minute} ${ampm}`;
    }

    function fitImage(imgW, imgH, maxW, maxH) {
        const ratio = Math.min(maxW / imgW, maxH / imgH);
        return { w: imgW * ratio, h: imgH * ratio };
    }

    async function createChart(labels, data, typeInfo) {
        return new Promise((resolve) => {
            const canvas = document.createElement("canvas");
            canvas.width = (typeInfo.type === "pie" || typeInfo.type === "doughnut") ? 400 : 750;
            canvas.height = 400;

            const ctx = canvas.getContext("2d");

            const isBarOrLine = typeInfo.type === "bar" || typeInfo.type === "line";
            const chartColors = [
                "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8",
                "#6c757d", "#fd7e14", "#20c997", "#6f42c1", "#e83e8c",
                "#6610f2", "#343a40"
            ];

            const chart = new Chart(ctx, {
                type: typeInfo.type,
                data: {
                    labels,
                    datasets: [{
                        label: "Contracts",
                        data,
                        backgroundColor: chartColors.slice(0, labels.length),
                        borderColor: "#0056b3",
                        fill: typeInfo.type !== "line",
                        tension: typeInfo.type === "line" ? 0.1 : undefined
                    }]
                },
                options: {
                    responsive: false,
                    animation: false,
                    plugins: {
                        title: {
                            display: true,
                            text: typeInfo.title,
                            font: { size: 18 }
                        },
                        legend: {
                            display: typeInfo.type === "pie" || typeInfo.type === "doughnut"
                        },
                        datalabels: {
                            color: '#000',
                            anchor: isBarOrLine ? 'end' : 'center',
                            align: isBarOrLine ? 'top' : 'center',
                            font: { weight: 'bold' },
                            formatter: (v) => v
                        }
                    },
                    scales: isBarOrLine ? {
                        x: { title: { display: true, text: typeInfo.xTitle || "" } },
                        y: {
                            beginAtZero: true,
                            title: { display: true, text: typeInfo.yTitle || "" },
                            ticks: { stepSize: 1 }
                        }
                    } : {},
                    cutout: typeInfo.type === "doughnut" ? "50%" : undefined
                },
                plugins: [ChartDataLabels]
            });

            const imgData = canvas.toDataURL("image/png");
            resolve({ imgData, canvas, width: canvas.width, height: canvas.height });
        });
    }

    for (let i = 0; i < chartTypes.length; i++) {
        const typeInfo = chartTypes[i];

        try {
            const response = await $.ajax({
                type: "POST",
                url: typeInfo.url,
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            });

            const labels = response[0];
            const values = response[1];

            if (!labels.length) continue;

            const { imgData, canvas, width, height } = await createChart(labels, values, typeInfo);

            if (i > 0) pdf.addPage();

            const maxWidth = pageWidth - 40;
            const maxHeight = pageHeight - 80;
            const fitted = fitImage(width, height, maxWidth, maxHeight);
            const x = (pageWidth - fitted.w) / 2;
            const y = 40;

            pdf.addImage(imgData, "PNG", x, y, fitted.w, fitted.h);

            // Add timestamp at bottom-right
            const timestamp = getFormattedTimestamp();
            pdf.setFontSize(10);
            pdf.setFont("helvetica", "normal");
            pdf.text(timestamp, pageWidth - margin, pageHeight - 10, { align: "right" });

            Chart.getChart(canvas)?.destroy();
            canvas.remove();

        } catch (error) {
            console.error(`Error generating chart for ${typeInfo.category}-${typeInfo.type}:`, error);
        }
    }

    pdf.save("contracts-all-charts.pdf");
}

$(function () {
    $("#ExportAllContractChartsBtn").click(exportAllContractChartsToPDF);
});
