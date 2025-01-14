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
	"github.com/stretchr/testify/require"
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

	req = mux.SetURLVars(req, vars)
	return req
}

func TestGetAll(t *testing.T) {
    t.Run("Get By EmployeeID Filter Successful", func(t *testing.T) {
		mockService := new(MockAvailabilityService)
		handler := handlers.NewAvailabilityHandler(mockService)
	
		empID := "2a105d01-58a1-4bfa-a1c9-d9468c2583a3"
		expectedAvailability := models.Availability{
			ID:         "2bfbfc40-5dd7-4b0d-aff8-4bd830804962",
			EmployeeID: empID,
			StartDate:  time.Date(2025, 2, 12, 14, 0, 0, 0, time.UTC),
			EndDate:    time.Date(2025, 2, 15, 22, 0, 0, 0, time.UTC),
		}
	
		mockAvailabilities := []models.Availability{expectedAvailability}
		mockService.On("GetAll", mock.Anything, empID, mock.Anything, mock.Anything).
			Return(mockAvailabilities, nil).Once()
	
		req := httptest.NewRequest("GET", "/availability?employeeId="+empID, nil)
		w := httptest.NewRecorder()
	
		handler.GetAll(w, req)
	
		t.Logf("Response Body: %s", w.Body.String()) // Add log to inspect the response
		assert.Equal(t, http.StatusOK, w.Code)
	
		var receivedAvailabilities []models.Availability
		err := json.Unmarshal(w.Body.Bytes(), &receivedAvailabilities)
		assert.NoError(t, err)
		assert.Len(t, receivedAvailabilities, 1)
		assert.Equal(t, expectedAvailability, receivedAvailabilities[0])
	
		mockService.AssertExpectations(t)
	})

    t.Run("Invalid UUID Format", func(t *testing.T) {
        mockService := new(MockAvailabilityService)
        handler := handlers.NewAvailabilityHandler(mockService)

        // Create request with invalid UUID format
        req := httptest.NewRequest("GET", "/availability?employeeId=invalid-uuid", nil)

        w := httptest.NewRecorder()
        handler.GetAll(w, req)

        assert.Equal(t, http.StatusBadRequest, w.Code)
    })

    t.Run("Invalid Date Format", func(t *testing.T) {
        mockService := new(MockAvailabilityService)
        handler := handlers.NewAvailabilityHandler(mockService)

        empID := "89ji0k34-k087-159j-fu3l-30718f822j434"
        // Create request with invalid date format
        req := httptest.NewRequest("GET", "/availability?employeeId="+empID+"&startDate=invalid-date", nil)

        w := httptest.NewRecorder()
        handler.GetAll(w, req)

        assert.Equal(t, http.StatusBadRequest, w.Code)
    })

    t.Run("Service Error", func(t *testing.T) {
        mockService := new(MockAvailabilityService)
        handler := handlers.NewAvailabilityHandler(mockService)

        empID := "89ji0k34-k087-159j-fu3l-30718f822j434"
        // Mock the GetAll service call returning an error
        mockService.On("GetAll", mock.Anything, empID, mock.Anything, mock.Anything).
            Return(nil, errors.New("service error"))

        req := httptest.NewRequest("GET", "/availability?employeeId="+empID, nil)

        w := httptest.NewRecorder()
        handler.GetAll(w, req)

        assert.Equal(t, http.StatusBadRequest, w.Code)
    })
}

func TestCreate(t *testing.T) {
    t.Run("Successful Creation", func(t *testing.T) {
        mockService := new(MockAvailabilityService)
        handler := handlers.NewAvailabilityHandler(mockService)
        
        //test data
        employeeID := "d536770f-12c9-4de4-8583-4a6bd2a302b4"
        startTime := time.Date(2025, 2, 15, 22, 0, 0, 0, time.UTC)
        endTime := startTime.Add(24 * time.Hour)
        
        availability := models.Availability{
            EmployeeID: employeeID,
            StartDate:  startTime,
            EndDate:    endTime,
        }
        
        createdAvailability := availability
        createdAvailability.ID = "1"
        
        mockService.On("Create", mock.Anything, mock.MatchedBy(func(av models.Availability) bool {
            return av.EmployeeID == employeeID &&
                av.StartDate.Equal(startTime) &&
                av.EndDate.Equal(endTime)
        })).Return(&createdAvailability, nil)
        
        jsonBody, err := json.Marshal(availability)
        require.NoError(t, err)
        
        req := httptest.NewRequest(http.MethodPost, "/availabilities", bytes.NewBuffer(jsonBody))
        req.Header.Set("Content-Type", "application/json")
        
        w := httptest.NewRecorder()
        
        handler.Create(w, req)
        
        assert.Equal(t, http.StatusCreated, w.Code)
        
        var response models.Availability
        err = json.Unmarshal(w.Body.Bytes(), &response)
        require.NoError(t, err)
        
        assert.NotEmpty(t, response.ID)
        assert.Equal(t, employeeID, response.EmployeeID)
        
        mockService.AssertExpectations(t)
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
				"rowKey":       "avail1",
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
				"rowKey":       "avail1",
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
				"rowKey":       "avail1",
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
				"rowKey":       "avail1",
			}, nil)

		w := httptest.NewRecorder()
		handler.Delete(w, req)

		assert.Equal(t, http.StatusNotFound, w.Code)
	})
}
