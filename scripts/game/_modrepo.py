"""Mod repository of the game tools (shared by the mods using ModLib: the tools live in GuiGuBaHuang-ModLib/scripts,
the logs, scenarios and configs live in the mod repository holding ModLib as a submodule).

MODLIB: the ModLib folder. repo(): $MOD_REPO, else the folder holding ModLib. settings(): <repo>/scripts/settings.json
("modProject", "modName", "modId", "projects"). project(): the mod project id.
"""
import json, os

MODLIB = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
EFFECTS = os.path.join(MODLIB, "docs", "Effects")


def repo():
    env = os.environ.get("MOD_REPO")
    if env and os.path.isdir(env):
        return os.path.abspath(env)
    parent = os.path.dirname(MODLIB)   # ModLib as a submodule: the mod repository around it; alone: ModLib itself
    return parent if os.path.isfile(os.path.join(parent, "scripts", "settings.json")) else MODLIB


def settings():
    try:
        return json.load(open(os.path.join(repo(), "scripts", "settings.json"), encoding="utf-8-sig"))
    except (OSError, ValueError):
        return {}


def project():
    s = settings()
    return s.get("modProject") or s.get("releaseAllVersionProject") or ""
