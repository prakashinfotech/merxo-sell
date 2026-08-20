# 🤝 Contributing to MerxoSell

First off, thank you for considering contributing to **MerxoSell**! It is contributions like yours that make MerxoSell a powerful, high-performance multi-vendor e-commerce platform.

Please take a moment to review these guidelines before submitting code or opening issues.

---

## 📜 Code of Conduct

By participating in this project, you agree to maintain a respectful, welcoming, and inclusive environment for everyone. Please treat all community members with courtesy and respect.

---

## 🛠️ How to Contribute

### 1. Reporting Bugs
- Search existing issues to ensure the bug hasn't already been reported.
- If not, create a new issue detailing:
  - Concise title describing the problem.
  - Steps to reproduce.
  - Expected vs actual behavior.
  - Environment details (OS, Browser, Node.js version, .NET version).
  - Screenshots or log tracebacks (if available).

### 2. Suggesting Enhancements
- Check if your feature suggestion is already listed or discussed.
- Open a feature request issue explaining:
  - Clear rationale for why the feature is valuable.
  - Proposed behavior or API design.

### 3. Pull Request Workflow
1. **Fork & Clone**: Fork the repository and clone your fork locally.
   ```bash
   git clone https://github.com/prakashinfotech/merxo-sell.git
   cd merxo-sell
   ```
2. **Branching**: Create a topic branch off of `master` / `main`:
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   ```
3. **Development**:
   - Follow clean code practices.
   - For backend (.NET): Adhere to Repository Pattern and Clean N-Tier Architecture.
   - For frontend (Angular): Keep components modular, use SCSS themes, and enforce strict type safety.
4. **Testing**:
   - Ensure all existing unit & integration tests pass.
   - Write tests for new functionality.
   ```bash
   # Backend tests
   dotnet test MerxoSell.sln

   # Frontend tests
   cd frontend && npm test
   ```
5. **Commit Messages**: Write clear, descriptive commit messages:
   ```text
   feat(payment): add Razorpay order verification endpoint
   fix(banner): correct image fallback path in hero carousel
   ```
6. **Submit PR**: Push your branch to GitHub and create a Pull Request detailing the changes made.

---

## 🎨 Coding Standards

### Backend (.NET 9.0)
- Use standard C# coding conventions and pascal casing for public properties/methods.
- Use async/await for I/O operations and database queries.
- Return standardized `ApiResponse<T>` wrappers for API responses.

### Frontend (Angular 19)
- Modular design with standalone components or feature modules.
- Use explicit RxJS subscription cleanup or `takeUntilDestroyed()`.
- Use standard BEM naming or CSS module conventions in SCSS files.

---

## 👥 Contributors

We appreciate all contributors! Check out the [Contributors](README.md#-contributors) section in the `README.md` file.

Thank you for building MerxoSell with us! 🚀
