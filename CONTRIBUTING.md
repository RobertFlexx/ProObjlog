# Contributing

We love contributions! Whether you’re fixing a typo, improving documentation, or adding new functionality — all help is appreciated.

---

## Getting Started

First, fork the repository and clone your fork locally:

```bash
git clone YOUR_FORK_URL
cd ProObjLog
```

Then, open the project in your preferred IDE (like Visual Studio, Rider, or VS Code). ProObjLog is a **C# project built on .NET 9**, so you’ll need the .NET SDK installed.

---

## Building

To build the project:

```bash
dotnet build
```

To run the logger directly:

```bash
dotnet run --project ProObjLog INFO "Hello from ProObjLog!"
```

If you’d like to make a self-contained binary:

```bash
dotnet publish -c Release -o out
```

This creates an executable in the `out/` directory that doesn’t require .NET to be installed on the target system.

---

## Code Style

* Use **PascalCase** for class names and **camelCase** for variables.
* Keep code formatted consistently with the built-in `.editorconfig` rules.
* Prefer `var` for local variables when the type is obvious.
* Avoid unnecessary dependencies — the goal is to keep ProObjLog lightweight and portable.
* All public methods should have XML-style doc comments.

---

## Submitting a Pull Request

1. Push your branch to your fork:

   ```bash
   git push origin your-feature-branch
   ```
2. Open a **pull request** against the main repository.
3. In your PR description:

   * Clearly describe what you changed and why.
   * Include any relevant screenshots or example log output if applicable.
   * Reference related issues if applicable.

After submission, a maintainer will review your changes and may request revisions before merging.

---

## Testing

ProObjLog doesn’t rely on external frameworks, so testing is simple:

```bash
dotnet test
```

You can also manually test by running the executable and inspecting the console and log file outputs under `/logs`.

---

## Contribution Guidelines

* Keep PRs small and focused — one feature or bugfix per request.
* Avoid unrelated formatting changes.
* Always test your changes before committing.
* If you’re adding a new feature, update or create an example in the documentation.

---

## License

By contributing to this project, you agree that your contributions will be licensed under the [Zlib License](LICENSE), the same license that governs ProObjLog.

---

## Need Help?

If you have questions about contributing, open a discussion or issue on GitHub — or check existing issues to see if someone else is working on a similar idea.
