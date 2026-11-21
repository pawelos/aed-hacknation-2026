-- Runs once, when the volume is created. The EF migration declares the extension too, so a database
-- created elsewhere (e.g. on the VPS) gets it from the migration.
CREATE EXTENSION IF NOT EXISTS postgis;
