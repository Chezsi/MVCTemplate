$(function () {
    // Initially hide buttons
    $("#ShowGraphBtn").hide();
    $("#RemoveGraphBtn").hide();
    $("#ExportChartBtn").hide();

    let chartInstance = null;

    // Toggle ShowGraphBtn visibility based on chart type selection
    $("#ChartType").on("change", function () {
        const selectedType = $(this).val();
        if (!selectedType || selectedType === "--Select Chart Type--") {
            $("#ShowGraphBtn").hide();
        } else {
            $("#ShowGraphBtn").show();
        }
    });

    $("#ShowGraphBtn").click(function () {
        const chartType = $("#ChartType").val();
        if (chartType === "--Select Chart Type--") {
            alert("Please select a chart type.");
            return;
        }

        $("#ChartView").show().html('<canvas id="myChart" style="max-width:750px; max-height:400px;"></canvas>');

        $.ajax({
            type: "POST",
            url: "/Admin/Package/GetPackagesCreatedPerDay",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                const labels = data[0];
                const counts = data[1];

                if (!labels.length) {
                    alert("No package data available.");
                    $("#ChartView").hide();
                    $("#RemoveGraphBtn").hide();
                    $("#ExportChartBtn").hide();
                    return;
                }

                const barColors = [
                    "#007bff", "#28a745", "#dc3545", "#ffc107",
                    "#17a2b8", "#6c757d", "#fd7e14", "#20c997"
                ];

                const ctx = document.getElementById("myChart").getContext("2d");

                // Destroy existing chart
                if (chartInstance) {
                    chartInstance.destroy();
                }

                // Create new chart
                chartInstance = new Chart(ctx, {
                    type: chartType,
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Packages Created',
                            backgroundColor: chartType === 'bar' ? barColors.slice(0, labels.length) : "#007bff",
                            borderColor: "#0056b3",
                            fill: chartType === 'line' ? false : true,
                            data: counts
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            title: {
                                display: true,
                                text: 'Packages Created Per Day',
                                font: { size: 18 },
                                padding: { top: 10, bottom: 10 }
                            },
                            legend: {
                                display: chartType === 'pie' || chartType === 'doughnut'
                            },
                            datalabels: {
                                color: '#000',
                                anchor: chartType === 'bar' || chartType === 'line' ? 'end' : 'center',
                                align: chartType === 'bar' || chartType === 'line' ? 'top' : 'center',
                                font: { weight: 'bold' },
                                formatter: function (value) {
                                    return value;
                                }
                            }
                        },
                        scales: (chartType === 'bar' || chartType === 'line') ? {
                            x: {
                                type: 'category',
                                title: { display: true, text: 'Date' }
                            },
                            y: {
                                beginAtZero: true,
                                title: { display: true, text: 'Number of Packages' }
                            }
                        } : {}
                    },
                    plugins: [ChartDataLabels]
                });

                $("#RemoveGraphBtn").show();
                $("#ExportChartBtn").show();
            },
            error: function (xhr, status, error) {
                console.error("Error fetching chart data:", error);
                alert("An error occurred while loading chart data.");
                $("#RemoveGraphBtn").hide();
                $("#ExportChartBtn").hide();
            }
        });
    });

    $("#RemoveGraphBtn").click(function () {
        if (chartInstance) {
            chartInstance.destroy();
            chartInstance = null;
        }
        $("#ChartView").hide().html('');
        $("#RemoveGraphBtn").hide();
        $("#ExportChartBtn").hide();
    });

    // Export chart as PNG image
    $("#ExportChartBtn").click(function () {
        const canvas = document.getElementById("myChart");
        if (canvas) {
            const image = canvas.toDataURL("image/png");
            const link = document.createElement('a');
            link.href = image;
            link.download = 'packages-created-chart.png';
            link.click();
        }
    });
});
