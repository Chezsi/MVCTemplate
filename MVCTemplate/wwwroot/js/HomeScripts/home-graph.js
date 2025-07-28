
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
        layout: {
            padding: {
                top: 30 // <-- prevents data labels from getting cut
            }
        },
        plugins: {
            legend: { display: false },
            datalabels: {
                anchor: 'end',
                align: 'top',
                clip: false,
                padding: {
                    top: 4,
                    bottom: 4
                },
                font: {
                    weight: 'bold'
                },
                formatter: function (value) {
                    return value;
                }
            }
        },
        scales: {
            x: {
                title: {
                    display: true,
                    text: 'Days Ago'
                }
            },
            y: {
                beginAtZero: true,
                title: {
                    display: true,
                    text: 'Count'
                }
            }
        }
    },
    plugins: [ChartDataLabels]
});

document.addEventListener("DOMContentLoaded", function () {
    // Get original labels and append ' Priority' to each
    const labels = window.priorityLabels.map(label => `${label} Priority`);
    const data = window.priorityCounts;

    const ctx = document.getElementById('priorityChart').getContext('2d');
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                label: 'Package Count by Priority',
                data: data,
                backgroundColor: [
                    'rgba(255, 99, 132, 0.6)',
                    'rgba(54, 162, 235, 0.6)',
                    'rgba(255, 206, 86, 0.6)',
                    'rgba(75, 192, 192, 0.6)',
                    'rgba(153, 102, 255, 0.6)'
                ],
                borderColor: '#fff',
                borderWidth: 1
            }]
        },
        options: {
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom'
                },
                datalabels: {
                    color: '#000',
                    font: {
                        weight: 'bold'
                    },
                    formatter: function (value) {
                        return value;
                    }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
});

$(document).ready(function () {
    $.ajax({
        url: '/Admin/Product/GetAllProducts',
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (response) {
            const products = response.data;

            // Group by description
            const descriptionMap = {};
            products.forEach(product => {
                const description = product.description || 'No Description';
                descriptionMap[description] = (descriptionMap[description] || 0) + 1;
            });

            const labels = Object.keys(descriptionMap);
            const counts = Object.values(descriptionMap);

            const backgroundColors = labels.map((_, index) => {
                const colors = [
                    'rgba(255, 99, 132, 0.6)',
                    'rgba(54, 162, 235, 0.6)',
                    'rgba(255, 206, 86, 0.6)',
                    'rgba(75, 192, 192, 0.6)',
                    'rgba(153, 102, 255, 0.6)',
                    'rgba(255, 159, 64, 0.6)',
                    'rgba(201, 203, 207, 0.6)'
                ];
                return colors[index % colors.length]; 
            });

            const borderColors = backgroundColors.map(color =>
                color.replace('0.6', '1') 
            );

            const ctx = document.getElementById('descriptionChart').getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Product Count by Description',
                        data: counts,
                        backgroundColor: backgroundColors,
                        borderColor: borderColors,
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            display: true,
                            onClick: null
                        },
                        datalabels: {
                            anchor: 'end',
                            align: 'top',
                            color: '#000',
                            font: {
                                weight: 'bold'
                            },
                            formatter: function (value) {
                                return value;
                            }
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                precision: 0
                            }
                        }
                    }
                },
                plugins: [ChartDataLabels]
            });
        },
        error: function () {
            alert('Failed to load product data.');
        }
    });
});
