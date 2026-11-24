#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Mod repository of the translation (the tool is shared by the mods using ModLib: it lives in ModLib, the projects and the
translation cache live in the mod repository).

The mod repository is the first of:
  1. the environment variable MOD_REPO;
  2. the current folder or one of its parents holding scripts/settings.json;
  3. a parent of this folder holding scripts/settings.json (ModLib as a submodule of the mod repository).
"""

import os

HERE = os.path.dirname(os.path.abspath(__file__))


def _find_up(start):
    d = os.path.abspath(start)
    while True:
        if os.path.isfile(os.path.join(d, 'scripts', 'settings.json')):
            return d
        parent = os.path.dirname(d)
        if parent == d:
            return None
        d = parent


def repo_root():
    env = os.environ.get('MOD_REPO')
    if env and os.path.isdir(env):
        return os.path.abspath(env)
    return _find_up(os.getcwd()) or _find_up(os.path.dirname(HERE)) or os.path.dirname(os.path.dirname(HERE))


def project_path(project):
    """A project folder: a path as is, a project id in the mod repository."""
    return project if os.path.isdir(project) else os.path.join(repo_root(), project)


def cache_dir(name=None):
    """Translation cache of the mod repository: <repo>/.translate-cache (in git: a text is never translated twice)."""
    return os.path.join(repo_root(), '.translate-cache')
