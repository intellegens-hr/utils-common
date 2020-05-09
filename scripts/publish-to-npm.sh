#!/bin/bash
cd "$(cd -P -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)";

# Exit when any command fails
set -e

# Build library version
echo '';
echo '> Building latest library version ...';
cd ../ngz
ng build --prod;

# Check if main project package.json and library package.json are have same proeprty values
echo '';
echo '> Comparing package.json files between repo and library...';
# Check version
repVersion=$( cat ../ngz/package.json | jq -r ".version" );
libVersion=$( cat ../ngz/dist/ngz-common/package.json | jq -r ".version");
echo "- version: '${repVersion}' ?= '${libVersion}'";
if [ "${repVersion}" != "${libVersion}" ]; then
  echo "ERROR: version in package.json is different between the showcase repo and the library!";
  exit 1;
fi
# Check name
repName=$( cat ../ngz/package.json | jq -r ".name" );
libName=$( cat ../ngz/dist/ngz-common/package.json | jq -r ".name");
echo "- name: '${repName}' ?= '${libName}'";
if [ "${repName}" != "${libName}" ]; then
  echo "ERROR: name in package.json is different between the showcase repo and the library!";
  exit 1;
fi
# Check description
repDescription=$( cat ../ngz/package.json | jq -r ".description" );
libDescription=$( cat ../ngz/dist/ngz-common/package.json | jq -r ".description");
echo "- description: '${repDescription}' ?= '${libDescription}'";
if [ "${repDescription}" != "${libDescription}" ]; then
  echo "ERROR: description in package.json is different between the showcase repo and the library!";
  exit 1;
fi
# Check repository
repRepository=$( cat ../ngz/package.json | jq -r ".repository" );
libRepository=$( cat ../ngz/dist/ngz-common/package.json | jq -r ".repository");
echo "- repository: '${repRepository}' ?= '${libRepository}'";
if [ "${repRepository}" != "${libRepository}" ]; then
  echo "ERROR: repository in package.json is different between the showcase repo and the library!";
  exit 1;
fi
# Check keywords
repKeywords=$( cat ../ngz/package.json | jq -r ".keywords" );
libKeywords=$( cat ../ngz/dist/ngz-common/package.json | jq -r ".keywords");
echo "- keywords: '${repKeywords}' ?= '${libKeywords}'";
if [ "${repKeywords}" != "${libKeywords}" ]; then
  echo "ERROR: keywords in package.json are different between the showcase repo and the library!";
  exit 1;
fi
# Check author
repAuthor=$( cat ../ngz/package.json | jq -r ".author" );
libAuthor=$( cat ../ngz/dist/ngz-common/package.json | jq -r ".author");
echo "- keywords: '${repAuthor}' ?= '${libAuthor}'";
if [ "${repAuthor}" != "${libAuthor}" ]; then
  echo "ERROR: author in package.json is different between the showcase repo and the library!";
  exit 1;
fi
# Check license
repLicense=$( cat ../ngz/package.json | jq -r ".license" );
libLicense=$( cat ../ngz/dist/ngz-common/package.json | jq -r ".license");
echo "- license: '${repLicense}' ?= '${libLicense}'";
if [ "${repLicense}" != "${libLicense}" ]; then
  echo "ERROR: author in package.json is different between the showcase repo and the library!";
  exit 1;
fi

# TODO: Add checks for: repository, keywords, author, license

# Publish via NPM
echo '';
echo '> Publishing to NPM ...';
cd ../ngz/dist/ngz-common
npm publish --access public
