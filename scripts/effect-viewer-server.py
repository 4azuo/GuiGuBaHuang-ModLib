"""Viewers with saving: a local web server for the effect viewer of the ModLib submodule
(GuiGuBaHuang-ModLib/effect-viewer/index.html, http://localhost:8765/GuiGuBaHuang-ModLib/effect-viewer/) and
logs/test-viewer/index.html (http://localhost:8765/logs/test-viewer/).

- GET  /GuiGuBaHuang-ModLib/effect-viewer/data/effects.js : built at every request from GuiGuBaHuang-ModLib/docs/Effects/<kind>/index.json (always fresh);
- POST /api/user-label {"kind", "name", "fields": {...}} : the tester's own assessment written into index.json as
  "user-*" fields (an empty value removes the field), under the lock of the effect scripts;
- POST /api/pin-run {"kind": "auto"|"unit", "id", "pin": true|false, "note", "file" (unit: the trx)} : pins / unpins a test run (pinned.json
  in its folder, as scripts/pin-test-run.ps1) and patches logs/test-viewer/data/runs.js so a reload shows it;
- every other path: the files of the repository (read only).
Opened as a file (no server) the viewer still shows everything but cannot save.

usage: python GuiGuBaHuang-ModLib/scripts/effect-viewer-server.py [--port 8765] [--open] [--page effect-viewer|test-viewer]
"""
import argparse, json, os, re, sys, threading, time, webbrowser
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer

# shared by the mods using ModLib: serves the mod repository holding ModLib (or $MOD_REPO), see scripts/game/_modrepo.py
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), "game"))
import _modrepo  # noqa: E402
REPO = _modrepo.repo()
MODLIB_URL = os.path.relpath(_modrepo.MODLIB, REPO).replace(os.sep, "/") + "/"
from collect_effects import locked, dump_aligned  # noqa: E402

KINDS = ("Skill", "Unit", "Scenes")
PAGES = {"effect-viewer": MODLIB_URL + "effect-viewer/", "test-viewer": "logs/test-viewer/"}


def index_file(kind):
    return os.path.join(_modrepo.EFFECTS, kind, "index.json")


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


RUN_ID = re.compile(r"^\d{8}-\d{6}(-s\d+)?$")
RUNS_JS = os.path.join(REPO, "logs", "test-viewer", "data", "runs.js")
RUNS_PREFIX = "window.TEST_DATA = "
_runs_lock = threading.Lock()


def run_dir(kind, run_id, trx_file=None):
    """Folder of a test run: logs/autotest/<id>, or the folder of the unit test run's trx file (under logs/unittests)."""
    if kind == "auto":
        if not RUN_ID.match(str(run_id)):
            raise ValueError("bad run id " + str(run_id))
        d = os.path.join(REPO, "logs", "autotest", run_id)
    elif kind == "unit":
        root = os.path.join(REPO, "logs", "unittests")
        d = os.path.dirname(os.path.abspath(os.path.join(REPO, str(trx_file or ""))))
        if not trx_file or os.path.commonpath([root, d]) != root:
            raise ValueError("bad unit test file " + str(trx_file))
    else:
        raise ValueError("unknown kind " + str(kind))
    if not os.path.isdir(d):
        raise ValueError("run not found: " + str(run_id))
    return d


def pin_run(kind, run_id, pin, note, trx_file=None):
    f = os.path.join(run_dir(kind, run_id, trx_file), "pinned.json")
    data = None
    if pin:
        data = {"pinnedAt": time.strftime("%Y-%m-%dT%H:%M:%S"), "note": str(note or ""), "auto": False}
        with open(f, "w", encoding="utf-8", newline="\n") as fh:
            json.dump(data, fh, ensure_ascii=False, indent=4)
    elif os.path.exists(f):
        os.remove(f)
    # the viewer data: the same pin on the run (no full regeneration by test-viewer/update-data.ps1)
    with _runs_lock:
        if os.path.exists(RUNS_JS):
            text = open(RUNS_JS, encoding="utf-8-sig").read().strip()
            if text.startswith(RUNS_PREFIX):
                all_data = json.loads(text[len(RUNS_PREFIX):].rstrip(";"))
                for r in all_data.get("autotest" if kind == "auto" else "unittest") or []:
                    if r.get("id") == run_id:
                        r["pin"] = data
                tmp = RUNS_JS + ".tmp"
                with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
                    fh.write(RUNS_PREFIX + json.dumps(all_data, ensure_ascii=False, separators=(",", ":")) + ";\n")
                os.replace(tmp, RUNS_JS)
    return data


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
        if self.path.split("?")[0] == "/" + MODLIB_URL + "effect-viewer/data/effects.js":
            return self.send_body(200, effects_js(), "application/javascript; charset=utf-8")
        if self.path in ("/", "/effect-viewer", "/effect-viewer/", "/" + MODLIB_URL + "effect-viewer"):
            self.send_response(302)
            self.send_header("Location", "/" + MODLIB_URL + "effect-viewer/")
            self.end_headers()
            return
        return super().do_GET()

    def do_POST(self):
        if self.path not in ("/api/user-label", "/api/pin-run"):
            return self.send_body(404, b"not found", "text/plain")
        try:
            req = json.loads(self.rfile.read(int(self.headers.get("Content-Length", 0))).decode("utf-8"))
            if self.path == "/api/pin-run":
                pin = pin_run(req.get("kind"), str(req.get("id")), bool(req.get("pin")), req.get("note"), req.get("file"))
                return self.send_body(200, json.dumps({"ok": True, "pin": pin}, ensure_ascii=False).encode("utf-8"), "application/json")
            saved = save_user_label(req.get("kind"), req.get("name"), req.get("fields") or {})
            return self.send_body(200, json.dumps({"ok": True, "fields": saved}, ensure_ascii=False).encode("utf-8"), "application/json")
        except Exception as ex:
            return self.send_body(400, json.dumps({"ok": False, "error": str(ex)}).encode("utf-8"), "application/json")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--port", type=int, default=8765)
    ap.add_argument("--open", action="store_true")
    ap.add_argument("--page", choices=("effect-viewer", "test-viewer"), default="effect-viewer")
    a = ap.parse_args()
    srv = ThreadingHTTPServer(("127.0.0.1", a.port), Handler)
    url = f"http://localhost:{a.port}/" + PAGES[a.page]
    print("Viewers:", *(f"http://localhost:{a.port}/{p}" for p in PAGES.values()), "(Ctrl+C to stop)")
    if a.open:
        webbrowser.open(url)
    try:
        srv.serve_forever()
    except KeyboardInterrupt:
        pass


if __name__ == "__main__":
    main()
