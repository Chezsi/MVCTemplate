const chartOptions = {
    annual: [
        { value: "annual-bar", text: "Bar" },
        { value: "annual-line", text: "Line" },
        { value: "annual-pie", text: "Pie" },
        { value: "annual-doughnut", text: "Doughnut" }
    ],
    monthly: [
        { value: "monthly-bar", text: "Bar" },
        { value: "monthly-line", text: "Line" },
        { value: "monthly-pie", text: "Pie" },
        { value: "monthly-doughnut", text: "Doughnut" }
    ]
};

let chartInstance = null;

function updateShowButtonVisibility() {
    const category = $('#ChartCategory').val();
    const type = $('#ChartType').val();
    $("#ShowGraphBtn").toggle(category && category !== "--Select Category--" && type && type !== "--Select Type--");
}

function showButtons() {
    $("#RemoveGraphBtn").show();
    $("#ExportChartBtn").show();
}

function hideButtons() {
    $("#RemoveGraphBtn").hide();
    $("#ExportChartBtn").hide();
}

function renderChart(chartType, labels, values, options = {}) {
    if (chartInstance) chartInstance.destroy();

    const canvas = document.getElementById("myChart");
    const ctx = canvas.getContext("2d");

    const isBarOrLine = chartType.includes('bar') || chartType.includes('line');
    const actualType = chartType.replace(/^.*?-/, ''); // removes 'annual-' or 'monthly-'

    const chartColors = [
        "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8",
        "#6c757d", "#fd7e14", "#20c997", "#6f42c1", "#e83e8c",
        "#6610f2", "#343a40"
    ];

    chartInstance = new Chart(ctx, {
        type: actualType,
        data: {
            labels,
            datasets: [{
                label: options.label || 'Contracts',
                data: values,
                backgroundColor: options.bgColor || chartColors.slice(0, labels.length),
                borderColor: "#0056b3",
                fill: actualType === 'line' ? false : true,
                tension: actualType === 'line' ? 0.1 : undefined
            }]
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: options.title || "Contracts Chart",
                    font: { size: 18 }
                },
                legend: {
                    display: actualType === 'pie' || actualType === 'doughnut'
                },
                datalabels: {
                    color: '#000',
                    anchor: isBarOrLine ? 'end' : 'center',
                    align: isBarOrLine ? 'top' : 'center',
                    font: { weight: 'bold' },
                    formatter: (val) => val
                }
            },
            scales: isBarOrLine ? {
                x: { title: { display: true, text: options.xTitle || "X" } },
                y: {
                    beginAtZero: true,
                    title: { display: true, text: options.yTitle || "Y" },
                    ticks: { stepSize: 1 }
                }
            } : {}
        },
        plugins: [ChartDataLabels]
    });

    showButtons();
}

function loadChart(url, chartType, label, title, xTitle, yTitle) {
    $("#ChartView").html('<canvas id="myChart" style="max-width:750px; max-height:400px;"></canvas>').show();

    $.ajax({
        type: "POST",
        url,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            const labels = data[0];
            const values = data[1];

            if (!labels.length) {
                alert("No data available.");
                $("#ChartView").hide();
                hideButtons();
                return;
            }

            renderChart(chartType, labels, values, {
                label,
                title,
                xTitle,
                yTitle
            });
        },
        error: function (xhr, status, error) {
            console.error("Error fetching chart data:", error);
            alert("Failed to load chart data.");
            hideButtons();
        }
    });
}

$(function () {
    $("#ShowGraphBtn, #RemoveGraphBtn, #ExportChartBtn").hide();

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

    $('#ChartType').on('change', updateShowButtonVisibility);

    $("#ShowGraphBtn").click(function () {
        const chartType = $("#ChartType").val();
        const category = $("#ChartCategory").val();

        if (!chartType || chartType.startsWith('--')) {
            toastr.error("Please select a valid chart type.", "Invalid Selection");
            return;
        }

        if (category === "annual") {
            if (chartType === "annual-bar" || chartType === "annual-line") {
                loadChart("/Admin/Contract/GetContractsPerYear", chartType, 'Contracts', 'Contracts Per Year', 'Year', 'Contracts');
            } else if (chartType === "annual-pie" || chartType === "annual-doughnut") {
                loadChart("/Admin/Contract/GetContractsPerYear", chartType, '', 'Annual Contracts Distribution');
            }
        } else if (category === "monthly") {
            if (chartType === "monthly-bar" || chartType === "monthly-line") {
                loadChart("/Admin/Contract/GetContractsPerMonth", chartType, 'Contracts', 'Monthly Contracts', 'Month', 'Contracts');
            } else if (chartType === "monthly-pie" || chartType === "monthly-doughnut") {
                loadChart("/Admin/Contract/GetContractsPerMonth", chartType, '', 'Contracts Per Month');
            }
        }
    });

    $("#RemoveGraphBtn").click(function () {
        if (chartInstance) {
            chartInstance.destroy();
            chartInstance = null;
        }
        $("#ChartView").hide().html('');
        hideButtons();
    });

    $("#ExportChartBtn").click(function () {
        const canvas = document.getElementById("myChart");
        const chartType = $("#ChartType").val();
        if (canvas && chartType) {
            const image = canvas.toDataURL("image/png");
            const link = document.createElement('a');
            link.href = image;
            link.download = `contracts-chart-${chartType}.png`; // Append chart type to filename
            link.click();
        }
    });
});
