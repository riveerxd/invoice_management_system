#!/bin/bash
# Standalone PostgreSQL launcher for OpenAPI Swagger Project
# Builds and runs PostgreSQL container independently

set -e

# Configuration (override with environment variables)
DB_USER="${DB_USER:-devuser}"
DB_PASSWORD="${DB_PASSWORD:-devpass123}"
DB_NAME="${DB_NAME:-webapi_db}"
DB_PORT="${DB_PORT:-5432}"
CONTAINER_NAME="${CONTAINER_NAME:-openapi-postgres}"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting PostgreSQL for OpenAPI Swagger Project${NC}"
echo "Database: ${DB_NAME}"
echo "User: ${DB_USER}"
echo "Port: ${DB_PORT}"
echo ""

# Check if container already exists
if [ "$(docker ps -aq -f name=${CONTAINER_NAME})" ]; then
    echo -e "${YELLOW}Container ${CONTAINER_NAME} already exists. Removing...${NC}"
    docker rm -f ${CONTAINER_NAME}
fi

# Build the PostgreSQL image
echo -e "${GREEN}Building PostgreSQL image...${NC}"
docker build -f Dockerfile.postgres -t openapi-postgres:latest .

# Run the container
echo -e "${GREEN}Starting PostgreSQL container...${NC}"
docker run -d \
    --name ${CONTAINER_NAME} \
    -e POSTGRES_USER=${DB_USER} \
    -e POSTGRES_PASSWORD=${DB_PASSWORD} \
    -e POSTGRES_DB=${DB_NAME} \
    -p ${DB_PORT}:5432 \
    -v openapi_postgres_data:/var/lib/postgresql/data \
    openapi-postgres:latest

# Wait for PostgreSQL to be ready
echo -e "${YELLOW}Waiting for PostgreSQL to be ready...${NC}"
for i in {1..30}; do
    if docker exec ${CONTAINER_NAME} pg_isready -U ${DB_USER} -d ${DB_NAME} > /dev/null 2>&1; then
        echo -e "${GREEN}PostgreSQL is ready!${NC}"
        echo ""
        echo "Connection details:"
        echo "  Host: localhost"
        echo "  Port: ${DB_PORT}"
        echo "  Database: ${DB_NAME}"
        echo "  User: ${DB_USER}"
        echo "  Password: ${DB_PASSWORD}"
        echo ""
        echo "Connection string:"
        echo "  Host=localhost;Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
        echo ""
        echo "To connect with psql:"
        echo "  docker exec -it ${CONTAINER_NAME} psql -U ${DB_USER} -d ${DB_NAME}"
        echo ""
        echo "To stop:"
        echo "  docker stop ${CONTAINER_NAME}"
        echo ""
        echo "To view logs:"
        echo "  docker logs ${CONTAINER_NAME}"
        exit 0
    fi
    sleep 1
done

echo -e "${YELLOW}PostgreSQL is taking longer than expected to start${NC}"
echo "Check logs with: docker logs ${CONTAINER_NAME}"
exit 1
