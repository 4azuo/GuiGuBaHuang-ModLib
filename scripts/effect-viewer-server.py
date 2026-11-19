"""Effect viewer with saving: a local web server for effect-viewer/index.html (http://localhost:8765/effect-viewer/).

- GET  /effect-viewer/data/effects.js : built at every request from docs/Effects/<kind>/index.json (always fresh);
- POST /api/user-label {"kind", "name", "fields": {...}} : the tester's own assessment written into index.json as
  "user-*" fields (an empty value removes the field), under the lock of the effect scripts;
- every other path: the files of the repository (read only).
Opened as a file (no server) the viewer still shows everything but cannot save.

usage: python scripts/effect-viewer-server.py [--port 8765] [--open]
"""
import argparse, json, os, sys, time, webbrowser
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
sys.path.insert(0, os.path.join(REPO, "scripts", "game"))
from collect_effects import locked, dump_aligned  # noqa: E402

KINDS = ("Skill", "Unit", "Scenes")


def index_file(kind):
    return os.path.join(REPO, "docs", "Effects", kind, "index.json")


def effects_js():
    kinds = {k: json.load(open(index_file(k), encoding="utf-8-sig")) for k in KINDS if os.path.exists(index_file(k))}
    data = {"generated": time.strftime("%Y-%m-%d %H:%M"), "kinds": kinds, "server": True}
    return ("window.EFFECT_DATA = " + json.dumps(data, ensure_ascii=False, separators=(",", ":")) + ";\n").encode("utf-8")


def save_user_label(kind, name, fields):
    if kind not in KINDS:
        raise ValueError("unknown kind " + str(kind))
    p = index_file(kind)
    with locked(p):
        index = json.load(open(p, encoding="utf-8-sig"))
        e = next((x for x in index if x["name"].lower() == str(name).lower()), None)
        if e is None:
            raise ValueError("unknown effect " + str(name))
        for k, v in fields.items():
            key = k if k.startswith("user-") else "user-" + k
            if v in (None, "", [], {}):
                e.pop(key, None)
            else:
                e[key] = v
        # an assessment: a user-* field with a value (the aligned index gives every effect the columns, null)
        if any(k.startswith("user-") and k != "user-updated" and e[k] not in (None, "", [], {}) for k in e):
            e["user-updated"] = time.strftime("%Y-%m-%d %H:%M")
        else:
            e.pop("user-updated", None)
        with open(p, "w", encoding="utf-8", newline="\n") as f:
            dump_aligned(index, f)
    return {k: v for k, v in e.items() if k.startswith("user-") and v not in (None, "", [], {})}


class Handler(SimpleHTTPRequestHandler):
    def __init__(self, *a, **kw):
        super().__init__(*a, directory=REPO, **kw)

    def log_message(self, fmt, *args):
        if "/api/" in (self.path or ""):
            super().log_message(fmt, *args)

    def send_body(self, code, body, ctype):
        self.send_response(code)
        self.send_header("Content-Type", ctype)
        self.send_header("Cache-Control", "no-store")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        if self.path.split("?")[0] == "/effect-viewer/data/effects.js":
            return self.send_body(200, effects_js(), "application/javascript; charset=utf-8")
        if self.path in ("/", "/effect-viewer"):
            self.send_response(302)
            self.send_header("Location", "/effect-viewer/")
            self.end_headers()
            return
        return super().do_GET()

    def do_POST(self):
        if self.path != "/api/user-label":
            return self.send_body(404, b"not found", "text/plain")
        try:
            req = json.loads(self.rfile.read(int(self.headers.get("Content-Length", 0))).decode("utf-8"))
            saved = save_user_label(req.get("kind"), req.get("name"), req.get("fields") or {})
            return self.send_body(200, json.dumps({"ok": True, "fields": saved}, ensure_ascii=False).encode("utf-8"), "application/json")
        except Exception as ex:
            return self.send_body(400, json.dumps({"ok": False, "error": str(ex)}).encode("utf-8"), "application/json")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--port", type=int, default=8765)
    ap.add_argument("--open", action="store_true")
    a = ap.parse_args()
    srv = ThreadingHTTPServer(("127.0.0.1", a.port), Handler)
    url = f"http://localhost:{a.port}/effect-viewer/"
    print("Effect viewer:", url, "(Ctrl+C to stop)")
    if a.open:
        webbrowser.open(url)
    try:
        srv.serve_forever()
    except KeyboardInterrupt:
        pass


if __name__ == "__main__":
    main()
