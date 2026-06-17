"""Unit tests for Polarion work item link SOAP helpers."""

from __future__ import annotations

from unittest.mock import MagicMock, patch

import pytest
import zeep.exceptions

from polarion_mcp.polarion_client import PolarionClient


def _client() -> PolarionClient:
    return PolarionClient(
        url="http://polarion.example.com/polarion",
        username="user",
        password="secret",
        project_id="MY_PROJ",
    )


def test_resolve_link_role_from_argument() -> None:
    assert _client()._resolve_link_role("verifies") == {"id": "verifies"}


def test_resolve_link_role_from_env(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setenv("POLARION_DEFAULT_LINK_ROLE", "relates_to")
    assert _client()._resolve_link_role("") == {"id": "relates_to"}


def test_resolve_link_role_missing_raises() -> None:
    with pytest.raises(ValueError, match="link_role"):
        _client()._resolve_link_role("")


def test_add_work_item_link_calls_add_linked_item() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.add_work_item_link("MY_PROJ-1", "MY_PROJ-2", link_role="verifies")

    assert result["ok"] is True
    assert result["link_role"] == "verifies"
    mock_service.addLinkedItem.assert_called_once_with(
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-1",
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-2",
        {"id": "verifies"},
    )


def test_add_work_item_link_cross_project_uris() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.add_work_item_link("PLT1-10", "OTHER-20", link_role="relates_to")

    assert result["ok"] is True
    mock_service.addLinkedItem.assert_called_once_with(
        "subterra:data-service:objects:/default/PLT1${WorkItem}PLT1-10",
        "subterra:data-service:objects:/default/OTHER${WorkItem}OTHER-20",
        {"id": "relates_to"},
    )


def test_remove_work_item_link_calls_remove_linked_item() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.remove_work_item_link(
            "MY_PROJ-1", "MY_PROJ-2", link_role="verifies"
        )

    assert result["ok"] is True
    mock_service.removeLinkedItem.assert_called_once_with(
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-1",
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-2",
        {"id": "verifies"},
    )


def test_add_work_item_link_soap_fault_returns_error() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.addLinkedItem.side_effect = zeep.exceptions.Fault("invalid role")
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.add_work_item_link("MY_PROJ-1", "MY_PROJ-2", link_role="bad")

    assert "error" in result
