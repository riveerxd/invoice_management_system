# PostgreSQL Database Setup

This project uses PostgreSQL 16 (Alpine) for data persistence. There are two ways to run the database:

## Option 1: Standalone PostgreSQL (Recommended for DB-only testing)

Use the standalone Dockerfile and helper script:

```bash
# Quick start - uses default credentials
./run-postgres.sh

# Custom configuration
DB_USER=myuser DB_PASSWORD=mypass DB_NAME=mydb DB_PORT=5433 ./run-postgres.sh
```

### Default Configuration

- **Host**: localhost
- **Port**: 5432
- **Database**: webapi_db
- **User**: devuser
- **Password**: devpass123

### Connection String

```
Host=localhost;Port=5432;Database=webapi_db;Username=devuser;Password=devpass123
```

### Useful Commands

```bash
# Connect with psql
docker exec -it openapi-postgres psql -U devuser -d webapi_db

# View logs
docker logs openapi-postgres

# Stop container
docker stop openapi-postgres

# Remove container and data
docker rm -f openapi-postgres
docker volume rm openapi_postgres_data
```

## Option 2: Full Stack with Docker Compose (Recommended for development)

The `dotnet-docker/docker-compose.yml` orchestrates both the WebAPI and PostgreSQL:

```bash
cd dotnet-docker
docker-compose up
```

This starts:
- PostgreSQL on port 5432 (or `DB_PORT` from `.env`)
- WebAPI on port 5000 (or `API_PORT` from `.env`)

### Configuration

Edit `dotnet-docker/.env` to customize:

```env
DB_USER=devuser
DB_PASSWORD=devpass123
DB_NAME=webapi_db
DB_PORT=5432
API_PORT=5000
ASPNETCORE_ENVIRONMENT=Development
```

## Database Initialization

The `db-init/` directory contains SQL scripts that run automatically when the database
container first starts (only on initial creation, not on restart).

Files are executed in alphabetical order:

- `01-init.sql` - Creates extensions, health check table, and initial data

### Adding Custom Initialization Scripts

Create new `.sql` files in `db-init/`:

```bash
# Example: Add seed data
cat > db-init/02-seed-data.sql <<EOF
INSERT INTO users (username, email) VALUES
  ('admin', 'admin@example.com'),
  ('test', 'test@example.com');
EOF
```

**Note**: Scripts only run on first container creation. To re-run:

```bash
# Standalone
docker rm -f openapi-postgres
docker volume rm openapi_postgres_data
./run-postgres.sh

# Docker Compose
cd dotnet-docker
docker-compose down -v
docker-compose up
```

## Entity Framework Core Migrations

Per the Constitution (Principle III), all schema changes MUST use EF Core migrations:

```bash
# Create migration
dotnet-docker/dotnet ef migrations add MigrationName

# Apply migration
dotnet-docker/dotnet ef database update

# Rollback migration
dotnet-docker/dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet-docker/dotnet ef migrations remove
```

### Connection String in appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=webapi_db;Username=devuser;Password=devpass123"
  }
}
```

For Docker Compose, use the service name `postgres` as the host:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=webapi_db;Username=devuser;Password=devpass123"
  }
}
```

## Health Checks

The PostgreSQL container includes a health check:

```bash
# Check health status
docker inspect openapi-postgres --format='{{.State.Health.Status}}'

# Manual health check
docker exec openapi-postgres pg_isready -U devuser -d webapi_db
```

## Backup and Restore

### Backup

```bash
# Full database backup
docker exec openapi-postgres pg_dump -U devuser webapi_db > backup.sql

# Compressed backup
docker exec openapi-postgres pg_dump -U devuser webapi_db | gzip > backup.sql.gz
```

### Restore

```bash
# From SQL file
cat backup.sql | docker exec -i openapi-postgres psql -U devuser -d webapi_db

# From compressed backup
gunzip -c backup.sql.gz | docker exec -i openapi-postgres psql -U devuser -d webapi_db
```

## Troubleshooting

### Container won't start

```bash
# Check logs
docker logs openapi-postgres

# Common issue: Port already in use
lsof -i :5432
# Kill the process or change DB_PORT
```

### Can't connect from host

```bash
# Verify container is running
docker ps | grep postgres

# Check port mapping
docker port openapi-postgres

# Test connection
docker exec openapi-postgres psql -U devuser -d webapi_db -c "SELECT version();"
```

### Permission denied errors

```bash
# Check volume permissions
docker volume inspect openapi_postgres_data

# Reset by removing volume
docker rm -f openapi-postgres
docker volume rm openapi_postgres_data
./run-postgres.sh
```

### Database doesn't persist

Verify volume is mounted:

```bash
docker inspect openapi-postgres | grep -A 10 Mounts
```

Should show: `/var/lib/postgresql/data`

## Production Considerations

For production deployments:

1. **Use secrets management** - Never hardcode passwords
2. **Enable SSL/TLS** - Configure `sslmode=require` in connection string
3. **Set up regular backups** - Use `pg_dump` with cron jobs
4. **Monitor performance** - Use `pg_stat_statements` extension
5. **Tune configuration** - Adjust `shared_buffers`, `work_mem`, etc.
6. **Use managed services** - Consider AWS RDS, Azure Database, or Google Cloud SQL

## Extensions Installed

The initialization script enables:

- `uuid-ossp` - UUID generation functions
- `pgcrypto` - Cryptographic functions

Add more extensions in `db-init/01-init.sql` as needed.
