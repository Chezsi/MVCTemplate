$(document).ready(() => {
    $("#ExportAllChartsBtn").click(async function () {
        const chartTypes = ["bar", "line", "pie", "doughnut"];
        const { jsPDF } = window.jspdf;

        try {
            // Fetch data
            const response = await $.ajax({
                type: "POST",
                url: "/Admin/Product/GetProductsData",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
            });

            const _chartLabels = response[0];
            const _chartData = response[1];

            if (!_chartLabels.length) {
                alert("No product data available.");
                return;
            }

            const barColors = [
                "#007bff",
                "#28a745",
                "#dc3545",
                "#ffc107",
                "#17a2b8",
                "#6c757d",
                "#fd7e14",
                "#20c997",
            ];

            // Create PDF with pixel units and A4 size scaled to pixels (approx)
            const pdfWidthPx = 595; // A4 width in points (1pt=1.33px approx)
            const pdfHeightPx = 842; // A4 height in points

            const pdf = new jsPDF({
                unit: "pt", // 'pt' is default, 1pt = 1/72 inch
                format: "a4",
            });

            // Helper: fit image within max box keeping aspect ratio
            function fitImage(imgW, imgH, maxW, maxH) {
                const ratio = Math.min(maxW / imgW, maxH / imgH);
                return { w: imgW * ratio, h: imgH * ratio };
            }

            // Helper: create chart, wait for animation complete, return image data
            function createChart(type) {
                return new Promise((resolve) => {
                    const canvas = document.createElement("canvas");
                    if (type === "pie" || type === "doughnut") {
                        canvas.width = 400;
                        canvas.height = 400;
                    } else {
                        canvas.width = 750;
                        canvas.height = 400;
                    }
                    const ctx = canvas.getContext("2d");

                    const options = {
                        responsive: false,
                        maintainAspectRatio: false,
                        animation: {
                            onComplete: () => {
                                // Ensure update done and then resolve
                                setTimeout(() => {
                                    const imgData = canvas.toDataURL("image/png");
                                    resolve({ imgData, canvas, width: canvas.width, height: canvas.height });
                                }, 100);
                            },
                        },
                        plugins: {
                            title: {
                                display: true,
                                text: `Product Quantities (${type.toUpperCase()})`,
                                font: { size: 18 },
                            },
                            legend: {
                                display: type === "pie" || type === "doughnut",
                            },
                            datalabels: {
                                color: "#000",
                                anchor: type === "bar" || type === "line" ? "end" : "center",
                                align: type === "bar" || type === "line" ? "top" : "center",
                                font: { weight: "bold" },
                                formatter: (value) => value,
                            },
                        },
                        scales:
                            type === "bar" || type === "line"
                                ? {
                                    x: { title: { display: true, text: "Product Name" } },
                                    y: { beginAtZero: true, title: { display: true, text: "Quantity" } },
                                }
                                : {},
                    };

                    if (type === "doughnut") {
                        options.cutout = "50%";
                    }

                    const chart = new Chart(ctx, {
                        type,
                        data: {
                            labels: _chartLabels,
                            datasets: [
                                {
                                    label: "Product Quantities",
                                    backgroundColor: barColors.slice(0, _chartLabels.length),
                                    data: _chartData,
                                },
                            ],
                        },
                        options,
                        plugins: [ChartDataLabels],
                    });
                });
            }

            for (let i = 0; i < chartTypes.length; i++) {
                const type = chartTypes[i];
                const { imgData, canvas, width, height } = await createChart(type);

                if (i > 0) pdf.addPage();

                // Fit chart image to a box on the page
                const maxWidth = pdf.internal.pageSize.getWidth() - 40; // margin 20 left/right
                const maxHeight = pdf.internal.pageSize.getHeight() - 80; // margin 40 top/bottom
                const fitted = fitImage(width, height, maxWidth, maxHeight);

                const x = (pdf.internal.pageSize.getWidth() - fitted.w) / 2;
                const y = 40;

                pdf.addImage(imgData, "PNG", x, y, fitted.w, fitted.h);

                // Cleanup chart instance
                Chart.getChart(canvas)?.destroy();
                canvas.remove();
            }

            pdf.save("product-charts-all-types.pdf");
        } catch (error) {
            console.error("Failed to generate charts:", error);
            alert("Something went wrong while exporting charts.");
        }
    });
});
