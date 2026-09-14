#!/bin/bash

set -e

echo "🧹 Nettoyage..."
rm -rf TestResults
rm -rf coverage-report

echo "🧪 Exécution des tests..."
dotnet test GeneralSurvey.slnx \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults

echo "📊 Recherche du coverage..."

COVERAGE_FILE=$(find TestResults -name "coverage.cobertura.xml" -print -quit)

if [ -z "$COVERAGE_FILE" ]; then
    echo "❌ Aucun fichier coverage trouvé."
    exit 1
fi

echo "✅ Coverage trouvé : $COVERAGE_FILE"

echo "📄 Génération du rapport HTML..."

reportgenerator \
    "-reports:$COVERAGE_FILE" \
    "-targetdir:coverage-report" \
    "-reporttypes:Html" \
    "-classfilters:-Microsoft.AspNetCore.OpenApi.Generated;-System.Runtime.CompilerServices"


echo "🌐 Ouverture du rapport..."

open coverage-report/index.html

echo "✅ Coverage terminé."