-- Initial database setup for OpenAPI Swagger Project
-- This script runs automatically when the container first starts

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create application schema (optional, can use public schema)
-- CREATE SCHEMA IF NOT EXISTS api;

-- Example: Create a simple health check table
CREATE TABLE IF NOT EXISTS health_check (
    id SERIAL PRIMARY KEY,
    service_name VARCHAR(100) NOT NULL,
    last_checked TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(20) DEFAULT 'healthy'
);

-- Insert initial health check record
INSERT INTO health_check (service_name, status)
VALUES ('webapi', 'healthy')
ON CONFLICT DO NOTHING;

-- Grant permissions (adjust as needed for your application user)
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO devuser;
-- GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO devuser;

-- Log completion
DO $$
BEGIN
    RAISE NOTICE 'Database initialization completed successfully';
END $$;
