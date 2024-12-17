// package unit_tests

// import (
// 	"bytes"
// 	"context"
// 	"duty-service/handlers"
// 	"duty-service/models"
// 	"duty-service/tests/mocks"
// 	"encoding/json"
// 	"errors"
// 	"net/http"
// 	"net/http/httptest"
// 	"testing"

// 	"github.com/google/uuid"
// 	"github.com/gorilla/mux"
// 	"github.com/stretchr/testify/mock"
// 	"github.com/stretchr/testify/require"
// )

// // SUCCESS CASES:
// func TestGetAllDuties(t *testing.T) {
// 	mockService := new(mocks.MockDutyService)
// 	handler := handlers.NewDutyHandler(mockService)

// 	// dummy data
// 	mockDuties := []models.Duty{
// 		{
// 			PartitionKey:    "TestPartitionKey",
// 			RowKey:          uuid.MustParse("123e4567-e89b-12d3-a456-426614174000"), // Sample UUID
// 			RoleId:          uuid.MustParse("123e4567-e89b-12d3-a456-426614174001"), // Sample UUID
// 			DutyName:        "Test Duty 1",
// 			DutyDescription: "Description for Test Duty 1",
// 		},
// 		{
// 			PartitionKey:    "TestPartitionKey",
// 			RowKey:          uuid.MustParse("123e4567-e89b-12d3-a456-426614174002"), // Sample UUID
// 			RoleId:          uuid.MustParse("123e4567-e89b-12d3-a456-426614174003"), // Sample UUID
// 			DutyName:        "Test Duty 2",
// 			DutyDescription: "Description for Test Duty 2",
// 		},
// 	}

// 	mockService.On("GetAllDuties", context.Background(), "TestName").Return(mockDuties, nil)

// 	req := httptest.NewRequest(http.MethodGet, "/duties?name=TestName", nil)
// 	rec := httptest.NewRecorder()

// 	handler.GetAllDuties(rec, req)

// 	require.Equal(t, http.StatusOK, rec.Result().StatusCode)
// 	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

// 	// parse the response
// 	var response []models.Duty
// 	err := json.NewDecoder(rec.Body).Decode(&response)
// 	require.NoError(t, err)
// 	require.Equal(t, mockDuties, response)

// 	mockService.AssertExpectations(t)
// }

// func TestGetDutiesByRole_Success(t *testing.T) {
// 	mockService := new(mocks.MockDutyService)
// 	handler := handlers.NewDutyHandler(mockService)

// 	// dummy data
// 	roleId := uuid.MustParse("123e4567-e89b-12d3-a456-426614174000")
// 	mockDuties := []models.Duty{
// 		{
// 			PartitionKey:    "Partition1",
// 			RowKey:          uuid.New(),
// 			RoleId:          roleId,
// 			DutyName:        "Duty 1",
// 			DutyDescription: "Desc 1",
// 		},
// 		{
// 			PartitionKey:    "Partition2",
// 			RowKey:          uuid.New(),
// 			RoleId:          roleId,
// 			DutyName:        "Duty 2",
// 			DutyDescription: "Desc 2",
// 		},
// 	}

// 	mockService.On("GetDutiesByRole", mock.Anything, roleId).Return(mockDuties, nil)

// 	req := httptest.NewRequest(http.MethodGet, "/duties?RoleId=123e4567-e89b-12d3-a456-426614174000", nil)
// 	rec := httptest.NewRecorder()

// 	handler.GetDutiesByRole(rec, req)

// 	require.Equal(t, http.StatusOK, rec.Result().StatusCode)
// 	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

// 	// parse the response body
// 	var response []models.Duty
// 	err := json.NewDecoder(rec.Body).Decode(&response)
// 	require.NoError(t, err)
// 	require.Equal(t, mockDuties, response)

// 	mockService.AssertExpectations(t)
// }

// func TestGetDutyById(t *testing.T) {
// 	mockService := new(mocks.MockDutyService)
// 	handler := handlers.NewDutyHandler(mockService)

// 	// dummy data
// 	mockDuty := models.Duty{
// 		PartitionKey:    "TestPartitionKey",
// 		RowKey:          uuid.MustParse("123e4567-e89b-12d3-a456-426614174000"),
// 		RoleId:          uuid.MustParse("123e4567-e89b-12d3-a456-426614174001"),
// 		DutyName:        "Test Duty",
// 		DutyDescription: "Description for Test Duty",
// 	}

// 	mockService.On("GetDutyById", context.Background(), "TestPartitionKey", "123e4567-e89b-12d3-a456-426614174000").Return(&mockDuty, nil)

// 	req := httptest.NewRequest(http.MethodGet, "/duties/TestPartitionKey/123e4567-e89b-12d3-a456-426614174000", nil)
// 	req = mux.SetURLVars(req, map[string]string{
// 		"PartitionKey": "TestPartitionKey",
// 		"RowKey":       "123e4567-e89b-12d3-a456-426614174000",
// 	})
// 	rec := httptest.NewRecorder()

// 	handler.GetDutyById(rec, req)

// 	require.Equal(t, http.StatusOK, rec.Result().StatusCode)
// 	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

// 	var response models.Duty
// 	err := json.NewDecoder(rec.Body).Decode(&response)
// 	require.NoError(t, err)
// 	require.Equal(t, mockDuty, response)

// 	mockService.AssertExpectations(t)
// }

// func TestCreateDuty_Success(t *testing.T) {
// 	mockService := new(mocks.MockDutyService)

// 	handler := handlers.NewDutyHandler(mockService)

// 	// dummy duty and JSON representation
// 	newDuty := models.Duty{
// 		PartitionKey:    "Duty",
// 		RowKey:          uuid.New(),
// 		RoleId:          uuid.New(),
// 		DutyName:        "Test Duty",
// 		DutyDescription: "Test Description",
// 	}
// 	body, _ := json.Marshal(newDuty)
// 	mockService.On("CreateDuty", mock.Anything, mock.AnythingOfType("models.Duty")).Return(nil)

// 	req := httptest.NewRequest(http.MethodPost, "/duties", bytes.NewReader(body))
// 	req.Header.Set("Content-Type", "application/json")
// 	rec := httptest.NewRecorder()

// 	handler.CreateDuty(rec, req)

// 	require.Equal(t, http.StatusCreated, rec.Result().StatusCode)
// 	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

// 	var response map[string]string
// 	err := json.NewDecoder(rec.Body).Decode(&response)
// 	require.NoError(t, err)
// 	require.Equal(t, "duty created successfully", response["message"])

// 	mockService.AssertExpectations(t)
// }

// func TestUpdateDuty_Success(t *testing.T) {
// 	mockService := new(mocks.MockDutyService)
// 	handler := handlers.NewDutyHandler(mockService)

// 	updatedDuty := models.Duty{
// 		PartitionKey:    "TestPartitionKey",
// 		RowKey:          uuid.New(),
// 		RoleId:          uuid.New(),
// 		DutyName:        "Updated Duty",
// 		DutyDescription: "Updated Description",
// 	}

// 	body, _ := json.Marshal(updatedDuty)

// 	mockService.On("UpdateDuty", mock.Anything, "TestPartitionKey", "TestRowKey", updatedDuty).Return(nil)

// 	req := httptest.NewRequest(http.MethodPut, "/duties/TestPartitionKey/TestRowKey", bytes.NewReader(body))
// 	req = mux.SetURLVars(req, map[string]string{
// 		"PartitionKey": "TestPartitionKey",
// 		"RowKey":       "TestRowKey",
// 	})
// 	req.Header.Set("Content-Type", "application/json")
// 	rec := httptest.NewRecorder()

// 	handler.UpdateDuty(rec, req)

// 	require.Equal(t, http.StatusOK, rec.Result().StatusCode)
// 	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

// 	var response map[string]string
// 	err := json.NewDecoder(rec.Body).Decode(&response)
// 	require.NoError(t, err)
// 	require.Equal(t, "Duty updated successfully", response["message"])

// 	mockService.AssertExpectations(t)
// }

// func TestDeleteDuty_Success(t *testing.T) {
// 	mockService := new(mocks.MockDutyService)
// 	handler := handlers.NewDutyHandler(mockService)

// 	mockService.On("DeleteDuty", mock.Anything, "TestPartitionKey", "TestRowKey").
// 		Return(nil)

// 	req := httptest.NewRequest(http.MethodDelete, "/duties/TestPartitionKey/TestRowKey", nil)
// 	req = mux.SetURLVars(req, map[string]string{
// 		"PartitionKey": "TestPartitionKey",
// 		"RowKey":       "TestRowKey",
// 	})
// 	rec := httptest.NewRecorder()

// 	handler.DeleteDuty(rec, req)

// 	require.Equal(t, http.StatusOK, rec.Result().StatusCode)
// 	require.Equal(t, "application/json", rec.Result().Header.Get("Content-Type"))

// 	var response map[string]string
// 	err := json.NewDecoder(rec.Body).Decode(&response)
// 	require.NoError(t, err)
// 	require.Equal(t, "Duty deleted successfully", response["message"])

// 	mockService.AssertExpectations(t)
// }

// // FAILURE CASES:
// func TestGetDutyById_MissingKeys(t *testing.T) {
// 	mockService := new(mocks.MockDutyService)
// 	handler := handlers.NewDutyHandler(mockService)

// 	// http request with missing keys
// 	req := httptest.NewRequest(http.MethodGet, "/duties//", nil)
// 	req = mux.SetURLVars(req, map[string]string{
// 		"PartitionKey": "",
// 		"RowKey":       "",
// 	})
// 	rec := httptest.NewRecorder()

// 	handler.GetDutyById(rec, req)

// 	require.Equal(t, http.StatusBadRequest, rec.Result().StatusCode)
// 	require.Equal(t, "text/plain; charset=utf-8", rec.Result().Header.Get("Content-Type"))
// 	require.Equal(t, "Missing PartitionKey or RrowKey\n", rec.Body.String())
// }

// func TestGetDutiesByRole_InvalidRoleId(t *testing.T) {
// 	mockService := new(mocks.MockDutyService)
// 	handler := handlers.NewDutyHandler(mockService)

// 	//  http request with invalid RoleId
// 	req := httptest.NewRequest(http.MethodGet, "/duties?RoleId=invalid-uuid", nil)
// 	rec := httptest.NewRecorder()

// 	handler.GetDutiesByRole(rec, req)

// 	require.Equal(t, http.StatusBadRequest, rec.Result().StatusCode)
// 	require.Equal(t, "text/plain; charset=utf-8", rec.Result().Header.Get("Content-Type"))
// 	require.Contains(t, rec.Body.String(), "Invalid 'RoleId' format")
// }

// func TestCreateDuty_InvalidBody(t *testing.T) {
// 	mockService := new(mocks.MockDutyService)

// 	handler := handlers.NewDutyHandler(mockService)

// 	// http request with an invalid JSON body
// 	req := httptest.NewRequest(http.MethodPost, "/duties", bytes.NewReader([]byte("{invalid json")))
// 	req.Header.Set("Content-Type", "application/json")
// 	rec := httptest.NewRecorder()

// 	handler.CreateDuty(rec, req)

// 	require.Equal(t, http.StatusBadRequest, rec.Result().StatusCode)
// 	require.Equal(t, "text/plain; charset=utf-8", rec.Result().Header.Get("Content-Type"))
// 	require.Equal(t, "Invalid request body\n", rec.Body.String())
// }

// func TestCreateDuty_ServiceError(t *testing.T) {
// 	mockService := new(mocks.MockDutyService)
// 	handler := handlers.NewDutyHandler(mockService)

// 	// du,mmy duty
// 	newDuty := models.Duty{
// 		PartitionKey:    "Duty",
// 		RoleId:          uuid.New(),
// 		DutyName:        "Test Duty",
// 		DutyDescription: "Test Description",
// 	}
// 	body, _ := json.Marshal(newDuty)

// 	mockService.On("CreateDuty", mock.Anything, mock.AnythingOfType("models.Duty")).Return(errors.New("database error"))

// 	req := httptest.NewRequest(http.MethodPost, "/duties", bytes.NewReader(body))
// 	req.Header.Set("Content-Type", "application/json")
// 	rec := httptest.NewRecorder()

// 	handler.CreateDuty(rec, req)

// 	require.Equal(t, http.StatusInternalServerError, rec.Result().StatusCode)
// 	require.Equal(t, "text/plain; charset=utf-8", rec.Result().Header.Get("Content-Type"))
// 	require.Equal(t, "Failed to create duty: database error\n", rec.Body.String())

// 	mockService.AssertExpectations(t)
// }
