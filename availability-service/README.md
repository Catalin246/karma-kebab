# Availability Microservice

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