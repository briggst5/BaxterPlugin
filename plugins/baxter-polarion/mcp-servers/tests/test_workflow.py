"""Unit tests for work item workflow / status changes."""

from __future__ import annotations

from unittest.mock import MagicMock, patch

import pytest
import zeep.exceptions

from polarion_mcp.polarion_client import PolarionClient, _format_workflow_action


def test_format_workflow_action_from_dict() -> None:
    assert _format_workflow_action(
        {
            "actionId": 12,
            "actionName": "Start Progress",
            "targetStatus": {"id": "inProgress"},
        }
    ) == {
        "action_id": 12,
        "action_name": "Start Progress",
        "target_status": "inProgress",
    }


def _client() -> PolarionClient:
    return PolarionClient(
        url="http://polarion.example.com/polarion",
        username="user",
        password="secret",
        project_id="MY_PROJ",
    )


def test_set_work_item_status_by_target_status() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.getAvailableActions.return_value = [
        {
            "actionId": 5,
            "actionName": "Resolve",
            "targetStatus": {"id": "closed"},
        }
    ]
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with (
        patch.object(client, "_tracker", return_value=mock_tracker),
        patch.object(
            client,
            "get_work_item",
            side_effect=[
                {"id": "MY_PROJ-1", "status": "open"},
                {"id": "MY_PROJ-1", "status": "closed"},
            ],
        ),
    ):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.set_work_item_status("MY_PROJ-1", status="closed")

    assert result["ok"] is True
    assert result["previous_status"] == "open"
    assert result["status"] == "closed"
    assert result["workflow_action_id"] == 5
    mock_service.performWorkflowAction.assert_called_once_with(
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-1",
        5,
    )


def test_set_work_item_status_by_action_id() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with (
        patch.object(client, "_tracker", return_value=mock_tracker),
        patch.object(
            client,
            "get_work_item",
            side_effect=[
                {"id": "MY_PROJ-1", "status": "open"},
                {"id": "MY_PROJ-1", "status": "inProgress"},
            ],
        ),
    ):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.set_work_item_status("MY_PROJ-1", workflow_action_id=9)

    assert result["ok"] is True
    assert result["workflow_action_id"] == 9
    mock_service.getAvailableActions.assert_not_called()
    mock_service.performWorkflowAction.assert_called_once_with(
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-1",
        9,
    )


def test_set_work_item_status_no_matching_action() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.getAvailableActions.return_value = [
        {"actionId": 1, "actionName": "Close", "targetStatus": {"id": "closed"}},
    ]
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with (
        patch.object(client, "_tracker", return_value=mock_tracker),
        patch.object(
            client, "get_work_item", return_value={"id": "MY_PROJ-1", "status": "open"}
        ),
    ):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.set_work_item_status("MY_PROJ-1", status="resolved")

    assert "error" in result
    assert result["available_actions"]
    mock_service.performWorkflowAction.assert_not_called()


def test_set_work_item_status_missing_selector_raises() -> None:
    client = _client()
    with pytest.raises(ValueError, match="status"):
        client.set_work_item_status("MY_PROJ-1")


def test_list_work_item_workflow_actions() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.getAvailableActions.return_value = [
        {"actionId": 2, "actionName": "Reopen", "targetStatus": {"id": "open"}},
    ]
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with (
        patch.object(client, "_tracker", return_value=mock_tracker),
        patch.object(
            client, "get_work_item", return_value={"id": "MY_PROJ-1", "status": "closed"}
        ),
    ):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.list_work_item_workflow_actions("MY_PROJ-1")

    assert result["current_status"] == "closed"
    assert len(result["available_actions"]) == 1
    assert result["available_actions"][0]["target_status"] == "open"


def test_set_work_item_status_soap_fault() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.performWorkflowAction.side_effect = zeep.exceptions.Fault("denied")
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with (
        patch.object(client, "_tracker", return_value=mock_tracker),
        patch.object(
            client, "get_work_item", return_value={"id": "MY_PROJ-1", "status": "open"}
        ),
    ):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.set_work_item_status("MY_PROJ-1", workflow_action_id=3)

    assert "error" in result
