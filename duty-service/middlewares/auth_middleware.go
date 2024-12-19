package middlewares

import (
	"fmt"
	"net/http"
	"strings"

	"github.com/golang-jwt/jwt/v5"
)

// Secret key used for validating the token signature (replace with your actual secret)
var jwtSecret = []byte("karma-kebab-client-secret")

// Claims struct to parse JWT payload
type Claims struct {
	Username string `json:"username"`
	Role     string `json:"role"`
	jwt.RegisteredClaims
}

// JWTAuthMiddleware validates the JWT token and extracts user info
func JWTAuthMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		// Get the Authorization header
		authHeader := r.Header.Get("Authorization")
		if authHeader == "" {
			http.Error(w, "Authorization header missing", http.StatusUnauthorized)
			return
		}

		// Check if the header is in the format "Bearer <token>"
		tokenString := strings.TrimPrefix(authHeader, "Bearer ")
		if tokenString == authHeader {
			http.Error(w, "Invalid Authorization header format", http.StatusUnauthorized)
			return
		}

		// Parse and validate the token
		claims := &Claims{}
		token, err := jwt.ParseWithClaims(tokenString, claims, func(token *jwt.Token) (interface{}, error) {
			return jwtSecret, nil
		})

		if err != nil || !token.Valid {
			http.Error(w, "Invalid token", http.StatusUnauthorized)
			return
		}

		// Pass user info to the next handler
		r.Header.Set("X-User", claims.Username)
		r.Header.Set("X-Role", claims.Role)

		next.ServeHTTP(w, r)
	})
}

// RoleMiddleware enforces role-based access control
func RoleMiddleware(requiredRole string, next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		role := r.Header.Get("X-Role")
		if role != requiredRole {
			http.Error(w, fmt.Sprintf("Forbidden: %s role required", requiredRole), http.StatusForbidden)
			return
		}
		next.ServeHTTP(w, r)
	})
}
