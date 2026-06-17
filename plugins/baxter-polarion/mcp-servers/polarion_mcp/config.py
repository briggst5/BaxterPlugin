"""Load PolarionMCP configuration from a single user-level env file."""

from __future__ import annotations

import os
from pathlib import Path

CONFIG_PATH = Path.home() / ".config" / "polarion-mcp.env"


def load_config() -> None:
    """Load key=value pairs from ``~/.config/polarion-mcp.env`` into ``os.environ``.

    Values from the config file override any existing environment variables for
    the same keys. Lines starting with ``#`` and blank lines are ignored.
    """
    if not CONFIG_PATH.is_file():
        return

    for raw_line in CONFIG_PATH.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, _, value = line.partition("=")
        key = key.strip()
        value = value.strip().strip("\"'")
        if key:
            os.environ[key] = value
