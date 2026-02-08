set -euo pipefail

RUNNER_TEMP="/Users/nick/BuildTemp"
ASC_KEY_ID="84CU2FLJA6"
ASC_ISSUER_ID="69a6de90-a430-47e3-e053-5b8c7c11a4d1"
IOS_BUNDLE_ID="com.nk.onedrivealbums"
mkdir -p "$RUNNER_TEMP/appstoreconnect"
KEY_PATH="$RUNNER_TEMP/appstoreconnect/AuthKey_${ASC_KEY_ID}.p8"

IPA="$(find UI/bin/Release/net10.0-ios -type f -name '*.ipa' -print -quit)"
if [ -z "$IPA" ]; then
    echo "IPA not found" >&2
    exit 1
fi

bundle exec fastlane ios upload_testflight \
    key_id:"$ASC_KEY_ID" \
    issuer_id:"$ASC_ISSUER_ID" \
    key_filepath:"$KEY_PATH" \
    ipa:"$IPA" \
    app_identifier:"$IOS_BUNDLE_ID"

