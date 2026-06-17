# PolarionMCP

An [MCP](https://modelcontextprotocol.io/) server that lets AI agents interact
with [Polarion ALM](https://www.plm.automation.siemens.com/global/en/products/polarion/)
work items through Polarion's WSDL/SOAP web services interface.

## Features

| Tool | Description |
|------|-------------|
| `get_work_item` | Retrieve a single work item by its ID (e.g. `PROJ-123`) |
| `set_work_item_status` | Change status/state via workflow (`performWorkflowAction`) |
| `list_work_item_workflow_actions` | List valid status transitions for a work item |
| `get_work_item_links` | Linked work items for a given item |
| `list_work_item_comments` | List comments on a work item |
| `add_work_item_comment` | Add a comment (or reply) on a work item |
| `add_work_item_link` | Link one work item to another (`addLinkedItem`) |
| `remove_work_item_link` | Remove a link between work items (`removeLinkedItem`) |
| `get_work_item_raw_fields` | Raw SOAP fields (diagnostics) |
| `query_work_items` | Query work items by type, status, and/or a Lucene query string |
| `list_documents` | Enumerate documents (modules) in the configured project |
| `list_document_work_items` | List work items in a document/module identified by name (and optional space) |
| `get_document_text` | Free text from a document (home page + narrative sections) |
| `get_document_work_items` | Same lookup; returns the `work_items` array only (legacy alias) |
| `list_reviewers` | List reviewers (approvees) and approval status on a work item |
| `add_reviewer` | Add a reviewer via `TrackerWebService.addApprovee` |
| `remove_reviewer` | Remove a reviewer via `TrackerWebService.removeApprovee` |
| `reset_review_status` | Set approval status back to *waiting* via `editApproval` |
| `create_test_result_record` | Record pass/fail on a Polarion Test Run |

## Requirements

* Python 3.11+
* Network access to the Polarion SOAP endpoint (typically `<base_url>/ws/services/…`)
* A Polarion user account with at least read access to the target project

## Installation

```bash
pip install .
```

Or for development (editable install):

```bash
pip install -e .
```

## Configuration

Configuration is read from a single file: **`~/.config/polarion-mcp.env`**.

The server and `scripts/test_tools.py` load this file automatically on startup.
Do not put credentials in the project directory.

| Variable | Required | Description |
|----------|----------|-------------|
| `POLARION_URL` | ✅ | Base URL of the Polarion instance, e.g. `http://polarion.example.com/polarion` |
| `POLARION_USER` | ✅ | Polarion username |
| `POLARION_PASSWORD` | one of password or PAT | Polarion account password |
| `POLARION_PAT` | one of password or PAT | [Personal access token](https://developer.siemens.com/polarion/authentication.html) for SOAP (native `logInWithToken` + `Authorization: Bearer`, with PAT-as-password fallback) |
| `POLARION_PROJECT` | optional | Default project ID (prepended to every query as `project.id:<id>`) |
| `POLARION_MCP_LOG_LEVEL` | optional | Log level (default `WARNING`) |
| `POLARION_MCP_MAX_CALLS` | optional | Cap SOAP calls per process (default unlimited) |
| `POLARION_APPROVAL_STATUS_WAITING_ID` | optional | Enum id for “waiting” when resetting reviews (default `waiting`) |
| `POLARION_DEFAULT_LINK_ROLE` | optional | Default link role when a tool omits `link_role` |

Set either `POLARION_PASSWORD` or `POLARION_PAT`. If both are set, the PAT path is tried first.

Reviewer tools use Polarion [TrackerWebService](https://testdrive.polarion.com/polarion/sdk/doc/javadoc/com/polarion/alm/ws/client/tracker/TrackerWebService.html) approval APIs: `addApprovee`, `removeApprovee`, `editApproval`, and the work item `approvals` field (`getWorkItemByUriWithFields`).

Link tools use `addLinkedItem` and `removeLinkedItem` with a project-specific **link role** enum id (see existing links via `get_work_item_links`).

Status changes use Polarion workflow: `getAvailableActions` then `performWorkflowAction`. Call `list_work_item_workflow_actions` first to see allowed transitions from the current state.

### 1. Create your config file

```bash
mkdir -p ~/.config
cp polarion-mcp.env.example ~/.config/polarion-mcp.env
chmod 600 ~/.config/polarion-mcp.env
# edit ~/.config/polarion-mcp.env with your Polarion URL, username, password or PAT, and project ID
```

**Never commit `~/.config/polarion-mcp.env`.**

## Running the server

### 2. Start the server

```bash
polarion-mcp
```

Or as a Python module:

```bash
python -m polarion_mcp.server
```

### Enabling debug logging

Set `POLARION_MCP_LOG_LEVEL=DEBUG` (or edit `logging.basicConfig` in `server.py`)
to see every SOAP request and response:

```bash
POLARION_MCP_LOG_LEVEL=DEBUG polarion-mcp
```

> **Note:** The server communicates over stdio (MCP protocol).  
> Running it directly in a terminal produces no visible output until an MCP client connects.

## Manual testing

### Option A – `scripts/test_tools.py` (recommended for quick smoke tests)

`scripts/test_tools.py` is a standalone CLI that connects to Polarion using the
same `PolarionClient` the MCP server uses and calls each tool directly.  No MCP
client or browser is required.

```bash
# Fetch a single work item:
python scripts/test_tools.py get_work_item PROJ-123

# Query open defects (up to 20):
python scripts/test_tools.py query_work_items --type defect --status open --limit 20

# Query with a Lucene expression:
python scripts/test_tools.py query_work_items --query "title:login AND priority:high"

# List documents in the project (optional space filter):
python scripts/test_tools.py list_documents
python scripts/test_tools.py list_documents --space Inputs

# List work items in a document by name:
python scripts/test_tools.py list_document_work_items "My Spec Document"
python scripts/test_tools.py list_document_work_items "Connex 360 SRS Configuration" --space Inputs

# Free text from a document (home page + narrative sections):
python scripts/test_tools.py get_document_text "Connex 360 SRS Configuration" --space Inputs

# Enable verbose/debug output (shows SOAP traffic):
python scripts/test_tools.py --debug get_work_item PROJ-123
```

Run `python scripts/test_tools.py --help` (or add a sub-command and `--help`) for
the full argument reference.

### Option B – MCP Inspector (interactive browser UI)

The `mcp` package ships with a development server and browser-based inspector
that lets you call tools interactively without writing any client code.

```bash
# Install the MCP CLI extras if not already present:
pip install "mcp[cli]"

# Launch the inspector (opens http://localhost:5173 in your browser):
mcp dev polarion_mcp/server.py
```

In the inspector you can select any tool, fill in its parameters, and inspect
the JSON response.

### Option C – `mcp run` (non-interactive stdio passthrough)

```bash
mcp run polarion_mcp/server.py
```

This starts the server on stdio so you can pipe raw JSON-RPC messages to it
from another terminal or script.

---



Add the following to your MCP client config (e.g. Cursor `.cursor/mcp.json` or
Claude `claude_desktop_config.json`). Credentials belong in
`~/.config/polarion-mcp.env`; the server loads that file automatically.

**Cursor (`.cursor/mcp.json`) — use the project virtualenv:**

```json
{
  "mcpServers": {
    "polarion-mcp": {
      "command": "bash",
      "args": [
        "-lc",
        "cd /path/to/PolarionMCP && source .venv/bin/activate && polarion-mcp"
      ]
    }
  }
}
```

Replace `/path/to/PolarionMCP` with the absolute path to this repository.

If `polarion-mcp` is on your `PATH` (e.g. after `pip install` into the active
environment), you can use `"command": "polarion-mcp"` instead.

## Tool reference

### `get_work_item`

Retrieve a single work item by its full ID.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Full work item ID in `PROJECT-NUMBER` format, e.g. `PROJ-123` |

**Example response**
```json
{
  "id": "PROJ-123",
  "title": "Login page crashes on empty password",
  "type": "defect",
  "status": "open",
  "priority": "high",
  "assignee": "jsmith",
  "description": "<p>Reproduces 100% of the time …</p>",
  "created": "2024-01-15T09:32:00Z",
  "updated": "2024-03-10T14:05:00Z"
}
```

---

### `list_work_item_comments`

List comments on a work item (`getWorkItemByUriWithFields` with the `comments` field).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Work item ID |

---

### `add_work_item_comment`

Add a comment on a work item (`TrackerWebService.addComment`, with `createComment` fallback on older servers).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Work item ID |
| `text` | string | ✅ | Comment body |
| `title` | string | | Optional title |
| `parent_comment_uri` | string | | Reply to an existing comment (URI from `list_work_item_comments`) |
| `content_type` | string | | `text/plain` (default) or `text/html` |

---

### `set_work_item_status`

Change a work item's status using Polarion workflow (`performWorkflowAction`).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Work item ID |
| `status` | string | | Target status enum id (e.g. `closed`, `inProgress`) |
| `workflow_action_name` | string | | Workflow action label instead of `status` |
| `workflow_action_id` | integer | | Action id from `list_work_item_workflow_actions` |

Provide one of `status`, `workflow_action_name`, or `workflow_action_id`. If no action matches, the response includes `available_actions`.

---

### `list_work_item_workflow_actions`

List workflow actions allowed from the work item's current status.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Work item ID |

---

### `query_work_items`

Query work items using any combination of type, status, and a Lucene query
string.  All filters are combined with `AND`.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `type` | string | | Work item type, e.g. `requirement`, `task`, `defect`, `testcase` |
| `status` | string | | Work item status, e.g. `open`, `inProgress`, `closed`, `resolved` |
| `query` | string | | Additional Polarion Lucene query string, e.g. `title:login AND priority:high` |
| `limit` | integer | | Maximum results (default `50`, maximum `512`) |

---

### `list_documents`

Enumerate Polarion documents (LiveDocs / modules) in **`POLARION_PROJECT`**.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `space` | string | | Wiki folder prefix, e.g. `Inputs` (documents under that folder) |
| `query` | string | | Additional Lucene query AND-ed with the project filter |
| `limit` | integer | | Maximum results (default `200`) |

**Example response**

```json
{
  "project_id": "MY_PROJ",
  "space": "Inputs",
  "count": 2,
  "limit": 200,
  "documents": [
    {
      "id": "…",
      "title": "Connex 360 SRS Configuration",
      "module_folder": "Inputs/Connex 360 SRS Configuration",
      "space": "Inputs",
      "uri": "subterra:data-service:objects:/default/…"
    }
  ]
}
```

---

### `list_document_work_items`

List work items contained in a Polarion document (LiveDoc / module), identified by **document name**.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `document_name` | string | ✅ | Title or name of the document/module |
| `space` | string | | Wiki folder path when needed, e.g. `Inputs` or `Design/Specifications` |
| `limit` | integer | | Maximum results (default `200`) |

**Example response**

```json
{
  "document_name": "Connex 360 SRS Configuration",
  "space": "Inputs",
  "count": 42,
  "limit": 200,
  "work_items": [
    {"id": "PROJ-101", "title": "…", "type": "requirement", "status": "open"}
  ]
}
```

`get_document_work_items` uses the same lookup but returns only the `work_items` array.

---

### `get_document_text`

Return free text from a document identified by name (requires `POLARION_PROJECT`). Combines the module **home page** (`homePageContent`) and **in-document narrative** from work item descriptions (`getModuleWorkItems`).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `document_name` | string | ✅ | Title or name of the document/module |
| `space` | string | | Wiki folder path, e.g. `Inputs` |
| `include_work_item_text` | boolean | | Include section/heading text from work items (default `true`) |

The response includes `home_page_content`, `sections` (work items with description text), and `combined_content` (concatenated for convenience). Content is returned as stored in Polarion (often HTML).

---

### `add_work_item_link`

Create a directed link from one work item to another (`TrackerWebService.addLinkedItem`).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Source work item ID |
| `linked_work_item_id` | string | ✅ | Target work item ID |
| `link_role` | string | ✅ | Link role enum id (e.g. `relates_to`, `verifies`, `implements`) |

---

### `remove_work_item_link`

Remove a link between two work items (`TrackerWebService.removeLinkedItem`). Use the same `link_role` as when the link was created.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Source work item ID |
| `linked_work_item_id` | string | ✅ | Target work item ID |
| `link_role` | string | ✅ | Link role enum id |

---

### `list_reviewers`

List reviewers (approvees) on a work item and each approval status.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Full work item ID, e.g. `PROJ-123` |

**Example response**

```json
{
  "work_item_id": "PROJ-123",
  "uri": "subterra:data-service:objects:/default/PROJ${WorkItem}PROJ-123",
  "reviewers": [
    {"reviewer_id": "jsmith", "status": "waiting"},
    {"reviewer_id": "adoe", "status": "approved"}
  ]
}
```

---

### `add_reviewer`

Add a reviewer to a work item (`TrackerWebService.addApprovee`).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Full work item ID |
| `reviewer_id` | string | ✅ | Polarion user id of the reviewer |

---

### `remove_reviewer`

Remove a reviewer from a work item (`TrackerWebService.removeApprovee`).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Full work item ID |
| `reviewer_id` | string | ✅ | Polarion user id of the reviewer |

---

### `reset_review_status`

Reset approval status to **waiting** (`TrackerWebService.editApproval` with
`IApprovalStatusOpt`-style enum id, default `waiting`).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `work_item_id` | string | ✅ | Full work item ID |
| `reviewer_ids` | string[] | | User ids to reset; omit to reset every reviewer on the item |

---

## Tests

```bash
pip install -e ".[dev]"
pytest
```

## Project structure

```
polarion_mcp/
├── __init__.py          Package marker
├── config.py            Loads ~/.config/polarion-mcp.env
├── server.py            MCP server – tool definitions and dispatch
└── polarion_client.py   Polarion SOAP/WSDL client wrapper
scripts/
└── test_tools.py        Manual test CLI (runs tools directly against Polarion)
pyproject.toml           Package metadata and dependencies
polarion-mcp.env.example Example config (copy to ~/.config/polarion-mcp.env)
tests/
├── test_reviews.py      Unit tests for reviewer / approval helpers
├── test_links.py        Unit tests for work item link helpers
├── test_documents.py    Unit tests for document work item listing
├── test_workflow.py     Unit tests for workflow / status changes
└── test_comments.py     Unit tests for work item comments
```
