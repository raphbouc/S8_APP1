# Livrable 9 : Recommandations d'architecture système et opérationnelle

**Projet CoupDeSonde - GEI-771**

Dans le cadre du projet CoupDeSonde, nous avons conçu l'API avec une approche de sécurité axée sur la défense en profondeur. Ce document résume nos choix d'architecture système et nos recommandations pour le déploiement en production afin de garantir la robustesse de l'application.

## 1. Architecture Zero Trust

Historiquement, la sécurité réseau reposait sur un périmètre fort, où tout ce qui se trouvait sur le réseau interne était implicitement digne de confiance. Pour ce projet, nous avons adopté un modèle **Zero Trust**. Nous partons du principe que le réseau est toujours hostile et qu'aucune requête ne doit être acceptée sans vérification, peu importe sa provenance. (Comme vu au procédurale 1)

C'est pour cette raison que l'API web n'a pas les droits nécessaires pour créer des jetons valides ; elle ne fait que les lire et les marquer comme consommés. En cas de compromission du serveur web, un attaquant ne pourrait pas générer de nouvelles clés pour truquer les votes.

De plus, nous appliquons le principe du moindre privilège. Les droits des clients sont restreints selon leur rôle (simulé par la clé d'API), et chaque requête HTTP est vérifiée explicitement par nos middlewares d'authentification avant même d'atteindre la logique d'affaires.

## 2. Protections offertes par le système d'exploitation (ASLR et DEP)

L'API est codée en C# (ASP.NET Core), ce qui réduit les risques associés à certaines vulnérabilités de gestion de mémoire grâce aux mécanismes du runtime .NET, notamment la gestion automatique de la mémoire et les vérifications effectuées par le runtime.

Néanmoins, pour consolider notre posture de sécurité face aux potentielles vulnérabilités du runtime lui-même ou de dépendances natives sous-jacentes, nous recommandons de déployer l'application sur un système d'exploitation moderne, comme une distribution Linux maintenue à long terme. Ces systèmes offrent plusieurs mécanismes de protection au niveau du système d'exploitation :

- **ASLR (Address Space Layout Randomization)** : Les adresses utilisées par différentes régions de la mémoire sont randomisées, ce qui complique certaines techniques d'exploitation reposant sur la connaissance d'adresses mémoire précises.

- **DEP / NX (Data Execution Prevention / No-eXecute)** : Certaines régions de mémoire utilisées pour les données sont marquées comme non exécutables. Cela peut empêcher l'exécution de code dans une région mémoire qui n'est pas destinée à contenir du code.

Ces mécanismes ne remplacent pas les mesures de sécurité applicatives, mais ajoutent une couche de protection supplémentaire contre certaines catégories d'exploitation.

## 3. Recommandations de déploiement (Conteneurisation)

Pour le déploiement en production, nous préconisons l'utilisation de **Docker**. La conteneurisation permet de standardiser l'environnement d'exécution et de limiter l'exposition de l'application au reste du serveur hôte.

Cependant, un conteneur mal configuré peut introduire des failles graves. Voici nos recommandations spécifiques pour le fichier `Dockerfile` et l'environnement d'exécution :

- **Exécution sans privilèges (Non-Root)** : Par défaut, les processus dans un conteneur peuvent être exécutés avec des privilèges élevés. Nous recommandons de créer un utilisateur dédié sans privilèges dans l'image Docker. Si l'API est compromise, l'attaquant ne disposera pas des privilèges `root` à l'intérieur du conteneur, ce qui limite les possibilités d'élévation de privilèges.

- **Permissions strictes sur le stockage** : Dans notre prototype, les données sont conservées dans des fichiers `JSON` et `txt`, notamment `participant-keys.json` et `responses.json`. Si cette approche est suffisante pour démontrer le fonctionnement de l'application, elle ne devrait pas être conservée telle quelle en production. Les fichiers contenant des données sensibles devraient être accessibles uniquement par le compte de service de l'API.

## 4. Utilisation d'une base de données sécurisée

Pour un déploiement réel, nous recommandons de remplacer les fichiers `JSON` utilisés par le prototype par une **base de données sécurisée**, par exemple PostgreSQL.

Une base de données permettrait notamment d'améliorer la gestion des accès, la concurrence entre les requêtes et l'intégrité des données. Les opérations liées à la consommation des clés de participants pourraient également être effectuées de manière atomique dans une transaction. Cela permettrait d'éviter qu'une même clé soit consommée simultanément par deux requêtes concurrentes.

Les données de sondage et les réponses devraient être séparées logiquement. Les informations permettant d'authentifier un participant ne devraient pas être enregistrées avec sa réponse. Les clés de participants continueraient d'être conservées sous forme de hash cryptographique plutôt qu'en clair.

L'accès à la base de données devrait également être limité au compte de service utilisé par l'API. Ce compte ne devrait disposer que des permissions nécessaires à l'exécution de l'application.

## 5. Gestion sécurisée de la clé d'API

Dans notre environnement de développement, la clé d'API peut être conservée dans les **User Secrets de .NET** afin d'éviter de l'intégrer directement au code source ou au dépôt Git.

En production, nous recommandons que la clé d'API soit conservée **localement sur le serveur ou dans un gestionnaire de secrets sécurisé**, et qu'elle soit injectée dans l'environnement de l'application au démarrage. Elle ne devrait jamais être inscrite directement dans le code source, dans le dépôt Git ou dans une image Docker.

L'accès au fichier ou au mécanisme permettant de récupérer cette clé devrait être limité au compte de service de l'API. Les clés d'API ne devraient également pas apparaître dans les journaux de l'application.

Cette séparation permet de conserver les secrets indépendamment du code de l'application et de les remplacer sans devoir modifier ou reconstruire la logique applicative.

## 6. HTTPS et protection des communications

Toutes les communications avec l'API doivent être effectuées au moyen de **HTTPS/TLS**. Cela protège notamment les clés d'API et les clés de participants contre l'interception lors de leur transmission.

En production, un certificat TLS valide devrait être utilisé et les versions obsolètes de TLS devraient être désactivées. Il serait également recommandé de placer l'API derrière un reverse proxy ou un équilibreur de charge afin de centraliser certaines fonctions comme la terminaison TLS, la limitation du débit et la journalisation.

## 7. Journalisation et surveillance

L'API devrait conserver des journaux permettant de détecter les comportements anormaux et de faciliter l'analyse d'incidents. Les événements importants, comme les erreurs d'authentification, les erreurs serveur et les accès inhabituels, devraient être journalisés.

Cependant, les journaux ne doivent pas contenir de données sensibles. Les clés d'API, les clés de participants et les réponses aux sondages ne devraient jamais être enregistrées en clair dans les logs.

Un mécanisme de surveillance pourrait également être ajouté afin de détecter un nombre anormal de requêtes ou de tentatives d'authentification, puis déclencher une limitation du débit ou une alerte.

## 8. Architecture de production recommandée

L'architecture proposée pour une version de production serait donc composée de plusieurs couches :

**Client → HTTPS → Reverse Proxy → API ASP.NET Core → Base de données sécurisée**

Le reverse proxy pourrait assurer la terminaison TLS et certaines protections réseau. L'API fonctionnerait dans un conteneur Docker avec un compte sans privilèges. La base de données serait isolée de l'accès direct provenant d'Internet et ne serait accessible que par l'API.

Les secrets nécessaires au fonctionnement de l'application, notamment la clé d'API et les informations de connexion à la base de données, seraient conservés séparément du code source et accessibles uniquement au processus de l'API.

Cette architecture permettrait de conserver les mécanismes de sécurité déjà présents dans le prototype tout en améliorant la persistance des données, la gestion des secrets, l'isolation des composants et la résistance aux accès non autorisés.
