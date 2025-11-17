# Idasen Desk Core Wiki

This directory contains comprehensive documentation for the Idasen Desk Core library.

## 📖 Wiki Pages

### Getting Started
- **[Home](Home.md)** - Main wiki landing page with overview
- **[Getting Started](Getting-Started.md)** - Installation, setup, and quick start guide

### Development
- **[Development Guide](Development-Guide.md)** - Complete development environment setup and workflow
- **[Architecture and Design](Architecture-and-Design.md)** - System architecture and design patterns
- **[Testing Guide](Testing-Guide.md)** - Comprehensive testing documentation
- **[Contributing](Contributing.md)** - Guidelines for contributing to the project

### Reference
- **[API Reference](API-Reference.md)** - Detailed API documentation with examples
- **[CI/CD Workflows](CI-CD-Workflows.md)** - Information about automated workflows
- **[Troubleshooting](Troubleshooting.md)** - Common issues and solutions

## 🚀 Quick Links

- [GitHub Repository](https://github.com/tschroedter/idasen-desk-core)
- [NuGet Package](https://www.nuget.org/packages/Idasen.Desk.Core/)
- [Issues](https://github.com/tschroedter/idasen-desk-core/issues)
- [Discussions](https://github.com/tschroedter/idasen-desk-core/discussions)

## 📋 Using This Wiki

### For New Users
1. Start with [Home](Home.md) for an overview
2. Follow [Getting Started](Getting-Started.md) to install and run
3. Check [API Reference](API-Reference.md) for usage examples

### For Contributors
1. Read [Contributing](Contributing.md) for guidelines
2. Set up your environment with [Development Guide](Development-Guide.md)
3. Learn about testing in [Testing Guide](Testing-Guide.md)

### For Troubleshooting
1. Check [Troubleshooting](Troubleshooting.md) for common issues
2. Review [CI/CD Workflows](CI-CD-Workflows.md) for build problems
3. Open an issue if you can't find a solution

## 🔄 GitHub Wiki Setup

These markdown files can be published to GitHub Wiki:

### Option 1: Manual Copy
1. Enable Wiki in repository settings
2. Navigate to Wiki tab
3. Create/edit pages with content from these files

### Option 2: Automated Sync
Use a GitHub Action to sync this directory to Wiki:
```yaml
- name: Upload to Wiki
  uses: SwiftDocOrg/github-wiki-publish-action@v1
  with:
    path: wiki
  env:
    GH_PERSONAL_ACCESS_TOKEN: ${{ secrets.GH_TOKEN }}
```

### Option 3: Clone Wiki Repository
```bash
git clone https://github.com/tschroedter/idasen-desk-core.wiki.git
cp wiki/*.md idasen-desk-core.wiki/
cd idasen-desk-core.wiki
git add .
git commit -m "Add wiki documentation"
git push
```

## 📝 Maintaining the Wiki

### Adding New Pages
1. Create markdown file in this directory
2. Add link to Home.md and README.md
3. Follow existing formatting conventions
4. Include code examples where helpful

### Updating Existing Pages
1. Keep information accurate and current
2. Update examples with latest API
3. Maintain consistent formatting
4. Test all code samples

### Documentation Standards
- Use clear, concise language
- Include practical code examples
- Link to related documentation
- Keep formatting consistent
- Update last modified dates

## 🤝 Contributing to Documentation

Documentation improvements are welcome! To contribute:

1. Fork the repository
2. Create a branch for your changes
3. Update or add documentation files
4. Submit a pull request

See [Contributing Guide](Contributing.md) for details.

## 📄 License

This documentation is part of the Idasen Desk Core project and is licensed under the MIT License.

---

*Last updated: November 2024*
