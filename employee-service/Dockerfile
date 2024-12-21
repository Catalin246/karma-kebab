# Use the .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the csproj file and restore dependencies
COPY ["employee-service.csproj", "./"]
RUN dotnet restore

# Copy the entire project and publish the app in Release mode
COPY . ./ 
RUN dotnet publish "employee-service.csproj" -c Release -o /app/publish

# Use the Azure Functions runtime image for running the app
FROM mcr.microsoft.com/azure-functions/dotnet:4 AS runtime

# Set the working directory inside the container
WORKDIR /home/site/wwwroot

# Copy the published app from the build stage
COPY --from=build /app/publish .

# Expose the port (default port for Azure Functions in Docker is 80)
EXPOSE 80

# The Azure Functions runtime image has the necessary entrypoint set up,
# so no need to explicitly call `dotnet` here. The runtime will handle it.
