"""Polarion SOAP/WSDL client.

Wraps Polarion's SessionWebService, TrackerWebService, and (where needed)
TestManagementWebService so MCP tools can call Polarion without zeep details.
"""

from __future__ import annotations

import logging
import os
import re
from urllib.parse import unquote
from typing import Any

import zeep
import zeep.plugins
import zeep.transports
from lxml import etree
from requests import Session
from zeep.helpers import serialize_object
from zeep.wsse import UsernameToken

logger = logging.getLogger(__name__)

# Work item fields to request from Polarion
_DEFAULT_FIELDS = [
    "id",
    "title",
    "type",
    "status",
    "priority",
    "severity",
    "assignee",
    "description",
    "created",
    "updated",
    "resolution",
]


def _wsdl_url(base_url: str, service: str) -> str:
    return f"{base_url.rstrip('/')}/ws/services/{service}?wsdl"


def _system_ca_bundle() -> str | None:
    """Return a best-effort system CA bundle path.

    Requests uses `certifi` by default. In corporate environments, the OS trust
    store is often where the internal CA is installed, so prefer common OS
    bundle locations when they exist.
    """
    candidates = [
        "/etc/ssl/certs/ca-certificates.crt",  # Debian/Ubuntu
        "/etc/pki/tls/certs/ca-bundle.crt",  # RHEL/CentOS/Fedora
        "/etc/ssl/ca-bundle.pem",  # SLES/OpenSUSE (common)
        "/etc/ssl/cert.pem",  # Alpine/macOS (sometimes)
    ]
    for path in candidates:
        if os.path.isfile(path):
            return path
    return None


class _PolarionSessionPlugin(zeep.plugins.Plugin):
    """Zeep plugin that injects the Polarion session ID into every outgoing
    SOAP envelope header.

    Polarion's ``DoAsUserWrapper`` resolves the caller's identity from a
    namespaced ``<sessionID>`` element in the SOAP ``<Header>``.  The
    required namespace is ``http://ws.polarion.com/SessionWebService-impl``,
    which is the same namespace used by Polarion's own .NET SDK
    ``SessionIdBehavior``.  A plain (un-namespaced) ``<sessionID>`` element
    is silently ignored and the call fails with "Not authorized."
    """

    _SOAP_ENV_NS = "http://schemas.xmlsoap.org/soap/envelope/"
    # Namespace varies by Polarion version/config. Some instances return and
    # expect the session header in `http://ws.polarion.com/session`, while
    # others use the `SessionWebService-impl` namespace.
    _DEFAULT_SESSION_NS = "http://ws.polarion.com/SessionWebService-impl"

    def __init__(self, session_id: str, *, session_ns: str | None = None) -> None:
        # lxml expects a string; some SOAP stacks can return non-str scalars.
        self.session_id = str(session_id) if session_id is not None else ""
        self.session_ns = session_ns or self._DEFAULT_SESSION_NS

    def egress(self, envelope, http_headers, operation, binding_options):  # type: ignore[override]
        header = envelope.find(f"{{{self._SOAP_ENV_NS}}}Header")
        if header is None:
            # SOAP requires Header to appear before Body.
            header = etree.Element(f"{{{self._SOAP_ENV_NS}}}Header")
            body = envelope.find(f"{{{self._SOAP_ENV_NS}}}Body")
            if body is not None:
                envelope.insert(envelope.index(body), header)
            else:
                envelope.insert(0, header)
        etree.SubElement(header, f"{{{self.session_ns}}}sessionID").text = self.session_id
        return envelope, http_headers


def _format_module_summary(raw: Any) -> dict[str, Any]:
    """Flatten a Polarion Module/Document for list responses."""
    if not isinstance(raw, dict):
        return {"title": str(raw) if raw is not None else None}

    mod_type = raw.get("type")
    status = raw.get("status")
    type_id = mod_type.get("id") if isinstance(mod_type, dict) else mod_type
    status_id = status.get("id") if isinstance(status, dict) else status

    folder = (raw.get("moduleFolder") or "").strip()
    title = (raw.get("title") or raw.get("moduleName") or "").strip()
    space: str | None = None
    if folder and "/" in folder:
        space = folder.rsplit("/", 1)[0]
    elif folder:
        space = folder

    return {
        "id": raw.get("id"),
        "title": title or None,
        "module_name": raw.get("moduleName"),
        "module_folder": folder or None,
        "space": space,
        "uri": raw.get("uri"),
        "type": type_id,
        "status": status_id,
    }


def _format_polarion_text(val: Any) -> dict[str, str]:
    """Normalize a Polarion ``Text`` object to type + content."""
    if not isinstance(val, dict):
        text = str(val).strip() if val is not None else ""
        return {"type": "text/plain", "content": text}
    return {
        "type": (val.get("type") or "text/plain"),
        "content": (val.get("content") or "").strip(),
    }


def _format_work_item(raw: dict) -> dict:
    """Flatten a serialised zeep WorkItem dict into a plain, JSON-friendly dict."""
    result: dict[str, Any] = {}

    for key, val in raw.items():
        if val is None:
            continue

        if key == "description":
            # Polarion descriptions are Text objects: {"type": "text/html", "content": "..."}
            if isinstance(val, dict):
                result[key] = val.get("content", "")
            else:
                result[key] = str(val)

        elif isinstance(val, dict):
            # EnumOption, User, Category, etc. – expose just the 'id' string
            id_val = val.get("id")
            if id_val is not None:
                result[key] = id_val
            else:
                # fall back to the full dict so nothing is lost
                result[key] = val

        elif isinstance(val, list):
            # Lists of references (e.g. assignee can be multi-valued)
            result[key] = [
                v.get("id", str(v)) if isinstance(v, dict) else str(v)
                for v in val
            ]

        else:
            result[key] = val

    return result


def _format_comment(raw: Any) -> dict[str, Any]:
    """Flatten a Polarion Comment object for JSON responses."""
    if not isinstance(raw, dict):
        return {"id": None, "uri": None, "text": str(raw) if raw is not None else None}

    author = raw.get("author")
    author_id: str | None = None
    if isinstance(author, dict):
        author_id = author.get("id") or author.get("name")
    elif author is not None:
        author_id = str(author)

    text_field = _format_polarion_text(raw.get("text"))

    child_uris = raw.get("childCommentURIs")
    children: list[str] = []
    if child_uris is not None:
        if isinstance(child_uris, list):
            children = [str(u) for u in child_uris if u]
        else:
            children = [str(child_uris)]

    return {
        "id": raw.get("id"),
        "uri": raw.get("uri"),
        "title": raw.get("title"),
        "text": text_field["content"],
        "content_type": text_field["type"],
        "author": author_id,
        "created": raw.get("created"),
        "resolved": raw.get("resolved"),
        "parent_comment_uri": raw.get("parentCommentURI"),
        "child_comment_uris": children,
    }


def _format_approval(raw: Any) -> dict[str, Any]:
    """Flatten a Polarion Approval object into a JSON-friendly dict."""
    if not isinstance(raw, dict):
        return {"reviewer_id": None, "status": str(raw) if raw is not None else None}

    user = raw.get("user")
    status = raw.get("status")
    reviewer_id: str | None = None
    if isinstance(user, dict):
        reviewer_id = user.get("id") or user.get("name")
    elif user is not None:
        reviewer_id = str(user)

    status_id: str | None = None
    if isinstance(status, dict):
        status_id = status.get("id")
    elif status is not None:
        status_id = str(status)

    return {"reviewer_id": reviewer_id, "status": status_id}


def _format_workflow_action(raw: Any) -> dict[str, Any]:
    """Flatten a Polarion WorkflowAction for JSON responses."""
    if not isinstance(raw, dict):
        action_id = getattr(raw, "actionId", None)
        action_name = getattr(raw, "actionName", None)
        target_status = getattr(raw, "targetStatus", None)
        if action_id is None and action_name is None:
            return {"action_id": None, "action_name": None, "target_status": None}
        target_id = None
        if isinstance(target_status, dict):
            target_id = target_status.get("id")
        elif target_status is not None:
            target_id = getattr(target_status, "id", str(target_status))
        return {
            "action_id": action_id,
            "action_name": action_name,
            "target_status": target_id,
        }

    target = raw.get("targetStatus")
    target_id: str | None = None
    if isinstance(target, dict):
        target_id = target.get("id")
    elif target is not None:
        target_id = str(target)

    return {
        "action_id": raw.get("actionId"),
        "action_name": raw.get("actionName"),
        "target_status": target_id,
    }


def _work_item_prefix_project_map() -> dict[str, str]:
    """Optional map of work-item id prefix → Polarion project id (``SEY=midacuityvitals``)."""
    raw = os.environ.get("POLARION_WORK_ITEM_PREFIX_PROJECTS", "SEY=midacuityvitals").strip()
    mapping: dict[str, str] = {}
    for part in raw.split(","):
        piece = part.strip()
        if not piece or "=" not in piece:
            continue
        prefix, project = piece.split("=", 1)
        prefix = prefix.strip()
        project = project.strip()
        if prefix and project:
            mapping[prefix] = project
    return mapping


def _project_id_from_work_item_id(work_item_id: str, *, fallback: str = "") -> str:
    """Derive Polarion project id from a work item id (``PROJECT-123``)."""
    wid = (work_item_id or "").strip()
    match = re.fullmatch(r"(.+)-(\d+)", wid)
    if match:
        prefix = match.group(1)
        mapped = _work_item_prefix_project_map().get(prefix)
        if mapped:
            return mapped
        return prefix
    return (fallback or "").strip()


def _parse_configured_by_field(raw: Any) -> list[str]:
    """Normalize Polarion ``configuredby`` / custom field values to display strings."""
    if raw is None:
        return []

    if isinstance(raw, list):
        values: list[str] = []
        for item in raw:
            values.extend(_parse_configured_by_field(item))
        return values

    if isinstance(raw, dict):
        for key in ("name", "id", "label", "value", "content"):
            val = raw.get(key)
            if isinstance(val, str) and val.strip():
                return [val.strip()]
        return []

    text = str(raw).strip()
    if not text:
        return []

    # Multi-select custom fields are often newline-separated in exports.
    parts = [p.strip() for p in re.split(r"[\r\n,;]+", text) if p.strip()]
    return parts or [text]


class PolarionClient:
    """Thin synchronous wrapper around Polarion's SOAP web services."""

    def __init__(
        self,
        url: str,
        username: str,
        password: str,
        project_id: str,
        *,
        personal_access_token: str = "",
    ) -> None:
        self.base_url = url.rstrip("/")
        self.username = username
        self.password = password
        self.project_id = project_id
        self.personal_access_token = (personal_access_token or "").strip()

        self._tracker_client: zeep.Client | None = None
        self._test_mgmt_client: zeep.Client | None = None
        self._history: zeep.plugins.HistoryPlugin | None = None
        self._remaining_calls: int | None = _read_max_calls()

    def _consume_call(self, action: str) -> None:
        remaining = self._remaining_calls
        if remaining is None:
            return
        if remaining <= 0:
            raise RuntimeError(
                "Polarion call limit reached. "
                "Increase POLARION_MCP_MAX_CALLS or restart the server."
            )
        self._remaining_calls = remaining - 1
        logger.debug("Polarion call budget: %d remaining (%s)", self._remaining_calls, action)

    # ------------------------------------------------------------------
    # Connection management
    # ------------------------------------------------------------------

    def _resolve_session_id(self, session_id_raw: Any) -> tuple[str, str | None]:
        """Normalize logIn / logInWithToken return value and optional SOAP header fallback."""
        session_id = str(session_id_raw) if session_id_raw is not None else ""
        session_ns: str | None = None
        if (not session_id or session_id.strip().lower() in {"none", "null"}) and self._history is not None:
            try:
                received = self._history.last_received
                env = received.get("envelope") if received else None
                if env is not None:
                    header = env.find("{http://schemas.xmlsoap.org/soap/envelope/}Header")
                    if header is not None:
                        for el in header.iter():
                            if etree.QName(el).localname == "sessionID":
                                session_id = (el.text or "").strip()
                                session_ns = etree.QName(el).namespace
                                break
            except Exception:  # noqa: BLE001
                pass
        return session_id, session_ns

    def _attach_tracker_clients(
        self,
        transport: zeep.transports.Transport,
        wsse: UsernameToken,
        session_id: str,
        session_ns: str | None,
    ) -> None:
        """Create TrackerWebService and TestManagementWebService clients after login."""
        tracker_wsdl = _wsdl_url(self.base_url, "TrackerWebService")
        self._tracker_client = zeep.Client(
            tracker_wsdl,
            transport=transport,
            wsse=wsse,
            plugins=[self._history, _PolarionSessionPlugin(session_id, session_ns=session_ns)],
        )
        test_mgmt_wsdl = _wsdl_url(self.base_url, "TestManagementWebService")
        self._test_mgmt_client = zeep.Client(
            test_mgmt_wsdl,
            transport=transport,
            wsse=wsse,
            plugins=[self._history, _PolarionSessionPlugin(session_id, session_ns=session_ns)],
        )

    def connect(self) -> None:
        """Establish a SOAP session with Polarion.

        Authentication flow
        -------------------
        **Password (default)**

        1. **HTTP Basic Auth** – attached to the underlying ``requests.Session``
           so every HTTP request (WSDL fetches and SOAP calls) carries the
           ``Authorization`` header.
        2. **SessionWebService.logIn()** – creates a server-side session and
           returns a session ID string.  The session cookie is also stored in
           the shared ``requests.Session`` cookie jar.
        3. **``<sessionID>`` SOAP header** – Polarion's ``DoAsUserWrapper``
           resolves the caller's identity from this header element, not from
           HTTP cookies or WS-Security tokens.  A :class:`_PolarionSessionPlugin`
           injects it into every outgoing ``TrackerWebService`` envelope
           automatically.
        4. **WS-Security UsernameToken** – kept as a belt-and-suspenders layer
           for Polarion configurations that also check the WSSE header.

        **Personal access token (PAT)**

        When ``personal_access_token`` is set (``POLARION_PAT``), the client first
        tries ``SessionWebService.logInWithToken("AccessToken", …)`` with
        ``Authorization: Bearer`` on HTTP (Polarion 3.22.1+). If that fails, it
        falls back to treating the PAT like a password: HTTP Basic + ``logIn()``.
        """
        if self._tracker_client is not None:
            return

        # When debugging auth issues, we want to see the *actual* SOAP envelope
        # zeep sends on the wire (including headers).
        self._history = zeep.plugins.HistoryPlugin()

        pat = self.personal_access_token
        if pat:
            cred_hint = "POLARION_USER and POLARION_PAT"
            try:
                self._connect_with_pat(pat)
                return
            except Exception as pat_exc:
                logger.info("PAT primary auth failed (%s); trying PAT as password", pat_exc)
                try:
                    self._connect_with_password_like(self.username, pat, cred_hint=cred_hint)
                    return
                except Exception as pwd_exc:
                    raise RuntimeError(
                        f"Polarion PAT authentication failed ({cred_hint}). "
                        f"logInWithToken/Bearer: {pat_exc!r}; password-style PAT: {pwd_exc!r}"
                    ) from pwd_exc

        self._connect_with_password_like(
            self.username, self.password, cred_hint="POLARION_USER and POLARION_PASSWORD"
        )

    def _connect_with_pat(self, pat: str) -> None:
        """Bearer HTTP auth + SessionWebService.logInWithToken (AccessToken mechanism)."""
        http_session = Session()
        http_session.headers["Authorization"] = f"Bearer {pat}"
        ca_bundle = _system_ca_bundle()
        if ca_bundle is not None:
            http_session.verify = ca_bundle
        transport = zeep.transports.Transport(session=http_session)

        session_wsdl = _wsdl_url(self.base_url, "SessionWebService")
        session_client = zeep.Client(
            session_wsdl,
            transport=transport,
            wsse=None,
            plugins=[self._history],
        )
        svc = session_client.service
        if not hasattr(svc, "logInWithToken"):
            raise RuntimeError(
                "SessionWebService has no logInWithToken (requires Polarion 3.22.1 or newer for native PAT)."
            )
        try:
            self._consume_call("SessionWebService.logInWithToken")
            # JavaDoc allows null for AccessToken, but some WSDL/zeep stacks still require
            # the ``username`` element; Polarion accepts the owning user id here.
            session_id_raw = svc.logInWithToken("AccessToken", self.username, pat)
        except Exception as exc:
            raise RuntimeError(
                f"Polarion logInWithToken(AccessToken) failed – check POLARION_PAT. Detail: {exc}"
            ) from exc

        session_id, session_ns = self._resolve_session_id(session_id_raw)
        if not session_id or session_id.strip().lower() in {"none", "null"}:
            raise RuntimeError(
                "Polarion logInWithToken() did not yield a usable session id (body was empty and no sessionID header was found)."
            )
        logger.debug(
            "Logged in to Polarion with PAT as %s (session %s…)",
            self.username,
            session_id[:8],
        )

        wsse = UsernameToken(self.username, pat)
        self._attach_tracker_clients(transport, wsse, session_id, session_ns)

    def _connect_with_password_like(self, username: str, secret: str, *, cred_hint: str) -> None:
        """HTTP Basic + logIn(username, secret) + WS-Security UsernameToken."""
        http_session = Session()
        http_session.auth = (username, secret)
        ca_bundle = _system_ca_bundle()
        if ca_bundle is not None:
            http_session.verify = ca_bundle
        transport = zeep.transports.Transport(session=http_session)

        wsse = UsernameToken(username, secret)

        session_wsdl = _wsdl_url(self.base_url, "SessionWebService")
        session_client = zeep.Client(
            session_wsdl,
            transport=transport,
            wsse=wsse,
            plugins=[self._history],
        )
        try:
            self._consume_call("SessionWebService.logIn")
            session_id_raw = session_client.service.logIn(username, secret)
        except Exception as exc:
            raise RuntimeError(
                f"Polarion logIn() failed – check {cred_hint}. Detail: {exc}"
            ) from exc

        session_id, session_ns = self._resolve_session_id(session_id_raw)

        if not session_id or session_id.strip().lower() in {"none", "null"}:
            raise RuntimeError(
                "Polarion logIn() did not yield a usable session id (body was empty and no sessionID header was found). "
                "This can indicate SSO-only auth or a server-side login policy preventing SOAP sessions."
            )
        logger.debug(
            "Logged in to Polarion as %s (session %s…)",
            username,
            session_id[:8],
        )

        self._attach_tracker_clients(transport, wsse, session_id, session_ns)

    def _tracker(self) -> zeep.Client:
        """Return the TrackerWebService client, connecting first if necessary."""
        if self._tracker_client is None:
            self.connect()
        if self._tracker_client is None:
            raise RuntimeError(
                "TrackerWebService client could not be initialised. "
                "Check that POLARION_URL, POLARION_USER and POLARION_PASSWORD (or POLARION_PAT) are correct."
            )
        return self._tracker_client

    def _test_mgmt(self) -> zeep.Client:
        """Return the TestManagementWebService client, connecting first if necessary."""
        if self._test_mgmt_client is None:
            self.connect()
        if self._test_mgmt_client is None:
            raise RuntimeError(
                "TestManagementWebService client could not be initialised. "
                "Check POLARION_URL, POLARION_USER and POLARION_PASSWORD (or POLARION_PAT)."
            )
        return self._test_mgmt_client

    def _work_item_uri(self, work_item_id: str, *, project_id: str | None = None) -> str:
        """Build a Subterra URI for a work item.

        Uses ``project_id`` when provided; otherwise derives the project from
        ``work_item_id`` (``PROJECT-123``) or falls back to ``POLARION_PROJECT``.
        """
        pid = (project_id or "").strip() or _project_id_from_work_item_id(
            work_item_id, fallback=self.project_id
        )
        if not pid:
            raise RuntimeError(
                "Cannot build work item URI: set POLARION_PROJECT or use a full "
                "work item id in PROJECT-NUMBER format (e.g. PROJ-123)."
            )
        return (
            "subterra:data-service:objects:/default/"
            f"{pid}${{WorkItem}}{work_item_id.strip()}"
        )

    def _approval_status_waiting_id(self) -> str:
        return (
            os.environ.get("POLARION_APPROVAL_STATUS_WAITING_ID", "waiting").strip()
            or "waiting"
        )

    def _resolve_link_role(self, link_role: str) -> dict[str, str]:
        """Return an ``EnumOptionId``-shaped dict for TrackerWebService link APIs."""
        role = (link_role or "").strip() or (
            os.environ.get("POLARION_DEFAULT_LINK_ROLE", "").strip()
        )
        if not role:
            raise ValueError(
                "link_role is required (e.g. relates_to, verifies, implements), "
                "or set POLARION_DEFAULT_LINK_ROLE in ~/.config/polarion-mcp.env"
            )
        return {"id": role}

    def _fetch_approvals_raw(self, work_item_id: str) -> dict[str, Any]:
        """Return raw ``approvals`` field for a work item via SOAP."""
        wid = work_item_id.strip()
        if not wid:
            raise ValueError("work_item_id is required")

        uri = self._work_item_uri(wid)
        self.connect()
        try:
            self._consume_call("TrackerWebService.getWorkItemByUriWithFields")
            wi = self._tracker().service.getWorkItemByUriWithFields(uri, ["approvals"])
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        raw = serialize_object(wi)
        if not isinstance(raw, dict):
            return {"work_item_id": wid, "uri": uri, "approvals": []}

        approvals = raw.get("approvals")
        if approvals is None:
            return {"work_item_id": wid, "uri": uri, "approvals": []}
        if hasattr(approvals, "__iter__") and not isinstance(approvals, (str, dict)):
            approval_list = list(approvals)
        else:
            approval_list = [approvals]
        return {"work_item_id": wid, "uri": uri, "approvals": approval_list}

    @staticmethod
    def _normalize_test_result_enum(result: str) -> str:
        """Map user input to Polarion EnumOptionId string for test verdict."""
        r = (result or "").strip().lower()
        if r in {"pass", "passed", "ok", "success"}:
            return (
                os.environ.get("POLARION_TEST_RESULT_PASSED_ID", "passed").strip() or "passed"
            )
        if r in {"fail", "failed", "error", "nok"}:
            return (
                os.environ.get("POLARION_TEST_RESULT_FAILED_ID", "failed").strip() or "failed"
            )
        raise ValueError(
            f"Unsupported result {result!r}; use pass or fail, "
            "or set POLARION_TEST_RESULT_PASSED_ID / POLARION_TEST_RESULT_FAILED_ID."
        )

    # ------------------------------------------------------------------
    # Helpers
    # ------------------------------------------------------------------

    def _build_query(
        self, extra_parts: list[str], *, project_id: str | None = None
    ) -> str:
        """Prepend the configured project filter to any extra query parts."""
        parts: list[str] = []
        pid = (project_id or "").strip() or self.project_id
        if pid:
            parts.append(f"project.id:{pid}")
        parts.extend(extra_parts)
        return " AND ".join(parts) if parts else ""

    def _run_query(self, query: str, limit: int) -> list[dict]:
        """Execute a Polarion Lucene query and return formatted work items."""
        return self._run_query_fields(query, limit=limit, fields=_DEFAULT_FIELDS)

    def _run_query_fields(
        self,
        query: str,
        *,
        limit: int,
        fields: list[str],
        project_id: str | None = None,
    ) -> list[dict]:
        """Execute a Polarion Lucene query returning only the requested fields."""
        logger.debug("Polarion query (limit=%d): %s", limit, query)
        try:
            self._consume_call("TrackerWebService.queryWorkItemsLimited")
            raw_result = self._tracker().service.queryWorkItemsLimited(
                query,
                "id",
                fields,
                limit,
            )
        except zeep.exceptions.Fault:
            if logger.isEnabledFor(logging.DEBUG) and self._history is not None:
                try:
                    sent = self._history.last_sent
                    if sent and "envelope" in sent:
                        logger.debug(
                            "Last SOAP envelope sent:\n%s",
                            etree.tostring(sent["envelope"], pretty_print=True).decode(),
                        )
                except Exception:  # noqa: BLE001
                    # Best-effort only; don't mask the real fault.
                    pass
            raise
        if not raw_result:
            return []
        items = list(raw_result) if hasattr(raw_result, "__iter__") else [raw_result]
        return [_format_work_item(serialize_object(item)) for item in items]

    # ------------------------------------------------------------------
    # Public API (called by MCP tools)
    # ------------------------------------------------------------------

    def get_work_item(self, work_item_id: str) -> dict:
        """Retrieve a single work item by its full ID (e.g. ``PROJ-123``).

        Work item IDs already encode the project (e.g. ``PLT1-2570`` belongs
        to project ``PLT1``), so no ``project.id`` filter is added here.
        Adding one would cause Polarion to return "Not authorized" whenever
        the work item's project differs from ``POLARION_PROJECT``.
        """
        items = self._run_query(f"id:{work_item_id}", limit=1)
        if not items:
            return {"error": f"Work item '{work_item_id}' not found"}
        return items[0]

    def get_work_item_raw_fields(self, work_item_id: str, *, keys: list[str]) -> dict:
        """Fetch a work item via getWorkItemByUriWithFields and return raw fields.

        This is primarily for diagnostics when certain fields (e.g. attachments)
        are not exposed through queryWorkItemsLimited on a given Polarion instance.
        """
        if not keys:
            raise ValueError("keys must be a non-empty list of field names")

        self.connect()
        uri = self._work_item_uri(work_item_id)
        try:
            self._consume_call("TrackerWebService.getWorkItemByUriWithFields")
            wi = self._tracker().service.getWorkItemByUriWithFields(uri, keys)
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}
        raw = serialize_object(wi)
        if isinstance(raw, dict):
            raw.setdefault("id", work_item_id)
            raw.setdefault("uri", uri)
            return raw
        return {"id": work_item_id, "uri": uri, "value": raw}

    def get_work_item_links(self, work_item_id: str) -> dict:
        """Retrieve link information for a single work item.

        Polarion exposes links through a work item field (commonly named
        ``linkedWorkItems``). The exact schema can vary, so we keep the result
        JSON-friendly and return raw link objects when present.
        """
        # Fetch only the link-related fields to keep responses small.
        fields = ["id", "linkedWorkItems"]
        items = self._run_query_fields(f"id:{work_item_id}", limit=1, fields=fields)
        if not items:
            return {"error": f"Work item '{work_item_id}' not found"}

        item = items[0]
        links = item.get("linkedWorkItems")
        if links is None:
            # Either no links or the field name differs on this Polarion instance.
            return {
                "id": item.get("id", work_item_id),
                "linkedWorkItems": [],
                "note": (
                    "No 'linkedWorkItems' field returned. "
                    "Either the item has no links, or this Polarion instance uses a different link field name."
                ),
            }
        return {"id": item.get("id", work_item_id), "linkedWorkItems": links}

    def list_work_item_comments(self, work_item_id: str) -> dict:
        """List comments on a work item via ``getWorkItemByUriWithFields``."""
        wid = work_item_id.strip()
        if not wid:
            raise ValueError("work_item_id is required")

        uri = self._work_item_uri(wid)
        self.connect()
        try:
            self._consume_call("TrackerWebService.getWorkItemByUriWithFields")
            wi = self._tracker().service.getWorkItemByUriWithFields(uri, ["comments"])
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        raw = serialize_object(wi)
        comments_raw: Any = []
        if isinstance(raw, dict):
            comments_raw = raw.get("comments") or []

        if comments_raw is None:
            comments_raw = []
        if hasattr(comments_raw, "__iter__") and not isinstance(
            comments_raw, (str, dict)
        ):
            comment_items = list(comments_raw)
        else:
            comment_items = [comments_raw] if comments_raw else []

        comments = [_format_comment(serialize_object(c)) for c in comment_items]
        return {
            "work_item_id": wid,
            "uri": uri,
            "count": len(comments),
            "comments": comments,
        }

    def add_work_item_comment(
        self,
        work_item_id: str,
        text: str,
        *,
        title: str = "",
        parent_comment_uri: str = "",
        content_type: str = "text/plain",
    ) -> dict:
        """Add a comment on a work item via ``TrackerWebService.addComment``.

        Pass ``parent_comment_uri`` to reply to an existing comment; otherwise
        the comment is attached to the work item.
        """
        wid = work_item_id.strip()
        body = (text or "").strip()
        if not wid:
            raise ValueError("work_item_id is required")
        if not body:
            raise ValueError("text is required")

        work_item_uri = self._work_item_uri(wid)
        parent_uri = (parent_comment_uri or "").strip() or work_item_uri
        comment_title = (title or "").strip()
        ctype = (content_type or "text/plain").strip() or "text/plain"
        content = {"type": ctype, "content": body}

        self.connect()
        tracker = self._tracker().service
        try:
            if hasattr(tracker, "addComment"):
                self._consume_call("TrackerWebService.addComment")
                tracker.addComment(parent_uri, comment_title, content)
            else:
                self._consume_call("TrackerWebService.createComment")
                tracker.createComment(work_item_uri, content)
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        return {
            "ok": True,
            "work_item_id": wid,
            "work_item_uri": work_item_uri,
            "parent_uri": parent_uri,
            "title": comment_title or None,
            "content_type": ctype,
            "is_reply": parent_uri != work_item_uri,
        }

    def add_work_item_link(
        self,
        work_item_id: str,
        linked_work_item_id: str,
        *,
        link_role: str = "",
    ) -> dict:
        """Link two work items via ``TrackerWebService.addLinkedItem``.

        Creates a link from ``work_item_id`` to ``linked_work_item_id`` with the
        given ``link_role`` (Polarion link role enum id, project-specific).
        """
        source_id = work_item_id.strip()
        target_id = linked_work_item_id.strip()
        if not source_id:
            raise ValueError("work_item_id is required")
        if not target_id:
            raise ValueError("linked_work_item_id is required")

        source_uri = self._work_item_uri(source_id)
        target_uri = self._work_item_uri(target_id)
        role = self._resolve_link_role(link_role)

        self.connect()
        try:
            self._consume_call("TrackerWebService.addLinkedItem")
            self._tracker().service.addLinkedItem(source_uri, target_uri, role)
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        return {
            "ok": True,
            "work_item_id": source_id,
            "work_item_uri": source_uri,
            "linked_work_item_id": target_id,
            "linked_work_item_uri": target_uri,
            "link_role": role["id"],
        }

    def remove_work_item_link(
        self,
        work_item_id: str,
        linked_work_item_id: str,
        *,
        link_role: str = "",
    ) -> dict:
        """Remove a link between work items via ``TrackerWebService.removeLinkedItem``."""
        source_id = work_item_id.strip()
        target_id = linked_work_item_id.strip()
        if not source_id:
            raise ValueError("work_item_id is required")
        if not target_id:
            raise ValueError("linked_work_item_id is required")

        source_uri = self._work_item_uri(source_id)
        target_uri = self._work_item_uri(target_id)
        role = self._resolve_link_role(link_role)

        self.connect()
        try:
            self._consume_call("TrackerWebService.removeLinkedItem")
            self._tracker().service.removeLinkedItem(source_uri, target_uri, role)
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        return {
            "ok": True,
            "work_item_id": source_id,
            "work_item_uri": source_uri,
            "linked_work_item_id": target_id,
            "linked_work_item_uri": target_uri,
            "link_role": role["id"],
        }

    def query_work_items(
        self,
        *,
        type_filter: str = "",
        status_filter: str = "",
        extra_query: str = "",
        limit: int = 50,
    ) -> list[dict]:
        """Query work items by type, status, and/or an arbitrary Lucene string."""
        extra_parts: list[str] = []
        if type_filter:
            extra_parts.append(f"type:{type_filter}")
        if status_filter:
            extra_parts.append(f"status:{status_filter}")
        if extra_query:
            extra_parts.append(f"({extra_query})")

        query = self._build_query(extra_parts)
        if not query:
            raise ValueError(
                "At least one filter must be provided: type, status, or query. "
                "Alternatively, set POLARION_PROJECT so all queries are scoped "
                "to a specific project."
            )
        return self._run_query(query, limit=limit)

    @staticmethod
    def _document_query_parts(document_name: str, *, space: str = "") -> tuple[str, str, list[str]]:
        """Build Lucene OR-clauses that match work items in a document by name."""
        raw_name = (document_name or "").strip()
        if not raw_name:
            raise ValueError("document_name is required")

        decoded_name = unquote(raw_name).strip()
        raw_space = (space or "").strip()
        space_norm = raw_space.strip("/")

        candidates: list[str] = []
        if space_norm:
            candidates.append(f'document.moduleFolder:"{space_norm}/{raw_name}"')
            if decoded_name and decoded_name != raw_name:
                candidates.append(
                    f'document.moduleFolder:"{space_norm}/{decoded_name}"'
                )
        candidates.append(f'document.title:"{raw_name}"')
        if decoded_name and decoded_name != raw_name:
            candidates.append(f'document.title:"{decoded_name}"')

        seen: set[str] = set()
        unique = [c for c in candidates if not (c in seen or seen.add(c))]
        return raw_name, space_norm, unique

    def list_documents(
        self,
        *,
        space: str = "",
        query: str = "",
        limit: int = 200,
    ) -> dict:
        """Enumerate Polarion documents (modules) in the configured project.

        Uses ``TrackerWebService.queryModules`` scoped to ``POLARION_PROJECT``.
        Optionally filter by wiki ``space`` (module folder prefix) or an extra
        Lucene query string.
        """
        if not self.project_id:
            raise RuntimeError(
                "POLARION_PROJECT is required to list documents in a project."
            )
        if limit < 1:
            raise ValueError("limit must be >= 1")

        extra_parts: list[str] = []
        space_norm = (space or "").strip().strip("/")
        if space_norm:
            extra_parts.append(f'moduleFolder:"{space_norm}/*"')
        if (query or "").strip():
            extra_parts.append(f"({query.strip()})")

        lucene = self._build_query(extra_parts)
        fields = ["id", "title", "moduleFolder", "moduleName", "type", "status"]

        self.connect()
        try:
            self._consume_call("TrackerWebService.queryModules")
            raw_modules = self._tracker().service.queryModules(
                lucene, "title", fields, limit
            )
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        documents: list[dict[str, Any]] = []
        if raw_modules:
            items = (
                list(raw_modules)
                if hasattr(raw_modules, "__iter__")
                else [raw_modules]
            )
            documents = [
                _format_module_summary(serialize_object(item)) for item in items
            ]

        return {
            "project_id": self.project_id,
            "space": space_norm or None,
            "query": query.strip() or None,
            "limit": limit,
            "count": len(documents),
            "documents": documents,
        }

    def list_document_work_items(
        self,
        document_name: str,
        *,
        space: str = "",
        limit: int = 200,
        project_id: str | None = None,
    ) -> dict:
        """List work items contained in a Polarion document/module identified by name.

        Polarion stores document membership in ``document.moduleFolder``
        (``<space>/<title>``) when a space is provided, or ``document.title``
        otherwise.
        """
        raw_name, space_norm, clauses = self._document_query_parts(
            document_name, space=space
        )
        doc_filter = "(" + " OR ".join(clauses) + ")"
        query = self._build_query([doc_filter], project_id=project_id)
        work_items = self._run_query_fields(
            query,
            limit=limit,
            fields=_DEFAULT_FIELDS,
            project_id=project_id,
        )
        return {
            "document_name": raw_name,
            "space": space_norm or None,
            "project_id": (project_id or "").strip() or self.project_id or None,
            "count": len(work_items),
            "limit": limit,
            "work_items": work_items,
        }

    def list_configuration_srs_inventory(
        self,
        document_name: str,
        *,
        space: str = "",
        project_id: str | None = None,
        tool_configured_by: str = "Facility Config",
        work_item_type: str = "config",
        limit: int = 512,
    ) -> dict:
        """List SRS configuration rows with Configured By and in-scope flags for a config tool.

        Returns a table-friendly list of work items in the document, each with
        ``configured_by`` (from the Polarion ``configuredby`` custom field when
        available) and ``in_scope`` when ``tool_configured_by`` appears in that list.
        """
        raw_name, space_norm, clauses = self._document_query_parts(
            document_name, space=space
        )
        pid = (project_id or "").strip() or self.project_id
        if not pid:
            raise RuntimeError(
                "project_id is required (argument or POLARION_PROJECT) for SRS inventory."
            )

        doc_filter = "(" + " OR ".join(clauses) + ")"
        type_filter = f'type:"{work_item_type.strip()}"' if work_item_type.strip() else ""
        extra = [doc_filter]
        if type_filter:
            extra.append(type_filter)
        query = self._build_query(extra, project_id=pid)

        raw_items = self._run_query_fields(
            query,
            limit=limit,
            fields=_DEFAULT_FIELDS,
            project_id=pid,
        )

        pdf_configured_by: dict[str, list[str]] = {}
        pdf_path = os.environ.get("POLARION_SRS_PDF_PATH", "").strip()
        if pdf_path and os.path.isfile(pdf_path):
            try:
                from pypdf import PdfReader  # noqa: PLC0415

                text = "\n".join(
                    (page.extract_text() or "")
                    for page in PdfReader(pdf_path).pages
                )
                known_labels = (
                    "Manufacturing",
                    "Facility UI",
                    "Facility Config",
                    "Service Tool",
                    "Clinician",
                )
                for match in re.finditer(
                    r"Configured\s+By\s*(.*?)\s*Data\s+Type",
                    text,
                    flags=re.IGNORECASE | re.DOTALL,
                ):
                    raw = match.group(1)
                    before = text[max(0, match.start() - 3000) : match.start()]
                    sey_hits = list(re.finditer(r"SEY-\d+", before))
                    if not sey_hits:
                        continue
                    sey_id = sey_hits[-1].group(0)
                    labels: list[str] = []
                    for label in known_labels:
                        if label.lower() in raw.lower():
                            labels.append(label)
                    pdf_configured_by[sey_id] = labels
            except Exception as exc:  # noqa: BLE001
                logger.warning("SRS PDF Configured By parse failed: %s", exc)

        tool_label = (tool_configured_by or "").strip()
        rows: list[dict[str, Any]] = []
        for item in raw_items:
            wi_id = str(item.get("id") or "")
            configured_by = _parse_configured_by_field(item.get("configuredby"))
            if not configured_by and wi_id in pdf_configured_by:
                configured_by = pdf_configured_by[wi_id]
            in_scope = bool(
                tool_label
                and any(
                    cb.strip().lower() == tool_label.lower() for cb in configured_by
                )
            )
            rows.append(
                {
                    "id": wi_id,
                    "title": item.get("title"),
                    "type": item.get("type"),
                    "status": item.get("status"),
                    "category": item.get("category"),
                    "configuration_name": item.get("name"),
                    "phase_release": item.get("phaseRelease"),
                    "data_type": item.get("dataType"),
                    "configured_by": configured_by,
                    "in_scope": in_scope,
                }
            )

        in_scope_rows = [r for r in rows if r.get("in_scope")]
        return {
            "document_name": raw_name,
            "space": space_norm or None,
            "project_id": pid,
            "tool_configured_by": tool_label,
            "work_item_type": work_item_type.strip() or None,
            "count": len(rows),
            "in_scope_count": len(in_scope_rows),
            "out_of_scope_count": len(rows) - len(in_scope_rows),
            "limit": limit,
            "items": rows,
        }

    def get_document_work_items(
        self,
        document_name: str,
        *,
        space: str = "",
        limit: int = 200,
    ) -> list[dict]:
        """Return work items in a document (list only; see ``list_document_work_items``)."""
        return self.list_document_work_items(
            document_name, space=space, limit=limit
        )["work_items"]

    @staticmethod
    def _module_location(document_name: str, *, space: str = "") -> str:
        """Polarion module location relative to the project's ``modules`` folder."""
        raw_name, space_norm, _ = PolarionClient._document_query_parts(
            document_name, space=space
        )
        return f"{space_norm}/{raw_name}" if space_norm else raw_name

    def _resolve_module(self, document_name: str, *, space: str = "") -> dict[str, Any]:
        """Locate a document/module by name using TrackerWebService."""
        if not self.project_id:
            raise RuntimeError(
                "POLARION_PROJECT is required to load documents by name."
            )

        raw_name, space_norm, clauses = self._document_query_parts(
            document_name, space=space
        )
        module_fields = [
            "uri",
            "id",
            "title",
            "moduleFolder",
            "moduleName",
            "homePageContent",
        ]
        self.connect()
        tracker = self._tracker().service

        location = self._module_location(document_name, space=space)
        module_raw: Any = None
        try:
            self._consume_call("TrackerWebService.getModuleByLocationWithFields")
            module_raw = tracker.getModuleByLocationWithFields(
                self.project_id, location, module_fields
            )
        except zeep.exceptions.Fault:
            module_raw = None

        if module_raw is not None:
            mod = serialize_object(module_raw)
            if isinstance(mod, dict) and not mod.get("unresolvable"):
                mod.setdefault("document_name", raw_name)
                mod.setdefault("space", space_norm or None)
                return mod

        query = self._build_query([f"({ ' OR '.join(clauses) })"])
        try:
            self._consume_call("TrackerWebService.queryModules")
            modules = tracker.queryModules(query, "id", module_fields, 5)
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        if not modules:
            return {
                "error": (
                    f"Document '{raw_name}' not found"
                    + (f" in space '{space_norm}'" if space_norm else "")
                )
            }

        items = list(modules) if hasattr(modules, "__iter__") else [modules]
        mod = serialize_object(items[0])
        if not isinstance(mod, dict):
            return {"error": f"Document '{raw_name}' not found"}
        mod.setdefault("document_name", raw_name)
        mod.setdefault("space", space_norm or None)
        return mod

    def get_document_text(
        self,
        document_name: str,
        *,
        space: str = "",
        include_work_item_text: bool = True,
    ) -> dict:
        """Return free text from a Polarion document identified by name.

        Loads the document module via ``getModuleByLocationWithFields`` (or
        ``queryModules`` as fallback) and returns:

        * ``home_page_content`` – document home page text (``homePageContent``)
        * ``sections`` – narrative/heading work items in the document that have
          a non-empty ``description`` (via ``getModuleWorkItems``)
        * ``combined_content`` – home page plus section text concatenated
        """
        module = self._resolve_module(document_name, space=space)
        if "error" in module:
            return module

        uri = (module.get("uri") or "").strip()
        if not uri:
            return {"error": "Resolved document has no URI"}

        home = _format_polarion_text(module.get("homePageContent"))
        sections: list[dict[str, Any]] = []

        if include_work_item_text:
            try:
                self._consume_call("TrackerWebService.getModuleWorkItems")
                raw_items = self._tracker().service.getModuleWorkItems(
                    uri,
                    "",
                    True,
                    ["id", "type", "title", "description", "outlineNumber"],
                )
            except zeep.exceptions.Fault as exc:
                return {"error": f"Polarion SOAP error: {exc.message}"}

            if raw_items:
                items = (
                    list(raw_items)
                    if hasattr(raw_items, "__iter__")
                    else [raw_items]
                )
                for item in items:
                    formatted = _format_work_item(serialize_object(item))
                    description = (formatted.get("description") or "").strip()
                    if not description:
                        continue
                    sections.append(
                        {
                            "work_item_id": formatted.get("id"),
                            "type": formatted.get("type"),
                            "title": formatted.get("title"),
                            "outline_number": formatted.get("outlineNumber"),
                            "content_type": "text/html",
                            "content": description,
                        }
                    )

        combined_parts = [home["content"]] if home["content"] else []
        combined_parts.extend(s["content"] for s in sections if s.get("content"))
        combined_content = "\n\n".join(combined_parts).strip()

        return {
            "document_name": module.get("document_name", document_name.strip()),
            "space": module.get("space"),
            "location": self._module_location(document_name, space=space),
            "uri": uri,
            "title": module.get("title") or module.get("moduleName"),
            "home_page_content": home,
            "sections": sections,
            "combined_content": combined_content,
        }

    def _get_available_workflow_actions(self, work_item_id: str) -> list[dict]:
        """Return available workflow actions for a work item."""
        wid = work_item_id.strip()
        if not wid:
            raise ValueError("work_item_id is required")

        uri = self._work_item_uri(wid)
        self.connect()
        self._consume_call("TrackerWebService.getAvailableActions")
        raw_actions = self._tracker().service.getAvailableActions(uri)
        if not raw_actions:
            return []
        actions = list(raw_actions) if hasattr(raw_actions, "__iter__") else [raw_actions]
        return [
            _format_workflow_action(serialize_object(action)) for action in actions
        ]

    def list_work_item_workflow_actions(self, work_item_id: str) -> dict:
        """List workflow actions that can change a work item's state from its current status."""
        wid = work_item_id.strip()
        if not wid:
            raise ValueError("work_item_id is required")

        try:
            actions = self._get_available_workflow_actions(wid)
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        current = self.get_work_item(wid)
        previous_status = current.get("status") if "error" not in current else None
        return {
            "work_item_id": wid,
            "uri": self._work_item_uri(wid),
            "current_status": previous_status,
            "available_actions": actions,
        }

    def set_work_item_status(
        self,
        work_item_id: str,
        *,
        status: str = "",
        workflow_action_id: int | None = None,
        workflow_action_name: str = "",
    ) -> dict:
        """Change work item status via ``TrackerWebService.performWorkflowAction``.

        Polarion enforces workflow rules: pick an available action by target
        ``status`` enum id (e.g. ``open``, ``inProgress``, ``closed``), by
        ``workflow_action_name``, or pass ``workflow_action_id`` directly.
        """
        wid = work_item_id.strip()
        if not wid:
            raise ValueError("work_item_id is required")

        if workflow_action_id is None:
            target_status = status.strip()
            target_name = workflow_action_name.strip()
            if not target_status and not target_name:
                raise ValueError(
                    "Provide status (target status enum id), workflow_action_name, "
                    "or workflow_action_id"
                )

        uri = self._work_item_uri(wid)
        before = self.get_work_item(wid)
        if "error" in before:
            return before
        previous_status = before.get("status")

        action_id: int | None = workflow_action_id
        matched_action: dict[str, Any] | None = None

        if action_id is None:
            try:
                available = self._get_available_workflow_actions(wid)
            except zeep.exceptions.Fault as exc:
                return {"error": f"Polarion SOAP error: {exc.message}"}

            target_status = status.strip()
            target_name = workflow_action_name.strip()

            if target_status:
                for action in available:
                    ts = (action.get("target_status") or "").strip()
                    if ts.lower() == target_status.lower():
                        matched_action = action
                        break
            elif target_name:
                for action in available:
                    name = (action.get("action_name") or "").strip()
                    if name.lower() == target_name.lower():
                        matched_action = action
                        break

            if matched_action is None:
                return {
                    "error": (
                        "No available workflow action matches the requested transition."
                    ),
                    "work_item_id": wid,
                    "previous_status": previous_status,
                    "requested_status": target_status or None,
                    "requested_action_name": target_name or None,
                    "available_actions": available,
                }

            action_id = int(matched_action["action_id"])
        else:
            matched_action = {"action_id": action_id}

        self.connect()
        try:
            self._consume_call("TrackerWebService.performWorkflowAction")
            self._tracker().service.performWorkflowAction(uri, action_id)
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        after = self.get_work_item(wid)
        new_status = after.get("status") if "error" not in after else None

        return {
            "ok": True,
            "work_item_id": wid,
            "uri": uri,
            "previous_status": previous_status,
            "status": new_status,
            "workflow_action_id": action_id,
            "workflow_action_name": (
                matched_action.get("action_name") if matched_action else None
            ),
            "target_status": (
                matched_action.get("target_status") if matched_action else None
            ),
        }

    def create_test_result_record(
        self,
        test_case_work_item_id: str,
        *,
        test_run_work_item_id: str = "",
        test_run_uri: str = "",
        result: str = "",
        evidence: str = "",
    ) -> dict:
        """Add a Test Record to a Test Run via TestManagementWebService.addTestRecordToTestRun.

        Polarion stores automated/manual execution against a **Test Run** work item.
        The **test case** is referenced by URI inside the ``TestRecord``.

        Parameters
        ----------
        test_case_work_item_id:
            Work item ID of the executed test case (e.g. ``PROJ-TC-42``).
        test_run_work_item_id:
            Work item ID of the target test run (required unless ``test_run_uri`` is set).
        test_run_uri:
            Full Subterra URI of the test run (optional; overrides ``test_run_work_item_id``).
        result:
            ``pass`` or ``fail`` (aliases: passed/failed/ok). Mapped to enum ids via
            ``POLARION_TEST_RESULT_PASSED_ID`` / ``POLARION_TEST_RESULT_FAILED_ID`` (defaults:
            ``passed`` / ``failed``).
        evidence:
            Free-text comment stored on the test record (plain text).
        """
        if not test_case_work_item_id.strip():
            raise ValueError("test_case_work_item_id is required")
        run_uri = (test_run_uri or "").strip()
        if not run_uri:
            tw = (test_run_work_item_id or "").strip()
            if not tw:
                raise ValueError(
                    "Provide test_run_work_item_id or test_run_uri (Subterra URI of the Test Run)."
                )
            run_uri = self._work_item_uri(tw)

        enum_id = self._normalize_test_result_enum(result)
        case_uri = self._work_item_uri(test_case_work_item_id.strip())

        test_record: dict[str, Any] = {
            "testCaseURI": case_uri,
            "result": {"id": enum_id},
        }
        if evidence.strip():
            test_record["comment"] = {"type": "text/plain", "content": evidence.strip()}

        try:
            self._consume_call("TestManagementWebService.addTestRecordToTestRun")
            self._test_mgmt().service.addTestRecordToTestRun(run_uri, test_record)
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        return {
            "ok": True,
            "test_run_uri": run_uri,
            "test_case_work_item_id": test_case_work_item_id.strip(),
            "result_enum_id": enum_id,
            "had_evidence": bool(evidence.strip()),
        }

    def list_reviewers(self, work_item_id: str) -> dict:
        """List reviewers (approvees) and their approval status on a work item.

        Uses ``TrackerWebService.getWorkItemByUriWithFields`` with the
        ``approvals`` field. See Polarion ``Approval`` / ``IApprovalStatusOpt``
        (``waiting``, ``approved``, ``disapproved``).
        """
        fetched = self._fetch_approvals_raw(work_item_id)
        if "error" in fetched:
            return fetched

        reviewers = [
            _format_approval(serialize_object(item))
            for item in fetched.get("approvals", [])
        ]
        return {
            "work_item_id": fetched["work_item_id"],
            "uri": fetched["uri"],
            "reviewers": reviewers,
        }

    def add_reviewer(self, work_item_id: str, reviewer_id: str) -> dict:
        """Add a reviewer (approvee) via ``TrackerWebService.addApprovee``."""
        wid = work_item_id.strip()
        rid = reviewer_id.strip()
        if not wid:
            raise ValueError("work_item_id is required")
        if not rid:
            raise ValueError("reviewer_id is required")

        uri = self._work_item_uri(wid)
        self.connect()
        try:
            self._consume_call("TrackerWebService.addApprovee")
            self._tracker().service.addApprovee(uri, rid)
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        return {"ok": True, "work_item_id": wid, "uri": uri, "reviewer_id": rid}

    def remove_reviewer(self, work_item_id: str, reviewer_id: str) -> dict:
        """Remove a reviewer via ``TrackerWebService.removeApprovee``."""
        wid = work_item_id.strip()
        rid = reviewer_id.strip()
        if not wid:
            raise ValueError("work_item_id is required")
        if not rid:
            raise ValueError("reviewer_id is required")

        uri = self._work_item_uri(wid)
        self.connect()
        try:
            self._consume_call("TrackerWebService.removeApprovee")
            self._tracker().service.removeApprovee(uri, rid)
        except zeep.exceptions.Fault as exc:
            return {"error": f"Polarion SOAP error: {exc.message}"}

        return {"ok": True, "work_item_id": wid, "uri": uri, "reviewer_id": rid}

    def reset_review_status(
        self,
        work_item_id: str,
        *,
        reviewer_ids: list[str] | None = None,
    ) -> dict:
        """Reset approval status to *waiting* via ``TrackerWebService.editApproval``.

        When ``reviewer_ids`` is omitted, every current reviewer on the work item
        is reset. Pass a list of Polarion user ids to reset only those reviewers.
        """
        wid = work_item_id.strip()
        if not wid:
            raise ValueError("work_item_id is required")

        uri = self._work_item_uri(wid)
        waiting_id = self._approval_status_waiting_id()
        status = {"id": waiting_id}

        targets: list[str]
        if reviewer_ids is not None:
            targets = [r.strip() for r in reviewer_ids if r and r.strip()]
            if not targets:
                raise ValueError("reviewer_ids must contain at least one reviewer id")
        else:
            listed = self.list_reviewers(wid)
            if "error" in listed:
                return listed
            targets = [
                r["reviewer_id"]
                for r in listed.get("reviewers", [])
                if r.get("reviewer_id")
            ]

        self.connect()
        reset: list[dict[str, str]] = []
        errors: list[str] = []
        for rid in targets:
            try:
                self._consume_call("TrackerWebService.editApproval")
                self._tracker().service.editApproval(uri, rid, status)
                reset.append({"reviewer_id": rid, "status": waiting_id})
            except zeep.exceptions.Fault as exc:
                errors.append(f"{rid}: {exc.message}")

        result: dict[str, Any] = {
            "ok": not errors,
            "work_item_id": wid,
            "uri": uri,
            "status_set_to": waiting_id,
            "reset": reset,
        }
        if errors:
            result["errors"] = errors
        if not reset and not errors:
            result["note"] = "No reviewers on this work item."
        return result


def _read_max_calls() -> int | None:
    """Read a hard cap for outbound Polarion SOAP calls.

    Set POLARION_MCP_MAX_CALLS=N to allow at most N SOAP operations per process.
    Unset (or empty) means unlimited.
    """
    raw = (os.environ.get("POLARION_MCP_MAX_CALLS") or "").strip()
    if not raw:
        return None
    try:
        value = int(raw)
    except ValueError as exc:
        raise RuntimeError("POLARION_MCP_MAX_CALLS must be an integer") from exc
    if value < 0:
        raise RuntimeError("POLARION_MCP_MAX_CALLS must be >= 0")
    return value
