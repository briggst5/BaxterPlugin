#!/usr/bin/env python3
"""Sync FutureState Platform.wiki pages to reference/wiki/."""

from __future__ import annotations

import base64
import json
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
from datetime import datetime, timezone
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[1]
WIKI_DIR = REPO_ROOT / "reference" / "wiki"
MANIFEST_PATH = REPO_ROOT / "scripts" / "wiki-manifest.json"
ENV_PATH = Path.home() / ".config" / "azure-devops-mcp.env"

ORG = "FLC-NPD"
PROJECT = "FutureState"
WIKI_ID = "beee7a67-95b1-4a9c-a188-c1fc5589d6b7"
API_VERSION = "7.1"


def load_env(path: Path) -> dict[str, str]:
    env: dict[str, str] = {}
    if not path.is_file():
        return env
    for line in path.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, _, value = line.partition("=")
        env[key.strip()] = value.strip().strip("'\"")
    return env


def resolve_pat(env: dict[str, str]) -> str | None:
    for key in (
        "PERSONAL_ACCESS_TOKEN",
        "AZURE_DEVOPS_EXT_PAT",
        "ADO_MCP_AUTH_TOKEN",
        "ADO_PAT",
        "PAT",
    ):
        if env.get(key):
            return env[key]
    return None


def auth_header(pat: str) -> str:
    # MCP env stores base64(":PAT") in PERSONAL_ACCESS_TOKEN; raw PAT uses :PAT encoding.
    if re.fullmatch(r"[A-Za-z0-9+/=]+", pat) and len(pat) > 40:
        return f"Basic {pat}"
    token = pat if pat.startswith(":") else f":{pat}"
    return f"Basic {base64.b64encode(token.encode()).decode()}"


def api_get(url: str, pat: str) -> dict:
    req = urllib.request.Request(
        url,
        headers={
            "Authorization": auth_header(pat),
            "Accept": "application/json",
        },
    )
    with urllib.request.urlopen(req, timeout=120) as resp:
        return json.loads(resp.read().decode())


def slugify_path(wiki_path: str) -> Path:
    """Convert /Agile Practices/SAFe Agile/Foo -> agile-practices/safe-agile/foo.md"""
    parts = [p.strip() for p in wiki_path.strip("/").split("/") if p.strip()]
    slugged = [re.sub(r"[^a-z0-9]+", "-", p.lower()).strip("-") for p in parts]
    slugged = [s for s in slugged if s]
    if not slugged:
        return Path("home.md")
    return Path(*slugged[:-1]) / f"{slugged[-1]}.md"


def strip_untrusted_markers(content: str) -> str:
    return re.sub(
        r"<<[a-f0-9]+>> \[UNTRUSTED WIKI PAGE CONTENT[^\]]*\] <<[a-f0-9]+>>\n?",
        "",
        content,
    ).strip()


def flatten_wiki_tree(node: dict, pages: list[dict]) -> None:
    path = node.get("path")
    if path:
        pages.append({"path": path, "id": node.get("id"), "order": node.get("order")})
    for child in node.get("subPages", []):
        flatten_wiki_tree(child, pages)


def list_all_pages(pat: str) -> list[dict]:
    url = (
        f"https://dev.azure.com/{ORG}/{PROJECT}/_apis/wiki/wikis/"
        f"{WIKI_ID}/pages?path=/&recursionLevel=full&includeContent=false"
        f"&api-version={API_VERSION}"
    )
    root = api_get(url, pat)
    pages: list[dict] = []
    flatten_wiki_tree(root, pages)
    return pages


def fetch_page_content(pat: str, wiki_path: str) -> str:
    encoded_path = urllib.parse.quote(wiki_path)
    url = (
        f"https://dev.azure.com/{ORG}/{PROJECT}/_apis/wiki/wikis/"
        f"{WIKI_ID}/pages?path={encoded_path}&includeContent=true"
        f"&api-version={API_VERSION}"
    )
    data = api_get(url, pat)
    return data.get("content", "")


def write_page(wiki_path: str, content: str, page_id: int | None) -> Path:
    rel = slugify_path(wiki_path)
    out = WIKI_DIR / rel
    out.parent.mkdir(parents=True, exist_ok=True)
    header = f"# {wiki_path.strip('/') or 'Home'}\n\n"
    source = (
        f"Source: [Platform.wiki](https://dev.azure.com/{ORG}/{PROJECT}"
        f"/_wiki/wikis/Platform.wiki/{page_id or ''}/{urllib.parse.quote(wiki_path.strip('/'))})\n\n"
        if page_id
        else ""
    )
    body = strip_untrusted_markers(content)
    out.write_text(f"{header}{source}{body}\n", encoding="utf-8")
    return rel


def update_manifest(pages: list[dict], synced: list[dict]) -> None:
    MANIFEST_PATH.parent.mkdir(parents=True, exist_ok=True)
    payload = {
        "wiki_id": WIKI_ID,
        "project": PROJECT,
        "organization": ORG,
        "last_sync": datetime.now(timezone.utc).isoformat(),
        "page_count": len(synced),
        "pages": synced,
    }
    MANIFEST_PATH.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")


def update_index(synced: list[dict]) -> None:
    index_path = REPO_ROOT / "reference" / "wiki-index.md"
    lines = [
        "# FutureState Wiki Index",
        "",
        f"Source: [Platform.wiki](https://dev.azure.com/{ORG}/{PROJECT}/_wiki/wikis/Platform.wiki)",
        "",
        f"Last sync: {datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M UTC')} via `scripts/sync_wiki.py`",
        "",
        "| Wiki path | Local file |",
        "|-----------|------------|",
    ]
    for entry in sorted(synced, key=lambda e: e["path"].lower()):
        lines.append(f"| `{entry['path']}` | [{entry['local']}](wiki/{entry['local']}) |")
    index_path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def main() -> int:
    env = load_env(ENV_PATH)
    pat = resolve_pat(env)
    if not pat:
        print(
            "Error: no PAT found. Set AZURE_DEVOPS_EXT_PAT or ADO_PAT in "
            f"{ENV_PATH}",
            file=sys.stderr,
        )
        return 1

    print(f"Listing pages from {ORG}/{PROJECT}...")
    pages = list_all_pages(pat)
    print(f"Found {len(pages)} pages. Fetching content...")

    synced: list[dict] = []
    errors: list[str] = []

    for i, page in enumerate(pages, 1):
        wiki_path = page.get("path", "/")
        page_id = page.get("id")
        try:
            content = fetch_page_content(pat, wiki_path)
            rel = write_page(wiki_path, content, page_id)
            synced.append(
                {
                    "path": wiki_path,
                    "id": page_id,
                    "local": rel.as_posix(),
                }
            )
            print(f"  [{i}/{len(pages)}] {wiki_path} -> reference/wiki/{rel}")
        except urllib.error.HTTPError as exc:
            msg = f"{wiki_path}: HTTP {exc.code}"
            errors.append(msg)
            print(f"  ERROR {msg}", file=sys.stderr)
        except urllib.error.URLError as exc:
            msg = f"{wiki_path}: {exc.reason}"
            errors.append(msg)
            print(f"  ERROR {msg}", file=sys.stderr)

    update_manifest(pages, synced)
    update_index(synced)
    print(f"\nSynced {len(synced)}/{len(pages)} pages to reference/wiki/")
    if errors:
        print(f"{len(errors)} errors.", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
