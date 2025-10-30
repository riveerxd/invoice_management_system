# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all source files
COPY . .

# Build and publish in one step
WORKDIR "/src/InvoiceManagement"
RUN dotnet publish "InvoiceManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check (defined in docker-compose.yml)

# Run the application (migrations run automatically on startup)
ENTRYPOINT ["dotnet", "InvoiceManagement.dll"]
