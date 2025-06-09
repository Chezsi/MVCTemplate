$(function () {
    // Initially hide both buttons
    $("#RemoveGraphBtn").hide();
    $("#ShowGraphBtn").hide();

    // Show/Hide ShowGraphBtn based on chart type selection
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
            url: "/Admin/Person/GetPersonsData",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                var _chartLabels = data[0];
                var _chartData = data[1];

                if (_chartLabels.length === 0) {
                    alert("No categories with employees found.");
                    $("#ChartView").hide();
                    $("#RemoveGraphBtn").hide();
                    return;
                }

                var colors = [
                    "#007bff", "#28a745", "#dc3545", "#ffc107",
                    "#17a2b8", "#6c757d", "#fd7e14", "#20c997"
                ];

                new Chart(document.getElementById("myChart"), {
                    type: chartType,
                    data: {
                        labels: _chartLabels,
                        datasets: [{
                            label: 'Number of Employees',
                            backgroundColor: colors.slice(0, _chartLabels.length),
                            data: _chartData
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            title: {
                                display: true,
                                text: 'Employees per Category'
                            },
                            legend: {
                                display: chartType === 'pie' || chartType === 'doughnut'
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
                                    callback: function (value) {
                                        if (Number.isInteger(value)) {
                                            return value;
                                        }
                                    },
                                    stepSize: 1
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