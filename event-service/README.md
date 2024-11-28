# event-service

## Set-up the project

1. Initialize the go.mod file

`go mod init event-service`

2. Import Packages  

`go get -u github.com/gorilla/mux`

3. Execute the project

`go run main.go` - this is not working. the connection string must be changed.

4. Execute unit tests

`go test ./tests/unit -v`