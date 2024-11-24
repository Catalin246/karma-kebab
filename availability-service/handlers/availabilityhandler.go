// The handler includes Swagger documentation comments that you can use with swaggo/swag to generate API documentation
package handlers
import (
    "net/http"
    "github.com/gin-gonic/gin"
    "availability-service/models"
	"availability-service/models/error.go"
    "availability-service/service"
)

type AvailabilityHandler struct {
    service *service.AvailabilityService
}

func NewAvailabilityHandler(service *service.AvailabilityService) *AvailabilityHandler {
    return &AvailabilityHandler{
        service: service,
    }
}

// Request/Response structures
type CreateAvailabilityRequest struct {
    EmployeeID string    `json:"employeeId" binding:"required"`
    StartDate  string    `json:"startDate" binding:"required"`
    EndDate    string    `json:"endDate" binding:"required"`
}

type UpdateAvailabilityRequest struct {
    EmployeeID string    `json:"employeeId" binding:"required"`
    StartDate  string    `json:"startDate" binding:"required"`
    EndDate    string    `json:"endDate" binding:"required"`
}

type AvailabilityResponse struct {
    ID         string    `json:"id"`
    EmployeeID string    `json:"employeeId"`
    StartDate  string    `json:"startDate"`
    EndDate    string    `json:"endDate"`
}

// GetAll godoc
// @Summary Get all availabilities
// @Description Get all availability records
// @Tags availability
// @Accept json
// @Produce json
// @Success 200 {array} AvailabilityResponse
// @Router /api/v1/availability [get]
func (h *AvailabilityHandler) GetAll(c *gin.Context) {
    availabilities, err := h.service.GetAll(c.Request.Context())
    if err != nil {
        c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
        return
    }

    c.JSON(http.StatusOK, availabilities)
}

// GetByID godoc
// @Summary Get availability by ID
// @Description Get a single availability record by ID
// @Tags availability
// @Accept json
// @Produce json
// @Param id path string true "Availability ID"
// @Success 200 {object} AvailabilityResponse
// @Router /api/v1/availability/{id} [get]
func (h *AvailabilityHandler) GetByID(c *gin.Context) {
    id := c.Param("id")
    availability, err := h.service.GetByID(c.Request.Context(), id)
    if err != nil {
        if err == errors.ErrNotFound {
            c.JSON(http.StatusNotFound, gin.H{"error": "Availability not found"})
            return
        }
        c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
        return
    }

    c.JSON(http.StatusOK, availability)
}

// Create godoc
// @Summary Create availability
// @Description Create a new availability record
// @Tags availability
// @Accept json
// @Produce json
// @Param availability body CreateAvailabilityRequest true "Availability Info"
// @Success 201 {object} AvailabilityResponse
// @Router /api/v1/availability [post]
func (h *AvailabilityHandler) Create(c *gin.Context) {
    var req CreateAvailabilityRequest
    if err := c.ShouldBindJSON(&req); err != nil {
        c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
        return
    }

    // Parse dates
    startDate, err := time.Parse(time.RFC3339, req.StartDate)
    if err != nil {
        c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid start date format"})
        return
    }

    endDate, err := time.Parse(time.RFC3339, req.EndDate)
    if err != nil {
        c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid end date format"})
        return
    }

    availability := models.Availability{
        EmployeeID: req.EmployeeID,
        StartDate:  startDate,
        EndDate:    endDate,
    }

    created, err := h.service.Create(c.Request.Context(), availability)
    if err != nil {
        switch err {
        case errors.ErrInvalidAvailability:
            c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
        case errors.ErrConflict:
            c.JSON(http.StatusConflict, gin.H{"error": err.Error()})
        default:
            c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
        }
        return
    }

    c.JSON(http.StatusCreated, created)
}

// Update godoc
// @Summary Update availability
// @Description Update an existing availability record
// @Tags availability
// @Accept json
// @Produce json
// @Param id path string true "Availability ID"
// @Param availability body UpdateAvailabilityRequest true "Availability Info"
// @Success 200 {object} AvailabilityResponse
// @Router /api/v1/availability/{id} [put]
func (h *AvailabilityHandler) Update(c *gin.Context) {
    id := c.Param("id")
    var req UpdateAvailabilityRequest
    if err := c.ShouldBindJSON(&req); err != nil {
        c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
        return
    }

    // Parse dates
    startDate, err := time.Parse(time.RFC3339, req.StartDate)
    if err != nil {
        c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid start date format"})
        return
    }

    endDate, err := time.Parse(time.RFC3339, req.EndDate)
    if err != nil {
        c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid end date format"})
        return
    }

    availability := models.Availability{
        ID:         id,
        EmployeeID: req.EmployeeID,
        StartDate:  startDate,
        EndDate:    endDate,
    }

    err = h.service.Update(c.Request.Context(), id, availability)
    if err != nil {
        switch err {
        case errors.ErrNotFound:
            c.JSON(http.StatusNotFound, gin.H{"error": "Availability not found"})
        case errors.ErrInvalidAvailability:
            c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
        default:
            c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
        }
        return
    }

    c.JSON(http.StatusOK, availability)
}

// Delete godoc
// @Summary Delete availability
// @Description Delete an availability record
// @Tags availability
// @Accept json
// @Produce json
// @Param id path string true "Availability ID"
// @Success 204 "No Content"
// @Router /api/v1/availability/{id} [delete]
func (h *AvailabilityHandler) Delete(c *gin.Context) {
    id := c.Param("id")
    err := h.service.Delete(c.Request.Context(), id)
    if err != nil {
        switch err {
        case errors.ErrNotFound:
            c.JSON(http.StatusNotFound, gin.H{"error": "Availability not found"})
        default:
            c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
        }
        return
    }

    c.Status(http.StatusNoContent)
}