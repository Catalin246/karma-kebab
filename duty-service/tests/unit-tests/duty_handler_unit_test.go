package unit_tests

import (
	"context"
	"duty-service/handlers"
	"duty-service/models"
	"duty-service/tests/mocks"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/require"
)

func TestGetAllDuties(t *testing.T) {
	// Create a mock service
	mockService := new(mocks.MockDutyService)

	// Create the handler with the mock service
	handler := handlers.NewDutyHandler(mockService)

	// Prepare mock data
	mockDuties := []models.Duty{
		{
			PartitionKey:    "TestPartitionKey",
			RowKey:          uuid.MustParse("123e4567-e89b-12d3-a456-426614174000"), // Sample UUID
			RoleId:          uuid.MustParse("123e4567-e89b-12d3-a456-426614174001"), // Sample UUID
			DutyName:        "Test Duty 1",
			DutyDescription: "Description for Test Duty 1",
		},
		{
			PartitionKey:    "TestPartitionKey",
			RowKey:          uuid.MustParse("123e4567-e89b-12d3-a456-426614174002"), // Sample UUID
			RoleId:          uuid.MustParse("123e4567-e89b-12d3-a456-426614174003"), // Sample UUID
			DutyName:        "Test Duty 2",
			DutyDescription: "Description for Test Duty 2",
		},
	}

	mockService.On("GetAllDuties", context.Background(), "TestName").Return(mockDuties, nil)

	// Create a test HTTP request
	req := httptest.NewRequest(http.MethodGet, "/duties?name=TestName", nil)
	rec := httptest.NewRecorder()

	// Call the handler
	handler.GetAllDuties(rec, req)

	// Assert the response
	require.Equal(t, http.StatusOK, rec.Result().StatusCode)
	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

	// Parse the response body
	var response []models.Duty
	err := json.NewDecoder(rec.Body).Decode(&response)
	require.NoError(t, err)
	require.Equal(t, mockDuties, response)

	// Ensure the mock expectations were met
	mockService.AssertExpectations(t)
}
