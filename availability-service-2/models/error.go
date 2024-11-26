package models

import "errors"

var (
	ErrNotFound            = errors.New("resource not found")
	ErrInvalidID           = errors.New("invalid id")
	ErrInvalidAvailability = errors.New("invalid availability data :(")
	ErrDatabaseOperation   = errors.New("database operation failed")
	ErrConflict            = errors.New("resource already exists")
)
