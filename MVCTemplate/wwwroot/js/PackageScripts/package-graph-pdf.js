$(function () {
    $("#ExportAllChartsBtn-package").click(async function () {
        const btn = $(this);
        if (btn.prop('disabled')) return;

        // Disable button and show spinner
        btn.prop('disabled', true);
        const originalHtml = btn.html();
        btn.html('<i class="fa-solid fa-spinner fa-spin" style="margin-right: 6px;"></i> Exporting...');

        try {
            // Step 1: Get download token
            const tokenResponse = await $.ajax({
                type: "POST",
                url: "/Admin/Package/GenerateDownloadToken",
                dataType: "json"
            });

            if (!tokenResponse.token) {
                alert("Failed to get download token.");
                throw new Error("No token returned");
            }

            // Step 2: Proceed with original chart generation/export logic

            const chartTypes = ["bar", "line"];
            const { jsPDF } = window.jspdf;

            const response = await $.ajax({
                type: "POST",
                url: "/Admin/Package/GetPackagesCreatedPerDay",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
            });

            const labels = response[0];
            const counts = response[1];

            if (!labels.length) {
                alert("No package data available.");
                return;
            }

            const barColors = [
                "#007bff", "#28a745", "#dc3545", "#ffc107",
                "#17a2b8", "#6c757d", "#fd7e14", "#20c997"
            ];

            const pdf = new jsPDF({
                unit: "pt",
                format: "a4",
            });

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
                    canvas.width = 750;
                    canvas.height = 400;
                    const ctx = canvas.getContext("2d");

                    const chart = new Chart(ctx, {
                        type,
                        data: {
                            labels: labels,
                            datasets: [{
                                label: "Packages Created",
                                backgroundColor: type === "bar" ? barColors.slice(0, labels.length) : "#007bff",
                                borderColor: "#0056b3",
                                fill: type === "line" ? false : true,
                                data: counts,
                            }],
                        },
                        options: {
                            responsive: false,
                            maintainAspectRatio: false,
                            animation: false,
                            plugins: {
                                title: {
                                    display: true,
                                    text: `Packages Created Per Day (${type.toUpperCase()})`,
                                    font: { size: 18 },
                                },
                                legend: {
                                    display: false,
                                },
                                datalabels: {
                                    color: "#000",
                                    anchor: type === "bar" || type === "line" ? "end" : "center",
                                    align: type === "bar" || type === "line" ? "top" : "center",
                                    font: { weight: "bold" },
                                    formatter: (value) => value,
                                },
                            },
                            scales: {
                                x: { title: { display: true, text: "Date" } },
                                y: {
                                    beginAtZero: true,
                                    title: { display: true, text: "Number of Packages" },
                                },
                            },
                        },
                        plugins: [ChartDataLabels],
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

                // Add timestamp at bottom-right
                const timestamp = getFormattedTimestamp();
                pdf.setFontSize(10);
                pdf.text(timestamp, pageWidth - margin, pageHeight - 10, { align: "right" });

                Chart.getChart(canvas)?.destroy();
                canvas.remove();
            }

            pdf.save("packages-graphs.pdf");

        } catch (error) {
            console.error("Failed to generate charts:", error);
            alert("Something went wrong while exporting charts.");
        } finally {
            // Re-enable button & restore original content after a slight delay
            setTimeout(() => {
                btn.prop('disabled', false);
                btn.html(originalHtml);
            }, 800);
        }
    });
});
// token validation may have been unnecessary since it cannot be accessed manually by typing the url, this is done for consistency