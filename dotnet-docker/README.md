# .NET Docker Tools

Docker-based replacement for the dotnet CLI command, perfect for development without installing .NET SDK locally.

## Quick Start

### 1. Use as dotnet command replacement
```bash
./dotnet --version
./dotnet new console -n MyApp
./dotnet build
./dotnet run
```

### 2. Initialize new WebAPI with EF
```bash
./init-webapi.sh MyAPI postgres jwt
cd MyAPI
./run.sh
```

### 3. Run existing projects
```bash
./run.sh /path/to/project run
./run.sh . build
./run.sh . test
```

## Files

- `dotnet` - Drop-in replacement for dotnet CLI
- `run.sh` - Run any .NET project with auto-detection
- `init-webapi.sh` - Create new WebAPI with EF Core
- `docker-compose.yml` - Full stack with PostgreSQL

## Examples

```bash
# Create new project
./init-webapi.sh TodoAPI postgres jwt

# Use dotnet command
./dotnet ef migrations add Initial
./dotnet add package Newtonsoft.Json

# Run with Docker Compose
docker-compose up
```