package middlewares

import (
	"log"
	"net/http"
)

// GatewayHeaderMiddleware validates the X-From-Gateway header
func GatewayHeaderMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		log.Printf("Incoming request: %s %s", r.Method, r.URL.Path)
		if r.Header.Get("X-From-Gateway") != "true" {
			http.Error(w, "Forbidden: Invalid Gateway Header", http.StatusForbidden)
			return
		}
		next.ServeHTTP(w, r)
	})
}
