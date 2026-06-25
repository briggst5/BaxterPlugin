# CWE code analysis patterns

Concise search patterns for common CWE classes. Adapt regex and API names to the project's language and frameworks.

## How to use

1. Identify CWE from CVE record, threat model, or audit scope
2. Find the CWE section below
3. Run suggested searches in the target codebase or diff
4. Inspect hits for missing mitigations (validation, authz, encoding, bounds checks)
5. Rate finding confidence: confirmed | likely | informational

## CWE-20 — Improper input validation

```bash
rg -n 'parseInt|parseFloat|Number\(|readString|scanf|atoi|stoi|FromBody|@RequestParam' --type-add 'web:*.{cs,ts,js,py,go}' -t web
rg -n 'validate|sanitiz' -i --glob '*.{cs,ts,js,py,go}' | head -50
```

Look for: request handlers without validation, unchecked array lengths, trust in client-supplied IDs.

## CWE-22 — Path traversal

```bash
rg -n 'Path\.Combine|join\(.*path|open\(|readFile|File\.Read|\.\./|getParameter.*file' --glob '*.{cs,ts,js,py,go,c,cpp}'
```

Look for: user input in file paths without canonicalization or allowlist.

## CWE-78 / CWE-77 — OS command injection

```bash
rg -n 'exec\(|system\(|popen|subprocess|Process\.Start|child_process|Runtime\.getRuntime' --glob '*.{cs,ts,js,py,go,c,cpp}'
```

Look for: shell=True, string concatenation into commands, unescaped user input.

## CWE-79 — Cross-site scripting (XSS)

```bash
rg -n 'innerHTML|document\.write|dangerouslySetInnerHtml|@Html\.Raw|v-html|eval\(' --glob '*.{cshtml,tsx,jsx,vue,js,ts}'
rg -n 'encode|escape|sanitize' -i --glob '*.{cshtml,tsx,jsx,vue,js,ts}'
```

Look for: unencoded user content in HTML/JS contexts; CSP headers in web configs.

## CWE-89 — SQL injection

```bash
rg -n 'ExecuteSql|FromSqlRaw|string\.Format.*SELECT|"\s*SELECT|"INSERT|"UPDATE|"DELETE' --glob '*.{cs,java,py,go}'
rg -n 'SqlCommand|cursor\.execute|Query\(|raw\(' --glob '*.{cs,java,py,go}'
```

Look for: string-built queries; prefer parameterized queries / ORM.

## CWE-94 / CWE-95 — Code injection

```bash
rg -n 'eval\(|Function\(|pickle\.loads|yaml\.load\(|Deserialize|BinaryFormatter|ObjectInputStream' --glob '*.{cs,ts,js,py,java}'
```

Look for: deserialization of untrusted data without type constraints.

## CWE-119 / CWE-120 / CWE-787 — Memory buffer issues

```bash
rg -n 'memcpy|strcpy|strncpy|sprintf|gets|read\(|alloca' --glob '*.{c,cpp,h,hpp}'
rg -n 'unsafe|fixed\s|stackalloc' --glob '*.cs'
```

Look for: missing bounds checks, fixed-size buffers with variable input.

## CWE-200 — Information exposure

```bash
rg -n 'password|secret|token|apikey|private_key' -i --glob '*.{cs,ts,js,py,go,json,yml,yaml,env*}'
rg -n 'console\.log|logger\.|Log\.|print\(|printf\(' --glob '*.{cs,ts,js,py,go}' | rg -i 'password|token|secret'
```

Look for: secrets in logs, verbose errors to clients, debug endpoints in production.

## CWE-287 / CWE-306 — Authentication weaknesses

```bash
rg -n 'AllowAnonymous|authorize.*false|skipAuth|noAuth|@PermitAll' -i --glob '*.{cs,java,ts,js}'
rg -n 'JWT|Bearer|session|cookie' --glob '*.{cs,java,ts,js,py}'
```

Look for: sensitive routes without auth middleware; weak session config.

## CWE-352 — CSRF

```bash
rg -n 'csrf|ValidateAntiForgeryToken|SameSite' -i --glob '*.{cs,java,ts,js}'
```

Look for: state-changing POST/PUT/DELETE without CSRF tokens or SameSite cookies.

## CWE-502 — Deserialization of untrusted data

```bash
rg -n 'BinaryFormatter|JsonConvert\.Deserialize|pickle|yaml\.load|unserialize|readObject' --glob '*.{cs,java,py,php}'
```

## CWE-611 — XML external entity (XXE)

```bash
rg -n 'XmlReader|XmlDocument|XDocument|lxml|etree\.parse' --glob '*.{cs,java,py}'
```

Look for: DTD processing enabled, external entity resolution not disabled.

## CWE-798 — Hard-coded credentials

```bash
rg -n '(password|passwd|api_?key|secret)\s*[=:]\s*["\x27][^"\x27]{8,}' -i --glob '*.{cs,ts,js,py,go,json,yml,yaml,properties}'
```

## CWE-918 — SSRF

```bash
rg -n 'HttpClient|fetch\(|requests\.(get|post)|urllib|WebRequest|curl' --glob '*.{cs,ts,js,py,go,java}'
```

Look for: user-supplied URLs in outbound requests without allowlist or IP blocklist.

## CWE-1333 — ReDoS

```bash
rg -n 'new RegExp|Regex\.|re\.compile|Pattern\.compile' --glob '*.{cs,ts,js,py,java,go}'
```

Look for: nested quantifiers in regex built from user input.

## Language-specific quick refs

| Stack | Also check |
|-------|------------|
| C/C++ embedded | `strcpy`, `sprintf`, unchecked `memcpy`, integer overflow before allocation |
| Yocto/Linux | world-writable paths, weak file permissions in recipes, debug tools in production images |
| .NET / ASP | `AllowAnonymous`, `Html.Raw`, `BinaryFormatter`, misconfigured `DataProtection` |
| Node / TS | `eval`, prototype pollution (`__proto__`), missing `helmet` |
| Python | `subprocess` with shell, `pickle`, `yaml.load` without SafeLoader |

## External references

- MITRE CWE: https://cwe.mitre.org/data/definitions/NNNN.html
- OWASP Cheat Sheet Series: https://cheatsheetseries.owasp.org/
- CWE Top 25: https://cwe.mitre.org/data/published/lists/2000/2000_cwe_top25.html
