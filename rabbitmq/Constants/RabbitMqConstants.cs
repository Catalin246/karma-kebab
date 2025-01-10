namespace rabbitmq.Constants
{
    public static class RabbitMqConstants
    {
        // Global exchange name
        public const string ExchangeName = "karma-kebab-exchange";

        // Queue names for each microservice
        public static class Queues
        {
            public const string Availability = "availability-queue";
            public const string Event = "event-queue";
            public const string Employee = "employee-queue";
            public const string Shift = "shift-queue";
            public const string Truck = "truck-queue";
            public const string Duty = "duty-queue";
        }

        // Routing keys for different events - THESE ARE THE GENERAL crud things - STILL NEED TO BE ADJUSTED TO SPECIFICS
        public static class RoutingKeys
        {
            // Availability service routing keys
            public const string AvailabilityCreated = "availability.created";
            public const string AvailabilityUpdated = "availability.updated";
            public const string AvailabilityDeleted = "availability.deleted";

            // Event service routing keys
            // when event created, it should create a list of shifts, get a truck id
            // when event deleted, shifts deleted, truck availabile again ... duties?
            //when event updated, shift info should also be updated
            public const string EventCreated = "event.created";
            public const string EventUpdated = "event.updated";
            public const string EventDeleted = "event.deleted";

            // Employee service routing keys
            // when employee deleted, and shift of that employee should be deleted
            public const string EmployeeCreated = "employee.created";
            public const string EmployeeUpdated = "employee.updated";
            public const string EmployeeDeleted = "employee.deleted";

            // Shift service routing keys
            //
            public const string ShiftCreated = "shift.created";
            public const string ShiftUpdated = "shift.updated";
            public const string ShiftDeleted = "shift.deleted";

            // Truck service routing keys
            public const string TruckCreated = "truck.created";
            public const string TruckUpdated = "truck.updated";
            public const string TruckDeleted = "truck.deleted";

            // Duty service routing keys
            public const string DutyCreated = "duty.created";
            public const string DutyUpdated = "duty.updated";
            public const string DutyDeleted = "duty.deleted";
        }
    }
}