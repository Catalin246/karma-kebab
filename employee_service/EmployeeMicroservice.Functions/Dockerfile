# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 3005


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["/EmployeeMicroservice.Functions.csproj", "employee_service/EmployeeMicroservice.Functions/"]
RUN dotnet restore "./employee_service/EmployeeMicroservice.Functions/EmployeeMicroservice.Functions.csproj"
COPY . .
WORKDIR "/src/employee_service/EmployeeMicroservice.Functions"
RUN dotnet build "./EmployeeMicroservice.Functions.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EmployeeMicroservice.Functions.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Add EF database update command
RUN dotnet ef database update --no-build -c ApplicationDatabase

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet-isolated", "EmployeeMicroservice.Functions.dll"].
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true