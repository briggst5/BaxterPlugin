# Continue Extension Integration

## Setup

1. Clone the ai-skills repository into `.agents/` at your project root: [Integration Strategy](INTEGRATION_STRATEGY.md#prerequisites)

2. Copy `templates/continue-project-template/.continue/` to your project root:

**Windows (PowerShell)**

```powershell
Copy-Item -Path .\.agents\templates\continue-project-template\.continue\ -Destination .continue\ -Recurse -Force
```

**Linux / macOS**

```bash
cp -r ./.agents/templates/continue-project-template/.continue/* .continue/
```
