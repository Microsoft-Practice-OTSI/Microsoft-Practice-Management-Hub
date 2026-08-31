// Microsoft Practice Hub - Executive Dashboard Chart.js Integration

document.addEventListener("DOMContentLoaded", function () {
    initDashboardCharts();
});

function initDashboardCharts() {
    // 1. Resource Utilization Donut Chart
    const utilDonutCtx = document.getElementById("utilizationDonutChart");
    if (utilDonutCtx && window.dashboardChartData && window.dashboardChartData.utilizationDonut) {
        const data = window.dashboardChartData.utilizationDonut;
        new Chart(utilDonutCtx, {
            type: 'doughnut',
            data: {
                labels: data.labels,
                datasets: [{
                    data: data.datasets[0].data,
                    backgroundColor: data.datasets[0].backgroundColors,
                    borderColor: data.datasets[0].borderColors,
                    borderWidth: 2,
                    hoverOffset: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '72%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            font: { family: "'Segoe UI', sans-serif", size: 12 },
                            padding: 16,
                            usePointStyle: true,
                            pointStyle: 'circle'
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const value = context.raw || 0;
                                const pct = ((value / total) * 100).toFixed(1);
                                return ` ${context.label}: ${value} resources (${pct}%)`;
                            }
                        }
                    }
                }
            }
        });
    }

    // 2. Technology Distribution Bar Chart
    const techBarCtx = document.getElementById("techDistributionBarChart");
    if (techBarCtx && window.dashboardChartData && window.dashboardChartData.techDistribution) {
        const data = window.dashboardChartData.techDistribution;
        new Chart(techBarCtx, {
            type: 'bar',
            data: {
                labels: data.labels,
                datasets: [{
                    label: 'Consultants',
                    data: data.datasets[0].data,
                    backgroundColor: data.datasets[0].backgroundColors,
                    borderRadius: 4,
                    barThickness: 24
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                return ` Headcount: ${ctx.raw} engineers`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { font: { family: "'Segoe UI', sans-serif", size: 11 } }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0, 0, 0, 0.06)' },
                        ticks: { font: { family: "'Segoe UI', sans-serif", size: 11 }, precision: 0 }
                    }
                }
            }
        });
    }

    // 3. Capacity vs Demand Stacked Bar Chart
    const capDemandCtx = document.getElementById("capacityDemandChart");
    if (capDemandCtx && window.dashboardChartData && window.dashboardChartData.capacityVsDemand) {
        const data = window.dashboardChartData.capacityVsDemand;
        new Chart(capDemandCtx, {
            type: 'bar',
            data: {
                labels: data.labels,
                datasets: data.datasets.map(ds => ({
                    label: ds.label,
                    data: ds.data,
                    backgroundColor: ds.backgroundColors[0],
                    borderRadius: 4,
                    barThickness: 20
                }))
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            font: { family: "'Segoe UI', sans-serif", size: 12 },
                            usePointStyle: true,
                            pointStyle: 'rectRounded'
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { font: { family: "'Segoe UI', sans-serif", size: 11 } }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0, 0, 0, 0.06)' },
                        ticks: { font: { family: "'Segoe UI', sans-serif", size: 11 } }
                    }
                }
            }
        });
    }

    // 4. 12-Month Utilization Trend Line Chart
    const utilTrendCtx = document.getElementById("utilizationTrendLineChart");
    if (utilTrendCtx && window.dashboardChartData && window.dashboardChartData.utilizationTrend) {
        const data = window.dashboardChartData.utilizationTrend;
        new Chart(utilTrendCtx, {
            type: 'line',
            data: {
                labels: data.labels,
                datasets: [{
                    label: 'Practice Utilization %',
                    data: data.datasets[0].data,
                    borderColor: '#0078d4',
                    backgroundColor: 'rgba(0, 120, 212, 0.12)',
                    fill: true,
                    tension: 0.35,
                    borderWidth: 3,
                    pointBackgroundColor: '#0078d4',
                    pointRadius: 4,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                return ` Utilization: ${ctx.raw}%`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { font: { family: "'Segoe UI', sans-serif", size: 11 } }
                    },
                    y: {
                        min: 70,
                        max: 95,
                        grid: { color: 'rgba(0, 0, 0, 0.06)' },
                        ticks: {
                            font: { family: "'Segoe UI', sans-serif", size: 11 },
                            callback: function (val) { return val + '%'; }
                        }
                    }
                }
            }
        });
    }
}
