#!/bin/sh
set -eu
if [ "$APP_DATABASE_USER" = "$POSTGRES_USER" ]; then
  echo "El usuario de aplicación debe ser distinto del administrador de PostgreSQL." >&2
  exit 1
fi
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<'SQL'
\getenv app_user APP_DATABASE_USER
\getenv app_password APP_DATABASE_PASSWORD
\getenv app_database POSTGRES_DB
CREATE ROLE :"app_user" LOGIN PASSWORD :'app_password' NOSUPERUSER NOCREATEDB NOCREATEROLE;
ALTER DATABASE :"app_database" OWNER TO :"app_user";
SQL
