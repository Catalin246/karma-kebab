package main

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/gorilla/mux"
)

// TestResponse represents the structure of the response for the /test endpoint
type TestResponse struct {
	Message string `json:"message"`
}

// TestHandler handles the /test route
func TestHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	response := TestResponse{Message: "This is a test endpoint"}
	json.NewEncoder(w).Encode(response)
}

func main() {
	router := mux.NewRouter()

	// Define the /test endpoint
	router.HandleFunc("/test", TestHandler).Methods("GET")

	log.Println("Server is running on port 8080")
	log.Fatal(http.ListenAndServe(":8080", router))
}
