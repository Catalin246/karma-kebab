package handlers

import (
	"duty-service/metrics"
	"net/http"
	"time"

	"github.com/prometheus/client_golang/prometheus/promhttp"
)

type MetricsHandler struct{}

func NewMetricsHandler() *MetricsHandler {
	return &MetricsHandler{}
}

// // RegisterMetricsRoutes registers the /metrics route for Prometheus scraping
// func (m *MetricsHandler) RegisterMetricsRoutes(r *mux.Router) {
// 	r.HandleFunc("/duties/metrics", m.HandleMetrics).Methods(http.MethodGet)
// }

func (m *MetricsHandler) HandleMetrics(w http.ResponseWriter, r *http.Request) {
	promhttp.Handler().ServeHTTP(w, r)
}

// TrackRequestDurationMiddleware is a middleware that tracks HTTP request durations and metrics
func (m *MetricsHandler) TrackRequestDurationMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()

		// Create a response writer to capture the status code
		rr := &statusRecordingResponseWriter{ResponseWriter: w}
		next.ServeHTTP(rr, r)

		//track the duration and status of request
		metrics.TrackRequestDuration(start, r.Method, http.StatusText(rr.statusCode))
		metrics.CountRequest(r.Method, http.StatusText(rr.statusCode))

		// If the status code indicates an error, increment error counter
		if rr.statusCode >= 400 {
			metrics.CountError(r.Method, http.StatusText(rr.statusCode))
		}
	})
}

// statusRecordingResponseWriter is used to capture the status code of the response
type statusRecordingResponseWriter struct {
	http.ResponseWriter
	statusCode int
}

func (rw *statusRecordingResponseWriter) WriteHeader(statusCode int) {
	rw.statusCode = statusCode
	rw.ResponseWriter.WriteHeader(statusCode)
}
