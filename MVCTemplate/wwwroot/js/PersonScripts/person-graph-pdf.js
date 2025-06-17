$(function () {
    $("#ExportAllChartsBtn-person").click(async function () {
        const chartTypes = ["bar", "line", "pie", "doughnut"];
        const { jsPDF } = window.jspdf;

        try {
            const response = await $.ajax({
                type: "POST",
                url: "/Admin/Person/GetPersonsData",
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            });

            const labels = response[0];
            const data = response[1];

            if (!labels.length) {
                alert("No categories with employees found.");
                return;
            }

            const barColors = [
                "#007bff", "#28a745", "#dc3545", "#ffc107",
                "#17a2b8", "#6c757d", "#fd7e14", "#20c997"
            ];

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

                    const chart = new Chart(ctx, {
                        type,
                        data: {
                            labels,
                            datasets: [{
                                label: "Number of Employees",
                                data,
                                backgroundColor: type === "bar" || type === "pie" || type === "doughnut"
                                    ? barColors.slice(0, labels.length)
                                    : "#007bff",
                                borderColor: "#0056b3",
                                fill: type === "line" ? false : true,
                            }]
                        },
                        options: {
                            responsive: false,
                            maintainAspectRatio: false,
                            animation: false,
                            plugins: {
                                title: {
                                    display: true,
                                    text: `Employees per Category (${type.toUpperCase()})`,
                                    font: { size: 18 },
                                    padding: { top: 10, bottom: 10 }
                                },
                                legend: {
                                    display: type === "pie" || type === "doughnut"
                                },
                                datalabels: {
                                    color: '#000',
                                    anchor: type === 'bar' || type === 'line' ? 'end' : 'center',
                                    align: type === 'bar' || type === 'line' ? 'top' : 'center',
                                    font: { weight: 'bold' },
                                    formatter: (value) => value
                                }
                            },
                            scales: (type === 'bar' || type === 'line') ? {
                                x: {
                                    title: { display: true, text: 'Category' }
                                },
                                y: {
                                    beginAtZero: true,
                                    title: { display: true, text: 'Employees' },
                                    ticks: {
                                        callback: v => Number.isInteger(v) ? v : null,
                                        stepSize: 1
                                    }
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

            for (let i = 0; i < chartTypes.length; i++) {
                const type = chartTypes[i];
                const { imgData, canvas, width, height } = await createChart(type);

                if (i > 0) pdf.addPage();

                const maxWidth = pageWidth - 40;
                const maxHeight = pageHeight - 80;
                const fitted = fitImage(width, height, maxWidth, maxHeight);
                const x = (pageWidth - fitted.w) / 2;
                const y = 40;

                pdf.addImage(imgData, "PNG", x, y, fitted.w, fitted.h);

                // Add timestamp to bottom-right of page
                const timestamp = getFormattedTimestamp();
                pdf.setFontSize(10);
                pdf.text(timestamp, pageWidth - margin, pageHeight - 10, { align: "right" });

                Chart.getChart(canvas)?.destroy();
                canvas.remove();
            }

            pdf.save("employees-graphs.pdf");
        } catch (error) {
            console.error("Error exporting charts:", error);
            alert("An error occurred while exporting charts.");
        }
    });
});
