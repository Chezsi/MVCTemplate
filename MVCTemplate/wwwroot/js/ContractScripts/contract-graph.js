const chartOptions = {
    annual: [
        { value: "bar", text: "Bar" },
        { value: "line", text: "Line" },
        { value: "annual-pie", text: "Pie" },
        { value: "annual-doughnut", text: "Doughnut" }
    ],
    monthly: [
        { value: "pie", text: "Pie" },
        { value: "doughnut", text: "Doughnut" },
        { value: "monthly-bar", text: "Bar" },
        { value: "monthly-line", text: "Line" }
    ]
};

function updateShowButtonVisibility() {
    const category = $('#ChartCategory').val();
    const type = $('#ChartType').val();
    if (category && category !== "--Select Category--" &&
        type && type !== "--Select Type--") {
        $("#ShowGraphBtn").show();
    } else {
        $("#ShowGraphBtn").hide();
    }
}

function showRemoveButton() {
    $("#RemoveGraphBtn").show();
}

function hideRemoveButton() {
    $("#RemoveGraphBtn").hide();
}

function loadBarOrLineChart(chartType) {
    $("#ChartView").show().html('<canvas id="myChart" style="max-width:750px; max-height:400px;"></canvas>');
    $.ajax({
        type: "POST",
        url: "/Admin/Contract/GetContractsPerYear",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var labels = data[0];
            var values = data[1];
            const colors = [
                "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8",
                "#6c757d", "#fd7e14", "#20c997", "#6f42c1", "#e83e8c",
                "#6610f2", "#343a40"
            ];
            let datasets = (chartType === 'line') ? [{
                label: 'Contracts Per Year',
                data: values,
                borderColor: "#007bff",
                backgroundColor: "#007bff",
                fill: false,
                tension: 0.1
            }] : labels.map((label, index) => ({
                label: label,
                data: labels.map((_, i) => i === index ? values[index] : null),
                backgroundColor: colors[index % colors.length],
                borderColor: colors[index % colors.length],
                fill: true
            }));
            new Chart(document.getElementById("myChart"), {
                type: chartType,
                data: { labels, datasets },
                options: {
                    responsive: true,
                    interaction: { mode: 'index', intersect: false },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: { precision: 0 },
                            title: { display: true, text: 'Number of Contracts' }
                        },
                        x: { title: { display: true, text: 'Year' } }
                    },
                    plugins: {
                        title: { display: true, text: `Contracts from ${labels[0]} to ${labels[labels.length - 1]}` },
                        legend: {
                            display: true,
                            onClick: function (e, legendItem, legend) {
                                if (legend.chart.config.type === 'line') return;
                                const index = legendItem.datasetIndex;
                                const meta = legend.chart.getDatasetMeta(index);
                                meta.hidden = meta.hidden === null ? !legend.chart.data.datasets[index].hidden : null;
                                legend.chart.update();
                            }
                        }
                    }
                }
            });
            showRemoveButton();
        },
        error: function (xhr, status, error) {
            console.error("Error loading bar/line data:", error);
        }
    });
}

function loadPieOrDoughnutChart(chartType) {
    $("#ChartView").show().html('<canvas id="myChart" style="max-width:750px; max-height:400px;"></canvas>');
    $.ajax({
        type: "POST",
        url: "/Admin/Contract/GetContractsPerMonth",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var labels = data[0];
            var values = data[1];
            var pieColors = [
                "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8",
                "#6c757d", "#fd7e14", "#20c997", "#6f42c1", "#e83e8c",
                "#6610f2", "#343a40"
            ];
            new Chart(document.getElementById("myChart"), {
                type: chartType,
                data: {
                    labels,
                    datasets: [{
                        backgroundColor: pieColors,
                        data: values
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: { display: true, text: 'Contracts Per Person' },
                        legend: { display: true }
                    }
                }
            });
            showRemoveButton();
        },
        error: function (xhr, status, error) {
            console.error("Error loading pie/doughnut data:", error);
        }
    });
}

function loadMonthlyBarOrLineChart(chartType) {
    $("#ChartView").show().html('<canvas id="myChart" style="max-width:750px; max-height:400px;"></canvas>');
    $.ajax({
        type: "POST",
        url: "/Admin/Contract/GetContractsPerMonth",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var labels = data[0];
            var values = data[1];
            const colors = [
                "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8",
                "#6c757d", "#fd7e14", "#20c997", "#6f42c1", "#e83e8c",
                "#6610f2", "#343a40"
            ];
            let dataset = {
                label: 'Contracts Per Month',
                data: values,
                backgroundColor: chartType === 'monthly-bar' ? colors : "#007bff",
                borderColor: "#007bff",
                fill: chartType === 'monthly-line' ? false : true,
                tension: chartType === 'monthly-line' ? 0.1 : undefined
            };
            new Chart(document.getElementById("myChart"), {
                type: chartType === 'monthly-bar' ? 'bar' : 'line',
                data: { labels, datasets: [dataset] },
                options: {
                    responsive: true,
                    interaction: { mode: 'index', intersect: false },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: { precision: 0 },
                            title: { display: true, text: 'Number of Contracts' }
                        },
                        x: { title: { display: true, text: 'Month' } }
                    },
                    plugins: {
                        title: { display: true, text: 'Monthly Contracts Overview' },
                        legend: { display: true }
                    }
                }
            });
            showRemoveButton();
        },
        error: function (xhr, status, error) {
            console.error("Error loading monthly bar/line data:", error);
        }
    });
}

function loadAnnualPieOrDoughnutChart(chartType) {
    $("#ChartView").show().html('<canvas id="myChart" style="max-width:750px; max-height:400px;"></canvas>');
    $.ajax({
        type: "POST",
        url: "/Admin/Contract/GetContractsPerYear",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var labels = data[0];
            var values = data[1];
            var pieColors = [
                "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8",
                "#6c757d", "#fd7e14", "#20c997", "#6f42c1", "#e83e8c",
                "#6610f2", "#343a40"
            ];
            new Chart(document.getElementById("myChart"), {
                type: chartType === 'annual-pie' ? 'pie' : 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        backgroundColor: pieColors,
                        data: values
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: `Annual Contracts (${chartType === 'annual-pie' ? 'Pie' : 'Doughnut'})`
                        },
                        legend: { display: true }
                    }
                }
            });
            showRemoveButton();
        },
        error: function (xhr, status, error) {
            console.error("Error loading annual pie/doughnut data:", error);
        }
    });
}

$(function () {
    $("#ShowGraphBtn").hide(); // hide show button by default
    hideRemoveButton(); // hide remove button by default

    $('#ChartCategory').on('change', function () {
        const category = $(this).val();
        const $chartType = $('#ChartType');
        $chartType.empty().append('<option selected>--Select Type--</option>');

        if (chartOptions[category]) {
            $chartType.prop('disabled', false);
            chartOptions[category].forEach(opt => {
                $chartType.append(new Option(opt.text, opt.value));
            });
        } else {
            $chartType.prop('disabled', true);
        }

        updateShowButtonVisibility();
    });

    $('#ChartType').on('change', function () {
        updateShowButtonVisibility();
    });

    $("#ShowGraphBtn").click(function () {
        var chartType = $("#ChartType").val();
        var category = $("#ChartCategory").val();

        if (!chartType || chartType.startsWith('--')) {
            toastr.error("Please select a valid chart type.", "Invalid Selection");
            return;
        }

        if (category === "annual") {
            if (chartType === "bar" || chartType === "line") {
                loadBarOrLineChart(chartType);
            } else if (chartType === "annual-pie" || chartType === "annual-doughnut") {
                loadAnnualPieOrDoughnutChart(chartType);
            } else {
                toastr.error("Unsupported annual chart type.", "Error");
            }
        } else if (category === "monthly") {
            if (chartType === "pie" || chartType === "doughnut") {
                loadPieOrDoughnutChart(chartType);
            } else if (chartType === "monthly-bar" || chartType === "monthly-line") {
                loadMonthlyBarOrLineChart(chartType);
            } else {
                toastr.error("Unsupported monthly chart type.", "Error");
            }
        } else {
            toastr.error("Please select a valid chart category.", "Invalid Selection");
        }
    });

    $("#RemoveGraphBtn").click(function () {
        $("#ChartView").hide().html('');
        hideRemoveButton();
    });
});