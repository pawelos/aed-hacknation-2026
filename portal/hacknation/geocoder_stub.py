"""Local stand-in for the portal's internal Pelias geocoder (geocoder.mcod.local), which is not part of the public
source. Answers GET /v1/place?ids=... for the 16 voivodeships (TERYT region codes 02-32) in the shape mcod/regions/api.py
reads, so the XML importer can validate and save the <regions> of a resource. Anything else returns no features.
"""
import json
from http.server import BaseHTTPRequestHandler, HTTPServer
from urllib.parse import parse_qs, urlparse

# TERYT code: (name, longitude, latitude of the voivodeship centre).
VOIVODESHIPS = {
    "02": ("dolnośląskie", 16.41, 51.09), "04": ("kujawsko-pomorskie", 18.49, 53.07),
    "06": ("lubelskie", 22.90, 51.22), "08": ("lubuskie", 15.34, 52.20),
    "10": ("łódzkie", 19.43, 51.60), "12": ("małopolskie", 20.27, 49.85),
    "14": ("mazowieckie", 21.10, 52.51), "16": ("opolskie", 17.92, 50.65),
    "18": ("podkarpackie", 22.17, 49.94), "20": ("podlaskie", 22.92, 53.27),
    "22": ("pomorskie", 18.03, 54.17), "24": ("śląskie", 19.02, 50.33),
    "26": ("świętokrzyskie", 20.76, 50.75), "28": ("warmińsko-mazurskie", 20.83, 53.86),
    "30": ("wielkopolskie", 17.23, 52.33), "32": ("zachodniopomorskie", 15.55, 53.47),
}


def wof_id(code: str) -> str:
    return f"85687{code}"  # stable fake Who's On First id per voivodeship


def feature(gid: str):
    source, layer, ident = gid.split(":", 2)
    if source == "teryt" and layer == "region" and ident in VOIVODESHIPS:
        name, lon, lat = VOIVODESHIPS[ident]
        props = {"gid": gid, "id": ident, "layer": "region", "source": "teryt", "name": name, "region": name,
                 "region_gid": f"whosonfirst:region:{wof_id(ident)}", "country": "Polska",
                 "addendum": {"terytdata": {"teryt_name": name}}}
    elif source == "whosonfirst" and layer == "region":
        code = next((c for c in VOIVODESHIPS if wof_id(c) == ident), None)
        if code is None:
            return None
        name, lon, lat = VOIVODESHIPS[code]
        props = {"gid": gid, "id": ident, "layer": "region", "source": "whosonfirst", "name": name}
    else:
        return None
    return {"type": "Feature", "geometry": {"type": "Point", "coordinates": [lon, lat]}, "properties": props}


class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        url = urlparse(self.path)
        ids = [i for i in parse_qs(url.query).get("ids", [""])[0].split(",") if i]
        if url.path.startswith("/parser/"):
            payload = {}  # Placeholder (placeholder.mcod.local): no extra geometry, the portal keeps the Pelias point
        else:
            features = [f for f in (feature(i) for i in ids) if f] if url.path.endswith("/place") else []
            payload = {"type": "FeatureCollection", "features": features}
        body = json.dumps(payload, ensure_ascii=False).encode()
        self.send_response(200)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)


if __name__ == "__main__":
    HTTPServer(("0.0.0.0", 80), Handler).serve_forever()
