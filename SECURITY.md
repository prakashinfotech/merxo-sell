# 🛡️ Security Policy

## Supported Versions

The following versions of **MerxoSell** are currently receiving security updates:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

---

## 🔒 Reporting a Vulnerability

We take the security of **MerxoSell** seriously. If you discover a security vulnerability, please report it responsibly so we can resolve it promptly.

### How to Report
- **Email**: Send security vulnerability details to `suthary980@gmail.com` or `security@prakashinfotech.com`.
- **Do NOT** open public GitHub issues for security vulnerabilities.

### What to Include in Your Report
1. Description of the vulnerability and its potential impact.
2. Step-by-step instructions or proof-of-concept to reproduce the issue.
3. Affected components (e.g. JWT Auth middleware, Razorpay webhook signature, SQL injection vectors).
4. Any suggested remediations or mitigations.

---

## ⏱️ Response Timeline

- **Acknowledgement**: Within 24-48 hours of report submission.
- **Triage & Assessment**: Within 3 business days.
- **Fix & Patch Release**: High-severity vulnerabilities are prioritized and patched within 7 business days.

---

## 🔐 Security Best Practices in MerxoSell

- **Authentication**: JWT authentication with BCrypt password hashing.
- **Authorization**: Strict Role-Based Access Control (`SuperAdmin`, `Seller`, `Buyer`).
- **Data Protection**: Parameterized SQL queries via Entity Framework Core to prevent SQL Injection.
- **Payment Security**: Payment processing handled securely via Razorpay Checkout JS modal with transaction verification.

Thank you for helping keep MerxoSell safe and safe for everyone!
