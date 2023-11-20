#!/bin/sh

echo ""
echo "Loading azd .env file from current environment"
echo ""

while IFS='=' read -r key value; do
    value=$(echo "$value" | sed 's/^"//' | sed 's/"$//')
    export "$key=$value"
done <<EOF
$(azd env get-values)
EOF

echo "Environment variables set."

if [ -z "$AZD_PREPENV_RAN" ] || [ "$AZD_PREPENV_RAN" = "false" ]; then
    echo 'Running "PrepareEnvironment.dll"'

    pwd

    dotnet run --project "src/PrepareEnvironment/PrepareEnvironment.csproj" -- \
      './data/hotels.json' \
      --openaihost "$OPENAI_HOST" \
      --openaikey "$OPENAI_API_KEY" \
      --openaiservice "$AZURE_OPENAI_SERVICE" \
      --openaimodelname "$AZURE_OPENAI_EMB_DEPLOYMENT" \
      --searchservice "$AZURE_SEARCH_SERVICE" \
      --index "$AZURE_SEARCH_INDEX"

    azd env set AZD_PREPENV_RAN "true"
else
    echo "AZD_PREPENV_RAN is set to true. Skipping the run."
fi