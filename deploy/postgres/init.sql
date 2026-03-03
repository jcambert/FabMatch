-- PostgreSQL initialization script
-- This script runs once when the container is first created.
-- EF Core migrations handle the actual schema; this only ensures the DB/user exist.

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";   -- for full-text search

-- Grant privileges (user already created via environment variables)
GRANT ALL PRIVILEGES ON DATABASE fabmatch TO fabmatch;
