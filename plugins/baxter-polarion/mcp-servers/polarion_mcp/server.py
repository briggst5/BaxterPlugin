"""PolarionMCP – MCP server entry point.

Configuration is read from ``~/.config/polarion-mcp.env`` (see ``polarion-mcp.env.example``).
"""

from __future__ import annotations

import asyncio
import json
import logging
import os
from typing import Any

import mcp.types as types
from mcp.server import Server
from mcp.server.stdio import stdio_server
import zeep.exceptions

from polarion_mcp.config import CONFIG_PATH, load_config
from polarion_mcp.polarion_client import PolarionClient

load_config()

_log_level = os.environ.get("POLARION_MCP_LOG_LEVEL", "WARNING").upper()
logging.basicConfig(level=getattr(logging, _log_level, logging.WARNING))
logger = logging.getLogger(__name__)

# ---------------------------------------------------------------------------
# MCP server instance
# ---------------------------------------------------------------------------

app = Server("polarion-mcp")

# ---------------------------------------------------------------------------
# Lazy-initialised Polarion client
# ---------------------------------------------------------------------------

_client: PolarionClient | None = None


def _get_client() -> PolarionClient:
    """Return the shared PolarionClient, creating it on first call."""
    global _client

    if _client is not None:
        return _client

    url = os.environ.get("POLARION_URL", "").strip()
    user = os.environ.get("POLARION_USER", "").strip()
    password = os.environ.get("POLARION_PASSWORD", "").strip()
    pat = os.environ.get("POLARION_PAT", "").strip()

    missing: list[str] = []
    if not url:
        missing.append("POLARION_URL")
    if not user:
        missing.append("POLARION_USER")
    if not password and not pat:
        missing.append("POLARION_PASSWORD or POLARION_PAT")
    if missing:
        raise RuntimeError(
            f"Required environment variable(s) not set: {', '.join(missing)}. "
            f"Set them in {CONFIG_PATH} (see polarion-mcp.env.example in the repo)."
        )

    _client = PolarionClient(
        url=url,
        username=user,
        password=password,
        project_id=os.environ.get("POLARION_PROJECT", ""),
        personal_access_token=pat,
    )
    return _client


# ---------------------------------------------------------------------------
# Tool definitions
# ---------------------------------------------------------------------------

@app.list_tools()
async def list_tools() -> list[types.Tool]:
    return [
        types.Tool(
            name="get_work_item",
            description=(
                "Retrieve a single Polarion work item by its ID "
                "(e.g. 'PROJ-123'). Returns the work item's title, type, "
                "status, priority, assignee, description, and timestamps."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": (
                            "Full work item ID in PROJECT-NUMBER format "
                            "(e.g. PROJ-123)"
                        ),
                    }
                },
                "required": ["work_item_id"],
            },
        ),
        types.Tool(
            name="set_work_item_status",
            description=(
                "Change a work item's status/state using Polarion workflow "
                "(TrackerWebService.performWorkflowAction). Provide the target "
                "status enum id (e.g. open, inProgress, closed), or a workflow_action_id."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": "Work item ID, e.g. PROJ-123",
                    },
                    "status": {
                        "type": "string",
                        "description": (
                            "Target status enum id after the transition "
                            "(e.g. open, inProgress, closed, resolved)"
                        ),
                    },
                    "workflow_action_name": {
                        "type": "string",
                        "description": (
                            "Optional workflow action label to run instead of "
                            "matching by status"
                        ),
                    },
                    "workflow_action_id": {
                        "type": "integer",
                        "description": (
                            "Optional workflow action id from "
                            "list_work_item_workflow_actions"
                        ),
                    },
                },
                "required": ["work_item_id"],
            },
        ),
        types.Tool(
            name="list_work_item_workflow_actions",
            description=(
                "List workflow actions available for a work item in its current "
                "state (use before set_work_item_status to see valid transitions)."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": "Work item ID, e.g. PROJ-123",
                    }
                },
                "required": ["work_item_id"],
            },
        ),
        types.Tool(
            name="get_work_item_links",
            description=(
                "Retrieve link information for a single Polarion work item by its ID "
                "(e.g. 'PROJ-123'). Returns the work item's linked work items when available."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": (
                            "Full work item ID in PROJECT-NUMBER format "
                            "(e.g. PROJ-123)"
                        ),
                    }
                },
                "required": ["work_item_id"],
            },
        ),
        types.Tool(
            name="add_work_item_link",
            description=(
                "Create a link from one Polarion work item to another via "
                "TrackerWebService.addLinkedItem. Requires link_role (e.g. relates_to, verifies)."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": "Source work item ID, e.g. PROJ-123",
                    },
                    "linked_work_item_id": {
                        "type": "string",
                        "description": "Target work item ID to link to",
                    },
                    "link_role": {
                        "type": "string",
                        "description": (
                            "Polarion link role enum id (project-specific), "
                            "e.g. relates_to, verifies, implements"
                        ),
                    },
                },
                "required": ["work_item_id", "linked_work_item_id", "link_role"],
            },
        ),
        types.Tool(
            name="remove_work_item_link",
            description=(
                "Remove a link between two Polarion work items via "
                "TrackerWebService.removeLinkedItem. Requires the same link_role "
                "used when the link was created."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": "Source work item ID, e.g. PROJ-123",
                    },
                    "linked_work_item_id": {
                        "type": "string",
                        "description": "Target work item ID to unlink",
                    },
                    "link_role": {
                        "type": "string",
                        "description": "Polarion link role enum id used for the link",
                    },
                },
                "required": ["work_item_id", "linked_work_item_id", "link_role"],
            },
        ),
        types.Tool(
            name="get_work_item_raw_fields",
            description=(
                "Fetch raw work item fields via getWorkItemByUriWithFields. "
                "Useful for diagnostics (e.g. attachments) when fields are not exposed via queries."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": (
                            "Full work item ID in PROJECT-NUMBER format "
                            "(e.g. PROJ-123)"
                        ),
                    },
                    "keys": {
                        "type": "array",
                        "items": {"type": "string"},
                        "description": "List of field keys to request (e.g. ['attachments','id','uri']).",
                    },
                },
                "required": ["work_item_id", "keys"],
            },
        ),
        types.Tool(
            name="query_work_items",
            description=(
                "Query Polarion work items using type, status, and/or a "
                "Polarion Lucene query string. All provided filters are "
                "combined with AND. The project configured in POLARION_PROJECT "
                "is always included as a filter when set."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "type": {
                        "type": "string",
                        "description": (
                            "Work item type to filter by "
                            "(e.g. 'requirement', 'task', 'defect', 'testcase')"
                        ),
                    },
                    "status": {
                        "type": "string",
                        "description": (
                            "Work item status to filter by "
                            "(e.g. 'open', 'inProgress', 'closed', 'resolved')"
                        ),
                    },
                    "query": {
                        "type": "string",
                        "description": (
                            "Additional Polarion Lucene query string to AND "
                            "with the other filters "
                            "(e.g. 'title:login AND priority:high')"
                        ),
                    },
                    "limit": {
                        "type": "integer",
                        "description": (
                            "Maximum number of work items to return "
                            "(default 50, maximum 512)"
                        ),
                        "default": 50,
                    },
                },
            },
        ),
        types.Tool(
            name="list_documents",
            description=(
                "Enumerate Polarion documents (LiveDocs/modules) in the configured "
                "project (POLARION_PROJECT). Optionally filter by wiki space folder "
                "or Lucene query."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "space": {
                        "type": "string",
                        "description": (
                            "Optional wiki folder / space prefix, e.g. 'Inputs' "
                            "(lists documents under that folder)"
                        ),
                    },
                    "query": {
                        "type": "string",
                        "description": (
                            "Optional additional Polarion Lucene query AND-ed "
                            "with the project filter"
                        ),
                    },
                    "limit": {
                        "type": "integer",
                        "description": (
                            "Maximum number of documents to return (default 200)"
                        ),
                        "default": 200,
                    },
                },
            },
        ),
        types.Tool(
            name="list_document_work_items",
            description=(
                "List work items owned by (contained in) a Polarion document or LiveDoc "
                "module, identified by document name. Optionally provide the space "
                "(wiki folder path) when the name alone is ambiguous."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "document_name": {
                        "type": "string",
                        "description": "Title or name of the Polarion document/module",
                    },
                    "space": {
                        "type": "string",
                        "description": (
                            "Space (folder) path that contains the document "
                            "(e.g. 'Inputs' or 'Design/Specifications'). Optional."
                        ),
                    },
                    "limit": {
                        "type": "integer",
                        "description": (
                            "Maximum number of work items to return "
                            "(default 200)"
                        ),
                        "default": 200,
                    },
                },
                "required": ["document_name"],
            },
        ),
        types.Tool(
            name="list_configuration_srs_inventory",
            description=(
                "List configuration SRS rows in a Polarion document as a table with "
                "Configured By values and in_scope flags for a config tool "
                "(default: in_scope when Configured By includes 'Facility Config'). "
                "Use for Connex 360 SRS Configuration scope audits."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "document_name": {
                        "type": "string",
                        "description": (
                            "Polarion document/module title "
                            "(e.g. 'Connex 360 SRS Configuration')"
                        ),
                    },
                    "space": {
                        "type": "string",
                        "description": (
                            "Wiki folder containing the document (e.g. 'Inputs')"
                        ),
                    },
                    "project_id": {
                        "type": "string",
                        "description": (
                            "Polarion project id when it differs from "
                            "POLARION_PROJECT (e.g. 'midacuityvitals')"
                        ),
                    },
                    "tool_configured_by": {
                        "type": "string",
                        "description": (
                            "Configured By label for this tool "
                            "(default 'Facility Config')"
                        ),
                        "default": "Facility Config",
                    },
                    "work_item_type": {
                        "type": "string",
                        "description": "Work item type filter (default 'config')",
                        "default": "config",
                    },
                    "limit": {
                        "type": "integer",
                        "description": "Maximum rows (default 512)",
                        "default": 512,
                    },
                },
                "required": ["document_name"],
            },
        ),
        types.Tool(
            name="get_document_text",
            description=(
                "Return free text from a Polarion document identified by name: "
                "home page content plus narrative/heading text from work items "
                "in the document (HTML/plain as stored in Polarion)."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "document_name": {
                        "type": "string",
                        "description": "Title or name of the Polarion document/module",
                    },
                    "space": {
                        "type": "string",
                        "description": (
                            "Space (folder) path that contains the document "
                            "(e.g. 'Inputs'). Optional."
                        ),
                    },
                    "include_work_item_text": {
                        "type": "boolean",
                        "description": (
                            "Include description text from work items in the "
                            "document body (default true)"
                        ),
                        "default": True,
                    },
                },
                "required": ["document_name"],
            },
        ),
        types.Tool(
            name="get_document_work_items",
            description=(
                "Alias for list_document_work_items (returns work_items array only). "
                "List work items in a Polarion document identified by name."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "document_name": {
                        "type": "string",
                        "description": "Title or name of the Polarion document/module",
                    },
                    "space": {
                        "type": "string",
                        "description": (
                            "Space (folder) path that contains the document "
                            "(e.g. 'Design/Specifications'). Optional."
                        ),
                    },
                    "limit": {
                        "type": "integer",
                        "description": (
                            "Maximum number of work items to return "
                            "(default 200)"
                        ),
                        "default": 200,
                    },
                },
                "required": ["document_name"],
            },
        ),
        types.Tool(
            name="list_work_item_comments",
            description=(
                "List comments on a Polarion work item (author, text, timestamps, "
                "replies via parent/child URIs)."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": "Work item ID, e.g. PROJ-123",
                    }
                },
                "required": ["work_item_id"],
            },
        ),
        types.Tool(
            name="add_work_item_comment",
            description=(
                "Add a comment on a work item via TrackerWebService.addComment. "
                "Optional parent_comment_uri for replies."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": "Work item ID, e.g. PROJ-123",
                    },
                    "text": {
                        "type": "string",
                        "description": "Comment body (plain text or HTML)",
                    },
                    "title": {
                        "type": "string",
                        "description": "Optional comment title",
                        "default": "",
                    },
                    "parent_comment_uri": {
                        "type": "string",
                        "description": (
                            "Optional URI of a parent comment to reply to "
                            "(from list_work_item_comments)"
                        ),
                        "default": "",
                    },
                    "content_type": {
                        "type": "string",
                        "description": "MIME type for the body (default text/plain)",
                        "default": "text/plain",
                    },
                },
                "required": ["work_item_id", "text"],
            },
        ),
        types.Tool(
            name="list_reviewers",
            description=(
                "List reviewers (approvees) on a Polarion work item and each approval "
                "status (e.g. waiting, approved, disapproved)."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": (
                            "Full work item ID in PROJECT-NUMBER format "
                            "(e.g. PROJ-123)"
                        ),
                    }
                },
                "required": ["work_item_id"],
            },
        ),
        types.Tool(
            name="add_reviewer",
            description=(
                "Add a reviewer (approvee) to a work item via TrackerWebService.addApprovee."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": "Work item ID, e.g. PROJ-123",
                    },
                    "reviewer_id": {
                        "type": "string",
                        "description": "Polarion user id of the reviewer to add",
                    },
                },
                "required": ["work_item_id", "reviewer_id"],
            },
        ),
        types.Tool(
            name="remove_reviewer",
            description=(
                "Remove a reviewer from a work item via TrackerWebService.removeApprovee."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": "Work item ID, e.g. PROJ-123",
                    },
                    "reviewer_id": {
                        "type": "string",
                        "description": "Polarion user id of the reviewer to remove",
                    },
                },
                "required": ["work_item_id", "reviewer_id"],
            },
        ),
        types.Tool(
            name="reset_review_status",
            description=(
                "Reset review/approval status to waiting for one or all reviewers on a "
                "work item via TrackerWebService.editApproval."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "work_item_id": {
                        "type": "string",
                        "description": "Work item ID, e.g. PROJ-123",
                    },
                    "reviewer_ids": {
                        "type": "array",
                        "items": {"type": "string"},
                        "description": (
                            "Optional list of reviewer user ids to reset. "
                            "When omitted, all reviewers on the item are reset to waiting."
                        ),
                    },
                },
                "required": ["work_item_id"],
            },
        ),
        types.Tool(
            name="create_test_result_record",
            description=(
                "Create a Polarion test record on a Test Run: records pass/fail for a "
                "test case work item and optional evidence text. Requires POLARION_PROJECT. "
                "Provide either test_run_work_item_id or test_run_uri (Subterra URI of the Test Run)."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "test_case_work_item_id": {
                        "type": "string",
                        "description": (
                            "Work item ID of the executed test case "
                            "(e.g. PROJ-TC-12)."
                        ),
                    },
                    "test_run_work_item_id": {
                        "type": "string",
                        "description": (
                            "Work item ID of the Test Run to append the record to. "
                            "Omit if test_run_uri is provided."
                        ),
                        "default": "",
                    },
                    "test_run_uri": {
                        "type": "string",
                        "description": (
                            "Optional full Subterra URI of the Test Run; overrides "
                            "test_run_work_item_id when set."
                        ),
                        "default": "",
                    },
                    "result": {
                        "type": "string",
                        "enum": ["pass", "fail"],
                        "description": "Verdict for this execution.",
                    },
                    "evidence": {
                        "type": "string",
                        "description": (
                            "Optional evidence or notes (stored as the test record comment)."
                        ),
                        "default": "",
                    },
                },
                "required": ["test_case_work_item_id", "result"],
            },
        ),
    ]


# ---------------------------------------------------------------------------
# Tool dispatch
# ---------------------------------------------------------------------------

def _json(obj: Any) -> str:
    return json.dumps(obj, default=str, indent=2)


@app.call_tool()
async def call_tool(
    name: str, arguments: dict
) -> list[types.TextContent]:
    loop = asyncio.get_running_loop()

    try:
        client = _get_client()

        if name == "get_work_item":
            result = await loop.run_in_executor(
                None,
                client.get_work_item,
                arguments["work_item_id"],
            )

        elif name == "set_work_item_status":
            action_id = arguments.get("workflow_action_id")
            result = await loop.run_in_executor(
                None,
                lambda: client.set_work_item_status(
                    arguments["work_item_id"],
                    status=arguments.get("status", ""),
                    workflow_action_id=int(action_id) if action_id is not None else None,
                    workflow_action_name=arguments.get("workflow_action_name", ""),
                ),
            )

        elif name == "list_work_item_workflow_actions":
            result = await loop.run_in_executor(
                None,
                client.list_work_item_workflow_actions,
                arguments["work_item_id"],
            )

        elif name == "get_work_item_links":
            result = await loop.run_in_executor(
                None,
                client.get_work_item_links,
                arguments["work_item_id"],
            )

        elif name == "get_work_item_raw_fields":
            result = await loop.run_in_executor(
                None,
                lambda: client.get_work_item_raw_fields(
                    arguments["work_item_id"],
                    keys=list(arguments["keys"]),
                ),
            )

        elif name == "add_work_item_link":
            result = await loop.run_in_executor(
                None,
                lambda: client.add_work_item_link(
                    arguments["work_item_id"],
                    arguments["linked_work_item_id"],
                    link_role=arguments.get("link_role", ""),
                ),
            )

        elif name == "remove_work_item_link":
            result = await loop.run_in_executor(
                None,
                lambda: client.remove_work_item_link(
                    arguments["work_item_id"],
                    arguments["linked_work_item_id"],
                    link_role=arguments.get("link_role", ""),
                ),
            )

        elif name == "query_work_items":
            result = await loop.run_in_executor(
                None,
                lambda: client.query_work_items(
                    type_filter=arguments.get("type", ""),
                    status_filter=arguments.get("status", ""),
                    extra_query=arguments.get("query", ""),
                    limit=int(arguments.get("limit", 50)),
                ),
            )

        elif name == "get_document_text":
            include_body = arguments.get("include_work_item_text", True)
            result = await loop.run_in_executor(
                None,
                lambda: client.get_document_text(
                    arguments["document_name"],
                    space=arguments.get("space", ""),
                    include_work_item_text=bool(include_body),
                ),
            )

        elif name == "list_documents":
            result = await loop.run_in_executor(
                None,
                lambda: client.list_documents(
                    space=arguments.get("space", ""),
                    query=arguments.get("query", ""),
                    limit=int(arguments.get("limit", 200)),
                ),
            )

        elif name == "list_configuration_srs_inventory":
            result = await loop.run_in_executor(
                None,
                lambda: client.list_configuration_srs_inventory(
                    arguments["document_name"],
                    space=arguments.get("space", ""),
                    project_id=arguments.get("project_id", ""),
                    tool_configured_by=arguments.get(
                        "tool_configured_by", "Facility Config"
                    ),
                    work_item_type=arguments.get("work_item_type", "config"),
                    limit=int(arguments.get("limit", 512)),
                ),
            )

        elif name in ("list_document_work_items", "get_document_work_items"):
            if name == "list_document_work_items":
                result = await loop.run_in_executor(
                    None,
                    lambda: client.list_document_work_items(
                        arguments["document_name"],
                        space=arguments.get("space", ""),
                        limit=int(arguments.get("limit", 200)),
                        project_id=arguments.get("project_id", ""),
                    ),
                )
            else:
                result = await loop.run_in_executor(
                    None,
                    lambda: client.get_document_work_items(
                        arguments["document_name"],
                        space=arguments.get("space", ""),
                        limit=int(arguments.get("limit", 200)),
                    ),
                )

        elif name == "list_work_item_comments":
            result = await loop.run_in_executor(
                None,
                client.list_work_item_comments,
                arguments["work_item_id"],
            )

        elif name == "add_work_item_comment":
            result = await loop.run_in_executor(
                None,
                lambda: client.add_work_item_comment(
                    arguments["work_item_id"],
                    arguments["text"],
                    title=arguments.get("title", ""),
                    parent_comment_uri=arguments.get("parent_comment_uri", ""),
                    content_type=arguments.get("content_type", "text/plain"),
                ),
            )

        elif name == "list_reviewers":
            result = await loop.run_in_executor(
                None,
                client.list_reviewers,
                arguments["work_item_id"],
            )

        elif name == "add_reviewer":
            result = await loop.run_in_executor(
                None,
                lambda: client.add_reviewer(
                    arguments["work_item_id"],
                    arguments["reviewer_id"],
                ),
            )

        elif name == "remove_reviewer":
            result = await loop.run_in_executor(
                None,
                lambda: client.remove_reviewer(
                    arguments["work_item_id"],
                    arguments["reviewer_id"],
                ),
            )

        elif name == "reset_review_status":
            reviewer_ids = arguments.get("reviewer_ids")
            result = await loop.run_in_executor(
                None,
                lambda: client.reset_review_status(
                    arguments["work_item_id"],
                    reviewer_ids=reviewer_ids if reviewer_ids is not None else None,
                ),
            )

        elif name == "create_test_result_record":
            result = await loop.run_in_executor(
                None,
                lambda: client.create_test_result_record(
                    arguments["test_case_work_item_id"],
                    test_run_work_item_id=arguments.get("test_run_work_item_id", ""),
                    test_run_uri=arguments.get("test_run_uri", ""),
                    result=arguments["result"],
                    evidence=arguments.get("evidence", ""),
                ),
            )

        else:
            raise ValueError(f"Unknown tool: {name!r}")

        return [types.TextContent(type="text", text=_json(result))]

    except (KeyError, TypeError, ValueError) as exc:
        logger.warning("Tool %r called with invalid arguments: %s", name, exc)
        return [
            types.TextContent(
                type="text",
                text=_json({"error": f"Invalid arguments: {exc}"}),
            )
        ]
    except RuntimeError as exc:
        logger.error("Configuration error in tool %r: %s", name, exc)
        return [
            types.TextContent(
                type="text",
                text=_json({"error": f"Configuration error: {exc}"}),
            )
        ]
    except zeep.exceptions.Fault as exc:
        logger.error("Polarion SOAP fault in tool %r: %s", name, exc)
        return [
            types.TextContent(
                type="text",
                text=_json({"error": f"Polarion SOAP error: {exc.message}"}),
            )
        ]
    except Exception as exc:  # noqa: BLE001
        logger.exception("Unexpected error in tool %r", name)
        return [
            types.TextContent(
                type="text",
                text=_json({"error": f"Unexpected error: {exc}"}),
            )
        ]


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

async def _async_main() -> None:
    async with stdio_server() as (read_stream, write_stream):
        await app.run(
            read_stream,
            write_stream,
            app.create_initialization_options(),
        )


def main() -> None:
    asyncio.run(_async_main())


if __name__ == "__main__":
    main()
