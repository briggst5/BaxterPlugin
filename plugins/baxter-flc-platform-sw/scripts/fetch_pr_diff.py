import os
import json
import subprocess
import requests


def main():
    org = os.environ.get("ADO_ORG", "https://dev.azure.com/FLC-NPD/")
    project = os.environ.get("ADO_PROJECT", "Proof Of Concept")
    repo = os.environ.get("ADO_REPO", "pocvitejs")
    pr_id = os.environ.get("PR_ID", os.environ.get("SYSTEM_PULLREQUEST_PULLREQUESTID", ""))
    token = os.environ["SYSTEM_ACCESSTOKEN"]

    if not pr_id:
        raise ValueError("PR_ID or SYSTEM_PULLREQUEST_PULLREQUESTID must be set")

    headers = {"Authorization": f"Bearer {token}"}

    # Get PR details to find source/target branches
    pr_url = f"{org}{project}/_apis/git/repositories/{repo}/pullRequests/{pr_id}?api-version=7.1"
    pr_info = requests.get(pr_url, headers=headers).json()
    source_ref = pr_info["sourceRefName"].replace("refs/heads/", "")
    target_ref = pr_info["targetRefName"].replace("refs/heads/", "")

    # Configure git auth with the build token
    subprocess.run(
        ["git", "config", "http.extraheader", f"Authorization: Bearer {token}"],
        check=True,
    )

    # Fetch both branches and get the actual diff
    subprocess.run(["git", "fetch", "origin", source_ref, target_ref], check=True)
    result = subprocess.run(
        ["git", "diff", f"origin/{target_ref}...origin/{source_ref}"],
        capture_output=True,
        text=True,
        check=True,
    )

    diff = result.stdout
    print(f"Diff length: {len(diff)} chars")
    print(f"Diff preview:\n{diff[:500]}")

    with open("diff.txt", "w") as f:
        f.write(diff)


if __name__ == "__main__":
    main()
