# Use the official .NET SDK image for building the app (development mode)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the csproj file and restore any dependencies (via dotnet restore)
COPY ["shift-service.csproj", "./"]
RUN dotnet restore

# Copy the entire project and build it
COPY . ./
RUN dotnet build -c Debug -o /app/build

# Expose the port the app will run on
EXPOSE 3003

# Set the entrypoint to run the application in development mode
CMD ["dotnet", "run", "--urls", "http://0.0.0.0:3003"]
