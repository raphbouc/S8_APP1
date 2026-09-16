# Livrable 9 : Recommandations d'architecture système et opérationnelle

**Projet CoupDeSonde - GEI-771**

Dans le cadre du projet CoupDeSonde, nous avons conçu l'API avec une approche de sécurité axée sur la défense en profondeur. Ce document résume nos choix d'architecture système et nos recommandations pour le déploiement en production afin de garantir la robustesse de l'application.

## 1. Architecture Zero Trust

Historiquement, la sécurité réseau reposait sur un périmètre fort, où tout ce qui se trouvait sur le réseau interne était implicitement digne de confiance. Pour ce projet, nous avons adopté un modèle **Zero Trust**. Nous partons du principe que le réseau est toujours hostile et qu'aucune requête ne doit être acceptée sans vérification, peu importe sa provenance. (Comme vue au procédurale 1)

C'est pour cette raison que l'API web n'a pas les droits nécessaires pour créer des jetons valides ; elle ne fait que les lire et les marquer comme consommés. En cas de compromission du serveur web, un attaquant ne pourrait pas générer de nouvelles clés pour truquer les votes.

De plus, nous appliquons le principe du moindre privilège. Les droits des clients sont restreints selon leur rôle (simulé par la clé d'API), et chaque requête HTTP est vérifiée explicitement par nos middlewares d'authentification avant même d'atteindre la logique d'affaires.

## 2. Protections offertes par le système d'exploitation (ASLR et DEP)

L'API est codée en C# (ASP.NET Core), ce qui nous protège intrinsèquement contre la majorité des failles de gestion de mémoire (comme les _buffer overflows_) grâce au _garbage collector_ et aux vérifications de bornes gérées par le _runtime_ .NET.

Néanmoins, pour consolider notre posture de sécurité face aux potentielles vulnérabilités du _runtime_ lui-même ou de dépendances natives sous-jacentes, nous recommandons fortement de déployer l'application sur un système d'exploitation moderne (comme Linux Ubuntu LTS, Alpine, ou macOS). Ces OS appliquent des mécanismes de protection au niveau du noyau :

- **ASLR (Address Space Layout Randomization)** : Les positions des zones de données, du tas (_heap_) et de la pile (_stack_) sont rendues aléatoires en mémoire à chaque exécution. Un attaquant ne peut donc pas prédire où injecter ou exécuter son code.
- **DEP (Data Execution Prevention / bit NX)** : L'OS marque certaines zones de mémoire (comme celles contenant nos variables) comme non-exécutables. Même si un attaquant réussit à écrire du code malveillant en mémoire, le processeur refusera de l'exécuter.

Ces deux mécanismes combinés rendent l'exploitation de failles de corruption de mémoire virtuellement impossible en pratique sur notre infrastructure.

## 3. Recommandations de déploiement (Conteneurisation)

Pour le déploiement en production, nous préconisons l'utilisation de **Docker**. La conteneurisation garantit l'immuabilité de l'environnement d'exécution et isole l'API du reste du serveur hôte.

Cependant, un conteneur mal configuré peut introduire des failles graves. Voici nos recommandations spécifiques pour le fichier `Dockerfile` et l'environnement d'exécution :

- **Exécution sans privilèges (Non-Root)** : Par défaut, les processus dans un conteneur roulent souvent en tant que `root`. Nous recommandons de créer un utilisateur dédié sans privilèges (`USER appuser`) dans l'image Docker. Si l'API est compromise, l'attaquant n'aura pas les droits _root_ à l'intérieur du conteneur, limitant ainsi ses possibilités d'élévation de privilèges ou de fuite vers le système hôte.
- **Permissions strictes sur le stockage** : Le dossier `data/`, qui stocke les fichiers sensibles (`sondage.txt`, `participant-keys.json`, `responses.json`), doit être monté en tant que volume avec des droits d'accès extrêmement restreints. Seul l'utilisateur `appuser` doit détenir les droits de lecture et d'écriture exclusifs (ex: `chmod 600` ou `700`) sur ces fichiers critiques.
