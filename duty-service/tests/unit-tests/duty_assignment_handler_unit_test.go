package unit_tests

import (
	"bytes"
	"context"
	"duty-service/handlers"
	"duty-service/models"
	"duty-service/tests/mocks"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/google/uuid"
	"github.com/gorilla/mux"
	"github.com/stretchr/testify/mock"
	"github.com/stretchr/testify/require"
)

func TestGetAllDutyAssignmentsByShiftId_Success(t *testing.T) {
	mockService := new(mocks.MockDutyAssignmentService)
	handler := handlers.NewDutyAssignmentHandler(mockService)

	// Dummy data
	mockDutyAssignments := []models.DutyAssignment{
		{
			PartitionKey:           uuid.MustParse("5b79da92-f140-45f8-ad71-6701729ae4a1"),
			RowKey:                 uuid.MustParse("ad032a49-57ab-4ba8-975b-8f0fefb11c4f"),
			DutyAssignmentStatus:   "Completed",
			DutyAssignmentImageUrl: nil,
			DutyAssignmentNote:     nil,
		},
		{
			PartitionKey:           uuid.MustParse("5b79da92-f140-45f8-ad71-6701729ae4a2"),
			RowKey:                 uuid.MustParse("ad032a49-57ab-4ba8-975b-8f0fefb11c4d"),
			DutyAssignmentStatus:   "Incomplete",
			DutyAssignmentImageUrl: nil,
			DutyAssignmentNote:     nil,
		},
	}

	mockService.On("GetAllDutyAssignmentsByShiftId", context.Background(), mock.AnythingOfType("uuid.UUID")).Return(mockDutyAssignments, nil)

	req := httptest.NewRequest(http.MethodGet, "/duty-assignments?shiftId=d9b2d63d-bbf7-4f2f-9d7c-0e67f060d8b0", nil)
	rec := httptest.NewRecorder()

	handler.GetAllDutyAssignmentsByShiftId(rec, req)

	require.Equal(t, http.StatusOK, rec.Result().StatusCode)
	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

	var response []models.DutyAssignment
	err := json.NewDecoder(rec.Body).Decode(&response)
	require.NoError(t, err)

	require.Equal(t, mockDutyAssignments, response)

	mockService.AssertExpectations(t)
}

func TestCreateDutyAssignments_Success(t *testing.T) {
	mockService := new(mocks.MockDutyAssignmentService)
	handler := handlers.NewDutyAssignmentHandler(mockService)

	// Dummy data
	shiftId := uuid.MustParse("d9b2d63d-bbf7-4f2f-9d7c-0e67f060d8b0")
	roleId := uuid.MustParse("e9b2d63d-bbf7-4f2f-9d7c-0e67f060d8b1")

	mockService.On("CreateDutyAssignments", context.Background(), shiftId, roleId).Return(nil)

	requestBody := map[string]string{
		"ShiftId": shiftId.String(),
		"RoleId":  roleId.String(),
	}
	body, err := json.Marshal(requestBody)
	require.NoError(t, err)

	req := httptest.NewRequest(http.MethodPost, "/duty-assignments", bytes.NewReader(body))
	rec := httptest.NewRecorder()

	handler.CreateDutyAssignments(rec, req)

	require.Equal(t, http.StatusCreated, rec.Result().StatusCode)
	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

	var response map[string]string
	err = json.NewDecoder(rec.Body).Decode(&response)
	require.NoError(t, err)
	require.Equal(t, map[string]string{"message": "Duty assignments created successfully"}, response)

	mockService.AssertExpectations(t)
}

func TestUpdateDutyAssignment_Success(t *testing.T) {
	mockService := new(mocks.MockDutyAssignmentService)
	handler := handlers.NewDutyAssignmentHandler(mockService)

	// mock DutyAssignment
	updatedDutyAssignment := models.DutyAssignment{
		PartitionKey:           uuid.New(), //  ShiftId
		RowKey:                 uuid.New(), //  DutyId
		DutyAssignmentStatus:   models.StatusCompleted,
		DutyAssignmentImageUrl: nil,
		DutyAssignmentNote:     nil,
	}

	body, _ := json.Marshal(updatedDutyAssignment)

	mockService.On("UpdateDutyAssignment", mock.Anything, updatedDutyAssignment).Return(nil)

	req := httptest.NewRequest(http.MethodPut, "/duty-assignments/"+updatedDutyAssignment.PartitionKey.String()+"/"+updatedDutyAssignment.RowKey.String(), bytes.NewReader(body))
	req = mux.SetURLVars(req, map[string]string{
		"ShiftId": updatedDutyAssignment.PartitionKey.String(),
		"DutyId":  updatedDutyAssignment.RowKey.String(),
	})
	req.Header.Set("Content-Type", "application/json")

	rec := httptest.NewRecorder()

	handler.UpdateDutyAssignment(rec, req)

	require.Equal(t, http.StatusOK, rec.Result().StatusCode)

	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

	var response map[string]string
	err := json.NewDecoder(rec.Body).Decode(&response)
	require.NoError(t, err)

	require.Equal(t, "Duty assignment updated successfully", response["message"])

	mockService.AssertExpectations(t)
}

func TestDeleteDutyAssignment_Success(t *testing.T) {
	mockService := new(mocks.MockDutyAssignmentService)
	handler := handlers.NewDutyAssignmentHandler(mockService)

	shiftId := uuid.New()
	dutyId := uuid.New()

	mockService.On("DeleteDutyAssignment", mock.Anything, shiftId, dutyId).Return(nil)

	req := httptest.NewRequest(http.MethodDelete, "/duty-assignments/"+shiftId.String()+"/"+dutyId.String(), nil)
	req = mux.SetURLVars(req, map[string]string{
		"ShiftId": shiftId.String(),
		"DutyId":  dutyId.String(),
	})
	req.Header.Set("Content-Type", "application/json")

	rec := httptest.NewRecorder()

	handler.DeleteDutyAssignment(rec, req)

	require.Equal(t, http.StatusOK, rec.Result().StatusCode)

	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

	var response map[string]string
	err := json.NewDecoder(rec.Body).Decode(&response)
	require.NoError(t, err)

	require.Equal(t, "Duty assignment deleted successfully", response["message"])

	mockService.AssertExpectations(t)
}
