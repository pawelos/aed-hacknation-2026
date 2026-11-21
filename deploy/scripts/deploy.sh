#!/usr/bin/env bash
# HackNation 2026 on the VPS: the wizytówka (deploy/stacks/site.yml) behind the car-reminder edge Traefik.
#
#   ./deploy/scripts/deploy.sh up       start / update (idempotent; the pipeline runs it on every deploy)
#   ./deploy/scripts/deploy.sh health   check the site through Traefik (https)
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

EDGE_NETWORK=car-reminder-edge
SITE_HOST="${SITE_HOST:-hacknation-2026.pl}"
export SITE_HOST

die() {
  echo "error: $*" >&2
  exit 1
}

site_compose() {
  docker compose -f deploy/stacks/site.yml "$@"
}

cmd_up() {
  docker network inspect "$EDGE_NETWORK" >/dev/null 2>&1 || docker network create "$EDGE_NETWORK" >/dev/null
  site_compose up -d --remove-orphans
  echo "Site is up: https://$SITE_HOST"
}

cmd_health() {
  local i
  for i in $(seq 1 12); do
    if curl -fsS -o /dev/null "https://$SITE_HOST/"; then
      echo "https://$SITE_HOST/ answers 200."
      return 0
    fi
    echo "Waiting for https://$SITE_HOST/ ($i/12)..."
    sleep 5
  done
  die "https://$SITE_HOST/ does not answer (DNS, certificate or container: ./deploy/scripts/deploy.sh logs)"
}

usage() {
  cat <<'EOF'
Usage: ./deploy/scripts/deploy.sh <command>

  up        Start / update the site container
  down      Stop and remove it
  health    Check https://<SITE_HOST>/ through Traefik
  logs      Follow the container logs
  ps        Show the container
  config    Validate the compose configuration
EOF
}

case "${1:-}" in
  up) cmd_up ;;
  down) site_compose down ;;
  health) cmd_health ;;
  logs) site_compose logs -f --tail=200 ;;
  ps) site_compose ps ;;
  config) site_compose config --quiet && echo "Compose configuration is valid." ;;
  ""|-h|--help|help) usage ;;
  *) die "Unknown command: $1 (try --help)" ;;
esac
