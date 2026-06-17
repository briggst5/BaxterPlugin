#!/usr/bin/env python3
"""Manual test script for PolarionMCP.

Connects directly to a Polarion instance using the same PolarionClient that
the MCP server uses, and calls one of the three available tools.

Credentials are read from ``~/.config/polarion-mcp.env`` (see ``polarion-mcp.env.example``).

Usage examples
--------------

  # Fetch a single work item by ID:
  python scripts/test_tools.py get_work_item PROJ-123

  # Query all open defects (up to 20):
  python scripts/test_tools.py query_work_items --type defect --status open --limit 20

  # Query with an arbitrary Lucene expression:
  python scripts/test_tools.py query_work_items --query "title:login AND priority:high"

  # List work items in a document (space optional):
  python scripts/test_tools.py get_document_work_items "My Spec Document"
  python scripts/test_tools.py get_document_work_items "My Spec Document" --space "Design/Specs"

Environment variables
---------------------
  POLARION_URL       Base URL, e.g. http://polarion.example.com/polarion
  POLARION_USER      Polarion username
  POLARION_PASSWORD  Polarion password (omit if POLARION_PAT is set)
  POLARION_PAT       Polarion personal access token (omit if POLARION_PASSWORD is set)
  POLARION_PROJECT   Project ID (optional; prepended to every query)

Copy ``polarion-mcp.env.example`` to ``~/.config/polarion-mcp.env`` and fill in your values.
"""

from __future__ import annotations

import argparse
import json
import logging
import os
import sys


# ---------------------------------------------------------------------------
# Argument parsing
# ---------------------------------------------------------------------------

def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="test_tools",
        description="Manually test the PolarionMCP tools against a real Polarion instance.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument(
        "--debug",
        action="store_true",
        help="Enable DEBUG-level logging (shows SOAP requests/responses).",
    )

    sub = parser.add_subparsers(dest="command", required=True)

    # -- get_work_item -------------------------------------------------------
    p_get = sub.add_parser(
        "get_work_item",
        help="Retrieve a single work item by its full ID (e.g. PROJ-123).",
    )
    p_get.add_argument("work_item_id", help="Work item ID, e.g. PROJ-123")

    # -- set_work_item_status ------------------------------------------------
    p_status = sub.add_parser(
        "set_work_item_status",
        help="Change work item status via Polarion workflow.",
    )
    p_status.add_argument("work_item_id", help="Work item ID, e.g. PROJ-123")
    p_status.add_argument(
        "status",
        nargs="?",
        default="",
        help="Target status enum id, e.g. closed, inProgress",
    )
    p_status.add_argument(
        "--action-name",
        dest="workflow_action_name",
        default="",
        metavar="NAME",
        help="Workflow action name instead of status",
    )
    p_status.add_argument(
        "--action-id",
        dest="workflow_action_id",
        type=int,
        default=None,
        metavar="ID",
        help="Workflow action id from list_work_item_workflow_actions",
    )

    # -- list_work_item_workflow_actions -------------------------------------
    p_wf = sub.add_parser(
        "list_work_item_workflow_actions",
        help="List available workflow actions for a work item.",
    )
    p_wf.add_argument("work_item_id", help="Work item ID, e.g. PROJ-123")

    # -- get_work_item_links -------------------------------------------------
    p_links = sub.add_parser(
        "get_work_item_links",
        help="Retrieve links for a single work item by its full ID (e.g. PROJ-123).",
    )
    p_links.add_argument("work_item_id", help="Work item ID, e.g. PROJ-123")

    # -- add_work_item_link --------------------------------------------------
    p_add_link = sub.add_parser(
        "add_work_item_link",
        help="Link one work item to another.",
    )
    p_add_link.add_argument("work_item_id", help="Source work item ID, e.g. PROJ-123")
    p_add_link.add_argument(
        "linked_work_item_id", help="Target work item ID to link to"
    )
    p_add_link.add_argument(
        "link_role",
        help="Polarion link role enum id, e.g. relates_to, verifies, implements",
    )

    # -- remove_work_item_link -----------------------------------------------
    p_rm_link = sub.add_parser(
        "remove_work_item_link",
        help="Remove a link between two work items.",
    )
    p_rm_link.add_argument("work_item_id", help="Source work item ID, e.g. PROJ-123")
    p_rm_link.add_argument(
        "linked_work_item_id", help="Target work item ID to unlink"
    )
    p_rm_link.add_argument(
        "link_role",
        help="Polarion link role enum id used when the link was created",
    )

    # -- query_work_items ----------------------------------------------------
    p_query = sub.add_parser(
        "query_work_items",
        help="Query work items by type, status, and/or a Lucene query string.",
    )
    p_query.add_argument(
        "--type",
        dest="type_filter",
        default="",
        metavar="TYPE",
        help="Work item type, e.g. requirement, task, defect, testcase",
    )
    p_query.add_argument(
        "--status",
        dest="status_filter",
        default="",
        metavar="STATUS",
        help="Work item status, e.g. open, inProgress, closed, resolved",
    )
    p_query.add_argument(
        "--query",
        dest="extra_query",
        default="",
        metavar="LUCENE",
        help="Additional Polarion Lucene query, e.g. \"title:login AND priority:high\"",
    )
    p_query.add_argument(
        "--limit",
        type=int,
        default=50,
        metavar="N",
        help="Maximum number of work items to return (default: 50)",
    )

    # -- list_documents ------------------------------------------------------
    p_docs = sub.add_parser(
        "list_documents",
        help="Enumerate documents (modules) in the configured Polarion project.",
    )
    p_docs.add_argument(
        "--space",
        default="",
        metavar="SPACE",
        help="Optional wiki folder prefix, e.g. Inputs",
    )
    p_docs.add_argument(
        "--query",
        default="",
        metavar="LUCENE",
        help="Optional additional Polarion Lucene query",
    )
    p_docs.add_argument(
        "--limit",
        type=int,
        default=200,
        metavar="N",
        help="Maximum number of documents to return (default: 200)",
    )

    # -- list_document_work_items --------------------------------------------
    p_doc = sub.add_parser(
        "list_document_work_items",
        help="List work items in a Polarion document identified by name.",
    )
    p_doc.add_argument("document_name", help="Title or name of the document/module")
    p_doc.add_argument(
        "--space",
        default="",
        metavar="SPACE",
        help="Space (folder) path containing the document, e.g. Design/Specifications",
    )
    p_doc.add_argument(
        "--limit",
        type=int,
        default=200,
        metavar="N",
        help="Maximum number of work items to return (default: 200)",
    )

    # -- get_document_text ---------------------------------------------------
    p_doc_text = sub.add_parser(
        "get_document_text",
        help="Return free text from a Polarion document identified by name.",
    )
    p_doc_text.add_argument("document_name", help="Title or name of the document/module")
    p_doc_text.add_argument(
        "--space",
        default="",
        metavar="SPACE",
        help="Space (folder) path containing the document",
    )
    p_doc_text.add_argument(
        "--home-page-only",
        action="store_true",
        help="Return only home page content (skip work item narrative text)",
    )

    # -- get_document_work_items (alias) -------------------------------------
    p_doc_alias = sub.add_parser(
        "get_document_work_items",
        help="Alias for list_document_work_items (returns work_items array only).",
    )
    p_doc_alias.add_argument("document_name", help="Title or name of the document/module")
    p_doc_alias.add_argument(
        "--space",
        default="",
        metavar="SPACE",
        help="Space (folder) path containing the document, e.g. Design/Specifications",
    )
    p_doc_alias.add_argument(
        "--limit",
        type=int,
        default=200,
        metavar="N",
        help="Maximum number of work items to return (default: 200)",
    )

    # -- list_work_item_comments ---------------------------------------------
    p_lc = sub.add_parser(
        "list_work_item_comments",
        help="List comments on a work item.",
    )
    p_lc.add_argument("work_item_id", help="Work item ID, e.g. PROJ-123")

    # -- add_work_item_comment -----------------------------------------------
    p_ac = sub.add_parser(
        "add_work_item_comment",
        help="Add a comment on a work item.",
    )
    p_ac.add_argument("work_item_id", help="Work item ID, e.g. PROJ-123")
    p_ac.add_argument("text", help="Comment body")
    p_ac.add_argument(
        "--title",
        default="",
        help="Optional comment title",
    )
    p_ac.add_argument(
        "--parent-uri",
        dest="parent_comment_uri",
        default="",
        metavar="URI",
        help="Parent comment URI for a reply",
    )
    p_ac.add_argument(
        "--content-type",
        dest="content_type",
        default="text/plain",
        help="MIME type for the body (default: text/plain)",
    )

    # -- list_reviewers ------------------------------------------------------
    p_lr = sub.add_parser(
        "list_reviewers",
        help="List reviewers and approval status on a work item.",
    )
    p_lr.add_argument("work_item_id", help="Work item ID, e.g. PROJ-123")

    # -- add_reviewer --------------------------------------------------------
    p_ar = sub.add_parser(
        "add_reviewer",
        help="Add a reviewer (approvee) to a work item.",
    )
    p_ar.add_argument("work_item_id", help="Work item ID, e.g. PROJ-123")
    p_ar.add_argument("reviewer_id", help="Polarion user id of the reviewer")

    # -- remove_reviewer -----------------------------------------------------
    p_rr = sub.add_parser(
        "remove_reviewer",
        help="Remove a reviewer from a work item.",
    )
    p_rr.add_argument("work_item_id", help="Work item ID, e.g. PROJ-123")
    p_rr.add_argument("reviewer_id", help="Polarion user id of the reviewer")

    # -- reset_review_status -------------------------------------------------
    p_reset = sub.add_parser(
        "reset_review_status",
        help="Reset approval status to waiting for all or selected reviewers.",
    )
    p_reset.add_argument("work_item_id", help="Work item ID, e.g. PROJ-123")
    p_reset.add_argument(
        "--reviewer",
        dest="reviewer_ids",
        action="append",
        default=None,
        metavar="USER",
        help="Reset only this reviewer (repeatable). Omit to reset all reviewers.",
    )

    # -- create_test_result_record ------------------------------------------
    p_tr = sub.add_parser(
        "create_test_result_record",
        help="Add a pass/fail test record to a Polarion Test Run for a test case work item.",
    )
    p_tr.add_argument(
        "test_case_work_item_id",
        help="Test case work item ID, e.g. PROJ-TC-12",
    )
    p_tr.add_argument(
        "--test-run",
        dest="test_run_work_item_id",
        default="",
        metavar="WI",
        help="Test Run work item ID (required unless --test-run-uri is set)",
    )
    p_tr.add_argument(
        "--test-run-uri",
        dest="test_run_uri",
        default="",
        metavar="URI",
        help="Full Subterra URI of the Test Run (overrides --test-run)",
    )
    p_tr.add_argument(
        "--result",
        required=True,
        choices=["pass", "fail"],
        help="Test verdict",
    )
    p_tr.add_argument(
        "--evidence",
        default="",
        help="Optional evidence or notes (test record comment)",
    )

    return parser


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def main() -> None:
    from polarion_mcp.config import CONFIG_PATH, load_config  # noqa: PLC0415

    load_config()

    parser = _build_parser()
    args = parser.parse_args()

    logging.basicConfig(
        level=logging.DEBUG if args.debug else logging.WARNING,
        format="%(levelname)s %(name)s: %(message)s",
    )

    # Import after env loading so POLARION_* vars are available
    from polarion_mcp.polarion_client import PolarionClient  # noqa: PLC0415

    url = os.environ.get("POLARION_URL", "").strip()
    user = os.environ.get("POLARION_USER", "").strip()
    password = os.environ.get("POLARION_PASSWORD", "").strip()
    pat = os.environ.get("POLARION_PAT", "").strip()
    project = os.environ.get("POLARION_PROJECT", "")

    missing: list[str] = []
    if not url:
        missing.append("POLARION_URL")
    if not user:
        missing.append("POLARION_USER")
    if not password and not pat:
        missing.append("POLARION_PASSWORD or POLARION_PAT")
    if missing:
        print(
            f"ERROR: Required environment variable(s) not set: {', '.join(missing)}\n"
            f"Set them in {CONFIG_PATH} (see polarion-mcp.env.example in the repo).",
            file=sys.stderr,
        )
        sys.exit(1)

    print(f"Connecting to {url} as {user} (project: {project or '<none>'}) …")
    client = PolarionClient(
        url=url,
        username=user,
        password=password,
        project_id=project,
        personal_access_token=pat,
    )

    try:
        if args.command == "get_work_item":
            result = client.get_work_item(args.work_item_id)

        elif args.command == "set_work_item_status":
            result = client.set_work_item_status(
                args.work_item_id,
                status=args.status,
                workflow_action_id=args.workflow_action_id,
                workflow_action_name=args.workflow_action_name,
            )

        elif args.command == "list_work_item_workflow_actions":
            result = client.list_work_item_workflow_actions(args.work_item_id)

        elif args.command == "get_work_item_links":
            result = client.get_work_item_links(args.work_item_id)

        elif args.command == "add_work_item_link":
            result = client.add_work_item_link(
                args.work_item_id,
                args.linked_work_item_id,
                link_role=args.link_role,
            )

        elif args.command == "remove_work_item_link":
            result = client.remove_work_item_link(
                args.work_item_id,
                args.linked_work_item_id,
                link_role=args.link_role,
            )

        elif args.command == "query_work_items":
            result = client.query_work_items(
                type_filter=args.type_filter,
                status_filter=args.status_filter,
                extra_query=args.extra_query,
                limit=args.limit,
            )
            print(f"Returned {len(result)} work item(s).")

        elif args.command == "list_documents":
            result = client.list_documents(
                space=args.space,
                query=args.query,
                limit=args.limit,
            )
            print(f"Returned {result['count']} document(s).")

        elif args.command == "get_document_text":
            result = client.get_document_text(
                args.document_name,
                space=args.space,
                include_work_item_text=not args.home_page_only,
            )

        elif args.command == "list_document_work_items":
            result = client.list_document_work_items(
                args.document_name,
                space=args.space,
                limit=args.limit,
            )
            print(f"Returned {result['count']} work item(s).")

        elif args.command == "get_document_work_items":
            result = client.get_document_work_items(
                args.document_name,
                space=args.space,
                limit=args.limit,
            )
            print(f"Returned {len(result)} work item(s).")

        elif args.command == "list_work_item_comments":
            result = client.list_work_item_comments(args.work_item_id)
            if "count" in result:
                print(f"Returned {result['count']} comment(s).")

        elif args.command == "add_work_item_comment":
            result = client.add_work_item_comment(
                args.work_item_id,
                args.text,
                title=args.title,
                parent_comment_uri=args.parent_comment_uri,
                content_type=args.content_type,
            )

        elif args.command == "list_reviewers":
            result = client.list_reviewers(args.work_item_id)

        elif args.command == "add_reviewer":
            result = client.add_reviewer(args.work_item_id, args.reviewer_id)

        elif args.command == "remove_reviewer":
            result = client.remove_reviewer(args.work_item_id, args.reviewer_id)

        elif args.command == "reset_review_status":
            result = client.reset_review_status(
                args.work_item_id,
                reviewer_ids=args.reviewer_ids,
            )

        elif args.command == "create_test_result_record":
            result = client.create_test_result_record(
                args.test_case_work_item_id,
                test_run_work_item_id=args.test_run_work_item_id,
                test_run_uri=args.test_run_uri,
                result=args.result,
                evidence=args.evidence,
            )

        else:
            print(f"Unknown command: {args.command}", file=sys.stderr)
            sys.exit(1)

    except Exception as exc:  # noqa: BLE001
        print(f"ERROR: {exc}", file=sys.stderr)
        if args.debug:
            import traceback
            traceback.print_exc()
        sys.exit(1)

    print(json.dumps(result, default=str, indent=2))


if __name__ == "__main__":
    main()
