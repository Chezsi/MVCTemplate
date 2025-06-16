$(function () {
    $("#ExportAllChartsBtn").click(async function () {
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF({ unit: "pt", format: "a4" });
        const pageWidth = pdf.internal.pageSize.getWidth();
        const pageHeight = pdf.internal.pageSize.getHeight();
        const margin = 20;
        const contentWidth = pageWidth - margin * 2;
        const contentHeight = pageHeight - margin * 2;

        const chartColors = [
            "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8",
            "#6c757d", "#fd7e14", "#20c997", "#6f42c1", "#e83e8c",
            "#6610f2", "#343a40"
        ];

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

        async function fetchData(url) {
            const response = await $.ajax({
                type: "POST",
                url,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
            });
            return { labels: response[0], data: response[1] };
        }

        async function createChart(labels, data, type, title, xTitle, yTitle) {
            return new Promise((resolve) => {
                const canvas = document.createElement("canvas");
                canvas.width = (type === "pie" || type === "doughnut") ? 350 : 400;
                canvas.height = 300;
                const ctx = canvas.getContext("2d");

                const isBarOrLine = type === "bar" || type === "line";

                const chart = new Chart(ctx, {
                    type,
                    data: {
                        labels,
                        datasets: [{
                            label: title,
                            data,
                            backgroundColor: chartColors.slice(0, labels.length),
                            borderColor: "#0056b3",
                            fill: type !== "line",
                            tension: type === "line" ? 0.3 : undefined
                        }]
                    },
                    options: {
                        responsive: false,
                        animation: false,
                        plugins: {
                            title: {
                                display: true,
                                text: title,
                                font: { size: 14 },
                                padding: { top: 10, bottom: 10 }
                            },
                            legend: { display: type === "pie" || type === "doughnut" },
                            datalabels: {
                                color: "#000",
                                anchor: isBarOrLine ? "end" : "center",
                                align: isBarOrLine ? "top" : "center",
                                font: { weight: "bold" },
                                formatter: (v) => v
                            }
                        },
                        scales: isBarOrLine ? {
                            x: { title: { display: !!xTitle, text: xTitle || "" } },
                            y: {
                                beginAtZero: true,
                                title: { display: !!yTitle, text: yTitle || "" },
                                ticks: { stepSize: 1 }
                            }
                        } : {},
                        cutout: type === "doughnut" ? "50%" : undefined
                    },
                    plugins: [ChartDataLabels]
                });

                const imgData = canvas.toDataURL("image/png");
                resolve({ imgData, canvas, width: canvas.width, height: canvas.height });
            });
        }

        async function addCategoryPage(chartsInfo, pageIndex, title) {
            if (pageIndex > 0) pdf.addPage();

            // Add title to top center
            pdf.setFontSize(16);
            pdf.setFont("helvetica", "bold");
            pdf.text(title, pageWidth / 2, 40, { align: "center" });

            const charts = [];
            for (const info of chartsInfo) {
                try {
                    const { labels, data } = await fetchData(info.url);
                    if (!labels.length) continue;
                    const chart = await createChart(labels, data, info.type, info.title, info.xTitle, info.yTitle);
                    charts.push(chart);
                } catch (e) {
                    console.error("Chart error:", info.title, e);
                }
            }

            const cols = 2;
            const rows = Math.ceil(charts.length / cols);
            const cellWidth = contentWidth / cols;
            const chartAreaHeight = contentHeight - 40; // reserve space for title
            const cellHeight = chartAreaHeight / rows;

            for (let i = 0; i < charts.length; i++) {
                const chart = charts[i];
                const row = Math.floor(i / cols);
                const col = i % cols;

                const fitted = fitImage(chart.width, chart.height, cellWidth, cellHeight);
                const x = margin + col * cellWidth + (cellWidth - fitted.w) / 2;
                const y = margin + 50 + row * cellHeight + (cellHeight - fitted.h) / 2;

                pdf.addImage(chart.imgData, "PNG", x, y, fitted.w, fitted.h);
                Chart.getChart(chart.canvas)?.destroy();
                chart.canvas.remove();
            }

            // Add timestamp at bottom right
            const timestamp = getFormattedTimestamp();
            pdf.setFontSize(10);
            pdf.setFont("helvetica", "normal");
            pdf.text(timestamp, pageWidth - margin, pageHeight - 10, { align: "right" });
        }

        // Category configurations
        const categories = [
            {
                title: "Product Data",
                charts: [
                    { type: "bar", url: "/Admin/Product/GetProductsData", title: "Product Quantities", xTitle: "Product", yTitle: "Quantity" },
                    { type: "line", url: "/Admin/Product/GetProductsData", title: "Product Quantities", xTitle: "Product", yTitle: "Quantity" },
                    { type: "pie", url: "/Admin/Product/GetProductsData", title: "Product Distribution" },
                    { type: "doughnut", url: "/Admin/Product/GetProductsData", title: "Product Distribution" }
                ]
            },
            {
                title: "Person Data",
                charts: [
                    { type: "bar", url: "/Admin/Person/GetPersonsData", title: "Employees per Category", xTitle: "Category", yTitle: "Employees" },
                    { type: "line", url: "/Admin/Person/GetPersonsData", title: "Employees per Category", xTitle: "Category", yTitle: "Employees" },
                    { type: "pie", url: "/Admin/Person/GetPersonsData", title: "Employee Distribution" },
                    { type: "doughnut", url: "/Admin/Person/GetPersonsData", title: "Employee Distribution" }
                ]
            },
            {
                title: "Package Data",
                charts: [
                    { type: "bar", url: "/Admin/Package/GetPackagesCreatedPerDay", title: "Packages Created Per Day", xTitle: "Date", yTitle: "Packages" },
                    { type: "line", url: "/Admin/Package/GetPackagesCreatedPerDay", title: "Packages Created Per Day", xTitle: "Date", yTitle: "Packages" }
                ]
            },
            {
                title: "Contract Data",
                charts: [
                    { type: "bar", url: "/Admin/Contract/GetContractsPerYear", title: "Contracts Per Year", xTitle: "Year", yTitle: "Contracts" },
                    { type: "line", url: "/Admin/Contract/GetContractsPerYear", title: "Contracts Per Year", xTitle: "Year", yTitle: "Contracts" },
                    { type: "pie", url: "/Admin/Contract/GetContractsPerYear", title: "Annual Contracts Distribution" },
                    { type: "doughnut", url: "/Admin/Contract/GetContractsPerYear", title: "Annual Contracts Distribution" }
                ]
            },
            {
                title: "Contract Data",
                charts: [
                    { type: "bar", url: "/Admin/Contract/GetContractsPerMonth", title: "Monthly Contracts", xTitle: "Month", yTitle: "Contracts" },
                    { type: "line", url: "/Admin/Contract/GetContractsPerMonth", title: "Monthly Contracts", xTitle: "Month", yTitle: "Contracts" },
                    { type: "pie", url: "/Admin/Contract/GetContractsPerMonth", title: "Contracts Per Month" },
                    { type: "doughnut", url: "/Admin/Contract/GetContractsPerMonth", title: "Contracts Per Month" }
                ]
            }
        ];

        // Generate all pages
        for (let i = 0; i < categories.length; i++) {
            await addCategoryPage(categories[i].charts, i, categories[i].title);
        }

        pdf.save("Graphs.pdf");
    });
});
