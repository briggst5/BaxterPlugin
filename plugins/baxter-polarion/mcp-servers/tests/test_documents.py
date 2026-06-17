"""Unit tests for document / module work item listing."""

from __future__ import annotations

from unittest.mock import MagicMock, patch

import pytest

from polarion_mcp.polarion_client import PolarionClient, _format_module_summary


def test_format_module_summary() -> None:
    summary = _format_module_summary(
        {
            "id": "doc-1",
            "title": "My Spec",
            "moduleFolder": "Inputs/My Spec",
            "moduleName": "My Spec",
            "uri": "subterra:…",
            "type": {"id": "specification"},
            "status": {"id": "draft"},
        }
    )
    assert summary["title"] == "My Spec"
    assert summary["space"] == "Inputs"
    assert summary["type"] == "specification"


def test_document_query_parts_title_only() -> None:
    name, space, clauses = PolarionClient._document_query_parts("My Spec")
    assert name == "My Spec"
    assert space == ""
    assert clauses == ['document.title:"My Spec"']


def test_document_query_parts_with_space() -> None:
    name, space, clauses = PolarionClient._document_query_parts(
        "Connex 360 SRS", space="Inputs"
    )
    assert name == "Connex 360 SRS"
    assert space == "Inputs"
    assert 'document.moduleFolder:"Inputs/Connex 360 SRS"' in clauses
    assert 'document.title:"Connex 360 SRS"' in clauses


def test_document_query_parts_url_encoded_name() -> None:
    _, _, clauses = PolarionClient._document_query_parts("Connex%20360%20SRS")
    assert 'document.title:"Connex%20360%20SRS"' in clauses
    assert 'document.title:"Connex 360 SRS"' in clauses


def test_document_query_parts_empty_name_raises() -> None:
    with pytest.raises(ValueError, match="document_name"):
        PolarionClient._document_query_parts("  ")


def test_list_document_work_items_returns_metadata() -> None:
    client = PolarionClient(
        url="http://polarion.example.com/polarion",
        username="user",
        password="secret",
        project_id="MY_PROJ",
    )
    items = [{"id": "MY_PROJ-1", "title": "Req 1"}]
    with patch.object(client, "_run_query", return_value=items) as mock_query:
        result = client.list_document_work_items(
            "My Spec", space="Design", limit=50
        )

    assert result["document_name"] == "My Spec"
    assert result["space"] == "Design"
    assert result["count"] == 1
    assert result["limit"] == 50
    assert result["work_items"] == items
    mock_query.assert_called_once()
    query_arg = mock_query.call_args[0][0]
    assert "project.id:MY_PROJ" in query_arg
    assert "document.moduleFolder" in query_arg


def test_module_location_with_space() -> None:
    loc = PolarionClient._module_location(
        "Connex 360 SRS", space="Inputs"
    )
    assert loc == "Inputs/Connex 360 SRS"


def test_get_document_text_home_page_and_sections() -> None:
    client = PolarionClient(
        url="http://polarion.example.com/polarion",
        username="user",
        password="secret",
        project_id="MY_PROJ",
    )
    mock_service = MagicMock()
    mock_service.getModuleByLocationWithFields.return_value = {
        "uri": "subterra:data-service:objects:/default/MY_PROJ${Module}doc",
        "title": "My Spec",
        "homePageContent": {
            "type": "text/html",
            "content": "<p>Intro paragraph</p>",
        },
        "unresolvable": False,
    }
    mock_service.getModuleWorkItems.return_value = [
        {
            "id": "MY_PROJ-1",
            "type": {"id": "heading"},
            "title": "Section 1",
            "outlineNumber": "1",
            "description": {"type": "text/html", "content": "<p>Section body</p>"},
        },
        {
            "id": "MY_PROJ-2",
            "type": {"id": "requirement"},
            "title": "Req without text",
            "description": {"type": "text/html", "content": ""},
        },
    ]
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.get_document_text("My Spec", space="Design")

    assert result["title"] == "My Spec"
    assert result["home_page_content"]["content"] == "<p>Intro paragraph</p>"
    assert len(result["sections"]) == 1
    assert result["sections"][0]["work_item_id"] == "MY_PROJ-1"
    assert "Intro paragraph" in result["combined_content"]
    assert "Section body" in result["combined_content"]
    mock_service.getModuleByLocationWithFields.assert_called_once_with(
        "MY_PROJ",
        "Design/My Spec",
        ["uri", "id", "title", "moduleFolder", "moduleName", "homePageContent"],
    )


def test_list_documents_queries_project() -> None:
    client = PolarionClient(
        url="http://polarion.example.com/polarion",
        username="user",
        password="secret",
        project_id="MY_PROJ",
    )
    mock_service = MagicMock()
    mock_service.queryModules.return_value = [
        {
            "id": "d1",
            "title": "Doc A",
            "moduleFolder": "Inputs/Doc A",
            "moduleName": "Doc A",
            "uri": "subterra:doc-a",
        }
    ]
    mock_tracker = MagicMock()
    mock_tracker.service = mock_service

    with patch.object(client, "_tracker", return_value=mock_tracker):
        client.connect = MagicMock()  # type: ignore[method-assign]
        result = client.list_documents(space="Inputs", limit=50)

    assert result["project_id"] == "MY_PROJ"
    assert result["space"] == "Inputs"
    assert result["count"] == 1
    assert result["documents"][0]["title"] == "Doc A"
    mock_service.queryModules.assert_called_once()
    args = mock_service.queryModules.call_args[0]
    assert args[0] == 'project.id:MY_PROJ AND moduleFolder:"Inputs/*"'
    assert args[3] == 50


def test_list_documents_requires_project() -> None:
    client = PolarionClient(
        url="http://polarion.example.com/polarion",
        username="user",
        password="secret",
        project_id="",
    )
    with pytest.raises(RuntimeError, match="POLARION_PROJECT"):
        client.list_documents()


def test_get_document_text_requires_project() -> None:
    client = PolarionClient(
        url="http://polarion.example.com/polarion",
        username="user",
        password="secret",
        project_id="",
    )
    with pytest.raises(RuntimeError, match="POLARION_PROJECT"):
        client.get_document_text("My Spec")
