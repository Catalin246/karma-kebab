package metrics

import (
	"log"
	"net/http"
	"time"

	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promhttp"
)

// HTTP Request Metrics
var (
	httpRequests = prometheus.NewCounterVec(
		prometheus.CounterOpts{
			Name: "http_requests_total",
			Help: "Total number of HTTP requests processed",
		},
		[]string{"method", "status"},
	)
	// New metric for request duration (Histogram)
	httpRequestDuration = prometheus.NewHistogramVec(
		prometheus.HistogramOpts{
			Name:    "http_request_duration_seconds",
			Help:    "Histogram of HTTP request durations",
			Buckets: prometheus.DefBuckets, // Default buckets for latency (0.1s, 0.3s, etc.)
		},
		[]string{"method", "status"},
	)

	// New metric for tracking error requests (Counter)
	httpRequestErrors = prometheus.NewCounterVec(
		prometheus.CounterOpts{
			Name: "http_request_errors_total",
			Help: "Total number of HTTP requests that resulted in an error",
		},
		[]string{"method", "status"},
	)

	// Health status gauge (0 = unhealthy, 1 = healthy)
	applicationHealth = prometheus.NewGaugeVec(
		prometheus.GaugeOpts{
			Name: "application_health_status",
			Help: "Indicates the health status of the application (1 = healthy, 0 = unhealthy)",
		},
		[]string{"service"},
	)
)

func init() {
	prometheus.MustRegister(httpRequests)
	prometheus.MustRegister(httpRequestDuration)
	prometheus.MustRegister(httpRequestErrors)
	prometheus.MustRegister(applicationHealth)

	log.Println("Prometheus metrics initialized")
}

// RegisterMetricsHandler registers the /metrics endpoint
func RegisterMetricsHandler() {
	http.Handle("/metrics", promhttp.Handler())
	log.Println("Prometheus metrics handler registered on /metrics")
}

// CountRequest increments the HTTP request counter
func CountRequest(method, status string) {
	httpRequests.WithLabelValues(method, status).Inc()
}

// TrackRequestDuration measures and records the duration of an HTTP request
func TrackRequestDuration(startTime time.Time, method, status string) {
	duration := time.Since(startTime).Seconds() // Duration in seconds
	httpRequestDuration.WithLabelValues(method, status).Observe(duration)
}

// CountError increments the error count for failed requests
func CountError(method, status string) {
	httpRequestErrors.WithLabelValues(method, status).Inc()
}

// SetHealthStatus sets the health status of the application
func SetHealthStatus(service string, status float64) {
	applicationHealth.WithLabelValues(service).Set(status)
}
