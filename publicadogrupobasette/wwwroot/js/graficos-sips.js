// Gráficos para SIPS usando Chart.js

let graficoConsumo = null;
let graficoDistribucion = null;

window.crearGraficoConsumoMensual = function(etiquetas, valores) {
    const ctx = document.getElementById('graficoConsumoMensual');
    if (!ctx) return;

    // Destruir gráfico anterior si existe
    if (graficoConsumo) {
        graficoConsumo.destroy();
    }

    graficoConsumo = new Chart(ctx, {
        type: 'line',
        data: {
            labels: etiquetas,
            datasets: [{
                label: 'Consumo Total (kWh)',
                data: valores,
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                tension: 0.4,
                fill: true,
                pointRadius: 4,
                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                },
                title: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return context.dataset.label + ': ' + context.parsed.y.toLocaleString('es-ES') + ' kWh';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return value.toLocaleString('es-ES') + ' kWh';
                        }
                    }
                }
            }
        }
    });
};

window.crearGraficoDistribucionPeriodos = function(totalP1, totalP2, totalP3) {
    const ctx = document.getElementById('graficoDistribucionPeriodos');
    if (!ctx) return;

    // Destruir gráfico anterior si existe
    if (graficoDistribucion) {
        graficoDistribucion.destroy();
    }

    graficoDistribucion = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Periodo 1', 'Periodo 2', 'Periodo 3'],
            datasets: [{
                label: 'Consumo Total por Periodo (kWh)',
                data: [totalP1, totalP2, totalP3],
                backgroundColor: [
                    'rgba(54, 162, 235, 0.7)',
                    'rgba(75, 192, 192, 0.7)',
                    'rgba(153, 102, 255, 0.7)'
                ],
                borderColor: [
                    'rgb(54, 162, 235)',
                    'rgb(75, 192, 192)',
                    'rgb(153, 102, 255)'
                ],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                title: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const total = totalP1 + totalP2 + totalP3;
                            const porcentaje = ((context.parsed.y / total) * 100).toFixed(1);
                            return context.parsed.y.toLocaleString('es-ES') + ' kWh (' + porcentaje + '%)';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return value.toLocaleString('es-ES') + ' kWh';
                        }
                    }
                }
            }
        }
    });
};
