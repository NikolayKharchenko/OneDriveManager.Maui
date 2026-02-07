set -euo pipefail

RUNNER_TEMP="/Users/nick/BuildTemp"
ASC_KEY_ID="523YK9ACR4"
ASC_ISSUER_ID="69a6de90-a430-47e3-e053-5b8c7c11a4d1"
IOS_BUNDLE_ID="com.nk.onedrivealbums"
mkdir -p "$RUNNER_TEMP/appstoreconnect"
KEY_PATH="$RUNNER_TEMP/appstoreconnect/AuthKey_${ASC_KEY_ID}.p8"
echo "$ASC_P8_BASE64" | base64 --decode > "$KEY_PATH"

API_KEY_JSON_PATH="$RUNNER_TEMP/appstoreconnect/api_key.json"
python3 - <<PY
import json
key_id = "${ASC_KEY_ID}"
issuer_id = "${ASC_ISSUER_ID}"
key_path = "${KEY_PATH}"

with open(key_path, "r", encoding="utf-8") as f:
        key = f.read()

# Write JSON with proper escaping of newlines
with open("${API_KEY_JSON_PATH}", "w", encoding="utf-8") as f:
        json.dump({"key_id": key_id, "issuer_id": issuer_id, "key": key}, f)
PY

IPA="$(find UI/bin/${{ env.CONFIGURATION }}/net10.0-ios -type f -name '*.ipa' -print -quit)"
if [ -z "$IPA" ]; then
        echo "IPA not found" >&2
        exit 1
fi

bundle exec fastlane pilot upload \
--ipa "$IPA" \
--api_key_path "$API_KEY_JSON_PATH" \
--app_identifier "$IOS_BUNDLE_ID" \
--skip_waiting_for_build_processing true

