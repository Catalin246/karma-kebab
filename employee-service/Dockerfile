# Use an official .NET runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Set the working directory in the container
WORKDIR /app

# Expose ports 80 and 443 in the container
EXPOSE 80
EXPOSE 443

# Use .NET 8.0 SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory to the project root in the container
WORKDIR /build

# Copy the project file to the container
COPY ["employee-service.csproj", "./"]

# Restore NuGet packages
RUN dotnet restore "./employee-service.csproj"

# Copy all the remaining project files
COPY . .

# Build the project in Release mode and output to /publish
RUN dotnet publish "employee-service.csproj" -c Release -o /publish

# Use the base image to run the application
FROM base AS final

# Set the working directory in the container
WORKDIR /app

# Copy the published application from the build container
COPY --from=build /publish .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "employee-service.dll"]
