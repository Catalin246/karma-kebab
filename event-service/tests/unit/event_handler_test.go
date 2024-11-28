package unit

import (
	"bytes"
	"encoding/json"
	"errors"
	"event-service/handlers"
	"event-service/models"
	"event-service/tests/mocks"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/google/uuid"
	"github.com/gorilla/mux"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

func TestGetEvents(t *testing.T) {
	mockService := new(mocks.MockEventService)
	handler := handlers.NewEventHandler(mockService)

	startTime := time.Date(2024, 11, 26, 0, 0, 0, 0, time.UTC).Add(-time.Hour * 24)
	endTime := time.Date(2024, 11, 26, 0, 0, 0, 0, time.UTC)

	mockEvents := []models.Event{
		{
			PartitionKey: "partition1",
			RowKey:       uuid.New(),
			Date:         time.Date(2024, 11, 26, 15, 30, 59, 0, time.UTC),
			Address:      "Grotestraat, 122",
			Venue:        "Christmas Event",
			Description:  "Charity Event",
			Money:        100.50,
			Status:       models.StatusPlanned,
			Person: models.Person{
				FirstName: "John",
				LastName:  "Doe",
				Email:     "john.doe@example.com",
			},
			Note: "Bring ID for entry",
		},
		{
			PartitionKey: "partition2",
			RowKey:       uuid.New(),
			Date:         time.Date(2024, 11, 26, 15, 30, 59, 0, time.UTC),
			Address:      "Van Zeggelenstaraat, 47",
			Venue:        "Community Hall",
			Description:  "Charity Event",
			Money:        1000.50,
			Status:       models.StatusPlanned,
			Person: models.Person{
				FirstName: "Mark",
				LastName:  "Gotze",
				Email:     "mark.gotze@example.com",
			},
			Note: "Keep your phone close",
		},
	}

	// Mock the GetAll response
	mockService.On("GetAll", mock.Anything, &startTime, &endTime).Return(mockEvents, nil)

	req := httptest.NewRequest(http.MethodGet, "/events?startDate="+startTime.Format(time.RFC3339)+"&endDate="+endTime.Format(time.RFC3339), nil)
	w := httptest.NewRecorder()

	handler.GetEvents(w, req)

	// Validate response
	assert.Equal(t, http.StatusOK, w.Code)
	var response []models.Event
	err := json.NewDecoder(w.Body).Decode(&response)
	assert.NoError(t, err)
	assert.Equal(t, mockEvents, response)

	mockService.AssertExpectations(t)
}

func TestGetEventByID(t *testing.T) {
	mockService := new(mocks.MockEventService)
	handler := handlers.NewEventHandler(mockService)

	event := &models.Event{
		PartitionKey: "partition1",
		RowKey:       uuid.New(),
		Date:         time.Date(2024, 11, 26, 15, 30, 59, 0, time.UTC),
		Address:      "Grotestraat, 122",
		Venue:        "Christmas Event",
		Description:  "Charity Event",
		Money:        100.50,
		Status:       models.StatusPlanned,
		Person: models.Person{
			FirstName: "John",
			LastName:  "Doe",
			Email:     "john.doe@example.com",
		},
		Note: "Bring ID for entry",
	}

	// Mock the GetByID response
	mockService.On("GetByID", mock.Anything, "partitionKey", "rowKey").Return(event, nil)

	req := httptest.NewRequest(http.MethodGet, "/events/partitionKey/rowKey", nil)
	req = mux.SetURLVars(req, map[string]string{"partitionKey": "partitionKey", "rowKey": "rowKey"})
	w := httptest.NewRecorder()

	handler.GetEventByID(w, req)

	// Validate response
	assert.Equal(t, http.StatusOK, w.Code)
	var response models.Event
	err := json.NewDecoder(w.Body).Decode(&response)
	assert.NoError(t, err)
	assert.Equal(t, *event, response)

	mockService.AssertExpectations(t)
}

func TestCreateEvent(t *testing.T) {
	mockService := new(mocks.MockEventService)
	handler := handlers.NewEventHandler(mockService)

	event := &models.Event{
		PartitionKey: "partition1",
		RowKey:       uuid.New(),
		Date:         time.Date(2024, 11, 26, 15, 30, 59, 0, time.UTC),
		Address:      "Grotestraat, 122",
		Venue:        "Christmas Event",
		Description:  "Charity Event",
		Money:        100.50,
		Status:       models.StatusPlanned,
		Person: models.Person{
			FirstName: "John",
			LastName:  "Doe",
			Email:     "john.doe@example.com",
		},
		Note: "Bring ID for entry",
	}

	// Mock the Create response
	mockService.On("Create", mock.Anything, mock.AnythingOfType("models.Event")).Return(nil)

	body, _ := json.Marshal(event)
	req := httptest.NewRequest(http.MethodPost, "/events", bytes.NewReader(body))
	req.Header.Set("Content-Type", "application/json")
	w := httptest.NewRecorder()

	handler.CreateEvent(w, req)

	// Validate Response
	assert.Equal(t, http.StatusCreated, w.Code)
	var response map[string]string
	err := json.NewDecoder(w.Body).Decode(&response)
	assert.NoError(t, err)
	assert.Equal(t, "Event created successfully", response["message"])

	mockService.AssertExpectations(t)
}

func TestCreateEvent_Error(t *testing.T) {
	mockService := new(mocks.MockEventService)
	handler := handlers.NewEventHandler(mockService)

	event := &models.Event{
		PartitionKey: "partition1",
		RowKey:       uuid.New(),
		Date:         time.Date(2024, 11, 26, 15, 30, 59, 0, time.UTC),
		Address:      "Grotestraat, 122",
		Venue:        "Christmas Event",
		Description:  "Charity Event",
		Money:        100.50,
		Status:       models.StatusPlanned,
		Person: models.Person{
			FirstName: "John",
			LastName:  "Doe",
			Email:     "john.doe@example.com",
		},
		Note: "Bring ID for entry",
	}

	// Mock the Create response
	mockService.On("Create", mock.Anything, mock.AnythingOfType("models.Event")).Return(errors.New("service error"))

	body, _ := json.Marshal(event)
	req := httptest.NewRequest(http.MethodPost, "/events", bytes.NewReader(body))
	req.Header.Set("Content-Type", "application/json")
	w := httptest.NewRecorder()

	handler.CreateEvent(w, req)

	// Validate response
	assert.Equal(t, http.StatusInternalServerError, w.Code)
	mockService.AssertExpectations(t)
}
