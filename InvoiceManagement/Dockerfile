# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["InvoiceManagement/InvoiceManagement.csproj", "InvoiceManagement/"]
COPY ["InvoiceManagement.Tests/InvoiceManagement.Tests.csproj", "InvoiceManagement.Tests/"]

# Restore dependencies
RUN dotnet restore "InvoiceManagement/InvoiceManagement.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/InvoiceManagement"
RUN dotnet build "InvoiceManagement.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "InvoiceManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage with EF tools
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Install PostgreSQL client and EF Core tools
RUN apt-get update && \
    apt-get install -y postgresql-client curl && \
    rm -rf /var/lib/apt/lists/* && \
    dotnet tool install --global dotnet-ef

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy published app
COPY --from=publish /app/publish .

# Copy migration files
COPY --from=build /src/InvoiceManagement/Migrations ./Migrations

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/swagger/index.html || exit 1

# Run the application (migrations run automatically on startup)
ENTRYPOINT ["dotnet", "InvoiceManagement.dll"]
