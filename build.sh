# Check for version number parameter
[[ "$1" =~ ^[0-9]+$ ]] || { echo "Invalid version number"; exit 1; }

dotnet publish UI/UI.csproj \
	-c Release \
	-f net10.0-ios \
	/p:BuildIpa=true \
	/p:RunAOTCompilation=false \
	/p:PublishTrimmed=true \
	/p:ApplicationVersion=$1
