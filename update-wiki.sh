#!/bin/bash

# Script to update the GitHub wiki with the fixed links
# This script applies the changes that fix wiki pages showing raw text

echo "Updating GitHub Wiki..."
echo ""
echo "This script will:"
echo "1. Clone the wiki repository"
echo "2. Apply the fix to remove .md extensions from internal links"
echo "3. Push the changes to GitHub"
echo ""

# Clone the wiki repository
if [ -d "idasen-desk-core.wiki" ]; then
    echo "Wiki repository already exists, pulling latest changes..."
    cd idasen-desk-core.wiki
    git pull
    cd ..
else
    echo "Cloning wiki repository..."
    git clone https://github.com/tschroedter/idasen-desk-core.wiki.git
fi

cd idasen-desk-core.wiki

# Apply the patch
echo "Applying wiki link fixes..."

# Fix Home.md
sed -i 's/Getting-Started\.md)/Getting-Started)/g' Home.md
sed -i 's/Getting-Started\.md#/Getting-Started#/g' Home.md
sed -i 's/Development-Guide\.md)/Development-Guide)/g' Home.md
sed -i 's/Architecture-and-Design\.md)/Architecture-and-Design)/g' Home.md
sed -i 's/Testing-Guide\.md)/Testing-Guide)/g' Home.md
sed -i 's/Contributing\.md)/Contributing)/g' Home.md
sed -i 's/API-Reference\.md)/API-Reference)/g' Home.md
sed -i 's/CI-CD-Workflows\.md)/CI-CD-Workflows)/g' Home.md
sed -i 's/Troubleshooting\.md)/Troubleshooting)/g' Home.md

# Fix README.md
sed -i 's/Home\.md)/Home)/g' README.md
sed -i 's/Getting-Started\.md)/Getting-Started)/g' README.md
sed -i 's/Development-Guide\.md)/Development-Guide)/g' README.md
sed -i 's/Architecture-and-Design\.md)/Architecture-and-Design)/g' README.md
sed -i 's/Testing-Guide\.md)/Testing-Guide)/g' README.md
sed -i 's/Contributing\.md)/Contributing)/g' README.md
sed -i 's/API-Reference\.md)/API-Reference)/g' README.md
sed -i 's/CI-CD-Workflows\.md)/CI-CD-Workflows)/g' README.md
sed -i 's/Troubleshooting\.md)/Troubleshooting)/g' README.md

# Fix Getting-Started.md
sed -i 's/API-Reference\.md)/API-Reference)/g' Getting-Started.md
sed -i 's/Architecture-and-Design\.md)/Architecture-and-Design)/g' Getting-Started.md
sed -i 's/Development-Guide\.md)/Development-Guide)/g' Getting-Started.md
sed -i 's/Troubleshooting\.md)/Troubleshooting)/g' Getting-Started.md
sed -i 's/Contributing\.md)/Contributing)/g' Getting-Started.md

# Fix Development-Guide.md
sed -i 's/Architecture-and-Design\.md)/Architecture-and-Design)/g' Development-Guide.md
sed -i 's/Testing-Guide\.md)/Testing-Guide)/g' Development-Guide.md
sed -i 's/API-Reference\.md)/API-Reference)/g' Development-Guide.md
sed -i 's/Contributing\.md)/Contributing)/g' Development-Guide.md

# Fix API-Reference.md
sed -i 's/Getting-Started\.md)/Getting-Started)/g' API-Reference.md

# Fix Architecture-and-Design.md
sed -i 's/API-Reference\.md)/API-Reference)/g' Architecture-and-Design.md

# Fix CI-CD-Workflows.md
sed -i 's/Development-Guide\.md)/Development-Guide)/g' CI-CD-Workflows.md

# Fix Testing-Guide.md
sed -i 's/Development-Guide\.md)/Development-Guide)/g' Testing-Guide.md
sed -i 's/Contributing\.md)/Contributing)/g' Testing-Guide.md

# Fix Troubleshooting.md
sed -i 's/Getting-Started\.md)/Getting-Started)/g' Troubleshooting.md
sed -i 's/Development-Guide\.md)/Development-Guide)/g' Troubleshooting.md
sed -i 's/API-Reference\.md)/API-Reference)/g' Troubleshooting.md
sed -i 's/Testing-Guide\.md)/Testing-Guide)/g' Troubleshooting.md

# Commit and push
git add .
git commit -m "Fix wiki links to prevent raw text display

Remove .md extensions from internal wiki page links. GitHub wiki expects 
links without the .md extension to properly render pages instead of 
showing raw markdown text."

echo ""
echo "Pushing changes to GitHub..."
git push

echo ""
echo "Wiki update complete!"
echo "Visit https://github.com/tschroedter/idasen-desk-core/wiki to verify the changes."
