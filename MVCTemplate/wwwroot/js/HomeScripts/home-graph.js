
const ageLabels = window.ageLabels;
const ageData = window.ageData;

    const ctx = document.getElementById('ageChart').getContext('2d');
    new Chart(ctx, {
        type: 'line',
    data: {
        labels: ageLabels,
    datasets: [{
        label: 'Products Created (days ago)',
    data: ageData,
    fill: false,
    borderColor: 'rgb(75, 192, 192)',
    backgroundColor: 'rgb(75, 192, 192)',
    tension: 0.3,
    pointRadius: 5,
    pointHoverRadius: 7
            }]
        },
    options: {
        plugins: {
        legend: {display: false },
    datalabels: {
        anchor: 'end',
    align: 'top',
    font: {
        weight: 'bold'
                    },
    formatter: function(value) {
                        return value;
                    }
                }
            },
    scales: {
        x: {
        title: {display: true, text: 'Days Ago' }
                },
    y: {
        title: {display: true, text: 'Count' },
    beginAtZero: true
                }
            }
        },
    plugins: [ChartDataLabels]
    });

