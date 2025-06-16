$(function () {
    // Initially hide buttons
    $("#RemoveGraphBtn").hide();
    $("#ShowGraphBtn").hide();
    $("#ExportChartBtn").hide();

    let chartInstance = null; // global Chart.js instance

    // Show/Hide ShowGraphBtn on chart type selection
    $("#ChartType").on("change", function () {
        const chartType = $(this).val();
        if (!chartType || chartType === "--Select Chart Type--") {
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
            url: "/Admin/Person/GetPersonsData",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                const _chartLabels = data[0];
                const _chartData = data[1];

                if (_chartLabels.length === 0) {
                    alert("No categories with employees found.");
                    $("#ChartView").hide();
                    $("#RemoveGraphBtn").hide();
                    $("#ExportChartBtn").hide();
                    return;
                }

                const barColors = [
                    "#007bff", "#28a745", "#dc3545", "#ffc107",
                    "#17a2b8", "#6c757d", "#fd7e14", "#20c997"
                ];

                // Destroy previous chart if any
                if (chartInstance) {
                    chartInstance.destroy();
                }

                const ctx = document.getElementById("myChart").getContext("2d");
                chartInstance = new Chart(ctx, {
                    type: chartType,
                    data: {
                        labels: _chartLabels,
                        datasets: [{
                            label: 'Number of Employees',
                            backgroundColor: barColors.slice(0, _chartLabels.length),
                            data: _chartData
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            title: {
                                display: true,
                                text: `Employees per Category`,
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
                                formatter: function (value) { return value; }
                            }
                        },
                        scales: (chartType === 'bar' || chartType === 'line') ? {
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
                        } : {}
                    },
                    plugins: [ChartDataLabels]
                });

                $("#RemoveGraphBtn").show();
                $("#ExportChartBtn").show();
            },
            error: function (xhr, status, error) {
                console.error("Error fetching chart data:", error);
                alert("Failed to load chart data.");
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
        $(this).hide();
        $("#ExportChartBtn").hide();
    });

    $("#ExportChartBtn").click(function () {
        const canvas = document.getElementById("myChart");
        const chartType = $("#ChartType").val(); // Get selected chart type
        if (canvas && chartType) {
            const image = canvas.toDataURL("image/png");
            const link = document.createElement('a');
            link.href = image;
            link.download = `employees-per-category-chart-${chartType}.png`; // Append chart type
            link.click();
        }
    });
});
