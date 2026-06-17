"""Minimal Baxter MCP server for plugin connectivity testing."""

from mcp.server.fastmcp import FastMCP

mcp = FastMCP("baxter-echo")


@mcp.tool()
def echo(message: str) -> str:
    """Echo a message to verify the Baxter MCP server is connected."""
    return message


@mcp.tool()
def health() -> str:
    """Return server health status."""
    return "ok"


def main() -> None:
    mcp.run()


if __name__ == "__main__":
    main()
