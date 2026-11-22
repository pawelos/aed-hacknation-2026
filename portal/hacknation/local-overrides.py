

# --- HackNation 2026: localhost without *.mcod.local (appended by hacknation/setup.sh) ---
# Host-only cookies, so the admin session works on http://localhost:8001 (the sample sets mcod.local).
SESSION_COOKIE_DOMAIN = None
CSRF_COOKIE_DOMAIN = None
INTERNAL_IPS = ("127.0.0.1", "localhost", "172.28.18.1")
# The sample passes "timeout" as an index setting, which Elasticsearch 6.8 rejects ("unknown setting [index.timeout]"),
# so tabular resources fail to index. Keep only the real index settings.
ELASTICSEARCH_DSL_INDEX_SETTINGS = {"number_of_shards": 1, "number_of_replicas": 0}
ELASTICSEARCH_HISTORIES_IDX_SETTINGS = {"number_of_shards": 1, "number_of_replicas": 0}
