import os
from pathlib import Path
from openai import OpenAI


def load_checklist(checklist_path):
    """Load a single checklist file or all .md files from a directory (excluding README.md)."""
    path = Path(checklist_path)
    if path.is_dir():
        parts = []
        for f in sorted(path.glob("*.md")):
            if f.name.lower() == "readme.md":
                continue
            parts.append(f"<checklist name=\"{f.stem}\">\n{f.read_text()}\n</checklist>")
        return "\n\n".join(parts)
    if path.exists():
        with open(path) as f:
            return f"<checklist name=\"{path.stem}\">\n{f.read()}\n</checklist>"
    return ""


def run_review(client, diff, checklist):
    system_prompt = """You are a senior engineer performing a pull request review.
The codebase uses React with TypeScript and Kotlin for Android.

<workflow>
1. DESIGN PASS: Before checking any checklist item, evaluate the overall design of the change. Does it make sense architecturally? Is the approach overly complex or over-engineered? If a significant design problem exists, raise it immediately — line-level findings may be irrelevant if the design needs to change.
2. READ TESTS FIRST: If test files appear in the diff, read them before the production code. Tests define intent and help you evaluate whether the implementation is correct.
3. CHECKLIST REVIEW: Evaluate EVERY item in each checklist against the diff. Apply Kotlin-specific checklists (coroutines, exception safety, zombie, data-iteration) only to .kt files. Apply general checklists to all files.
4. RE-EVALUATE FINDINGS: Before reporting any finding, verify it is a real issue by examining the full context — not just the changed line in isolation. Discard any finding based on speculation, surface appearance, or incomplete analysis. Do not invent problems.
</workflow>

<rules>
- For each issue found, state: the file and line, what the problem is, and the severity (🔴 Blocking / 🟡 Warning / 🔵 Suggestion).
- Blocking: security vulnerabilities, data leaks, coroutine/resource leaks, or bugs that will break production.
- Warning: code quality problems, missing best practices, or potential bugs.
- Suggestion: style, naming, or minor improvements.
- If the diff has NO issues, explicitly say "No issues found."
- Be specific: reference the exact code that is problematic and explain why.
- Highlight strengths as well as issues.
- End with a clear merge recommendation: Approve / Approve with suggestions / Request changes.
</rules>"""

    if checklist:
        system_prompt += f"\n\n{checklist}"

    completion = client.chat.completions.create(
        model="gpt-4.1",
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": f"<diff>\n{diff}\n</diff>"},
        ],
    )
    return completion.choices[0].message.content


def main():
    endpoint = os.environ["AI_ENDPOINT"]
    key = os.environ["AI_KEY"]
    checklist_path = os.environ.get("CHECKLIST_PATH", "checklists")

    with open("diff.txt") as f:
        diff = f.read()

    client = OpenAI(base_url=endpoint, api_key=key)
    checklist = load_checklist(checklist_path)
    review = run_review(client, diff, checklist)

    with open("review.md", "w") as f:
        f.write(review)

    print(f"Review length: {len(review)} chars")
    print(f"Review preview:\n{review[:500]}")


if __name__ == "__main__":
    main()
