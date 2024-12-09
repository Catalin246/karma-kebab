# Use the official .NET 8 SDK image as the base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the .csproj file and restore the dependencies
COPY shift-service.csproj ./
RUN dotnet restore "shift-service.csproj"

# Copy the rest of the application files
COPY . .

# Build the application
RUN dotnet build "shift-service.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "shift-service.csproj" -c Release -o /app/publish

# Use the official .NET 8 runtime image as the base image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

# Copy the built app from the build stage
COPY --from=build /app/publish .

# Define the entry point for the application
ENTRYPOINT ["dotnet", "shift-service.dll"]
