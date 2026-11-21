#!/usr/bin/env bash
# HackNation 2026 on the VPS, behind the car-reminder edge Traefik:
#   - the wizytówka hacknation-2026.pl (deploy/stacks/site.yml);
#   - the AED prototype aed.hacknation-2026.pl (deploy/stacks/app.yml: PostGIS, API, web).
#
#   ./deploy/scripts/deploy.sh up                         start / update the site (the pipeline runs it on every deploy)
#   ./deploy/scripts/deploy.sh health                     check the site through Traefik (https)
#   ./deploy/scripts/deploy.sh app load <images.tar.gz>   load the release images and remember their version
#   ./deploy/scripts/deploy.sh app up                     start / update the prototype with the loaded version
#   ./deploy/scripts/deploy.sh app health                 check the prototype through Traefik (https)
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

EDGE_NETWORK=car-reminder-edge
SITE_HOST="${SITE_HOST:-hacknation-2026.pl}"
export SITE_HOST

# Outside deploy/: the pipeline replaces deploy/ on every run.
APP_ENV_FILE="$ROOT_DIR/aed.env"
APP_VERSION_FILE="$ROOT_DIR/aed.version"
APP_IMAGES=(aed/api aed/web)

die() {
  echo "error: $*" >&2
  exit 1
}

ensure_edge_network() {
  docker network inspect "$EDGE_NETWORK" >/dev/null 2>&1 || docker network create "$EDGE_NETWORK" >/dev/null
}

site_compose() {
  docker compose -f deploy/stacks/site.yml "$@"
}

cmd_up() {
  ensure_edge_network
  # Always a new container: the pipeline replaces site/ on every deploy, and a running container would keep its
  # bind mount on the deleted directory (empty: httpd answers 404, the healthcheck fails, Traefik drops the route).
  site_compose up -d --remove-orphans --force-recreate
  echo "Site is up: https://$SITE_HOST"
}

# Waits until a URL answers 2xx through Traefik (certificate, routing, container).
wait_for_url() {
  local url="$1" i
  for i in $(seq 1 24); do
    if curl -fsS -o /dev/null "$url"; then
      echo "$url answers."
      return 0
    fi
    echo "Waiting for $url ($i/24)..."
    sleep 5
  done
  return 1
}

cmd_health() {
  wait_for_url "https://$SITE_HOST/" \
    || die "https://$SITE_HOST/ does not answer (DNS, certificate or container: ./deploy/scripts/deploy.sh logs)"
}

# Creates aed.env with random secrets on the first run; an existing file is never overwritten.
ensure_app_env() {
  if [[ -f "$APP_ENV_FILE" ]]; then
    return 0
  fi
  command -v openssl >/dev/null || die "openssl is needed to generate $APP_ENV_FILE"
  (
    umask 077
    {
      echo "# Created by deploy.sh on $(date -u +%Y-%m-%dT%H:%M:%SZ). Variables: deploy/stacks/aed.env.example"
      echo "APP_HOST=aed.hacknation-2026.pl"
      echo "POSTGRES_USER=aed"
      echo "POSTGRES_DB=aed"
      echo "POSTGRES_PASSWORD=$(openssl rand -hex 24)"
      echo "JWT_SECRET=$(openssl rand -hex 32)"
      echo "SEED_ADMIN_PASSWORD="
      echo "SEED_DEMO_PASSWORD="
    } > "$APP_ENV_FILE"
  )
  chmod 600 "$APP_ENV_FILE"
  echo "Created $APP_ENV_FILE with new secrets (keep a copy in a password manager)."
}

app_version() {
  [[ -f "$APP_VERSION_FILE" ]] || die "No images loaded yet: ./deploy/scripts/deploy.sh app load <images.tar.gz>"
  tr -d '[:space:]' < "$APP_VERSION_FILE"
}

app_compose() {
  ensure_app_env
  local version
  version="$(app_version)"
  APP_VERSION="$version" docker compose --env-file "$APP_ENV_FILE" -f deploy/stacks/app.yml "$@"
}

app_host() {
  local host
  host="$(grep -E '^APP_HOST=' "$APP_ENV_FILE" 2>/dev/null | tail -n1 | cut -d= -f2- || true)"
  echo "${host:-aed.hacknation-2026.pl}"
}

cmd_app_load() {
  local archive="${1:-}"
  [[ -f "$archive" ]] || die "Usage: deploy.sh app load <images.tar.gz> (file not found: ${archive:-none})"
  local output version
  output="$(docker load -i "$archive")"
  echo "$output"
  # "Loaded image: aed/api:<version>" -> <version>
  version="$(echo "$output" | sed -n 's#^Loaded image: aed/api:##p' | tail -n1)"
  [[ -n "$version" ]] || die "The archive has no aed/api image."
  echo "$version" > "$APP_VERSION_FILE"
  echo "Loaded release $version."
}

cmd_app_up() {
  ensure_edge_network
  app_compose up -d --remove-orphans
  echo "Prototype $(app_version) is starting: https://$(app_host)"
}

cmd_app_health() {
  local host
  host="$(app_host)"
  wait_for_url "https://$host/pl" \
    || die "https://$host/pl does not answer (./deploy/scripts/deploy.sh app logs, app ps)"
  wait_for_url "https://$host/api/v1/OpenData/metadata.json" \
    || die "The API does not answer through Traefik (./deploy/scripts/deploy.sh app logs)"
}

# Removes the images of releases other than the running one.
cmd_prune() {
  local current image tag
  current="$(app_version 2>/dev/null || true)"
  for image in "${APP_IMAGES[@]}"; do
    while read -r tag; do
      if [[ -n "$tag" && "$tag" != "$current" && "$tag" != "<none>" ]]; then
        docker image rm "$image:$tag" >/dev/null && echo "Removed $image:$tag"
      fi
    done < <(docker image ls "$image" --format '{{.Tag}}')
  done
  docker image prune -f >/dev/null
}

usage() {
  cat <<'EOF'
Usage: ./deploy/scripts/deploy.sh <command>

Site (hacknation-2026.pl):
  up | down | health | logs | ps | config

Prototype (aed.hacknation-2026.pl):
  app load <images.tar.gz>   Load release images (apps/tools/build-release.ps1)
  app up                     Start / update with the loaded release (creates ~/hacknation-2026/aed.env once)
  app down                   Stop the containers (the database volume stays)
  app health                 Check https://<APP_HOST>/pl and the API through Traefik
  app logs | app ps | app config

  prune                      Remove images of older releases
EOF
}

case "${1:-}" in
  up) cmd_up ;;
  down) site_compose down ;;
  health) cmd_health ;;
  logs) site_compose logs -f --tail=200 ;;
  ps) site_compose ps ;;
  config) site_compose config --quiet && echo "Compose configuration is valid." ;;
  app)
    case "${2:-}" in
      load) cmd_app_load "${3:-}" ;;
      up) cmd_app_up ;;
      down) app_compose down ;;
      health) cmd_app_health ;;
      logs) app_compose logs -f --tail=200 ;;
      ps) app_compose ps ;;
      config) app_compose config --quiet && echo "Compose configuration is valid." ;;
      *) die "Unknown app command: ${2:-none} (try --help)" ;;
    esac
    ;;
  prune) cmd_prune ;;
  ""|-h|--help|help) usage ;;
  *) die "Unknown command: $1 (try --help)" ;;
esac
