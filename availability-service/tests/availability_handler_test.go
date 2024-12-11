package tests

import (
	"bytes"
	"encoding/json"
	"errors"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"availability-service/handlers"
	"availability-service/models"

	"github.com/gorilla/mux"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

// Helper function to create a request with Gorilla Mux variables
func createRequestWithVars(method, path string, vars map[string]string, body interface{}) *http.Request {
	var req *http.Request
	var err error

	if body != nil {
		jsonBody, _ := json.Marshal(body)
		req, err = http.NewRequest(method, path, bytes.NewBuffer(jsonBody))
	} else {
		req, err = http.NewRequest(method, path, nil)
	}

	if err != nil {
		panic(err)
	}

	// Add Mux vars
	req = mux.SetURLVars(req, vars)
	return req
}

func TestGetAll(t *testing.T) {
	t.Run("Successful Retrieval", func(t *testing.T) {
		// Create mock service
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)

		// Prepare mock data
		mockAvailabilities := []models.Availability{
			{
				ID:         "1",
				EmployeeID: "emp1",
				StartDate:  time.Now(),
				EndDate:    time.Now().Add(24 * time.Hour),
			},
		}

		// Set expectations
		mockService.On("GetAll", mock.Anything, mock.Anything, mock.Anything).
			Return(mockAvailabilities, nil)

		// Create request
		req, err := http.NewRequest("GET", "/availabilities", nil)
		assert.NoError(t, err)

		// Create response recorder
		w := httptest.NewRecorder()

		// Call handler
		handler.GetAll(w, req)

		// Assert response
		assert.Equal(t, http.StatusOK, w.Code)

		// Decode response
		var receivedAvailabilities []models.Availability
		err = json.Unmarshal(w.Body.Bytes(), &receivedAvailabilities)
		assert.NoError(t, err)
		assert.Len(t, receivedAvailabilities, 1)
	})

	t.Run("Error Scenario", func(t *testing.T) {
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)

		mockService.On("GetAll", mock.Anything, mock.Anything, mock.Anything).
			Return([]models.Availability{}, errors.New("service error"))

		req, err := http.NewRequest("GET", "/availabilities", nil)
		assert.NoError(t, err)

		w := httptest.NewRecorder()
		handler.GetAll(w, req)

		assert.Equal(t, http.StatusInternalServerError, w.Code)
	})
}

func TestGetByEmployeeID(t *testing.T) {
	t.Run("Successful Retrieval", func(t *testing.T) {
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)

		mockAvailabilities := []models.Availability{
			{
				ID:         "1",
				EmployeeID: "emp1",
				StartDate:  time.Now(),
				EndDate:    time.Now().Add(24 * time.Hour),
			},
		}

		mockService.On("GetByEmployeeID", mock.Anything, "emp1").
			Return(mockAvailabilities, nil)

		req := createRequestWithVars("GET", "/employees/emp1/availabilities", 
			map[string]string{"partitionKey": "emp1"}, nil)

		w := httptest.NewRecorder()
		handler.GetByEmployeeID(w, req)

		assert.Equal(t, http.StatusOK, w.Code)

		var receivedAvailabilities []models.Availability
		err := json.Unmarshal(w.Body.Bytes(), &receivedAvailabilities)
		assert.NoError(t, err)
		assert.Len(t, receivedAvailabilities, 1)
	})

	t.Run("No Availabilities Found", func(t *testing.T) {
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)

		mockService.On("GetByEmployeeID", mock.Anything, "emp1").
			Return([]models.Availability{}, models.ErrNotFound)

		req := createRequestWithVars("GET", "/employees/emp1/availabilities", 
			map[string]string{"partitionKey": "emp1"}, nil)

		w := httptest.NewRecorder()
		handler.GetByEmployeeID(w, req)

		assert.Equal(t, http.StatusNotFound, w.Code)
	})
}

func TestCreate(t *testing.T) {
	t.Run("Successful Creation", func(t *testing.T) {
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)

		availability := models.Availability{
			EmployeeID: "emp1",
			StartDate:  time.Now(),
			EndDate:    time.Now().Add(24 * time.Hour),
		}

		createdAvailability := availability
		createdAvailability.ID = "1"

		mockService.On("Create", mock.Anything, availability).
			Return(&createdAvailability, nil)

		jsonBody, _ := json.Marshal(availability)
		req, err := http.NewRequest("POST", "/availabilities", bytes.NewBuffer(jsonBody))
		assert.NoError(t, err)

		w := httptest.NewRecorder()
		handler.Create(w, req)

		assert.Equal(t, http.StatusCreated, w.Code)

		var receivedAvailability models.Availability
		err = json.Unmarshal(w.Body.Bytes(), &receivedAvailability)
		assert.NoError(t, err)
		assert.NotEmpty(t, receivedAvailability.ID)
	})

	t.Run("Conflict Scenario", func(t *testing.T) {
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)

		availability := models.Availability{
			EmployeeID: "emp1",
			StartDate:  time.Now(),
			EndDate:    time.Now().Add(24 * time.Hour),
		}

		mockService.On("Create", mock.Anything, availability).
			Return(nil, errors.New("availability conflicts"))

		jsonBody, _ := json.Marshal(availability)
		req, err := http.NewRequest("POST", "/availabilities", bytes.NewBuffer(jsonBody))
		assert.NoError(t, err)

		w := httptest.NewRecorder()
		handler.Create(w, req)

		assert.Equal(t, http.StatusConflict, w.Code)
	})
}

func TestUpdate(t *testing.T) {
	t.Run("Successful Update", func(t *testing.T) {
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)

		updateReq := handlers.UpdateAvailabilityRequest{
			EmployeeID: "emp1",
			StartDate:  time.Now().UTC().Format(time.RFC3339),
			EndDate:    time.Now().Add(24 * time.Hour).UTC().Format(time.RFC3339),
		}

		mockService.On("Update", 
			mock.Anything, 
			"emp1", 
			"avail1", 
			mock.AnythingOfType("models.Availability")).
			Return(nil)

		req := createRequestWithVars("PUT", "/employees/emp1/availabilities/avail1", 
			map[string]string{
				"partitionKey": "emp1", 
				"rowKey": "avail1",
			}, updateReq)

		w := httptest.NewRecorder()
		handler.Update(w, req)

		assert.Equal(t, http.StatusOK, w.Code)
	})

	t.Run("Not Found Scenario", func(t *testing.T) {
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)

		updateReq := handlers.UpdateAvailabilityRequest{
			EmployeeID: "emp1",
			StartDate:  time.Now().UTC().Format(time.RFC3339),
			EndDate:    time.Now().Add(24 * time.Hour).UTC().Format(time.RFC3339),
		}

		mockService.On("Update", 
			mock.Anything, 
			"emp1", 
			"avail1", 
			mock.AnythingOfType("models.Availability")).
			Return(models.ErrNotFound)

		req := createRequestWithVars("PUT", "/employees/emp1/availabilities/avail1", 
			map[string]string{
				"partitionKey": "emp1", 
				"rowKey": "avail1",
			}, updateReq)

		w := httptest.NewRecorder()
		handler.Update(w, req)

		assert.Equal(t, http.StatusNotFound, w.Code)
	})
}

func TestDelete(t *testing.T) {
	t.Run("Successful Deletion", func(t *testing.T) {
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)

		mockService.On("Delete", mock.Anything, "emp1", "avail1").
			Return(nil)

		req := createRequestWithVars("DELETE", "/employees/emp1/availabilities/avail1", 
			map[string]string{
				"partitionKey": "emp1", 
				"rowKey": "avail1",
			}, nil)

		w := httptest.NewRecorder()
		handler.Delete(w, req)

		assert.Equal(t, http.StatusNoContent, w.Code)
	})

	t.Run("Not Found Scenario", func(t *testing.T) {
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)

		mockService.On("Delete", mock.Anything, "emp1", "avail1").
			Return(models.ErrNotFound)

		req := createRequestWithVars("DELETE", "/employees/emp1/availabilities/avail1", 
			map[string]string{
				"partitionKey": "emp1", 
				"rowKey": "avail1",
			}, nil)

		w := httptest.NewRecorder()
		handler.Delete(w, req)

		assert.Equal(t, http.StatusNotFound, w.Code)
	})
}