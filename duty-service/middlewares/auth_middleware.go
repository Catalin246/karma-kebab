package middlewares

import (
	"crypto/rsa"
	"crypto/x509"
	"encoding/pem"
	"fmt"
	"log"
	"net/http"
	"strings"

	"github.com/golang-jwt/jwt/v5"
)

func JWTMiddleware(publicKeyPEM string, next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		authHeader := r.Header.Get("Authorization")
		if authHeader == "" || !strings.HasPrefix(authHeader, "Bearer ") {
			http.Error(w, "Unauthorized: No token provided", http.StatusUnauthorized)
			return
		}
		tokenString := strings.TrimPrefix(authHeader, "Bearer ")
		// Parse the certificate
		block, _ := pem.Decode([]byte(publicKeyPEM))
		if block == nil {
			log.Println("Failed to decode PEM block")
			log.Println("Public Key PEM:", publicKeyPEM)
			http.Error(w, "Failed to parse certificate", http.StatusInternalServerError)
			return
		}
		cert, err := x509.ParseCertificate(block.Bytes)
		if err != nil {
			fmt.Println("Public Key PEM:")
			fmt.Println(publicKeyPEM)
			http.Error(w, "Failed to parse certificate", http.StatusInternalServerError)
			return
		}
		publicKey := cert.PublicKey.(*rsa.PublicKey)
		token, err := jwt.Parse(tokenString, func(token *jwt.Token) (interface{}, error) {
			if _, ok := token.Method.(*jwt.SigningMethodRSA); !ok {
				return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
			}
			return publicKey, nil
		})
		if err != nil || !token.Valid {
			http.Error(w, "Unauthorized: invalid token", http.StatusUnauthorized)
			return
		}
		claims, ok := token.Claims.(jwt.MapClaims)
		if !ok {
			http.Error(w, "Invalid token claims", http.StatusUnauthorized)
			return
		}
		// Check for admin role
		hasAdminRole := false
		if realmAccess, ok := claims["realm_access"].(map[string]interface{}); ok {
			if roles, ok := realmAccess["roles"].([]interface{}); ok {
				for _, role := range roles {
					if roleStr, ok := role.(string); ok && roleStr == "admin" {
						hasAdminRole = true
						break
					}
				}
			}
		}
		if !hasAdminRole {
			http.Error(w, "Forbidden: admin role required", http.StatusForbidden)
			return
		}
		next.ServeHTTP(w, r)
	})
}
