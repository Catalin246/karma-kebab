package http

import (
	"availability-service/handlers"
	"availability-service/service"

	"github.com/gin-gonic/gin"
)

func SetupRouter(availabilityService *service.AvailabilityService) *gin.Engine {
	router := gin.Default()

	// Health check
	router.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{"status": "ok"})
	})

	// API v1 group
	v1 := router.Group("/api/v1")
	{
		// Availability routes
		availability := v1.Group("/availability")
		{
			handler := handlers.NewAvailabilityHandler(availabilityService)
			availability.GET("", handler.GetAll)
			availability.GET("/:id", handler.GetByID)
			availability.POST("", handler.Create)
			availability.PUT("/:id", handler.Update)
			availability.DELETE("/:id", handler.Delete)
		}
	}

	return router
}
