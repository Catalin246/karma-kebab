package routes

import (
	"availability-service/handlers"
	"availability-service/repository"
	"availability-service/service"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/gin-gonic/gin"
)

func RegisterRouter(serviceClient *aztables.ServiceClient) *gin.Engine {
	// Create the repository and service instances
	availabilityRepository := repository.NewTableStorageAvailabilityRepository(serviceClient)
	availabilityService := service.NewAvailabilityService(availabilityRepository)

	// Create the availability handler and inject the service
	availabilityHandler := handlers.NewAvailabilityHandler(availabilityService)

	// Create the Gin router
	router := gin.Default()

	// Health check route
	router.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{"status": "ok"})
	})

	// API v1 group
	v1 := router.Group("/api/v1")
	{
		// Availability routes
		availability := v1.Group("/availability")
		{
			availability.GET("", availabilityHandler.GetAll)
			availability.GET("/:id", availabilityHandler.GetByID)
			availability.POST("", availabilityHandler.Create)
			availability.PUT("/:id", availabilityHandler.Update)
			availability.DELETE("/:id", availabilityHandler.Delete)
		}
	}

	return router
}
