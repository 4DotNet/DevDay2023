# DevDay2023 - OpenAI in practice

De 4DotNet DevDay is een dag waarop we ons onderdompelen in nieuwe technologie en op dit moment kunnen we niet om AI heen. Large Language Models (LLMs) zoals GPT zijn een enorme hype, maar wat kun je er nu echt mee?

We gaan aan de slag met OpenAI en Azure Cognitive Search om een Retrieval Augmented Generation (RAG) applicatie te bouwen. Met deze techniek maak je het mogelijk om vragen te stellen over je eigen data in natuurlijke taal en relevante antwoorden te genereren. 
Dit is een hele practische toepassing van Large Language Models (LLM) en het is een van de eerste toepassingen die beschikbaar is voor het grote publiek. We gaan zien wat er komt kijken bij het voorbereiden van data, hoe AI integreert met search, en we gaan prompts engineeren in Jupyter notebooks.

Om ons goed op weg te helpen komt Henk Boelman (Microsoft) 's morgens de concepten introduceren en daarna gaan we in teams zelf aan de slag. Aan het eind van de dag mag elk team presenteren wat ze geleerd en gemaakt hebben. 

## Voorbereiding

Om vlot van start te gaan is het handig als je wat dingen voorbereidt:

* Zorg dat je laptop up-to-date is 
* Update Visual Studio
* Installeer [Visual Studio Code](https://code.visualstudio.com/)
* Installeer [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
* Zorg voor een Azure Subscription met tegoed via [My Visual Studio](https://my.visualstudio.com/Benefits)

## Handige Links

### Azure Cognitive Search
* [Retrieval Augmented Generation (RAG) in Azure Cognitive Search](https://learn.microsoft.com/en-us/azure/search/retrieval-augmented-generation-overview) on Microsoft Learn
* [Vector search in Azure Cognitive Search](https://learn.microsoft.com/en-us/azure/search/vector-search-overview) on Microsoft Learn
* [Hybrid search using vectors and full text in Azure Cognitive Search](https://learn.microsoft.com/en-us/azure/search/hybrid-search-overview) on Microsoft Learn
* [Introducing Vector Search in Azure Cognitive Search | Azure Friday](https://www.youtube.com/watch?v=Bd9LWW4cxEU) on YouTube
  
* **Demo's**
  * Vector search (public preview) - Azure Cognitive Search  
  [Azure/cognitive-search-vector-pr](https://github.com/azure/cognitive-search-vector-pr) on Github
  * ChatGPT + Enterprise data with Azure OpenAI and Cognitive Search  
  [Azure-Samples/azure-search-openai-demo](https://github.com/Azure-Samples/azure-search-openai-demo) on Github


### Open AI
* [OpenAI pricing](https://openai.com/pricing)
* [Open AI API reference](https://platform.openai.com/docs/api-reference)
* [OpenAI Cookbook](https://github.com/openai/openai-cookbook) on GitHub



## Datasets

* Q&A content; [StackOverflow](https://archive.org/details/stackexchange)
* Restaurant or Hotel reviews
* Classic literature excerpts
* Imdb: <https://developer.imdb.com/non-commercial-datasets/>
* Dutch Government data: <https://data.overheid.nl/datasets>
* Tweede kamer: <https://opendata.tweedekamer.nl/>
* CBS: <https://www.cbs.nl/nl-nl/onze-diensten/open-data>
* RDW: <https://opendata.rdw.nl/>
* Rijksmuseum: <https://data.rijksmuseum.nl/object-metadata/download/> (Dublin Core dataset is het makkelijkst bruikbaar)
* Dialoog : <https://convokit.cornell.edu/>  
  o.a. Movies / Reddit / Supreme Court / Tennis Interviews / Friends
* [Kaggle](https://www.kaggle.com/datasets) :
  * Dutch House Prices <https://www.kaggle.com/datasets/bryan2k19/dutch-house-prices-dataset>
  * Municipalities of the Netherlands
   <https://www.kaggle.com/datasets/justinboon/municipalities-of-the-netherlands>
  * Imdb Top 1000 movies : <https://www.kaggle.com/datasets/harshitshankhdhar/imdb-dataset-of-top-1000-movies-and-tv-shows>
  * Anime with synopsis : <https://www.kaggle.com/datasets/hernan4444/anime-recommendation-database-2020?select=anime_with_synopsis.csv>

## Ideeen

* [easy] Predict Boston house prices (<https://www.kaggle.com/datasets/vikrishnan/boston-house-prices/data>)
* [medium] Make a chatbot that thinks it's a famous (historical) person so a child might be able to learn history in a playful manner.
* [medium] Make a chatbot that can read Microsoft documentation and with that teach or improve for example your bicep skills
