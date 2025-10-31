# Reporting Security Issues

If you find a potential security issue or vulnerability in **ProObjLog**, please report it responsibly. We take security seriously and appreciate your help in keeping this project safe and trustworthy.

## How to Report

You can report vulnerabilities by either:

1. Opening a **security advisory** or issue on GitHub (recommended), or
2. Submitting a **pull request** that includes both a fix and an explanation of the issue.

> Please **do not disclose vulnerabilities publicly** until a fix has been merged and released by the maintainers.

---

## Before Reporting

Before filing a report, please make sure:

* You’re using a **supported version** of ProObjLog (see table below).
* The issue isn’t caused by a **misconfigured environment** or **modified source code**.
* You’ve tested the issue against the **latest stable release**.

Reports related to poor user setup, third-party modifications, or unsupported .NET runtimes will not be addressed unless they impact default, recommended, or documented configurations.

If the vulnerability affects multiple versions, please specify which environments or builds are affected.

---

## Supported Versions

| Version | Supported             |
| ------- | --------------------- |
| 3.x.x   | ✅ Active support      |
| 2.x.x   | ✅ Security fixes only |
| <2.x.x  | ❌ Unsupported         |

---

## Response Process

1. **Acknowledge receipt** within 48 hours.
2. **Verify and reproduce** the issue in a secure environment.
3. **Develop and test a fix**, typically within 1–2 weeks depending on severity.
4. **Release a patched version** and update the changelog.
5. **Credit the reporter** (if desired) in release notes.

---

## Security Philosophy

* Transparency is key — every confirmed fix will be publicly documented once safe to do so.
* Default configurations should be **secure by design**.
* No sensitive data (like tokens, credentials, or paths) should ever be logged by default.
* Input validation and process sandboxing are enforced where possible.

---

If you’re unsure whether something qualifies as a security issue, err on the side of caution — **report it privately**. We’ll take it from there.
