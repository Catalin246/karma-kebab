# Availability Service

This microservice manages employee (un)availability for events in the Karma Kebab application.
each record will be a record of when an employee is NOT available


### for local testing
   npm install -g azurite
   azurite

**Build the Docker Image**:

   docker compose up

Partition key = employeeID
rowkey = id
methods: 
  r.HandleFunc("/availability", availabilityHandler.GetAll).Methods(http.MethodGet)
	r.HandleFunc("/availability/{partitionKey}", availabilityHandler.GetByEmployeeID).Methods(http.MethodGet)
	r.HandleFunc("/availability", availabilityHandler.Create).Methods(http.MethodPost)
	r.HandleFunc("/availability/{partitionKey}/{rowKey}", availabilityHandler.Update).Methods(http.MethodPut)
	r.HandleFunc("/availability/{partitionKey}/{rowKey}", availabilityHandler.Delete).Methods(http.MethodDelete)



   Querying the getall with specific dates: 

1. Basic full date range query:
```
http://localhost:3002/availability?startDate=2024-01-01T00:00:00Z&endDate=2024-12-31T23:59:59Z
```

2. Query with only start date:
```
http://localhost:3002/availability?startDate=2024-03-15T00:00:00Z
```

3. Query with only end date:
```
http://localhost:3002/availability?endDate=2024-06-30T23:59:59Z
```

4. Narrow date range (specific month):
```
http://localhost:3002/availability?startDate=2024-04-01T00:00:00Z&endDate=2024-04-30T23:59:59Z
```

5. Current year query:
```
http://localhost:3002/availability?startDate=2024-01-01T00:00:00Z&endDate=2024-12-31T23:59:59Z
```
- Ensure you're using RFC3339 format (YYYY-MM-DDTHH:MM:SSZ)
- The 'Z' indicates UTC time zone
- Use URL encoding if needed (though most tools handle this automatically)
- Verify your server's timezone handling
