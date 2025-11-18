# Deployment – Caddy, Nginx, systemd

Überblick zu Reverse Proxy und Service-Betrieb.

## Caddy
- Config: `Caddyfile`, `deploy/caddy/Caddyfile`
- Aufgaben: Statisches Hosting, Reverse Proxy `/api` → Backend, Security Headers
- Start lokal: `caddy run --config Caddyfile`

## Nginx
- Config: `deploy/nginx/nginx.conf`
- Alternative zum Caddy-Betrieb

## systemd
- Unit: `deploy/systemd/aigs.service`
- Logs: `journalctl -u aigs -f`
- Skripte: `deploy/install.sh` für Setup

