## Inhoudsopgave

- [Toepassingsarchitectuur](#toepassingsarchitectuur)
- [Azure-accountvereisten](#accountvereisten)
- [Aan de slag](#aan-de-slag)
	- [Projectconfiguratie](#projectconfiguratie)
		- [GitHub Codespaces](#github-codespaces)
		- [VS Code Dev Containers](#vs-code-dev-containers)
		- [Lokale omgeving](#lokale-omgeving)
	- [Implementatie](#implementatie)
		- [Implementatie vanaf scratch](#implementatie-vanaf-scratch)
		- [Lokaal uitvoeren](#lokaal-uitvoeren)
		- [Omgevingen delen](#omgevingen-delen)
		- [Bronnen opruimen](#bronnen-opruimen)

# ChatGPT + data met Azure OpenAI en Cognitive Search

Deze voorbeeldtoepassing demonstreert een implementatie van de lab2 use-case met behulp van C#.  Het maakt gebruik van de Azure OpenAI-service om toegang te krijgen tot het ChatGPT-model (`gpt-35-turbo`) en Azure Cognitive Search voor gegevensindexering.

## Toepassingsarchitectuur

- **Gebruikersinterface** - De userinterface van de applicatie is een [Blazor](https://learn.microsoft.com/aspnet/core/blazor/) toepassing. Deze interface laat de gebruiker een vraag stellen, routeert deze vraag naar de backend en geeft gegenereerde antwoorden weer.
- **Backend** - De backend van de applicatie is een [ASP.NET Core Minimal API](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/overview). De services die in deze toepassing worden gebruikt, zijn onder andere:
   - [**Azure Cognitive Search**](https://learn.microsoft.com/azure/search/search-what-is-azure-search) - Helpt bij het zoeken naar gegevens door middel van [vector search](https://learn.microsoft.com/azure/search/search-get-started-vector)-mogelijkheden.
   - [**Azure OpenAI Service**](https://learn.microsoft.com/azure/ai-services/openai/overview) - levert de Large Language Models voor het genereren van antwoorden. 

## Aan de slag

### Accountvereisten

Om dit voorbeeld te implementeren en uit te voeren, hebt je het volgende nodig:

- **Azure-account** - Als je nieuw bent bij Azure, krijg je een [gratis Azure-account](https://aka.ms/free) en ontvangt je wat gratis Azure-tegoed om mee te beginnen.
- **Azure-abonnement met ingeschakelde toegang voor de Azure OpenAI-service** - [U kunt toegang aanvragen](https://aka.ms/oaiapply). U kunt ook [de documentatie van Cognitive Search](https://azure.microsoft.com/free/cognitive-search/) raadplegen voor gratis Azure-tegoed om mee te beginnen.

> **Waarschuwing**<br>
> Standaard maakt dit voorbeeld een Azure Container App en een Azure Cognitive Search-resource aan die maandelijkse kosten met zich meebrengen. Je kan ze naar gratis versies wijzigen als je deze kosten wilt vermijden door het parametersbestand onder de map 'infra' te wijzigen (hoewel er enkele beperkingen zijn om rekening mee te houden; bijvoorbeeld, kan je maximaal 1 gratis Cognitive Search-resource per abonnement hebben).

### Projectconfiguratie

Je hebt verschillende opties om dit project in te stellen. De eenvoudigste manier om te beginnen is GitHub Codespaces, omdat het alle tools voor u instelt, maar je kan het ook [lokaal instellen](#lokale-omgeving) indien gewenst.

#### GitHub Codespaces

Je kan deze repository virtueel uitvoeren met GitHub Codespaces, wat een op web gebaseerde VS Code in uw browser opent:

[![Openen in GitHub - Codespaces](https://img.shields.io/static/v1?style=for-the-badge&label=GitHub+Codespaces&message=Openen&color=brightgreen&logo=github)](https://github.com/codespaces)

#### VS Code Dev Containers

Een gerelateerde optie is VS Code Dev Containers, die het project in je lokale VS Code opent met behulp van de [Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)-extensie:

[![Openen in Remote - Containers](https://img.shields.io/static/v1?style=for-the-badge&label=Remote%20-%20Containers&message=Openen&color=blue&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume)

#### Lokale omgeving

Installeer de volgende vereisten:

- [Azure Developer CLI](https://aka.ms/azure-dev/install)
- [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/downloads)
- [Powershell 7+ (pwsh)](https://github.com/powershell/powershell) - Alleen voor Windows-gebruikers.

   > **Belangrijk**<br> 
   > Zorg ervoor dat je `pwsh.exe` kunt uitvoeren vanuit een PowerShell-commando. Als dit mislukt, moet je waarschijnlijk PowerShell upgraden.

- [Docker](https://www.docker.com/products/docker-desktop/)

   > **Belangrijk**<br>
   > Zorg ervoor dat Docker actief is voordat je `azd` provisioning/deploy opdrachten uitvoert.

Voer vervolgens de volgende opdrachten uit om het project op uw lokale omgeving te krijgen:

   1. Voer `azd auth login` uit
   1. Kloon de repository of voer `azd init -t 4dotnet-devday-csharp` uit
   1. Voer `azd env new 4dotnet-devday-csharp` uit

### Implementatie

#### Implementatie vanaf scratch

> **Belangrijk**<br>
> Zorg ervoor dat Docker actief is voordat je enige `azd` provisioning/deploy opdrachten uitvoert.

Voer de volgende opdracht uit als je vooraf geen bestaande Azure-services hebt en met een lege Azure subscription wilt beginnen.

1. Voer `azd up` uit - Hiermee worden Azure-resources ingericht en wordt dit voorbeeld naar die resources geïmplementeerd, inclusief het bouwen van de searchindex op basis van de bestanden in de map `./data`.
   - Kies voor de resourcegroup locatie een regio's die momenteel het gebruikte van de OpenAI modellen in dit voorbeeld ondersteunen **East US 2**, **East US**, **North Central US**, **South Central US**, **France Central**, **UK South**. Voor een actuele lijst van regio's en modellen kan je [hier](https://learn.microsoft.com/azure/cognitive-services/openai/concepts/models) controleren.
   - Als je toegang hebt tot meerdere Azure-abonnementen, wordt je gevraagd het abonnement te selecteren dat je wilt gebruiken. Als je slechts toegang hebt tot één abonnement, wordt dit automatisch geselecteerd.

   > **Opmerking**<br>
   > Deze toepassing gebruikt het model `gpt-35-turbo`. Bij het kiezen van de regio om naar te deployen, moet je ervoor zorgen dat dit model beschikbaar is in die regio (bijv. EastUS). Zie voor meer informatie de [Azure OpenAI Service-documentatie](https://learn.microsoft.com/azure/cognitive-services/openai/concepts/models#gpt-35-models).

1. Nadat de toepassing succesvol is deployed, wordt een URL afgedrukt op de console. Klik op die URL om met de browser naar de applicatie te navigeren.

> **Opmerking**<br>
> Het kan enkele minuten duren voordat de toepassing volledig is deployed.

#### Gebruik van bestaande resources

Als je bestaande resources in Azure hebt die je wilt gebruiken, kunt je `azd` configureren om deze resources te gebruiken door de volgende `azd`-omgevingsvariabelen in te stellen:

1. Voer `azd env set AZURE_OPENAI_SERVICE {Naam van de bestaande OpenAI-service}` uit
1. Voer `azd env set AZURE_OPENAI_RESOURCE_GROUP {Naam van de bestaande resourcegroep waarin de OpenAI-service is ingericht}` uit
1. Voer `azd env set AZURE_OPENAI_CHATGPT_DEPLOYMENT {Naam van de bestaande ChatGPT-implementatie}` uit. Alleen nodig als uw ChatGPT-implementatie niet de standaard 'chat' is.
1. Voer `azd env set AZURE_OPENAI_EMBEDDING_DEPLOYMENT {Naam van de bestaande implementatie van het embedding-model}` uit. Alleen nodig als uw implementatie van het embedding-model niet de standaard `embedding` is.
1. Voer `azd up` uit

Als alternatief is het ook mogelijk om van OpenAI services gebruik te maken, hiervoor is een openai key nodig en dit is als volgt te configureren:

1. Voer `azd env set OPENAI_HOST openai` uit
1. Voer `azd env set OPENAI_API_KEY {Naam van de gegenereerde key waarmee met de OpenAI-service gecommuniceerd kan worden}` uit

#### Lokaal uitvoeren

> **Belangrijk**<br>
> Zorg ervoor dat Docker actief is voordat je enige `azd` provisioning/deploy opdrachten uitvoert.

1. Voer `azd auth login` uit
1. Voer de volgende .NET CLI-opdracht uit om de ASP.NET Core Minimal API-server (client host) te starten:

   ```dotnetcli
   dotnet run --project ./src/DevDay.AppHost
   ```

Ga naar <http://localhost:5184> en test de app uit.

#### Omgevingen delen

Voer het volgende uit als je iemand anders toegang wilt geven tot de geïmplementeerde en bestaande omgeving.

1. Installeer de [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
1. Voer `azd init -t 4dotnet-devday-csharp` uit
1. Voer `azd env refresh -e {environmentname}` uit - Merk op dat men de azd-environmentname, subscription-ID en locatie nodig hebben om deze opdracht uit te voeren - je kunt die waarden vinden in je `./azure/{environmentname}/.env`-bestand. Hiermee wordt het .env-bestand van de andere azd-omgeving gevuld met alle instellingen die nodig zijn om de app lokaal uit te voeren.
1. Voer `pwsh ./scripts/roles.ps1` uit - Hiermee worden alle benodigde rollen aan de gebruiker toegewezen, zodat ook deze de app lokaal kan uitvoeren. Als deze gebruiker niet de nodige toestemming heeft om rollen te maken in de subscription, moet je mogelijk dit script voor hen uitvoeren. Zorg er gewoon voor dat de omgevingsvariabele `AZURE_PRINCIPAL_ID` is ingesteld in het azd .env-bestand of in de actieve shell naar hun Azure-ID, die de gebruiker kunnen krijgen met `az account show`.

### Bronnen opruimen

Voer `azd down` uit