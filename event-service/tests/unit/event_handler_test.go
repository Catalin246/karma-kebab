package unit

import (
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/Catalin246/karma-kebab/handlers"
	"github.com/Catalin246/karma-kebab/models"
	"github.com/Catalin246/karma-kebab/tests/mocks"
	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

func TestGetEvents(t *testing.T) {
	// Create a mock service
	mockService := new(mocks.MockEventService)

	// Sample events to return from the mock
	event1 := models.Event{
		RowKey:      uuid.New(),
		Description: "This is the first event",
		StartTime:   time.Now().Add(-24 * time.Hour),
		EndTime:     time.Now(),
	}
	event2 := models.Event{
		RowKey:      uuid.New(),
		Description: "This is the second event",
		StartTime:   time.Now().Add(-48 * time.Hour),
		EndTime:     time.Now().Add(-24 * time.Hour),
	}
	events := []models.Event{event1, event2}

	// Set up expectations for the mock service
	mockService.On("GetAll", mock.Anything, mock.Anything, mock.Anything).Return(events, nil)

	// Create the handler with the mock service
	handler := handlers.NewEventHandler(mockService, nil)

	// Create a new HTTP request
	req, err := http.NewRequest(http.MethodGet, "/events", nil)
	assert.NoError(t, err)

	// Add query parameters to the request
	query := req.URL.Query()
	query.Add("startTime", time.Now().Add(-72*time.Hour).Format(time.RFC3339))
	query.Add("endTime", time.Now().Format(time.RFC3339))
	req.URL.RawQuery = query.Encode()

	// Create a response recorder to capture the response
	rr := httptest.NewRecorder()

	// Call the handler's GetEvents method
	handler.GetEvents(rr, req)

	// Assert the response
	assert.Equal(t, http.StatusOK, rr.Code)

	// Parse the response body
	var response map[string]interface{}
	err = json.Unmarshal(rr.Body.Bytes(), &response)
	assert.NoError(t, err)

	// Assert the response structure
	assert.Equal(t, true, response["success"])
	assert.Equal(t, "Events retrieved successfully", response["message"])

	// Assert the data
	data, ok := response["data"].([]interface{})
	assert.True(t, ok)
	assert.Len(t, data, len(events))

	// Check that the mock's expectations were met
	mockService.AssertExpectations(t)
}
