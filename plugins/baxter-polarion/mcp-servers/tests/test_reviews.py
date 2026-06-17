"""Unit tests for Polarion reviewer / approval SOAP helpers."""

from __future__ import annotations

from unittest.mock import MagicMock, patch

import pytest
import zeep.exceptions

from polarion_mcp.polarion_client import (
    PolarionClient,
    _format_approval,
    _project_id_from_work_item_id,
)


@pytest.mark.parametrize(
    ("work_item_id", "fallback", "expected"),
    [
        ("PLT1-100", "", "PLT1"),
        ("flc.platform.01-42", "", "flc.platform.01"),
        ("BADID", "MY_PROJ", "MY_PROJ"),
    ],
)
def test_project_id_from_work_item_id(
    work_item_id: str, fallback: str, expected: str
) -> None:
    assert _project_id_from_work_item_id(work_item_id, fallback=fallback) == expected


def test_format_approval_from_dict() -> None:
    assert _format_approval(
        {"user": {"id": "jsmith"}, "status": {"id": "waiting"}}
    ) == {"reviewer_id": "jsmith", "status": "waiting"}


def test_format_approval_missing_fields() -> None:
    assert _format_approval({}) == {"reviewer_id": None, "status": None}


def _client() -> PolarionClient:
    return PolarionClient(
        url="http://polarion.example.com/polarion",
        username="user",
        password="secret",
        project_id="MY_PROJ",
    )


def test_work_item_uri_derives_project_from_id() -> None:
    client = _client()
    uri = client._work_item_uri("PLT1-100")
    assert uri == "subterra:data-service:objects:/default/PLT1${WorkItem}PLT1-100"


def test_list_reviewers_returns_formatted_reviewers() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.getWorkItemByUriWithFields.return_value = {
        "approvals": [
            {"user": {"id": "alice"}, "status": {"id": "approved"}},
            {"user": {"id": "bob"}, "status": {"id": "waiting"}},
        ]
    }
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.list_reviewers("PLT1-100")

    assert result["work_item_id"] == "PLT1-100"
    assert result["reviewers"] == [
        {"reviewer_id": "alice", "status": "approved"},
        {"reviewer_id": "bob", "status": "waiting"},
    ]
    mock_service.getWorkItemByUriWithFields.assert_called_once()
    args = mock_service.getWorkItemByUriWithFields.call_args[0]
    assert args[0] == "subterra:data-service:objects:/default/PLT1${WorkItem}PLT1-100"
    assert args[1] == ["approvals"]


def test_add_reviewer_calls_add_approvee() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.add_reviewer("MY_PROJ-1", "jsmith")

    assert result["ok"] is True
    assert result["reviewer_id"] == "jsmith"
    mock_service.addApprovee.assert_called_once_with(
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-1",
        "jsmith",
    )


def test_remove_reviewer_calls_remove_approvee() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.remove_reviewer("MY_PROJ-1", "jsmith")

    assert result["ok"] is True
    mock_service.removeApprovee.assert_called_once()


def test_reset_review_status_all_reviewers() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with (
        patch.object(client, "_tracker", return_value=mock_tracker),
        patch.object(
            client,
            "list_reviewers",
            return_value={
                "work_item_id": "MY_PROJ-1",
                "uri": "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-1",
                "reviewers": [
                    {"reviewer_id": "alice", "status": "approved"},
                    {"reviewer_id": "bob", "status": "disapproved"},
                ],
            },
        ),
    ):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.reset_review_status("MY_PROJ-1")

    assert result["ok"] is True
    assert result["status_set_to"] == "waiting"
    assert len(result["reset"]) == 2
    assert mock_service.editApproval.call_count == 2
    mock_service.editApproval.assert_any_call(
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-1",
        "alice",
        {"id": "waiting"},
    )


def test_reset_review_status_specific_reviewers() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.reset_review_status(
            "MY_PROJ-1", reviewer_ids=["alice"]
        )

    assert result["ok"] is True
    assert result["reset"] == [{"reviewer_id": "alice", "status": "waiting"}]
    mock_service.editApproval.assert_called_once()


def test_reset_review_status_custom_waiting_enum(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setenv("POLARION_APPROVAL_STATUS_WAITING_ID", "OPT_WAITING")
    client = _client()
    mock_service = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.reset_review_status("MY_PROJ-1", reviewer_ids=["alice"])

    assert result["status_set_to"] == "OPT_WAITING"
    mock_service.editApproval.assert_called_once_with(
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-1",
        "alice",
        {"id": "OPT_WAITING"},
    )


def test_add_reviewer_soap_fault_returns_error() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.addApprovee.side_effect = zeep.exceptions.Fault("not allowed")
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.add_reviewer("MY_PROJ-1", "jsmith")

    assert "error" in result


def test_reset_review_status_empty_reviewer_ids_raises() -> None:
    client = _client()
    with pytest.raises(ValueError, match="reviewer_ids"):
        client.reset_review_status("MY_PROJ-1", reviewer_ids=[])
