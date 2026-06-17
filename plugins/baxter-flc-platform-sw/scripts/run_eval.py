import os
import json
from openai import OpenAI
from run_ai_review import load_checklist, run_review


def evaluate_response(client, test_case, review):
    """Use LLM as judge to evaluate the review against expected criteria."""
    eval_prompt = f"""You are evaluating an AI PR review for correctness.

Test case: {test_case['description']}
Expected severity: {test_case['severity']}
Expected flags (keywords that should appear in the review): {json.dumps(test_case['expected_flags'])}

The AI review output:
---
{review}
---

Evaluate:
1. Did the review correctly identify the issues? (yes/no)
2. Did it flag the expected keywords or equivalent concepts? List which were found and which were missed.
3. Is the severity assessment appropriate? (yes/no)
4. Overall score (1-5, where 5 = perfect)

If expected_flags is empty, the review should NOT flag any blocking issues.

Respond in JSON format:
{{
  "issues_identified": true/false,
  "flags_found": ["list of matched flags"],
  "flags_missed": ["list of missed flags"],
  "severity_correct": true/false,
  "score": 1-5,
  "reasoning": "brief explanation"
}}"""

    completion = client.chat.completions.create(
        model="gpt-4.1",
        messages=[
            {"role": "system", "content": "You are a strict evaluator. Respond only in valid JSON."},
            {"role": "user", "content": eval_prompt},
        ],
    )
    return completion.choices[0].message.content


def main():
    endpoint = os.environ["AI_ENDPOINT"]
    key = os.environ["AI_KEY"]
    checklist_path = os.environ.get("CHECKLIST_PATH", "checklists/pr-review-checklist.md")
    test_cases_path = os.environ.get("TEST_CASES_PATH", "evals/test_cases.json")

    client = OpenAI(base_url=endpoint, api_key=key)
    checklist = load_checklist(checklist_path)

    with open(test_cases_path) as f:
        test_cases = json.load(f)

    results = []
    total_score = 0

    for tc in test_cases:
        print(f"\n{'='*60}")
        print(f"Test: {tc['id']} - {tc['description']}")
        print(f"{'='*60}")

        # Run the review
        review = run_review(client, tc["diff"], checklist)
        print(f"Review preview: {review[:200]}...")

        # Evaluate with LLM judge
        eval_raw = evaluate_response(client, tc, review)
        print(f"Eval: {eval_raw}")

        try:
            eval_result = json.loads(eval_raw)
        except json.JSONDecodeError:
            eval_result = {"score": 0, "reasoning": "Failed to parse eval response"}

        eval_result["test_id"] = tc["id"]
        eval_result["review"] = review
        results.append(eval_result)
        total_score += eval_result.get("score", 0)

    # Summary
    avg_score = total_score / len(test_cases) if test_cases else 0
    print(f"\n{'='*60}")
    print(f"EVAL SUMMARY")
    print(f"{'='*60}")
    print(f"Total tests: {len(test_cases)}")
    print(f"Average score: {avg_score:.1f}/5")
    print(f"Pass rate: {sum(1 for r in results if r.get('score', 0) >= 4)}/{len(test_cases)}")

    for r in results:
        status = "PASS" if r.get("score", 0) >= 4 else "FAIL"
        print(f"  [{status}] {r['test_id']}: {r.get('score', 0)}/5 - {r.get('reasoning', '')[:80]}")

    # Write results
    with open("eval_results.json", "w") as f:
        json.dump({"average_score": avg_score, "results": results}, f, indent=2)

    # Fail if average below threshold
    threshold = float(os.environ.get("EVAL_THRESHOLD", "3.5"))
    if avg_score < threshold:
        raise Exception(f"Eval failed: average score {avg_score:.1f} below threshold {threshold}")


if __name__ == "__main__":
    main()
