#!/usr/bin/env bash
# Local dev edge (deploy/edge/local): Traefik on https://*.hacknation-2026.test in front of the wizytówka.
# Git Bash on Windows (Docker Desktop, openssl from Git for Windows). Step by step: deploy/edge/local/README.md.
# Pattern: C:\car-reminder\deploy\scripts\local.sh.
#
#   ./deploy/scripts/local.sh certs     local CA + certificate (once; then trust the CA, see the output)
#   ./deploy/scripts/local.sh up        start (creates the certificate when missing)
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

# Git Bash would turn "/CN=..." into a Windows path.
export MSYS_NO_PATHCONV=1

EDGE_DIR=deploy/edge/local
CERT_DIR=$EDGE_DIR/certs
DOMAIN=hacknation-2026.test
HOSTS=("$DOMAIN" "www.$DOMAIN" "aed.$DOMAIN")
CERT_DAYS=825

die() {
  echo "error: $*" >&2
  exit 1
}

local_compose() {
  docker compose -f "$EDGE_DIR/docker-compose.yml" "$@"
}

cmd_certs() {
  local host san=""
  command -v openssl >/dev/null || die "openssl not found (it comes with Git for Windows: run this in Git Bash)"
  mkdir -p "$CERT_DIR"
  if [[ ! -f "$CERT_DIR/ca.key" ]]; then
    openssl req -x509 -new -nodes -newkey rsa:2048 -sha256 -days 3650 \
      -subj "/CN=HackNation 2026 local dev CA" \
      -keyout "$CERT_DIR/ca.key" -out "$CERT_DIR/ca.crt" 2>/dev/null
    echo "Created the local CA: $CERT_DIR/ca.crt"
  fi

  for host in "$DOMAIN" "*.$DOMAIN"; do
    san="${san:+$san,}DNS:$host"
  done
  printf '%s\n' \
    "basicConstraints=CA:FALSE" \
    "keyUsage=digitalSignature,keyEncipherment" \
    "extendedKeyUsage=serverAuth" \
    "subjectAltName=$san" > "$CERT_DIR/dev.ext"
  openssl req -new -nodes -newkey rsa:2048 -subj "/CN=$DOMAIN" \
    -keyout "$CERT_DIR/dev.key" -out "$CERT_DIR/dev.csr" 2>/dev/null
  openssl x509 -req -sha256 -days "$CERT_DAYS" -in "$CERT_DIR/dev.csr" \
    -CA "$CERT_DIR/ca.crt" -CAkey "$CERT_DIR/ca.key" -CAcreateserial \
    -extfile "$CERT_DIR/dev.ext" -out "$CERT_DIR/dev.crt" 2>/dev/null
  rm -f "$CERT_DIR/dev.csr"
  echo "Certificate for $DOMAIN and *.$DOMAIN: $CERT_DIR/dev.crt"
  echo
  echo "Trust the CA once (current Windows user, no admin; Windows asks to confirm), from the repo root:"
  echo "  certutil -user -addstore Root deploy\\edge\\local\\certs\\ca.crt"
}

cmd_up() {
  [[ -f "$CERT_DIR/dev.crt" ]] || cmd_certs
  local_compose up -d --remove-orphans
  local https_port="${HTTPS_PORT:-443}" suffix=""
  [[ "$https_port" == "443" ]] || suffix=":$https_port"
  echo
  echo "Local edge is up:"
  local host
  for host in "${HOSTS[@]}"; do
    echo "  https://$host$suffix/"
  done
  printf '%s\n' 'Hosts file (C:\Windows\System32\drivers\etc\hosts, as Administrator), once:'
  echo "  127.0.0.1 ${HOSTS[*]}"
}

usage() {
  cat <<'EOF'
Usage: ./deploy/scripts/local.sh <command>

  certs      Create the local CA (once) and a new certificate for hacknation-2026.test and *.hacknation-2026.test
  up         Start the local Traefik and the site (certificate created when missing)
  restart    Restart Traefik (after a change in deploy/edge/local/routes.yml)
  down       Stop it
  logs       Follow the Traefik logs

Ports 80/443 are also used by the Car Reminder local edge: stop it first (C:\car-reminder\deploy:
./scripts/local.sh down) or start this one on other ports: HTTP_PORT=8080 HTTPS_PORT=8443 ./deploy/scripts/local.sh up
EOF
}

case "${1:-}" in
  certs) cmd_certs ;;
  up) cmd_up ;;
  restart) local_compose restart traefik ;;
  down) local_compose down ;;
  logs) local_compose logs -f --tail=200 traefik ;;
  ""|-h|--help|help) usage ;;
  *) die "Unknown command: $1 (try --help)" ;;
esac
