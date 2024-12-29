package unit_tests

import (
	"bytes"
	"context"
	"duty-service/handlers"
	"duty-service/models"
	"duty-service/tests/mocks"
	"encoding/json"
	"mime/multipart"
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

	// dummy data
	shiftId := uuid.MustParse("d9b2d63d-bbf7-4f2f-9d7c-0e67f060d8b0")
	roleId := 1

	mockService.On("CreateDutyAssignments", context.Background(), shiftId, roleId).Return(nil)

	requestBody := map[string]interface{}{
		"ShiftId": shiftId.String(),
		"RoleId":  roleId,
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

	//dummy id data as PK and RK
	partitionKey := uuid.New()
	rowKey := uuid.New()

	//setup the dutyassignment
	dutyAssignmentStatus := models.StatusCompleted
	dutyAssignment := models.DutyAssignment{
		PartitionKey:         partitionKey,
		RowKey:               rowKey,
		DutyAssignmentStatus: dutyAssignmentStatus,
	}

	// creating a buffer to hold the multipart form data
	body := &bytes.Buffer{}
	writer := multipart.NewWriter(body)

	// adding required fields to the form data
	_ = writer.WriteField("PartitionKey", partitionKey.String())
	_ = writer.WriteField("RowKey", rowKey.String())
	_ = writer.WriteField("DutyAssignmentStatus", string(dutyAssignmentStatus))

	writer.Close() // closing the writer to finalize the multipart form

	mockService.On("UpdateDutyAssignment", mock.Anything, dutyAssignment, mock.Anything).Return(nil)

	// PUT request with the multipart form data
	req := httptest.NewRequest(http.MethodPut, "/duty-assignments/"+partitionKey.String()+"/"+rowKey.String(), body)
	req = mux.SetURLVars(req, map[string]string{
		"ShiftId": partitionKey.String(),
		"DutyId":  rowKey.String(),
	})
	req.Header.Set("Content-Type", writer.FormDataContentType())

	rec := httptest.NewRecorder()

	handler.UpdateDutyAssignment(rec, req)

	require.Equal(t, http.StatusOK, rec.Result().StatusCode) // asseritng the status code is 200 OK
	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

	// verify response body
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
