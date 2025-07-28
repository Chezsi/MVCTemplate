try {
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
                    top: 30
                }
            },
            plugins: {
                title: {
                    display: true,
                    text: 'Report Age Distribution',
                    font: {
                        size: 18,
                        weight: 'bold'
                    },
                    padding: {
                        bottom: 20
                    }
                },
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
} catch (error) {
    console.error("Failed to render 'Report Chart'", error);
}

document.addEventListener("DOMContentLoaded", function () {
    try {
        const labels = window.priorityLabels.map(label => `${label} Priority`);
        const data = window.priorityCounts;

        const ctx = document.getElementById('priorityChart').getContext('2d');
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
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
                    title: {
                        display: true,
                        text: 'Package Priority Distribution',
                        font: {
                            size: 18,
                            weight: 'bold'
                        },
                        padding: {
                            bottom: 20
                        }
                    },
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
    } catch (error) {
        console.error("Failed to render 'Package Chart'", error);
    }
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

            const descriptionMap = {};
            products.forEach(product => {
                const description = product.description || 'Miscellaneous';
                descriptionMap[description] = (descriptionMap[description] || 0) + product.quantity;
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
                        data: counts,
                        backgroundColor: backgroundColors,
                        borderColor: borderColors,
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    layout: {
                        padding: {
                            top: 30
                        }
                    },
                    plugins: {
                        title: {
                            display: true,
                            text: 'Product Description Distribution',
                            font: {
                                size: 18,
                                weight: 'bold'
                            },
                            padding: {
                                bottom: 20
                            }
                        },
                        legend: {
                            display: false
                        },
                        datalabels: {
                            anchor: 'end',
                            align: 'top',
                            clip: false,
                            color: '#000',
                            font: {
                                weight: 'bold'
                            },
                            padding: {
                                top: 4
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
        error: function (xhr, status, error) {
            console.error("Failed to render 'Product Chart'", error);
        }
    });
});

