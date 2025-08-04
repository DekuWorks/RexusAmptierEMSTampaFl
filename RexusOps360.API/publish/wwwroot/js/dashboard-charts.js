// Dashboard Charts and Analytics
class DashboardCharts {
    constructor() {
        this.charts = {};
        this.updateInterval = null;
        this.initializeCharts();
        this.startRealTimeUpdates();
    }

    initializeCharts() {
        this.createIncidentChart();
        this.createResponderStatusChart();
        this.createEquipmentChart();
        this.createResponseTimeChart();
        this.createMapView();
    }

    createIncidentChart() {
        const ctx = document.getElementById('incidentChart');
        if (!ctx) return;

        this.charts.incident = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Active', 'Resolved', 'Pending', 'Critical'],
                datasets: [{
                    data: [12, 45, 8, 3],
                    backgroundColor: [
                        '#ff6384',
                        '#36a2eb',
                        '#ffce56',
                        '#ff6384'
                    ],
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom'
                    },
                    title: {
                        display: true,
                        text: 'Incident Status Distribution'
                    }
                }
            }
        });
    }

    createResponderStatusChart() {
        const ctx = document.getElementById('responderChart');
        if (!ctx) return;

        this.charts.responder = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Available', 'On Call', 'Off Duty', 'Training'],
                datasets: [{
                    label: 'Responders',
                    data: [15, 8, 5, 3],
                    backgroundColor: [
                        '#4CAF50',
                        '#2196F3',
                        '#FF9800',
                        '#9C27B0'
                    ]
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: 'Responder Status'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }

    createEquipmentChart() {
        const ctx = document.getElementById('equipmentChart');
        if (!ctx) return;

        this.charts.equipment = new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['00:00', '04:00', '08:00', '12:00', '16:00', '20:00'],
                datasets: [{
                    label: 'Equipment Utilization',
                    data: [65, 70, 85, 90, 75, 80],
                    borderColor: '#36a2eb',
                    backgroundColor: 'rgba(54, 162, 235, 0.1)',
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: 'Equipment Utilization (24h)'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100
                    }
                }
            }
        });
    }

    createResponseTimeChart() {
        const ctx = document.getElementById('responseTimeChart');
        if (!ctx) return;

        this.charts.responseTime = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Fire', 'Medical', 'Traffic', 'Rescue'],
                datasets: [{
                    label: 'Average Response Time (minutes)',
                    data: [4.2, 6.8, 3.5, 8.1],
                    backgroundColor: [
                        '#ff6384',
                        '#36a2eb',
                        '#ffce56',
                        '#4bc0c0'
                    ]
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: 'Response Times by Incident Type'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Minutes'
                        }
                    }
                }
            }
        });
    }

    createMapView() {
        // Initialize map if map container exists
        const mapContainer = document.getElementById('incidentMap');
        if (mapContainer) {
            this.initializeMap();
        }
    }

    initializeMap() {
        // Mock map initialization - in real implementation, use Leaflet or Google Maps
        const mapContainer = document.getElementById('incidentMap');
        if (mapContainer) {
            mapContainer.innerHTML = `
                <div style="background: #f0f0f0; height: 300px; border-radius: 8px; display: flex; align-items: center; justify-content: center;">
                    <div style="text-align: center;">
                        <i class="fas fa-map-marker-alt" style="font-size: 48px; color: #666;"></i>
                        <p style="margin-top: 10px; color: #666;">Interactive Map View</p>
                        <p style="font-size: 12px; color: #999;">Real-time incident locations</p>
                    </div>
                </div>
            `;
        }
    }

    updateCharts(data) {
        if (data.incidents) {
            this.updateIncidentChart(data.incidents);
        }
        if (data.responders) {
            this.updateResponderChart(data.responders);
        }
        if (data.equipment) {
            this.updateEquipmentChart(data.equipment);
        }
    }

    updateIncidentChart(incidentData) {
        if (this.charts.incident) {
            this.charts.incident.data.datasets[0].data = incidentData;
            this.charts.incident.update();
        }
    }

    updateResponderChart(responderData) {
        if (this.charts.responder) {
            this.charts.responder.data.datasets[0].data = responderData;
            this.charts.responder.update();
        }
    }

    updateEquipmentChart(equipmentData) {
        if (this.charts.equipment) {
            this.charts.equipment.data.datasets[0].data = equipmentData;
            this.charts.equipment.update();
        }
    }

    startRealTimeUpdates() {
        // Update charts every 30 seconds
        this.updateInterval = setInterval(() => {
            this.fetchDashboardData();
        }, 30000);

        // Initial data fetch
        this.fetchDashboardData();
    }

    async fetchDashboardData() {
        try {
            const response = await fetch('/api/dashboard/analytics');
            if (response.ok) {
                const data = await response.json();
                this.updateCharts(data);
                this.updateDashboardMetrics(data);
            }
        } catch (error) {
            console.error('Error fetching dashboard data:', error);
        }
    }

    updateDashboardMetrics(data) {
        // Update key metrics
        if (data.activeIncidents !== undefined) {
            this.updateMetric('activeIncidents', data.activeIncidents);
        }
        if (data.availableResponders !== undefined) {
            this.updateMetric('availableResponders', data.availableResponders);
        }
        if (data.avgResponseTime !== undefined) {
            this.updateMetric('avgResponseTime', data.avgResponseTime);
        }
        if (data.equipmentUtilization !== undefined) {
            this.updateMetric('equipmentUtilization', data.equipmentUtilization);
        }
    }

    updateMetric(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value;
        }
    }

    destroy() {
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
        }
        
        Object.values(this.charts).forEach(chart => {
            if (chart && chart.destroy) {
                chart.destroy();
            }
        });
    }
}

// Initialize dashboard charts when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    if (typeof Chart !== 'undefined') {
        window.dashboardCharts = new DashboardCharts();
    }
}); 