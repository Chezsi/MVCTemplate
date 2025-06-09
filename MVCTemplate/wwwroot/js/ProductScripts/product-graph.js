$(function () {
    // Initially hide buttons
    $("#RemoveGraphBtn").hide();
    $("#ShowGraphBtn").hide();

    // Toggle ShowGraphBtn visibility based on chart type selection
    $("#ChartType").on("change", function () {
        const chartType = $(this).val();
        if (!chartType || chartType === "--Select Chart Type--") {
            $("#ShowGraphBtn").hide();
        } else {
            $("#ShowGraphBtn").show();
        }
    });

    $("#ShowGraphBtn").click(function () {
        var chartType = $("#ChartType").val();
        if (chartType === "--Select Chart Type--") {
            alert("Please select a chart type.");
            return;
        }

        $("#ChartView").show().html('<canvas id="myChart" style="max-width:750px; max-height:400px;"></canvas>');

        $.ajax({
            type: "POST",
            url: "/Admin/Product/GetProductsData",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                var _chartLabels = data[0];
                var _chartData = data[1];

                if (!_chartLabels.length) {
                    alert("No product data available.");
                    $("#ChartView").hide();
                    $("#RemoveGraphBtn").hide();
                    return;
                }

                var barColors = [
                    "#007bff", "#28a745", "#dc3545", "#ffc107",
                    "#17a2b8", "#6c757d", "#fd7e14", "#20c997"
                ];

                new Chart(document.getElementById("myChart"), {
                    type: chartType,
                    data: {
                        labels: _chartLabels,
                        datasets: [{
                            label: 'Product Quantities',
                            backgroundColor: barColors.slice(0, _chartLabels.length),
                            data: _chartData
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            title: {
                                display: true,
                                text: 'Product Quantities by Name'
                            },
                            legend: {
                                display: chartType === 'pie' || chartType === 'doughnut'
                            }
                        },
                        scales: (chartType === 'bar' || chartType === 'line') ? {
                            x: {
                                title: {
                                    display: true,
                                    text: 'Product Name'
                                }
                            },
                            y: {
                                beginAtZero: true,
                                title: {
                                    display: true,
                                    text: 'Quantity'
                                }
                            }
                        } : {}
                    }
                });

                $("#RemoveGraphBtn").show();
            },
            error: function (xhr, status, error) {
                console.error("Error fetching chart data:", error);
                alert("Failed to load chart data.");
                $("#RemoveGraphBtn").hide();
            }
        });
    });

    $("#RemoveGraphBtn").click(function () {
        $("#ChartView").hide().html('');
        $(this).hide();
    });
});