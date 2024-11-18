package main

import (
	"log"
	"net/http"

	"github.com/gorilla/mux"
)

func main() {
	router := mux.NewRouter()

	log.Println("Server is running on port 3001")
	log.Fatal(http.ListenAndServe(":3001", router))
}
