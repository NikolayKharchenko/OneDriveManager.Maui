# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction

## Git Operations
- When diagnosing Git push failures, check for ambiguous refs (e.g., both refs/heads/master and refs/tags/master) since Visual Studio push can fail with 'dst refspec master matches more than one'.

## CI/CD Setup
- Use Fastlane for CI/CD setup with MAUI.
- Decode the App Store Connect API key (.p8) from base64 using `printf` (not `echo`), and normalize it with `tr -d \r`.
- Validate the API key using `openssl pkey`.
- Set the iOS build version via `/p:ApplicationVersion`.
- Run Android and iOS builds in parallel jobs.