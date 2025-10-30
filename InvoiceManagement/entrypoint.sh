#!/bin/bash
set -e

echo "Waiting for PostgreSQL to be ready..."
until PGPASSWORD=devpass123 psql -h postgres -U devuser -d invoice_db -c '\q' 2>/dev/null; do
  >&2 echo "PostgreSQL is unavailable - sleeping"
  sleep 2
done

echo "PostgreSQL is up - running migrations..."
dotnet InvoiceManagement.dll --migrate || true

echo "Starting application..."
exec "$@"
