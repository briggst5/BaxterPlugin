"""Unit tests for work item comment helpers."""

from __future__ import annotations

from unittest.mock import MagicMock, patch

import pytest
import zeep.exceptions

from polarion_mcp.polarion_client import PolarionClient, _format_comment


def test_format_comment_from_dict() -> None:
    formatted = _format_comment(
        {
            "id": "c1",
            "uri": "subterra:comment:1",
            "title": "Note",
            "text": {"type": "text/plain", "content": "Looks good"},
            "author": {"id": "jsmith"},
            "resolved": False,
            "parentCommentURI": None,
            "childCommentURIs": ["subterra:comment:2"],
        }
    )
    assert formatted["text"] == "Looks good"
    assert formatted["author"] == "jsmith"
    assert formatted["child_comment_uris"] == ["subterra:comment:2"]


def _client() -> PolarionClient:
    return PolarionClient(
        url="http://polarion.example.com/polarion",
        username="user",
        password="secret",
        project_id="MY_PROJ",
    )


def test_list_work_item_comments() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.getWorkItemByUriWithFields.return_value = {
        "comments": [
            {
                "id": "c1",
                "uri": "subterra:comment:1",
                "text": {"type": "text/plain", "content": "First"},
                "author": {"id": "alice"},
            }
        ]
    }
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.list_work_item_comments("MY_PROJ-9")

    assert result["count"] == 1
    assert result["comments"][0]["text"] == "First"
    mock_service.getWorkItemByUriWithFields.assert_called_once_with(
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-9",
        ["comments"],
    )


def test_add_work_item_comment_uses_add_comment() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.addComment = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.add_work_item_comment("MY_PROJ-9", "Please review")

    assert result["ok"] is True
    mock_service.addComment.assert_called_once_with(
        "subterra:data-service:objects:/default/MY_PROJ${WorkItem}MY_PROJ-9",
        "",
        {"type": "text/plain", "content": "Please review"},
    )


def test_add_work_item_comment_reply() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.addComment = MagicMock()
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    parent = "subterra:comment:parent"
    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.add_work_item_comment(
            "MY_PROJ-9",
            "Reply text",
            parent_comment_uri=parent,
            title="Re:",
        )

    assert result["is_reply"] is True
    mock_service.addComment.assert_called_once_with(
        parent,
        "Re:",
        {"type": "text/plain", "content": "Reply text"},
    )


def test_add_work_item_comment_fallback_create_comment() -> None:
    client = _client()
    mock_service = MagicMock(spec=["createComment"])
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.add_work_item_comment("MY_PROJ-9", "Legacy path")

    assert result["ok"] is True
    mock_service.createComment.assert_called_once()


def test_add_work_item_comment_empty_text_raises() -> None:
    client = _client()
    with pytest.raises(ValueError, match="text"):
        client.add_work_item_comment("MY_PROJ-9", "  ")


def test_add_work_item_comment_soap_fault() -> None:
    client = _client()
    mock_service = MagicMock()
    mock_service.addComment.side_effect = zeep.exceptions.Fault("denied")
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.add_work_item_comment("MY_PROJ-9", "Nope")

    assert "error" in result
