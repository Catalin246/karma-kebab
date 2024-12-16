# Use the .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the csproj file and restore dependencies
COPY ["shift-service.csproj", "./"]
RUN dotnet restore

# Copy the entire project and publish the app in Release mode
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build /app/publish .

# Expose the port the app will run on
EXPOSE 3003

# Run the application in Production mode
ENTRYPOINT ["dotnet", "shift-service.dll"]
