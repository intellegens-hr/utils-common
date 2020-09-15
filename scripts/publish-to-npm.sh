#!/bin/bash
cd "$(cd -P -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)";

# Exit when any command fails
set -e

# Build library version
echo '';
echo '> Building latest library version ...';
cd ../ngz
ng build --prod;
cp -f ../ngz/README.md ../ngz/dist/ngz-common;

# Check if main project package.json and library package.json are have same proeprty values
echo '';
echo '> Comparing package.json files between repo and library...';
npx jasmine ../ngz/package.spec.ts

# Publish via NPM
echo '';
echo '> Publishing to NPM ...';
cd ../ngz/dist/ngz-common
npm publish --access public
