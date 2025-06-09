$(function () {
    // Initially hide both buttons
    $("#ShowGraphBtn").hide();
    $("#RemoveGraphBtn").hide();

    // Show "ShowGraphBtn" only when a valid chart type is selected
    $("#ChartType").on("change", function () {
        const selectedType = $(this).val();
        if (!selectedType || selectedType === "--Select Chart Type--") {
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
                    return;
                }

                const barColors = [
                    "#007bff", "#28a745", "#dc3545", "#ffc107",
                    "#17a2b8", "#6c757d", "#fd7e14", "#20c997"
                ];

                new Chart(document.getElementById("myChart"), {
                    type: chartType,
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Packages Created Per Day',
                            backgroundColor: chartType === 'bar' ? barColors : "#007bff",
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
                                text: 'Packages Created Per Day'
                            },
                            legend: {
                                display: false
                            }
                        },
                        scales: {
                            x: {
                                type: 'category',
                                title: {
                                    display: true,
                                    text: 'Date'
                                }
                            },
                            y: {
                                beginAtZero: true,
                                title: {
                                    display: true,
                                    text: 'Number of Packages'
                                }
                            }
                        }
                    }
                });

                $("#RemoveGraphBtn").show();
            },
            error: function (xhr, status, error) {
                console.error("Error fetching chart data:", error);
                alert("An error occurred while loading chart data.");
                $("#RemoveGraphBtn").hide();
            }
        });
    });

    $("#RemoveGraphBtn").click(function () {
        $("#ChartView").hide().html('');
        $(this).hide();
    });
});