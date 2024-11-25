package services

import (
	"context"
	"event-service/models"
	"event-service/repositories"
	"time"
)

type EventService struct {
	repo repositories.EventRepositoryInterface
}

// NewEventService creates a new EventService
func NewEventService(repo repositories.EventRepositoryInterface) *EventService {
	return &EventService{repo: repo}
}

func (s *EventService) Create(ctx context.Context, event models.Event) error {
	return s.repo.Create(ctx, event)
}

func (s *EventService) GetByID(ctx context.Context, partitionKey, rowKey string) (*models.Event, error) {
	return s.repo.GetByID(ctx, partitionKey, rowKey)
}

func (s *EventService) GetAll(ctx context.Context, startDate, endDate *time.Time) ([]models.Event, error) {
	var filter string

	if startDate != nil {
		filter = "Date ge datetime'" + startDate.Format(time.RFC3339) + "'"
	}
	if endDate != nil {
		if filter != "" {
			filter += " and Date le datetime'" + endDate.Format(time.RFC3339) + "'"
		} else {
			filter = "Date le datetime'" + endDate.Format(time.RFC3339) + "'"
		}
	}

	return s.repo.GetAll(ctx, filter)
}

func (s *EventService) Update(ctx context.Context, partitionKey, rowKey string, event models.Event) error {
	return s.repo.Update(ctx, partitionKey, rowKey, event)
}

func (s *EventService) Delete(ctx context.Context, partitionKey, rowKey string) error {
	return s.repo.Delete(ctx, partitionKey, rowKey)
}
